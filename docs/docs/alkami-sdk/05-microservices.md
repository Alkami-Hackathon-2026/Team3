# Microservices

What this covers: building, configuring, testing, packaging, and installing custom Alkami SDK microservices (WCF services hosted as Windows services under Topshelf and registered with the Alkami Subscription service). It walks the "My First Microservice" provider-service tutorial, lists the template catalog, shows how to refactor a logical service into a provider-based (configurable settings) service, how to wire a widget to a microservice client, how to use the Alkami-supplied GenericProxy microservice instead of writing your own, and how to test with the Micro Service Tester and Sidekick. It also covers AlkamiManifest.xml, the package validation checks that block submissions, the Common.Corelation v4 upgrade, and sending email and alerts from a widget. The "Microservice Update Guide" page was not in this topic's source list.

## 1. Microservice types and SDK template catalog

Templates come from `choco install Alkami.SDK.Templates -y`. Generic Proxy is pre-built by Alkami and installed rather than generated.

| Service / Template | Kind | Tests | Contract by | Created / supported by |
|---|---|---|---|---|
| Generic Proxy Microservice | Pre-configured | N/A | Alkami | Alkami |
| Slim Microservice | Template | N/A | SDK User | SDK User |
| Provider Microservice | Template | Unit | SDK User | SDK User |
| Logic Microservice | Template | Unit and integration | SDK User | SDK User |
| SSO Microservice | Template | Unit | Alkami | SDK User |
| Quick Apply | Provider integration | Unit | Alkami | SDK User |
| Card Management | Provider integration | Unit | Alkami | SDK User |
| Check Imaging | Provider integration | Unit | Alkami | SDK User |
| Facts | Provider integration | Integration | Alkami | SDK User |

- Generic Proxy: call a secure web service or page without a new microservice (section 7).
- Slim: minimum components for a self-contained service and client proxy.
- Provider: settings defined in C# inside the service, managed by admins in the Admin Portal under Setup > Integration Settings > Providers. Use this whenever the service needs configuration.
- Logic: no persistent data storage; settings possible but the Provider template is preferred if you need them.
- SSO: Generic SSO Host with the method implementations for a custom SSO service on Alkami's standard contracts.
- Quick Apply (product lists/options for the Quick Apply widget), Card Management (implements `ICardManagementProviderContract`), Check Imaging (retrieves check images from an external source), Facts ("facts" for package assignment rules and the rules engine): Alkami-defined contracts and data objects.

The per-feature columns on the source page (Configurable Settings, Filters, Sorters, Mappers, Validations) are icons that did not survive extraction.

