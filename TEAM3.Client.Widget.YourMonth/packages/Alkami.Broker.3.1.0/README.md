# Alkami.Broker

Base library providing the publish/subscribe broker infrastructure for Alkami services.
Consumers choose a transport back-end (Redis, ZeroMQ, …) by plugging in one of the
companion packages; this library supplies the static API, message model, and abstract
transport contracts that all back-ends share.

**Target Frameworks:** `net472` · `net8.0` · `net10.0`

---

## Installation

```xml
<PackageReference Include="Alkami.Broker" Version="*" />
```

---

## Required Environment Variables

These are read by `EnvironmentInfo` at runtime and must be set before the broker is
initialized. An exception is thrown if any value is missing or whitespace.

| Variable | Example | Description |
|---|---|---|
| `ALKAMI_ENVIRONMENT_FULLNAME` | `AWS Production 15` | Full, human-readable environment name |
| `ALKAMI_ENVIRONMENT_NAME` | `Production 15` | Short environment name used in channel routing |
| `ALKAMI_ENVIRONMENT_TYPE` | `Production` | Environment type used in channel routing |

---

## Optional App Settings

| Key | Default | Description |
|---|---|---|
| `BrokerPollingInterval` | `100` | Polling interval in milliseconds for the subscriber dispatch loop |

Configured via `Alkami.Utilities.Configuration.Manager.GetSetting(...)` (supports
`appSettings` in `App.config` / `Web.config` or equivalent configuration sources).

---

## Logging

| Target Framework | Default back-end | Override |
|---|---|---|
| `net472` | `Common.Logging` | — |
| `net8.0` | `Common.Logging` | Call `BrokerLogging.UseLoggerFactory(loggerFactory)` |
| `net10.0` | Silent discard (`NullLoggerFactory`) | **Must** call `BrokerLogging.UseLoggerFactory(loggerFactory)` |

> **Important:** `BrokerLogging.UseLoggerFactory` must be called **before** any call to
> `Broadcaster.InitializePublisher`, `Subscription.InitializeSubscriber`, or the
> companion `Setup` methods in `Alkami.Broker.Redis` / `Alkami.Broker.ZeroMq`.

```csharp
// net8.0 — optional, routes logs through Microsoft.Extensions.Logging
BrokerLogging.UseLoggerFactory(loggerFactory);

// net10.0 — mandatory, otherwise all broker logs are silently discarded
BrokerLogging.UseLoggerFactory(loggerFactory);
```

---

## Publishing Events

```csharp
using Alkami.Broker.App;
using Alkami.Broker.Base;
using Alkami.Broker.MessageTemplates;

// 1. Assign a tenant resolver — called on every Dispatch to stamp the bank identifier.
//    Must be set before the first call to Broadcaster.Dispatch or InitializePublisher.
Broadcaster.TenantResolver = () => currentBankGuid;

// 1b. Optional: resolve the bank instance identifier for multi-instance tenants.
//     Called per-message inside Enqueue when BankInstanceIdentifier is not already set.
//     Receives a clone of the envelope with BankIdentifier already stamped, allowing
//     lookups against other envelope fields. Must be set before the first Dispatch or
//     InitializePublisher call.
Broadcaster.TenantInstanceIdentifierResolver = (envelope) => ResolveBankInstanceId(envelope.BankIdentifier);

// 2. Optional: label this service in diagnostic logs and envelope metadata
AbstractPublisher.Owner = "MyService";

// 3. Configure a transport back-end before the first dispatch.
//    (Done by the companion package, e.g. Alkami.Broker.Redis.Setup.PublishUsingRedis())

// 4. Initialize explicitly — or let Dispatch call it automatically on first use
Broadcaster.InitializePublisher();

// 5. Dispatch using a typed BrokerArgs subclass
Broadcaster.Dispatch(Events.Login, new LoginArgs { UserIdentifier = userId });

// 5b. Dispatch with a raw dictionary
Broadcaster.Dispatch(Events.CacheRemoved, new Dictionary<string, string>
{
    ["Key"] = "some-cache-key"
});

// 5c. Override the bank identifier or bank instance identifier for a single message;
//     supplying bankInstanceIdentifier here bypasses TenantInstanceIdentifierResolver.
Broadcaster.Dispatch(Events.Transfer, payload, bankIdentifier: specificBankGuid, bankInstanceIdentifier: specificInstanceGuid);

// 6. Stop cleanly during application shutdown
Broadcaster.StopPublisher();
```

---

## Subscribing to Events

```csharp
using Alkami.Broker.App;

// 1. Configure a transport back-end before the first subscription.
//    (Done by the companion package, e.g. Alkami.Broker.Redis.Setup.SubscribeUsingRedis())

// 2. Initialize explicitly — or let Subscription.Add call it automatically on first use
Subscription.InitializeSubscriber();

// 3. Register a callback; returns a unique subscription ID
Guid subId = Subscription.Add(Events.Login, (payload) =>
{
    string userId = payload["UserIdentifier"];
    // handle event ...
});

// 4. Unsubscribe when no longer needed
Subscription.Remove(subId);

// 5. Stop cleanly during application shutdown
Subscription.StopSubscriber();
```

Callbacks are dispatched on `Task.Factory.StartNew` (fire-and-forget), so each handler
runs asynchronously. Multiple subscriptions to the same event are all invoked
independently.

---

## Reserved Keys in the Callback Payload

Every callback `Dictionary<string, string>` is automatically enriched with the following
read-only keys from `ReservedKeyNames`. Do **not** use these key names in your own payload.

