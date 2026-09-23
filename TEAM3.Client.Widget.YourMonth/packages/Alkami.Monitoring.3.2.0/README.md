# Alkami.Monitoring

Base contracts and abstractions for application monitoring at Alkami. Provides a unified `Metric` static API and the `IMonitor`/`IMonitorMinion` interfaces that back-end implementations (such as `Alkami.Monitoring.OpenTelemetry`) plug into.

## Target Frameworks

| TFM | Notes |
|-----|-------|
| `net472` | .NET Framework 4.7.2 |
| `net8.0` | .NET 8 |
| `net10.0` | .NET 10 |

## Installation

```shell
dotnet add package Alkami.Monitoring
```

## Setup

### .NET 8 / .NET 10 — Dependency Injection

Register a monitoring implementation via the `IServiceCollection` extension. This is typically done alongside an `IAlkamiInstrumentationBuilder` (see `Alkami.Monitoring.OpenTelemetry`):

```csharp
services.AddAlkamiMonitoring<MyMonitorMinion>();

// Or with a factory:
services.AddAlkamiMonitoring<MyMonitorMinion>(sp => sp.GetRequiredService<MyMonitorMinion>());
```

The `SetupMonitoringHost` background service wires the DI-resolved monitor into the static `Metric` class automatically.

### .NET Framework 4.7.2 — Legacy Static Setup

```csharp
// Register an IMonitorMinion implementation once at application startup:
AlkamiMonitoring.SetUp(new MyMonitorMinion());

// Set the area name used as a prefix in all metric names:
Metric.AreaName = "MyService";
```

## API Reference

### `Metric` — Static Facade

All methods are safe to call before a monitor is configured; they are silently no-ops when no monitor is registered.

#### Naming

```csharp
// Produces: "Custom/{AreaName}/Payments/Process"
string name = Metric.CreateName("Payments", "Process");
```

#### `RecordMetric`

```csharp
Metric.RecordMetric("Custom/MyService/Queue/Depth", (float)42.0);

// Segment-based overload (CreateName is called internally):
Metric.RecordMetric(42.0f, "Queue", "Depth");
```

#### `RecordResponseTimeMetric`

```csharp
Metric.RecordResponseTimeMetric("Custom/MyService/Api/Latency", TimeSpan.FromMilliseconds(128));
```

#### `IncrementCounter`

```csharp
Metric.IncrementCounter("Custom/MyService/Login/Success");

// Segment-based overload:
Metric.IncrementCounter("Login", "Success");
```

#### `NoticeError`

```csharp
try { /* ... */ }
catch (Exception ex)
{
    Metric.NoticeError(ex);
    // With additional context:
    Metric.NoticeError(ex, new Dictionary<string, string> { ["userId"] = "abc123" });
}

// String overload:
Metric.NoticeError("Validation failed", new Dictionary<string, string> { ["field"] = "email" });
```

#### `RecordCustomEvent`

```csharp
var payload = new MonitorEvent("correlation-id", bankGuid, userGuid);
payload["customKey"] = "customValue";
Metric.RecordCustomEvent("MyCustomEvent", payload);
```

#### `RecordExternalCall`

Records three metrics for every third-party API call:
- `alkami.external_call.duration` — histogram (ms)
- `alkami.external_call.total` — counter
- `alkami.external_call.failures` — counter (only when `IsSuccess = false`)

```csharp
var stopwatch = Stopwatch.StartNew();
bool success = false;
try
{
    await stripeClient.ChargeAsync(request);
    success = true;
}
finally
{
    stopwatch.Stop();
    Metric.RecordExternalCall(new ExternalCall(
        isSuccess:              success,
        duration:               stopwatch.Elapsed,
        providerName:           "Stripe",
        providerType:           "Payment",
        bankInstanceIdentifier: bankId));
}
```

#### `AddCustomProperty`

Adds a name/value pair to the current monitoring transaction context (requires `IMonitorProperties` support):

```csharp
Metric.AddCustomProperty("tenantId", tenantId);
```

#### `SetTransactionName`

```csharp
Metric.SetTransactionName("Background", "ProcessQueue");
```

#### `SetUserProperties`

```csharp
Metric.SetUserProperties(userValue: "jdoe", accountValue: "acme", productValue: "banking");
```

#### `BeginSuppression`

Temporarily suppresses metrics for a specific monitor minion type:

```csharp
using (Metric.BeginSuppression("OpenTelemetry"))
{
    // metrics are suppressed inside this scope
}
```

## Key Types

| Type | Description |
|------|-------------|
| `IMonitor` | Core interface: `RecordMetric`, `IncrementCounter`, `NoticeError`, `RecordCustomEvent`, `RecordExternalCall` |
| `IMonitorProperties` | Optional extension: `AddCustomProperty`, `SetTransactionName`, `SetUserProperties` |
| `IMonitorMinion` | Combines `IMonitor` + `IMonitorProperties`; implement this for a full back-end |
| `IMonitorSuppression` | Optional extension for per-minion suppression scopes |
| `MonitorEvent` | `Dictionary<string, object>` subclass for custom event payloads |
| `ExternalCall` | DTO for third-party API call metrics (see `RecordExternalCall`) |

## Repository

https://gitlab.com/alkami-technology/alkplat/dotnet/libraries/alkami.monitoring