Sources: SDK Microservice Features (https://confluence.alkami.com/spaces/SDKC/pages/95498547)

## 2. My First Microservice (provider service tutorial)

Updated 2023-09-15. Builds a provider-based service from the Alkami Provider Service template, then adds a "Loan Decision" feature. Source is in the Alkami Samples package under `MyMoney\ProviderService` (`choco upgrade Alkami.SDK.Samples -y`).

### 2.1 Create, build, configure, run

1. Close Visual Studio (the choco installer runs `VisualStudioInstaller.exe`), then `choco install Alkami.SDK.Templates -y`.
2. Open Visual Studio **as Administrator** (required to attach the debugger to the service). Search "alkami", select **Alkami Provider Service**. Name the project per the Microservice Coding Guidelines page (tutorial uses `USBFI.MS.MyMoney`), use a path close to `C:\`, select no options, Create.
3. Build. VS prompts for feed credentials, the same as for https://feeds.alkamitech.com/ (https://feeds.alkamitech.com/feeds/choco.dev). `Service.Host` must be the Startup Project.
4. Configure: run `insert_provider_setting.sql` from the `Service.Host` project (`ProviderScripts` folder) against server `.`. It inserts the provider type, provider, and Connector item into `DeveloperDynamic`. Local development only; on submission the SDK team configures the Alkami environment from the values in your script.
5. Run: `Alkami.Services.Subscriptions.Host` (Windows Services name `Alkami.Services.SubscriptionHost`) must be running. Set a breakpoint on the first line of `ServiceImp.GetSettingsAsync()`, press F5; a `cmd.exe` window wraps the Topshelf host.
6. Test with the Micro Service Tester (section 3): select the service, `GetSettings`, Send Message; the breakpoint hits.
7. Unit tests need the NUnit 3 Test Adapter (https://marketplace.visualstudio.com/items?itemName=NUnitDevelopers.NUnit3TestAdapter). The template test `CanGetSettings()` calls the service through `DistributedServices.cs` and asserts the two default settings.

Solution layout: `Contracts` (interface, `Requests`, `Responses`), `Data` (DataContract POCOs, `ProviderSettings\SettingNames`), `Validations`, `Service` (`ServiceImp`), `Service.Client`, `Service.Host` (`DistributedService`, `Program.cs`, `Installer`, `ProviderScripts`), tests.

### 2.2 Add settings

Settings are defined in the service; Admin Portal values persisted to the database override `DefaultSettings`. To add `MaxLoanAmount` (default 95000) and `MinLoanAmount` (default 5000): add constants to `SettingNames` (`USBFI.MS.MyMoney.Data.ProviderSettings`), add defaults in `DefaultSettings()`, add a `SettingDescriptor` for each in `SettingDescriptors()` (required; this is what the Admin Portal shows), add integer-parse validation in `ValidateChangedSetting(...)` (runs when an admin edits the value). Full code for these overrides is in section 4.2; reading the settings is in 2.6.

### 2.3 Data objects, request, response

Every class and member crossing the wire needs `[DataContract]` / `[DataMember]` from `System.Runtime.Serialization`. Data project classes use `[DataContract(IsReference = false)]` and `[DataMember(EmitDefaultValue = false)]` on each property: `MemberApplicationDetail` (`string ApplicationNumber`, `string MemberIdentifier`, `int TransUnionScore`, `int EquifaxScore`, `int AnnualIncome`) and `LoanDecisionDetail` (`Guid DecisionId`, `bool Approved`, `MemberApplicationDetail ApplicationDetail`, `int CustomCompositeScore`, `int LoanAmount`).

Contracts project, `Requests` / `Responses` folders. Requests derive from `Alkami.Contracts.BaseRequest`; responses from `Alkami.Contracts.BaseResponse<T>` (provides `ItemList`):

```csharp
[DataContract(IsReference = true)]
public class GetLoanDecisionRequest : BaseRequest
{
    [DataMember(EmitDefaultValue = true)] public MemberApplicationDetail ApplicationDetail { get; set; }
}

[DataContract(IsReference = true)]
public class LoanDecisionResponse : BaseResponse<LoanDecisionDetail> { }
```

### 2.4 Entity validators

Validations project. Derive from `Alkami.Data.Validations.EntityValidatorImpl<T>` and override `ValidateInternal`. `ValidationResult` fields: `ErrorCode` (`ErrorCode.ValidationError`), `Field`, `Message`, `Severity` (`Severity.Error` / `Severity.Warning`), `SubCode` (`SubCode.None`, `SubCode.MalformedRequest`). A nested object's validator is invoked via the `Validate(out results)` extension:

```csharp
public class GetLoanDecisionRequestValidator : EntityValidatorImpl<GetLoanDecisionRequest>
{
    protected override List<ValidationResult> ValidateInternal(GetLoanDecisionRequest src)
    {
        var results = new List<ValidationResult>();
        if (src.ApplicationDetail == null)
        {
            results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Field = "GetLoanDecisionRequest",
                Message = "Member Application Details cannot be null.", Severity = Severity.Error, SubCode = SubCode.MalformedRequest });
            return results;
        }
        if (!src.ApplicationDetail.Validate(out results))   // runs MemberApplicationDetailValidator
            results.Add(new ValidationResult { ErrorCode = ErrorCode.ValidationError, Field = "GetLoanDecisionRequest",
                Message = "The request contained incomplete Member Application Details.", Severity = Severity.Error, SubCode = SubCode.MalformedRequest });
        return results;
    }
}
```

`MemberApplicationDetailValidator : EntityValidatorImpl<MemberApplicationDetail>` has the same shape: null source is an Error; `AnnualIncome <= 0` and `MemberIdentifier == null` are `Severity.Error`; `EquifaxScore <= 0` and `TransUnionScore <= 0` are `Severity.Warning`; all `SubCode.None`.

### 2.5 Contract, client, host

```csharp
using System.ServiceModel;
[ServiceContract]
public interface IMyMoneyServiceContract
{
    [OperationContract] Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request);
    [OperationContract] Task<LoanDecisionResponse> GetLoanDecisionAsync(GetLoanDecisionRequest request);
}
```

Client (`Service.Client`). `ProviderType` must match the provider type in the service headers and the SQL script:

```csharp
using Alkami.MicroServices.Settings.ProviderBasedClient;
public class MyMoneyServiceClient : ProviderBasedClient<IMyMoneyServiceContract>, IMyMoneyServiceContract
{
    private const string ProviderType = "USBFI";
    public MyMoneyServiceClient() : base(ProviderType) { }
    public MyMoneyServiceClient(long providerId) : base(ProviderType, providerId) { }
    public Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request)
        => ProxyCall((operation, inner) => operation.GetSettingsAsync(inner), request);
    public Task<LoanDecisionResponse> GetLoanDecisionAsync(GetLoanDecisionRequest request)
        => ProxyCall((operation, inner) => operation.GetLoanDecisionAsync(inner), request);
}
```

Host (`Service.Host`). Validators are registered in `OnStart()`; the `Alkami.Broker` lines let the running service receive setting changes from the Admin Portal:

```csharp
using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.ProviderBasedService;
public class DistributedService : ProviderBasedService<IMyMoneyServiceContract, ServiceImp>, IMyMoneyServiceContract
{
    private IMyMoneyServiceContract _serviceContract;
    private readonly string _providerName, _providerType;
    public DistributedService(string friendlyName, string providerName, string providerType)
        : base(friendlyName, providerName, providerType)
    { _providerName = providerName; _providerType = providerType; }

