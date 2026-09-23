# Alkami.Subscription.ParticipatingClient

Utilities and client-layer abstractions for microservice clients participating in the Subscription Service registry pattern. Normalizes cross-client behavior: endpoint resolution, load balancing, caching, and claims-identity serialization.

Targeted Runtimes: .NET Framework 4.7.2, .NET 8, .NET 10

## Setup (.NET 8+)

Register ParticipatingClient dependencies in your DI container before creating any `SelfResolvingClient<T>` instances:

```csharp
// Program.cs
services.AddParticipatingClientDependencies();
```

`AddParticipatingClientDependencies` registers:

| Service | Lifetime | Description |
|---|---|---|
| `IParticipatingClientBindings` | Singleton | Binding configuration for WCF channels |
| `IServiceEndpointResolver<T>` | Singleton (open generic) | Endpoint selection logic |
| `IServiceEndpointRepository<T>` | Singleton (open generic) | Local service definition cache |
| `IAlkamiClientFactoryCache<T>` | Singleton (open generic) | WCF channel factory cache |
| `IProviderCacheManagerFactory` | Singleton | Per-provider-type cache of `ProviderDefinition` data fetched from the Settings service |
| `IProviderBasedEndpointResolver<T>` | Singleton (open generic) | Endpoint selection logic for provider-based clients |
| `ISettingsServiceContract` | Singleton | WCF client for the Settings microservice |
| `ISettingsClient` | Singleton | Abstraction over the Settings service used to fetch provider definitions; injectable for testability |
| `IHostedService` (ParticipatingClientBootstrapper) | — | Initialization hook that wires DI into `SelfResolvingClient<T>` |

## Usage

### Creating a Client

Extend `SelfResolvingClient<T>` to create a typed client for any WCF service contract:

```csharp
public class MyServiceClient : SelfResolvingClient<IMyServiceContract>, IMyServiceContract
{
    public MyServiceClient(Version minVersion = null) : base(minVersion) { }

    public Task<MyResponse> DoWork(MyRequest request)
        => ProxyCallInternal(endpoint => endpoint.Channel.DoWork(request), request);
}
```

Inject and use the client like any other service:

```csharp
public class ConsumerService
{
    private readonly MyServiceClient _client;

    public ConsumerService(MyServiceClient client)
    {
        _client = client;
    }

    public async Task CallService()
    {
        var response = await _client.DoWork(new MyRequest());
    }
}
```

### SelfResolvingClient<T>

Base class for WCF service clients. Handles endpoint resolution, load balancing, and WCF channel lifecycle automatically.

**Constructor parameter:**
- **MinVersion** (optional): minimum service assembly version the client will accept. Defaults to the client's own assembly version.

**Protected method:**
- **ProxyCallInternal**: Routes a single call through the resolved endpoint. Handles retries, circuit-breaker logic, and error translation.

### ProviderBasedClient<T>

Base class for clients that participate in the **provider-based pattern** — one-to-many communication where multiple services implement the same contract and are each backed by a registered provider record. Extends `SelfResolvingClient<T>`.

```csharp
public class PushNotificationClient : ProviderBasedClient<IPushNotificationProvider>, IMultiplePushNotificationProvider
{
    public static readonly string ProviderType = "Push";

    public PushNotificationClient() : base(ProviderType) { }

    // .NET 8+ DI constructor
    public PushNotificationClient(IServiceProvider serviceProvider)
        : base(serviceProvider, ProviderType) { }

    public Task<SendNotificationResponse>[] Send(SendNotificationRequest request)
        => ProxyCallMultiple((contract, r) => contract.Send(r), request);
}
```

**Constructor parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `providerType` | `string` | ✓ | Name of the provider type (e.g., `"Push"`) used to look up configured providers from the Settings service |
| `providerId` | `long` | — | Narrows results to a single provider by database ID; use only when custom conditional logic determines the provider |
| `providerName` | `string` | — | Narrows results to a single provider by name |
| `version` | `Version` | — | Minimum acceptable service assembly version; defaults to the client's own assembly version |
| `timeoutSeconds` | `int?` | — | WCF operation timeout override |
| `serviceProvider` | `IServiceProvider` | .NET 8+ | Injected service provider; preferred constructor on .NET 8 and .NET 10 |
| `options` | `IOptions<ParticipatingClientOptions<T>>` | .NET 8+ | Wraps `version` and `timeoutSeconds` as DI-compatible options |

**Protected methods:**

- **ProxyCallMultiple**: Fan-out — dispatches the operation to **every** configured provider for the FI and returns an array of tasks, one per provider. Use this for the fan-out pattern.
- **ProxyCallSingle**: Single-provider — dispatches to the **first** configured provider and returns a single task. Throws `ServiceInstanceNotFoundException` if no provider is found.

### Provider-Based Pattern

A provider-based service shares its WCF contract with multiple independent services. Each implementation registers itself as a **provider**, controlled by a `core.provider` record paired with a `core.item` record in the Settings service. The client fetches and caches this data and routes calls only to providers that are currently enabled for the requesting FI.

**Why you must use `ProxyCallMultiple` or `ProxyCallSingle` — not `ProxyCallInternal`**

`SelfResolvingClient<T>.ProxyCallInternal` selects an endpoint using the standard load balancer, which has no knowledge of which service definitions correspond to enabled providers for the FI. `ProviderBasedClient<T>` overrides endpoint selection to first resolve the correct provider set from the Settings service and only then map each provider to its service endpoint. Calling `ProxyCallInternal` directly bypasses this and will route to whichever endpoint the load balancer picks, regardless of provider configuration.

