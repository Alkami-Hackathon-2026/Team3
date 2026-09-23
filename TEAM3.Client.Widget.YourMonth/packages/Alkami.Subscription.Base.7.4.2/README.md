# Alkami.Subscription.Base

Core resolver and data contracts for the Subscription Service pattern. Provides service endpoint discovery and registration infrastructure used by microservices and clients participating in the subscription registry.

## Setup (.NET 8+)

Register the subscription resolver in your DI container during application startup:

```csharp
// Program.cs
services.AddSubscriptionResolver();
```

`AddSubscriptionResolver` registers the following services:

| Service | Lifetime | Description |
|---|---|---|
| `ISettingsUtility` | Singleton | Configuration reader for subscription settings |
| `ISubscriptionService` | Singleton | WCF channel factory to the Subscription Service |
| `IResolver` | Singleton | Core resolver (local service definition cache and refresh) |
| `ISubscriptionServiceResolver` | Singleton | Wrapper around `ServiceResolver` for DI-based access |

## Usage

### Synchronous access (static API)

The static `ServiceResolver` gateway provides convenient access to live service endpoints without DI:

```csharp
// Retrieve all currently known service definitions
List<ServiceDefinition> all = ServiceResolver.AllRegisteredDefinitions();

// Force a refresh from the Subscription Service
await ServiceResolver.Refresh();

// Register this process as a provider of a service
await ServiceResolver.RegisterAsync(myServiceDefinition);

// Gracefully unregister before shutdown
await ServiceResolver.UnRegisterAsync(myServiceDefinition);

// React to any change pushed from the Subscription Service
ServiceResolver.OnChange += definitions => { /* update local caches */ };
```

### Dependency-injected access (.NET 8+)

Inject `ISubscriptionServiceResolver` for testability and composability:

```csharp
public class MyService
{
    private readonly ISubscriptionServiceResolver _resolver;

    public MyService(ISubscriptionServiceResolver resolver)
    {
        _resolver = resolver;
    }

    public async Task GetServices()
    {
        var definitions = await _resolver.GetServices(...);
    }
}
```

### ServiceResolver Key Members

| Member | Description |
| --- | --- |
| `AllRegisteredDefinitions()` | Returns a snapshot of all `ServiceDefinition` objects currently known |
| `RegisterAsync(ServiceDefinition)` | Sends a heartbeat registering the given endpoint |
| `RegisterAsync(List<ServiceDefinition>)` | Batch registration |
| `UnRegisterAsync(ServiceDefinition)` | Notifies the Subscription Service this endpoint is going offline |
| `Refresh()` | Triggers an immediate pull from the Subscription Service |
| `OnChange` | Event fired whenever the service-definition list changes |
| `EstablishedConnectionAtLeastOnce` | `true` once at least one successful pull has been made |
| `IsConnected` | `true` when the most recent pull succeeded |
| `ClientVersion` | Optional version string forwarded to the Subscription Service for filtering |

### Testing: Replacing the resolver

For testing or advanced overrides, replace the resolver singleton before any usage:

```csharp
ServiceResolver.ResolverFactory = () => new MyCustomResolver();
```

## ServiceResolver Architecture

### InnerResolver

Default singleton implementation that manages:
- A background timer that polls the Subscription Service at the configured interval
- A `ReaderWriterLockSlim` protecting the in-memory `ServiceDefinitions` list
- ETag-style hash comparison to avoid unnecessary data transfers
- Firing `OnChange` whenever the definition set changes
- Graceful disposal and cleanup on .NET 8+

### SubscriptionServiceProxy

Internal WCF channel-factory wrapper that communicates with the Subscription Service endpoint. Reads the machine endpoint from environment configuration and manages channel lifecycle.

## Data Types

These types define the contracts shared between client, service, and the Subscription Service.

### ServiceDefinition

The central DTO describing a running service instance.

| Property | Description |
| --- | --- |
| `Name` | Fully-qualified contract type name (e.g. `My.Namespace.IMyContract`) |
| `FriendlyName` | Human-readable display name shown in dashboards and logs |
| `CurrentVersion` | Assembly version of the running service |
| `EndpointUri` | NetTCP URI the service is listening on |
| `ProcessId` | OS PID of the hosting process |
| `ProviderConfiguration` | Metadata for provider-pattern services |
| `K8sConfiguration` | Kubernetes-specific metadata when running in K8s |
| `SupportedClaimsIdentitySerializationMethods` | Bitmask of supported token serialization formats |

### ISubscriptionService

The `[ServiceContract]` interface for the Subscription Service itself. Normally consumed only by `SubscriptionServiceProxy`; exposed publicly for advanced scenarios.

## Environment Variables

| Variable | Description |
|----------|-------------|
| `ALKAMI_SUBSCRIPTION_SERVICE_MACHINE` | *(required)* Hostname of the Subscription Service |
| `ALKAMI_SUBSCRIPTION_CACHING_ENABLED` | Enable ETag-style caching (default: `true`) |
| `ServiceDefinitionRefreshIntervalSeconds` | Poll interval for service definitions (default: `60`) |
| `ENABLE_SUBSCRIPTION_GARBAGE_COLLECTION` | Run GC after Subscription Service calls (default: `true`) |

### App Settings (.NET Framework 4.7.2)

| Key | Description |
|-----|-------------|
| `SubscriptionServiceMachine` | Hostname of the Subscription Service |
| `ServiceDefinitionRefreshIntervalSeconds` | Poll interval for service definitions |
| `OnDemandServiceDefinitionRefreshIsEnabled` | Force default refresh interval |