    public void OnStart()
    {
        _serviceContract = new ServiceImp(_providerType, _providerName);
        EntityValidator.AddValidator(new GetLoanDecisionRequestValidator());
        EntityValidator.AddValidator(new MemberApplicationDetailValidator());
        Alkami.Broker.ZeroMq.Setup.PublishUsingZeroMqLocally();
        Alkami.Broker.ZeroMq.Setup.SubscribeUsingZeroMqLocally(_serviceCancellationToken.Token);
        Alkami.Broker.App.Subscription.InitializeSubscriber();
        base.Start();
    }
    public void OnStop(TimeSpan fromSeconds) { base.Stop(fromSeconds); }
    public Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request) => _serviceContract.GetSettingsAsync(request);
    public Task<LoanDecisionResponse> GetLoanDecisionAsync(GetLoanDecisionRequest request) => _serviceContract.GetLoanDecisionAsync(request);
}
```

### 2.6 Implementation

Put feature logic in its own partial class file, `ServiceImp.LoanDecision.cs` (Service project):

```csharp
public partial class ServiceImp : IMyMoneyServiceContract
{
    private int _maxLoanAmount, _minLoanAmount;
    public async Task<LoanDecisionResponse> GetLoanDecisionAsync(GetLoanDecisionRequest request)
    {
        var response = new LoanDecisionResponse();
        using (var scope = await GetScopeAsync(request))
        {
            int.TryParse(scope.GetSettingOrDefault<string>(SettingNames.MaxLoanAmount), out _maxLoanAmount);
            int.TryParse(scope.GetSettingOrDefault<string>(SettingNames.MinLoanAmount), out _minLoanAmount);
        }
        response.ItemList = new List<LoanDecisionDetail> { DetermineLoanDecisionDetail(request.ApplicationDetail) };
        return await Task.FromResult(response);
    }
    // DetermineLoanDecisionDetail: private business logic (average of the two scores decides the loan amount
    // using _minLoanAmount / _maxLoanAmount; Approved = LoanAmount > 0). Its magic numbers are candidates for more settings.
}
```

Test in the Micro Service Tester: select `GetLoanDecision`, expand `ApplicationDetail`, fill test data, OK, Send Message.

Sources: My First Microservice (https://confluence.alkami.com/spaces/SDKC/pages/67617391)

## 3. Testing tools: Micro Service Tester and Sidekick

Micro Service Tester (https://feeds.alkamitech.com/feeds/choco.dev/Alkami.MicroServiceTester/): `choco install Alkami.MicroServiceTester -y`, then Start Menu > MicroServiceTester. Left margin lists registered services; double-click for contract methods; populate the request; Send Message.

Sidekick (2024, https://feeds.alkamitech.com/feeds/choco.dev/Alkami.Sidekick): `choco install Alkami.Sidekick -y`, launch from Start Menu (asks for admin elevation). The **WCF Microservice Request Tool** (Ctrl+R) constructs, replays, edits, and saves requests against the target environment; saved requests are shareable and can go in source control.

- Target Name: registered microservices on the target environment, with version. Request Name: magnifying glass lists contract signatures. Body Type: request class or subclass (e.g. Risk Management).
- Scaffolding generates a JSON request from the running contract (inserts get a sample item; fetches get filter, mapping, sorter properties). "Show Parsed" validates JSON syntax only; serialization failures show in the status bar.
- Polymorphic bodies: add a `$type` member with the fully qualified type name, e.g. `Alkami.MicroServices.Contacts.Data.UnParsedAddress` and `Alkami.MicroServices.Contacts.Data.Phone` in one Contacts collection.
- Auth tab: tenant and user (Service Admin, FI Admin with optional masquerading); claims preview. Response tab: raw or parsed JSON; double-click a node to copy.
- Assertions: default `HasError = False`; JPath expressions (https://goessner.net/articles/JsonPath/); the request succeeds when all pass.

Sources: My First Microservice (https://confluence.alkami.com/spaces/SDKC/pages/67617391); Using Sidekick to Test Microservices (https://confluence.alkami.com/spaces/SDKC/pages/378932012)

## 4. Refactoring a logical service to a provider-based service

Page updated 2020-04-20. SDK developers before Fall 2019 typically built logical services (`DistributedServiceBase`) with a settings helper whose settings were not editable in the Admin Portal. The guide converts the Alkami Samples v2.1.1 `Sample.MS.ExchangeRates` service (calls https://api.gdax.com/products) to a provider. This is the authoritative code for the settings plumbing the Provider template generates.

### 4.1 Setting names (Data project)

Folder `Settings`, file `SettingNames.cs`, attributed like everything in Data:

```csharp
[DataContract(IsReference = true)]
public class SettingNames
{
    [DataMember(EmitDefaultValue = false)] public const string ExchangeBaseUrl = "ExchangeBaseUrl";
    [DataMember(EmitDefaultValue = false)] public const string ApiResource = "ApiResource";
}
```

### 4.2 Service project: inherit from Plugin

NuGet (Alkami NuGet.Dev feed): `Alkami.ConnectorsAndProcessors`, `Alkami.MicroServices.Settings.ProviderBasedClient`, `Alkami.Utilities`, `Alkami.Utilities.Configuration`; NuGet.ThirdParty: `RestSharp` (example only). Make `ServiceImp` partial, deriving from `Alkami.TrackableObjects.Plugins.Plugin`, split into three files.

`ServiceImp.Headers.cs`:

```csharp
using Alkami.TrackableObjects.Plugins;
public partial class ServiceImp : Plugin
{
    private string _providerName = "";
    public ServiceImp(string providerType) : base(providerType) { _providerName = GetType().AssemblyQualifiedName; }
    public ServiceImp(string providerType, string providerName) : base(providerType, providerName)
    { _providerName = string.IsNullOrWhiteSpace(providerName) ? GetType().AssemblyQualifiedName : providerName; }