| Key constant | Key string | Description |
|---|---|---|
| `ReservedKeyNames.AreaName` | `___AreaName` | Service area that raised the event (`AbstractPublisher.Owner`) |
| `ReservedKeyNames.SourceServer` | `___SourceServer` | Machine name of the publisher |
| `ReservedKeyNames.MessageId` | `___MessageId` | Unique `Guid` for this message |
| `ReservedKeyNames.ProcessId` | `___ProcessId` | Publisher process ID |
| `ReservedKeyNames.ThreadId` | `___ThreadId` | Publisher managed thread ID |
| `ReservedKeyNames.BankIdentifier` | `___BankIdentifier` | Tenant bank `Guid` |
| `ReservedKeyNames.BankInstanceIdentifier` | `___BankInstanceIdentifier` | Tenant bank instance `Guid`; present only when resolved via `TenantInstanceIdentifierResolver` or supplied per-message |
| `ReservedKeyNames.Event` | `___Event` | String name of the `Events` enum value |

---

## Pre / Post-Processing Hooks

Assign delegates to inject cross-cutting logic (e.g. thread-local tenant context) around
every event callback. Both are called with the tenant `BankIdentifier` from the envelope.

```csharp
// Called on the callback thread before each event handler
Subscription.PreProcessor = (bankId) =>
{
    // e.g. set ambient tenant context for this thread
};

// Called on the callback thread after each event handler (even on exception)
Subscription.PostProcessor = (bankId) =>
{
    // e.g. clear ambient tenant context
};
```

---

## Disconnected Event

Raised when the underlying transport reports that the remote broker has disconnected.

```csharp
Subscription.Disconnected += (sender, e) =>
{
    // e.g. log the disconnection, trigger reconnect logic
};
```

---

## Events Reference

| Name | Numeric value | Description |
|---|---|---|
| `StateChangeArgs` | `-2` | Service going up or down |
| `Login` | `1` | User logged in |
| `Logout` | `2` | User logged out |
| `Transfer` | `3` | A transfer occurred |
| `EmailChanged` | `4` | User changed email address |
| `AddressChanged` | `5` | User changed physical address |
| `CacheRemoved` | `6` | A cache entry was removed |
| `TransactionsUpdated` | `7` | Transaction cache needs refreshing |
| `EntityUpdated` | `8` | A widget / entity was updated |
| `EntityRemoved` | `9` | A widget / entity was removed |
| `TrackableEntityChangeCommited` | `10` | A trackable entity setting changed |
| `EntityProgressChanged` | `11` | Entity changed after post-processing |
| `DumpCache` | `12` | Cache dump requested for a user or FI |
| `BillPaySyncCompleted` | `13` | Bill Pay sync completed |
| `BillPayEnrolled` | `14` | User enrolled in Bill Pay |
| `RegistrationDisclosureAccepted` | `15` | User accepted registration disclosure |
| `PendingTransferCompleted` | `16` | Pending transfer completed |
| `ElevateLogging` | `17` | Request to increase logging for a bank |
| `ManipulateServiceInstance` | `18` | Request to restart or stop a service |
| `LoginNonRegisteredDevice` | `19` | User logged in from an unregistered device / IP |
| `BillPaySubscriberGenerated` | `20` | Bill Pay subscriber ID and password generated |
| `ContactChanged` | `21` | User changed a contact |
| `TransferFailed` | `22` | A transfer failed |
| `AccountSyncForUserCompleted` | `23` | Account sync for a user completed |
| `BusinessAchCreditSubmitted` | `24` | Business user submitted an ACH credit |
| `BusinessAchActivity` | `25` | Business ACH activity occurred |
| `CmsUserListModified` | `26` | A CMS user list was modified |

> `Heartbeat` (`-1`) is **obsolete** and automatically filtered out by all built-in
> subscribers. Do not publish or subscribe to it in new code.

---

## Message Templates

Pre-built `BrokerArgs` subclasses live in `Alkami.Broker.MessageTemplates`.
`BrokerArgs` extends `Dictionary<string, string>`, so instances can be passed directly
to `Broadcaster.Dispatch`.

| Class | Intended event | Key properties |
|---|---|---|
| `LoginArgs` | `Events.Login` | `UserIdentifier` (`Guid`) |
| `AddressChangedArgs` | `Events.AddressChanged` | `UserIdentifier`, `OldAddress`, `NewAddress` |
| `StatusChangeArgs` | `Events.StateChangeArgs` | `Owner`, `ServerName`, `IsStartingUp`, `IsShuttingDown` |

### Custom BrokerArgs

```csharp
using Alkami.Broker.MessageTemplates;

public class TransferArgs : BrokerArgs
{
    public Guid UserIdentifier
    {
        get => Guid.TryParse(this.GetValueOrDefault("UserIdentifier"), out var g) ? g : Guid.Empty;
        set => this["UserIdentifier"] = value.ToString();
    }

    public decimal Amount
    {
        get => decimal.TryParse(this.GetValueOrDefault("Amount"), out var d) ? d : 0m;
        set => this["Amount"] = value.ToString();
    }

    public override bool IsValid() =>
        ContainsKey("UserIdentifier") && ContainsKey("Amount");
}

// Usage
Broadcaster.Dispatch(Events.Transfer, new TransferArgs
{
    UserIdentifier = userId,
    Amount = 500.00m
});
```

---

## Extensibility: Custom Transport Back-End

Implement `AbstractPublisher` and `AbstractSubscriber` to plug in any transport.

```csharp
// Publisher
Broadcaster.PublisherFactory = () => new MyCustomPublisher();

// Subscriber
Subscription.SubscriberFactory = () => new MyCustomSubscriber(cancellationToken);
```

The factory **must** be assigned before the first call to `InitializePublisher` /
`InitializeSubscriber` (or the first `Dispatch` / `Add` call that triggers auto-init).