**Single-provider pattern**

Multiple services implement the same contract, but only one is enabled for a given FI. Use `ProxyCallSingle` (or aggregate `ProxyCallMultiple` results knowing exactly one task will be returned). Typical examples: core banking provider, billing provider.

**Fan-out pattern**

Multiple services implement the same contract and all are expected to be called for each request; results are aggregated by the caller. Use `ProxyCallMultiple` and `await Task.WhenAll(...)` to collect all responses. Typical example: push notification channels.

**Provider and item record requirements**

For a provider to be selected, **both** a `core.provider` record and a `core.item` record must exist and be enabled for the FI. Only items of type `Connector` or `Processor` are considered valid; all others are filtered out.

### Endpoint Resolution

`ServiceEndpointResolver<T>` selects an endpoint for each request:
1. Queries the local cache maintained by `ServiceEndpointRepository<T>`
2. Filters to endpoints whose `CurrentVersion` meets `MinVersion` constraints (same major, >= minor)
3. Falls back to the highest available version if no exact match exists
4. Applies the configured load balancer to choose among matching endpoints

### Load Balancers

Configure the load-balancing strategy via the `ParticipatingClient.LoadBalancerImplementation` setting.

#### RoundRobinLoadBalancer (default)
- Value: `roundrobin`
- Cycles through endpoints sequentially (first → last → first). Thread-safe via `Interlocked`.

#### RandomLoadBalancer
- Value: `random`
- Picks an endpoint at random using a thread-local `Random` instance.

#### BestAverageThroughputLoadBalancer
- Value: `best-average-throughput`
- Selects endpoints based on average response time to optimize throughput.

## Circuit Breaker (Cool Down)

When a non-managed endpoint failure occurs (network timeout, endpoint gone, access error), the client triggers a cool-down period during which **all** calls to that service fail immediately. This prevents cascading load on unhealthy services and improves fail-fast behavior.

**Configuration:**
- **CoolDownPeriodInSeconds**: Duration of the cool-down period after a failure (default: `60` seconds)
- Once the cool-down period elapses, the circuit resets automatically

## Configuration

### Environment Variables (.NET 8+)

| Variable | Default | Description |
|----------|---------|-------------|
| `ALKAMI_PARTICIPATING_CLIENT_MEMORY_CACHE_TTL` | `15` | Sliding TTL (seconds) for the WCF channel cache |
| `ParticipatingClient.LoadBalancerImplementation` | `roundrobin` | Load-balancing strategy |
| `ALKAMI_FAVORED_SUT_SERIALIZATION` | `CompressedJson` | WCF claims-identity serialization method |
| `ALKAMI_ENABLE_VERSIONLESS_URLS` | `false` | When `true`, internal K8s service URLs omit the version segment (e.g., `alk-svc-rpc-iservice` instead of `alk-svc-rpc-iservice-v7`) |
| `ProviderBasedClientCacheRefreshIntervalSeconds` | `600` | How often (seconds) the provider definition cache is refreshed from the Settings service |

### App Settings (.NET Framework 4.7.2)

| Key | Default | Description |
|-----|---------|-------------|
| `ServiceDefinitionRefreshIntervalSeconds` | `60` | How often to refresh service definitions |
| `OnDemandServiceDefinitionRefreshIsEnabled` | `false` | Force refresh interval to default value |
| `ParticipatingClient.LoadBalancerImplementation` | `roundrobin` | Load-balancing strategy |
| `CoolDownPeriodInSeconds` | `60` | Cool-down duration after endpoint failure |
| `ParticipatingClientMemoryCacheTTL` | `15` | WCF channel cache TTL (seconds) |
| `ProviderBasedClientCacheRefreshIntervalSeconds` | `600` | How often (seconds) the provider definition cache is refreshed from the Settings service |

### Serialization Methods

The `ALKAMI_FAVORED_SUT_SERIALIZATION` setting determines how claims-identity tokens are serialized when not explicitly set on the `BaseRequest`. Accepted values:

- `Json` — Plain JSON (smallest for small tokens, larger for large tokens)
- `CompressedJson` (default) — JSON compressed with GZip (smallest for large tokens)
- `DataContractSerializer` (.NET Framework 4.7.2 only)

## Advanced

### ServiceResolver

Access the global resolver singleton for advanced scenarios (e.g., periodic health checks, custom refreshes):

```csharp
var definitions = ServiceResolver.AllRegisteredDefinitions();
await ServiceResolver.Refresh();
ServiceResolver.OnChange += defs => { /* react to changes */ };
```

For testability on .NET 8+, prefer injecting `ISubscriptionServiceResolver` instead.

### IServiceEndpointRepository<T>

The local cache of `ServiceEndpoint<T>` objects for a given contract. Subscribes to `ServiceResolver.OnChange` and updates automatically. Can be injected for custom cache inspection or invalidation logic.

### ISettingsClient

Abstraction over the Settings microservice used by `ProviderBasedClient<T>` to fetch and refresh provider definitions. On .NET 8+, `ISettingsClient` is registered by `AddParticipatingClientDependencies` and can be injected into tests or custom infrastructure to stub or verify provider lookups without making real network calls.

```csharp
// In tests or custom infrastructure (.NET 8+)
services.AddSingleton<ISettingsClient, MyFakeSettingsClient>();
```

For .NET Framework, the default `SettingsClient` is constructed internally and cannot be replaced without subclassing.

## Observability

Clients automatically participate in OpenTelemetry instrumentation. WCF channel operations are traced and tagged with service name, endpoint URI, and latency. Cool-down periods are also instrumented.