    public override string ItemType => "Connector";     // do not change on configurable microservices
    public override string Name { get => _providerName; }
    public const string StaticName = "Alkami Sample Exchange Rates Service";   // friendly name
    public const string StaticProviderType = "SDKSample";                      // Alkami SDK samples use "SDKSample"
    public const string StaticProviderName = "Sample.MS.ExchangeRates";        // unique; usually the base namespace
}
```

`ServiceImp.SettingManagement.cs`:

```csharp
using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.ProviderBased.Contracts;
public partial class ServiceImp : Plugin
{
    public override Dictionary<string, string> DefaultSettings()
    {
        var ret = base.DefaultSettings() ?? new Dictionary<string, string>();
        if (!ret.ContainsKey(SettingNames.ExchangeBaseUrl)) ret.Add(SettingNames.ExchangeBaseUrl, "https://api.gdax.com");
        if (!ret.ContainsKey(SettingNames.ApiResource)) ret.Add(SettingNames.ApiResource, "/products");
        return ret;
    }
    public override List<SettingDescriptor> SettingDescriptors()
    {
        var defaultSettings = new List<SettingDescriptor>();
        defaultSettings.AddRange(base.SettingDescriptors());
        defaultSettings.Add(new SettingDescriptor(SettingNames.ExchangeBaseUrl, "A base URL for the Exchange Rates service.", typeof(string), true, "Validated against a list of known exchanges", false));
        defaultSettings.Add(new SettingDescriptor(SettingNames.ApiResource, "An API resource used in conjunction with the exchange base url", typeof(string), true, "Anything you'd like to send", false));
        return defaultSettings;
    }
    protected override void ValidateChangedSetting(SettingDescriptor settingDescriptor, string settingValue,
        List<ValidationResult> errors, bool isValidated, ref bool performedValidation)
    {
        switch (settingDescriptor.Name)
        {
            case SettingNames.ExchangeBaseUrl:
                if (string.IsNullOrWhiteSpace(settingValue))
                { errors.AddValidationError(SettingNames.ExchangeBaseUrl, "Value is required", SubCode.ValueUnsupported); isValidated = false; }
                if (!Uri.TryCreate(settingValue, UriKind.Absolute, out Uri testUri) || testUri.Host.IndexOf("api.gdax") < 0)
                { errors.AddValidationError(SettingNames.ExchangeBaseUrl, "URL can only be within the api.gdax domain", SubCode.ValueOutOfRange); isValidated = false; }
                performedValidation = true;
                break;
            case SettingNames.ApiResource:
                performedValidation = true;   // accept anything
                break;
            default:
                Logger.Info($"{settingDescriptor.Name} value was not validated! Has it been defined within the DefaultSettings class and has it been added to the SettingsDescriptors collections?");
                performedValidation = false;
                break;
        }
    }
}
```

(The source splits the URL check into separate parse-failure and wrong-host branches; the condensed form is equivalent.) `ServiceImp.cs` is the only partial that implements `IExchangeRatesServiceContract`. Its `GetRatesAsync` reads settings inside `using (var scope = await GetScopeAsync(request)) { baseUrl = scope.GetSettingOrDefault<string>(SettingNames.ExchangeBaseUrl); ... }`, calls the URL with RestSharp, records `Alkami.Monitoring.Metric.RecordMetric("Custom/SampleMSExchangeRates/DataRetrievalTime", stopwatch.ElapsedMilliseconds)`, and returns `await Task.FromResult(response)`. Logger is `Common.Logging` (`LogManager.GetLogger<ServiceImp>()`).

### 4.3 Client and host projects

Client: add `Alkami.MicroServices.Settings.ProviderBasedClient`. Change the base class from `SelfResolvingClient` to `ProviderBasedClient<IExchangeRatesServiceContract>`, add `private const string ProviderType = "SDKSample";` and the constructors `base(ProviderType)` and `base(ProviderType, providerId)`. Existing `ProxyCall` stubs are unchanged. **The client's ProviderType must equal `StaticProviderType` in `ServiceImp.Headers.cs`.**

Host: add `Alkami.ConnectorsAndProcessors`, `Alkami.MicroServices.Settings.ProviderBasedClient`, `Alkami.Utilities`, `Alkami.Utilities.Configuration`. Change `DistributedService` to `ProviderBasedService<IExchangeRatesServiceContract, ServiceImp>` with the `(friendlyName, providerName, providerType)` constructor and the `Alkami.Broker` lines in `OnStart()` exactly as in section 2.5. In `Program.cs`:

```csharp
settings.ConstructUsing(() => new DistributedService(ServiceImp.StaticName, ServiceImp.StaticProviderName, ServiceImp.StaticProviderType));
```

### 4.4 SQL configuration

The provider rows must exist before the service runs. Edit only the block between `START EDIT` and `STOP EDIT` in `exchangerates.insertsettings.dev.sql`; the values **must match** `ServiceImp.Headers.cs`:

```sql
declare @providerTypeName nvarchar(30) = 'SDKSample'
declare @providerTypeDisplayName nvarchar(max) = 'SDKSample'
declare @providerTypeDescription nvarchar(max) = 'Alkami SDK Samples'
declare @providerName nvarchar(50) = 'Sample.MS.ExchangeRates'
declare @providerDisplayName nvarchar(100) = 'Alkami SDK MS Sample - Exchange Rates'
declare @providerAssemblyInfo nvarchar(max) = ''
DECLARE @ticket nvarchar(20) = 'SDKSample'   /* ticket number for audit */
```

The rest of the script is the same as the GenericProxy script in section 7.1.

Sources: Refactoring To A Provider Based Service (https://confluence.alkami.com/spaces/SDKC/pages/64946486)

## 5. Packaging a microservice installer

With `Alkami.SDK.Templates` **1.11.0 or greater** the installer files are already in the Host project's `Installer` folder (older projects had to install an external installer package into the project). Alkami uses Bamboo internally; client packages use a custom `.nuspec` for company metadata and file entries, set up once and preserved across installer upgrades. Applies to all flavors: logical, persisted database, persisted master database.

1. In the Host project root, edit the `.nuspec` in `Installer`: un-comment the line containing `<file target="src"` (around line 18) and save.
2. Rebuild in **Release** mode. The Release build packages the service with Chocolatey and writes the `.nupkg` into the `Installer` directory.
3. The Host `.nuspec` must also copy the manifest: `<file src="../AlkamiManifest.xml" target="/" />` (section 9).

Sources: Package A Microservice Installer (https://confluence.alkami.com/spaces/SDKC/pages/48811361)

## 6. Installing a microservice locally

Normally F5 is enough. Install as a Windows service to test the final choco package, avoid a second VS instance while working on a widget, hand the service to another developer, or install on an internal QA server. Confirm it runs under F5 first, then build the package (section 5).

- From your dev feed: upload the package (Add a package to your development feed, https://confluence.alkami.com/spaces/SDKC/pages/51351561), open the package page on the feeds server, copy the install command with all parameters, run it on the target machine.
- From a local directory (PowerShell as admin), e.g. `c:\LocalPackageFeed`:

```
choco install USBFI.MS.Service.SomethingSpecial.Host --version 1.0.0 --source c:/LocalPackageFeed/USBFI.MS.Service.SomethingSpecial.Host.1.0.0.nupkg
```

Warning: once installed, the service is registered and auto-starts; F5 then fails because a service of the same name is running. Stop and disable it in Windows Services, or uninstall:

```
choco uninstall USBFI.MS.Service.SomethingSpecial.Host
choco list -lo     # list locally installed packages
```

Sources: Installing Your Microservice Locally (https://confluence.alkami.com/spaces/SDKC/pages/92224158)

## 7. GenericProxy microservice

Alkami-built and supported service for calling an HTTP web service or page from a widget without writing a microservice. Two pages (how-to 2022-11-16; tutorial 2025-09-16) agree.

### 7.1 Install

Run `Insert_Provider_Setting.sql` against `DeveloperDynamic`, then install the host:

```sql
WHILE @@TRANCOUNT > 0 ROLLBACK
SET XACT_ABORT ON
SET NOCOUNT ON
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
USE DeveloperDynamic
BEGIN TRAN
declare @providerId table (id bigint); declare @itemIdtable table (id bigint);
/**** START EDIT ****/
declare @providerTypeName nvarchar(30) = 'GenericProxy'
declare @providerTypeDisplayName nvarchar(max) = 'Generic Proxy'
declare @providerTypeDescription nvarchar(max) = 'Generic Proxy'
declare @providerName nvarchar(50) = 'Alkami.MS.GenericProxy'
declare @providerDescription nvarchar(100) = 'Alkami MS GenericProxy'
declare @providerAssemblyInfo nvarchar(max) = ''
DECLARE @ticket nvarchar(20) = 'SDKCustom'
/**** STOP EDIT ****/
if not exists (select * from core.ProviderType where name = @providerTypeName)
  insert into core.ProviderType (Name, DisplayName, Description, CreateDate, ViewPage)
  select @providerTypeName, @providerTypeDisplayName, @providerTypeDescription, getutcdate(), null;
if not exists (select * from core.Provider p join core.providertype pt on pt.id = p.ProviderTypeID where pt.name = @providerTypeName and p.Name = @providerName)
  insert into core.Provider (ProviderTypeID, Name, Description, AssemblyInfo, CreateDate) output inserted.id into @providerId
  select pt.id, @providerName, @providerDescription, @providerAssemblyInfo, GETUTCDATE() from core.ProviderType pt where name = @providerTypeName;
else
  insert into @providerId (id) select p.id from core.provider p join core.providertype pt on pt.id = p.ProviderTypeID where pt.name = @providerTypeName and p.Name = @providerName;
if not exists (select * from core.item i join core.bank b on b.id = i.SecondaryId and i.ItemType = 'Connector' join @providerId t on t.id = i.ParentId)
  insert into core.item (ItemType, ParentId, SecondaryId, Name, CreatedUtc, Version, Deleted) output inserted.id into @itemIdtable
  select 'Connector', t.id, (select id from core.bank), @providerName, GETUTCDATE(), '1.0.0.0', 0 from @providerId t;
COMMIT TRAN
select * from core.Item where [Name] = 'Alkami.MS.GenericProxy';
SET TRANSACTION ISOLATION LEVEL READ COMMITTED
WHILE @@TRANCOUNT > 0 ROLLBACK
```

(The source wraps each branch in `begin ... end` and also fills `@itemIdtable` when the item already exists; otherwise verbatim.)

```
choco install Alkami.MS.GenericProxy.Service.Host --source https://feeds.alkamitech.com/nuget/choco.dev/ -y
```

### 7.2 Whitelist hosts

The provider keeps a host/domain whitelist (the 2022 page says www.google.com is on it by default; the 2025 page says add it if you get an error). Admin site (`admin-developer.dev.alkamitech.com`) > Setup > Integration Settings > Providers > Provider Types: GenericProxy > Edit next to `Alkami.MS.GenericProxy` > add URLs > Save Changes, then run `iisreset`. **In production, every URL you need to reach must be given to Networking for whitelisting.**

### 7.3 Widget integration

Add NuGet `Alkami.MS.GenericProxy.Client` and `Alkami.MS.GenericProxy.Data` (Client pulls in Data). Update other packages (except any not permitted). Add to the widget `.nuspec`:

```xml
<file src="bin\Alkami.MS.GenericProxy.*" target="lib" exclude="**\*.config"/>
```

Build, then add controller code:

```csharp
using Alkami.MS.GenericProxy.Contracts;
using Alkami.MS.GenericProxy.Contracts.Requests;
using Alkami.MS.GenericProxy.Contracts.Responses;
using Alkami.MS.GenericProxy.Service.Client;
using WebToolkit;

public static Func<IGenericProxyServiceContract> genericProxyServiceFactory = () => new GenericProxyServiceClient();

// GET (inside the Index action try block)
ProxyRequest req = new ProxyRequest();
this.AugmentRequest(req);
req.HttpMethod = "GET";
req.RequestUri = new Uri("https://www.google.com");
ProxyResponse resp = AsyncHelper.RunSync(() => genericProxyServiceFactory().ExecuteRequestAsync(req));
myModel.DisplayMethod = resp.Body;

// POST (PostData action); requestModel.Url example: http://schematic-ipsum.herokuapp.com/?n=1
req.HttpMethod = "POST";
req.RequestUri = new Uri(requestModel.Url);
req.ContentHeaders = new Dictionary<string, string>() { { "Content-Type", "application/json" } };
req.Body = requestModel.Body;      // JSON string
req.MaxResults = 10;
ProxyResponse resp = AsyncHelper.RunSync(() => genericProxyServiceFactory().ExecuteRequestAsync(req));
if (resp.Body != null) responseModel = JsonConvert.DeserializeObject<PostDataResponseModel>(resp.Body);
```

`ProxyRequest` members: `HttpMethod`, `RequestUri`, `ContentHeaders` (Dictionary<string,string>), `Body`, `MaxResults`; `ProxyResponse.Body` is the content. Supported content types (validated against an enumeration): `text/plain`, `application/x-www-form-urlencoded`, `application/json`, `application/xml`, `application/pdf`, `application/zip`, `application/vnd.ms-excel`, `image/*`.

The tutorial's POST example uses three models deriving from `Alkami.Client.Framework.Mvc.BaseModel` (`PostDataRequestModel { Url, Body }`, `PostDataResponseModel { Id, Name, Email, Bio, Age, Avatar }`, `PostDataRequestResponseModel { Request, Response }`, because a Razor view takes one model), a `PostData.cshtml` view, and `WidgetRoute` entries in `WidgetDescription.cs` `NavigationRoutes` (`Identifier = "Index", Area = "TrainingProxTest", Controller = "TrainingProxTest", Action = "Index", Title = "Get Data"` and `Identifier = "PostData", Controller = "TrainingProxTest", Action = "PostData", Title = "Post Data"`; replace `TrainingProxTest` with the folder containing your views). Views use `@using Alkami.Client.WebClient.Shared.Helpers`, the `TitleContentPlaceholder` / `StyleSheetContentPlaceholder` / `JavaScriptIncludeContentPlaceholder` sections, and Iris markup (`iris-card`, `iris-card__header`, `iris-card__content`).

### 7.4 Building widgets with Generic Proxy (2025 guidance)

Two patterns: standalone API-driven widgets (server-side calls through the proxy, data rendered in the widget) and embedded iFrame widgets (proxy fetches a one-time tokenized URL used as `<iframe src>`, optional keep-alive). Rules: route all API requests through the generic proxy from server-side code, never from the browser; put auth headers (API key/secret) on the request and a member ID or equivalent in the body; **store keys/secrets in widget settings, not in provider configuration**; use HTTPS; prefer POST so parameters are not leaked in URLs; use one-time tokens exchanged for JWTs; log at trace level without PII.

Sources: Using the Alkami GenericProxy Microservice (https://confluence.alkami.com/spaces/SDKC/pages/92223681); Work with the Generic Proxy Microservice (https://confluence.alkami.com/spaces/SDKC/pages/94536857)

## 8. Integrating a widget with any microservice

Prerequisites: widget installer v2.0.1 or newer, feed access to https://feeds.alkamitech.com, ORB environment 2018.05 or later.

1. Add the service's Client and Contract (and Data) NuGet packages to the widget.
2. The widget must carry the service assemblies into its `/bin` so Chocolatey packages them (NuGet defaults `CopyLocal` to true; confirm it).
3. Add a `<file>` line per service assembly prefix to the widget `.nuspec`, next to the existing `bin\USBFI.*` line (the example adds the Transactions service):

```xml
<file src="AlkamiManifest.xml" target="AlkamiManifest.xml" />
<file src="bin\USBFI.*" target="lib" exclude="**\*.config"/>
<file src="bin\Alkami.MicroServices.Transactions.*" target="lib" exclude="**\*.config"/>
<file src="**\Scripts\" target="content\Areas\App" />
<file src="**\Styles\" target="content\Areas\App" exclude="**\*.scss" />
<file src="**\Views\" target="content\Areas\App" />
<file src="**\Images\" target="content\Areas\App" />
<file src="**\_SiteText\" target="content\Areas\App" exclude="**\*.xx.xml"/>
```

(The full block also has `tools\chocolateyInstall.ps1`, `tools\chocolateyUninstall.ps1`, and a `**\*.*` to `src` entry excluding `obj`, `.vs`, `bin`, `packages`, `.nuget`, `.git`, `node_modules`, `.suo`, `.user`, test folders, and `tools\**\chocolatey*.ps1`.)

4. Call the service through a static delegate factory and always `AugmentRequest` first:

```csharp
using Webtoolkit;
using USBFI.MS.MyService.Data.Contracts;
using USBFI.MS.MyService.Service.Client;

public static Func<IServiceContract> serviceFactory = () => new ServiceClient();

public async Task<ActionResult> Index()
{
    try
    {
        Logger.DebugFormat("[GET] Controller/Index");
        var request = new GetSomethingRequest();
        this.AugmentRequest(request);
        var results = await serviceFactory().GetSomethingAsync(request);
        return View("Index", new WidgetModel { Data = results.Data });
    }
    catch (Exception e) { Logger.Error("Error [GET] Controller/Index", e); return View("Error"); }
}
```

Sources: Integrate a widget with a microservice (https://confluence.alkami.com/spaces/SDKC/pages/52595468)

## 9. AlkamiManifest.xml

`AlkamiManifest.xml` sits at the project root and describes package metadata.

- `<version>` is the manifest spec version, always `1.0`; not your package version.
- A copy must be at the **root of the chocolatey package**. Microservices: in the `...Host.nuspec` in the `InstallerOverrides` folder add `<file src="../AlkamiManifest.xml" target="/" />`. Widgets: `<file src="AlkamiManifest.xml" target="AlkamiManifest.xml" />`.
- An empty `<dependencies></dependencies>` throws a null reference exception; delete it if there are none. A `<migrations>` element must contain a real migration (not empty or comment-only); usually delete it.
- `bankIdentifier` is the bare GUID: never wrapped in `{}`, `[]`, `''`, or `""`. Partners/vendors remove `<bankIdentifiers>` entirely.

`<general>` fields: `creatorCode` [Required] (usually an Alkami Jira project key; must not be "Alkami"); `element` [Required] (package id/name); `componentType` [Required] (`Service`, `Widget`, `Provider`); `dependencies` [Optional, recommended for microservices] (nuget dependency spec and version semantics; used for contract validation in an environment, not at install); `bankIdentifiers` [Required for SDK clients, not for partner submissions; values from SDK Support]; `releaseManagement/alwaysDeploy` [Optional, default `false`; `true` only for a componentized provider that must deploy regardless of existing configuration, after consulting your team lead].

Create from the project root (folder with the csproj) in PowerShell, add the existing file to the solution, then hand-edit:

```
New-AlkamiManifest -Type Service
New-AlkamiManifest -Type Widget
New-AlkamiManifest -Type Provider
```

Microservice manifest. The `provider` values must match the arguments passed to the `ProviderBasedService` constructor in `DistributedService.cs` (e.g. type `SkipPayment`, name `Corelation SkipPayment Provider`):

```xml
<?xml version="1.0"?>
<packageManifest>
  <version>1.0</version>
  <general>
    <creatorCode>Your_Bank_Identifier</creatorCode>
    <element>Your_Full_ServiceName</element>
    <componentType>Service</componentType>
    <bankIdentifiers>
      <bankIdentifier name="FI Friendly Name">bank_guid</bankIdentifier>
    </bankIdentifiers>
    <releaseManagement><alwaysDeploy>false</alwaysDeploy></releaseManagement>
  </general>
  <serviceManifest>
    <runtime>framework</runtime>
    <entryPoint>Your_Full_ServiceName.Host.exe</entryPoint>
    <provider>
      <providerType>Billpay</providerType>
      <providerName>Bill Pay</providerName>
    </provider>
  </serviceManifest>
</packageManifest>
```

Widget manifest (`widgetManifest`): `widgetName` (`Your.Full.WidgetName`), `widgetDescription`, `widgetInstall` (`Client`), `areaName` (`BankIdentifier_WidgetName`), `assemblyInfo` (assembly name, as in `core.Widget`), `displaySettings` (`Desktop`), `iconName`. Provider manifest (`providerManifest`): `appInstall` (`BankService`), `assemblyInfo` as `{Full path to provider class including namespace}, {Assembly name}` (as in `core.Provider`), `providerType` (e.g. `Core`), `providerName` (e.g. `OSICoreProvider`), `pluginType` (`Connector`).

Validate (PowerShell as Administrator); the last output line must be `True` (warnings may precede it):

```
Test-AlkamiManifest -f "<Full path to your AlkamiManifest.xml>"
```

Sources: Adding or Updating an AlkamiManifest (https://confluence.alkami.com/spaces/SDKC/pages/235930406)

## 10. Package validation checks

| Check | Message | Fix |
|---|---|---|
| MissingAssembliesCheck | "The following assemblies are not included in the package..." | All referenced assemblies must be in the final nupkg at the required version; check the reference and the `.nuspec`; inspect the package. |
| ApplicationIs32BitCheck | "...contains a 32-bit application..." | Set `Prefer32Bit` to `false` in the `.csproj` for Debug and Release. |
| AppConfigFilesCheck | "Invalid Configuration detected..." | In app.config/web.config: `NewRelic.AppName` must not be `REPLACEME`; `NewRelic.AgentEnabled` must not be `false` or unset. |
| ForbiddenAssembliesCheck | "Package contains forbidden assemblies..." | Assemblies no longer acceptable in production; upgrade dependency versions. |
| HostVersionCheck | (also prints "Package contains forbidden assemblies...") | The Subscription service binds contracts partly by Assembly Version. The assembly implementing `DistributedServiceBase<T>` or `ProviderBasedService<T, TPlugin>` must share major.minor Assembly Version with the contract assembly `T`. Align `AssemblyInfo.cs` in `*.Service.Host` with `*.Contracts`. Assembly Version is independent of package version (semver). |
| NuspecDependenciesCheck | "Nuspec contains invalid dependencies..." | Remove all `<dependencies>` from the `.nuspec`. |
| SharedDllsCheck | "Package contains invalid assemblies ..." | Web extensions, providers, and widgets must not ship ORB shared DLLs (e.g. `Alkami.Common`). Drop the unused dependency or the `.nuspec` line that copies it. |
| InvalidManifestCheck | "Package contains an invalid Alkami Manifest ..." | `AlkamiManifest.xml` at the nupkg root, valid, all elements filled (section 9). |
| LoggingConfigCheck | "log4net.config should not leverage the following log levels ..." | Change the log4net level to an allowed value. |

Sources: Resolving Package Validations (https://confluence.alkami.com/spaces/SDKC/pages/303534036)

## 11. Upgrading to v4 of Common.Corelation

Only for Corelation-core developers with a custom microservice calling the core via KeyBridge using NuGet `Alkami.Microservices.Core.Common.Corelation`. Moving from v2/v3 to v4 breaks compile and runtime because v4 caches Corelation session details and needs a `BaseRequest` for the cache key. With the Builders and BusinessLogic approach:

```csharp
// before
public class GetCPAccountAction : Alkami.MicroServices.Core.Common.Corelation.BaseAction
public GetCPAccountAction(ProviderSettings parameters, GetCustomerIdRequest request) : base(parameters)
// after: BaseAction<BaseRequest> or BaseAction<GetCustomerIdRequest>, and pass the request to base
public class GetCPAccountAction : Alkami.MicroServices.Core.Common.Corelation.BaseAction<GetCustomerIdRequest>
public GetCPAccountAction(ProviderSettings parameters, GetCustomerIdRequest request) : base(parameters, request)
```

A runtime error inside `Alkami.Microservices.Cache.AsyncCache` remains until the cache is initialized: add the Redis cache package to the Host project (named only in a screenshot on the page) and in `Program.cs`, before the `OnStart()` call:

```csharp
Alkami.Microservices.Cache.Redis.Setup.UseRedisForCaching();
```

Sources: Upgrading to v4 of Common.Corelation (https://confluence.alkami.com/spaces/SDKC/pages/250881776)

## 12. Work with Email

Install Papercut SMTP (https://github.com/ChangemakerStudios/Papercut-SMTP/releases) and confirm the service is running.

Switch the Notification provider from No Op to SMTP with the page's SQL against `DeveloperDynamic` (change `Use` for a stage-matched database). It updates `core.Provider` (ProviderType `Notification`, Name `SMTP`, Description like `%No Op%`) to `Description = 'SMTP Provider'`, `AssemblyInfo = 'Alkami.App.Notification.Provider.Smtp.Provider, Alkami.App.Notification.Provider.Smtp'` (NoOp value: `Alkami.App.Notification.Provider.NoOp.Provider, Alkami.App.Notification.Provider.NoOp`); renames the provider's `core.Item` from `Email` to `SMTP` if needed; upserts `core.ItemSetting` rows `SMTP Server` = `127.0.0.1` and `SMTP Port` = `25`; sets `Use SSL` = `false` only if the row exists (default false). Insert shape:

```sql
INSERT INTO core.ItemSetting (ItemId, Name, Value, [Version], CreatedUtc)
SELECT Id, @SMTPServerSettingName, @SMTPServer, '1.0.0.0', GETUTCDATE()
FROM core.Item WHERE Name = @ItemName AND ParentId = @ProviderId;
```

The script ends with `ROLLBACK` and a commented `--COMMIT`; swap them to apply.

Send from a widget (from the `SendEmail` sample in `Alkami.Sdk.Samples`). The API method is spelled `SendToSpecificAddess`:

```csharp
using Alkami.Client.Services.Notification.Repository;
using Alkami.App.Notification.Contracts;
using Alkami.Security.Common.DataContracts;
using Alkami.Client.Messages;

UserContactEmail emailContact =
    CurrentUser.GetUserContact<UserContactEmail>(x => x.IsPrimary).FirstOrDefault()
    ?? CurrentUser.GetUserContact<UserContactEmail>().FirstOrDefault();

var result = NotificationRepository.SendToSpecificAddess(
    this.CurrentBankIdentifier, CurrentBankName, CurrentUserIdentifier,
    TransportAgentMedium.Email, emailContact.Email.ToString(), subject, body, true, true);
if (result == null) throw new InvalidOperationException("SendToUser response returns null.");
if (result.Status != Status.Success)
    throw new InvalidOperationException($"SendEmail failed. Status [{result.Status}] Error [{result.Exception?.Message}]");
```

The sample controller `[HttpPost] Index(TrainingSendEmailModel model)` sets `ViewData["IsErrorState"]` and `ViewData["Message"]`; the model `TrainingSendEmailModel : Alkami.Client.Framework.Mvc.BaseModel` has `EmailSubject` and `EmailBody`; the view uses `@using (Html.BeginForm("Index", "TrainingSendEmail", FormMethod.Post))` with Iris `iris-notification--postive` / `--negative` banners and `iris-form-group` fields. Deploy, submit the form, confirm the message in Papercut.

Sources: Work with Email (https://confluence.alkami.com/spaces/SDKC/pages/101806202)

## 13. Work with Alerts Part 1: Send Email Alerts

Prerequisites: complete Work with Email, and install the Alerts microservice (https://feeds.alkamitech.com/feeds/choco.dev/Alkami.MicroServices.Alerts.Service.Host):

```
choco install Alkami.MicroServices.Alerts.Service.Host -y
```

Every alert starts from a template in `notification.DefaultTemplates` (Alkami maintains many; custom ones are per FI). Templates are wrapped in the predefined `GLOBAL_TEMPLATE` (header, footer, styling); a custom template supplies only the `<body/>` content as raw un-escaped HTML with Razor variables (`@Model.Date`).

```sql
USE [DeveloperDynamic]
GO
INSERT INTO [notification].[DefaultTemplates]
 ([BankIdentifier],[OriginatorName],[TemplateContent],[TemplateTarget],[TemplateName],[NotificationActionName]
 ,[MediumType],[TemplateTransformType],[LocaleId],[CreateDate],[Subject])
VALUES ('78554577-9DE6-43CD-9085-5868977156D1', 'USBFI',
 '<p>You are receiving this from a sample SDK project on @Model.Date</p>',
 '{"id": "System.Object","type": "object","properties": {}}',
 'USBFIGeneralEmailAlert', 'USBFIGeneralEmailAlert', 1, 2, 1033, GETDATE(), 'USBFI Hello World Alert')
GO
```

`OriginatorName` and `TemplateName` are identifiers of your choice; `NotificationActionName` is typically the same as `TemplateName`; leave `TemplateTarget` as is; `MediumType` and `TemplateTransformType` select Email/SMS etc.; `LocaleId` 1033 is English; `Subject` is the email subject.

Send an alert (add the Alerts Client and Contract NuGet packages):

```csharp
using Alkami.MicroServices.Alerts.Contracts.Requests;
using Alkami.MicroServices.Alerts.Contracts.Responses;
using Alkami.MicroServices.Alerts.Contracts;
using Alkami.MicroServices.Alerts;

private static readonly Func<IAlertsServiceContract> AlertsFactory = () => new AlertsServiceClient();

[HttpPost]
public async Task<JsonResult> SendAlert()
{
    var request = new SendAlertRequest();
    this.AugmentRequest(request);
    var notifcationData = new { AlertTitle = "Sample Alert!", Date = DateTime.UtcNow,
        AlertReason = "This is an example alert.", Signature = "Your Special Signed Signature" };
    request.Alert = new GeneralAlert(notifcationData)
    {
        ContactIds = null,
        TemplateName = "USBFIGeneralEmailAlert",
        OriginatorName = "USBFI",
        DestinationUserIdentifier = request.UserIdentifier,
        TemplateTransformType = TemplateTransformType.Razor,
        FallbackToPrimaryEmail = true,
        AlertDate = DateTime.UtcNow
    };
    var response = await AlertsFactory().SendAlertAsync(request);
    return Json(new { success = !response.HasError });
}
```

Trigger from the view with `<button class="iris-button iris-button--primary" id="send_alert">Send Alert</button>` and `$.ajax({ method: 'POST', url: 'SampleAlerts/SendAlert', data: '{}', contentType: 'application/json', ... })`. Each alert writes a row to `audit.UserNotificationAction` (`select * from [audit].[UserNotificationAction]`); with local SMTP configured the email appears in Papercut.

Sources: Work with Alerts Part 1: Send Email Alerts (https://confluence.alkami.com/spaces/SDKC/pages/115511533)
