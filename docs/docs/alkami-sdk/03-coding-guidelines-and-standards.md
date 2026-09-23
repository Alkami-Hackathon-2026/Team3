# Coding Guidelines and Standards

**What this covers.** The rules an SDK developer must follow when naming, structuring, building, logging, testing, packaging, and securing Alkami SDK widgets and microservices. It consolidates the Confluence "Code Guidelines and Standards" section (naming, 64-bit, widget and microservice layout, nuspec metadata, logging, cryptography), the unit and integration testing guide, the SDK Pattern Documentation overview (widget plus microservice pairing and SSO pattern), and the Technical Guide templates. Where a newer page conflicts with an older one, both are noted with dates.

---

## 1. Your well-known identifier (project prefix)

Every SDK project, solution, namespace, assembly, package ID, and title must be prefixed with the financial institution's **well-known identifier** (also called the unique identifier or client code).

- The identifier is the short name (ticket prefix) of your JIRA **Client Services** or **Delivery** project, depending on when you joined Alkami. Example: ticket `CSTEST-154` in the "Client Services: Test" project means the identifier is `CSTEST`, and a widget for it is named `CSTEST.Client.Widget.MemberServices`.
- Many clients have a JIRA project whose code ends in `AM` (Account Management), for example `CSTESTAM`. Drop the `AM` so it reads `CSTEST`.
- If unsure, ask sdksupport (sdksupport@alkamitech.com).
- **Do NOT leave `Alkami` as the well-known identifier.** `Alkami.Client.Widget.MyNewWidget` and `Alkami.MS.AtLogin` will fail validation and the submission will be cancelled. All `Alkami.*` examples in the guideline pages are illustration only.
- **Do NOT include the word `Mobile` in any SDK project or widget name.** It conflicts with Alkami routing and the widget will not display in mobile. Invalid: `USBFI.Client.Widget.MyMobileSpecialFeature`.
- Follow Microsoft's NuGet guidance on unique package identifiers and version numbers: https://learn.microsoft.com/en-us/nuget/create-packages/creating-a-package#choose-a-unique-package-identifier-and-setting-the-version-number

Sources: Name Your SDK Projects (https://confluence.alkami.com/spaces/SDKC/pages/55353106); Widget Coding Guidelines (https://confluence.alkami.com/spaces/SDKC/pages/53743045); Microservice Coding Guidelines (https://confluence.alkami.com/spaces/SDKC/pages/53743063)

---

## 2. 64-bit only

Alkami production environments are configured for 64-bit programs and no longer support 32-bit programs. For every C# project, in Visual Studio Project Properties:

- Set **Platform target** to `Any CPU`.
- **Clear the "Prefer 32-bit" checkbox** (csproj: `<Prefer32Bit>false</Prefer32Bit>`).
- Solutions have exactly one build target: `AnyCPU`.

Sources: 64 bit only (SDK) (https://confluence.alkami.com/spaces/SDKC/pages/51352148); Microservice Coding Guidelines (https://confluence.alkami.com/spaces/SDKC/pages/53743063)

---

## 3. Widget Coding Guidelines

Examples below use identifier `USBFI` (US Best Financial Institution) with widget `ToyFair`, URL slug `USBFIToyFair`.

### 3.1 Naming

```
<well-known identifier>.Client.Widget.<url slug>
```

Examples: `Alkami.Client.Widget.MyAccountsV2`, `Alkami.Admin.Widget.WidgetManagement`, `USBFI.Client.Widget.USBFIToyFair`.

- Widget names and derived names contain **no spaces, periods, underscores, or other separators**. Numbers are permitted after the first letter. Invalid: `2ndMyAccounts`. Valid: `MyAccounts2nd`.
- `<url slug>` is the widget name and the root name of the area folder for URL routing; it controls how the widget is distributed and installed.
- Admin widgets substitute `.Admin.` for `.Client.`. `.Apps` is not used; `.Client.Widget` is the preferred strategy.
- No `Mobile` in the name; no `Alkami` identifier (section 1).

### 3.2 Solution organization (repository root)

`<well-known identifier>.Client.Widget.<url slug>.sln` (matches the primary assembly root namespace; valid `USBFI.Client.Widget.USBFIToyFair.sln`, invalid `Alkami.Apps.SampleDemo.sln` or `USBFIToyFair.sln`; single build target `AnyCPU`), `.git`, `.gitignore`, `README.md`, `AssemblyInfo.cs` (sem.ver, used with Bamboo builds), `<well-known identifier>.Client.Widget.<url slug>.nuspec`, `tools\` (`chocolateyInstall.ps1`, `chocolateyUninstall.ps1`), the main project folder `<well-known identifier>.Client.Widget.<url slug>\`, and `<well-known identifier>.Client.Widget.<url slug>.Tests\`.

### 3.3 Project organization

- Project file `<well-known identifier>.Client.Widget.<url slug>.csproj`; target **.NET 4.8**; `RootNamespace` and `AssemblyName` both equal `<well-known identifier>.Client.Widget.<url slug>`:

```xml
<PropertyGroup>
  <RootNamespace>USBFI.Client.Widget.USBFIToyFair</RootNamespace>
  <AssemblyName>USBFI.Client.Widget.USBFIToyFair</AssemblyName>
  <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
</PropertyGroup>
```

- **Bin output must contain only the minimum**: assemblies built by your project, microservice client proxies, and custom assemblies unique to your widget (not in the ORB base). Set all other references to Private / Copy Local = false.

**Controllers**: `Base<url slug>Controller` (optional, often used; put helpers and common functions such as auditing here), `<url slug>Controller` (required; at least one controller must match the slug), `Mobile<url slug>Controller` (optional). Examples: `BaseSampleDemoController`, `SampleDemoController`, `MobileSampleDemoController`, `USBFIToyFairController`. Multiple controllers are fine but require additional configuration in `WidgetDescription.cs`.

**WidgetDescription.cs (Client widgets only)**

- Path `<well-known identifier>.Client.Widget.<url slug>\WidgetDescription.cs`; class and file are both named `WidgetDescription`. Formerly `App.cs`; rename if it is anything else.
- Inherits `Alkami.Client.Framework.Mvc.WidgetDescription` (`Alkami.Client.Framework.dll`). Defines MVC routes and basic widget info, loaded at widget registration. **Omitting it makes the widget unregisterable and errors the application.**
- Override `Name` and `Title`. `Name` **MUST** equal the `<url slug>` or the widget will not work. `Title` is usually overridden by the `DisplayName` on the `core.Widget` database entity.

```csharp
public override string Name { get { return "USBFIToyFair"; } }
public override string Title { get { return "Toy Fair"; } }
```

**Area.cs (Admin widgets only)**: `<well-known identifier>.Admin.Widget.<url slug>\Area.cs`; the Admin widget template default is sufficient for most cases.

### 3.4 Folder organization

```
<well-known identifier>.Client.Widget.<url slug>\
  _SiteText\<well-known identifier>.Client.Widget.<url slug>.sitetext.en.xml   (plus .es.xml etc.)
  Controllers\Base<url slug>Controller.cs, <url slug>Controller.cs, Mobile<url slug>Controller.cs
  Helpers\   (optional)
  Images\    referenced from views as /<url slug>/Images/<file>
  Models\    (optional)
  Properties\
  Scripts\   TypeScript preferred; referenced as /<url slug>/Scripts/<file>
  Styles\    referenced as /<url slug>/Styles/<file>
  Views\<url slug>\Index.cshtml
  Views\<url slug>\Mobile\Index.cshtml
```

- SASS or other style files must **never reference Alkami ORB files directly** without consulting the front-end architect or archdev team.
- View subfolders match controller names without `Mobile` or `Base`; mobile views go in a `Mobile` subfolder so the view engine finds them. Use `return View("<view name>")` or `return View()`; never a fully qualified path.
- `WidgetSummary.cshtml` is legacy and no longer used by ORB. A `web.config` in Views is not recommended; consult a technical lead if you think you need one.

### 3.5 Test project

- Approved strategy: **NUnit and Moq** (supported by Alkami automated builds). For complex strategies or multiple test projects, provide an `.nunit` file at solution level.
- Test harnesses inherit `Alkami.Client.Common.Test.ClientUnitTestBase`, which supplies common stubs including `HttpContext`. BankIdentifier, UserIdentifier, etc. are generated dynamically; never assume they are static. Generated GUIDs are exposed as mock properties on the base class.
- `<well-known identifier>.Client.Widget.<url slug>.Tests\<...>.Tests.csproj`; root namespace and assembly name are `<well-known identifier>.Client.Widget.<url slug>.Tests`.

### 3.6 Chocolatey tools and nuspec

The `tools` folder at the solution root is populated when you install the `Alkami.Installer.Widget` package. It holds `chocolateyInstall.ps1`, `chocolateyUninstall.ps1`, and `install_widget.sql` (contains variables you must edit).

```powershell
# chocolateyInstall.ps1
[CmdletBinding()]
Param()
process { & C:\ProgramData\Alkami\Installer\Widget\install.ps1 $PSScriptRoot; return; }

# chocolateyUninstall.ps1
[CmdletBinding()]
Param()
process { & C:\ProgramData\Alkami\Installer\Widget\uninstall.ps1 $PSScriptRoot; return; }
```

Minimum `<well-known identifier>.Client.Widget.<url slug>.nuspec`:

```xml
<?xml version="1.0"?>
<package>
  <metadata>
    <id><well-known identifier>.Client.Widget.<url slug></id>
    <version>$version$</version>
    <title><well-known identifier>.Client.Widget.<url slug></title>
    <authors>Alkami Technology, Inc</authors>
    <owners>Alkami Technology, Inc</owners>
    <projectUrl>https://confluence.alkami.com/display/ORB/<well-known identifier>.Client.Widget.<url slug></projectUrl>
    <iconUrl>https://www.alkami.com/files/alkamilogo75x75.png</iconUrl>
    <licenseUrl>http://alkami.com/files/orblicense.html</licenseUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <description>A descriptive note above the widget should be here for exploration of available widgets in the future</description>
    <releaseNotes></releaseNotes>
    <tags>Widget</tags>
    <copyright>Copyright (c) 2020 Alkami Technology, Inc</copyright>
    <dependencies>
      <dependency id="Alkami.Installer.Widget" version="3.0.3" />
    </dependencies>
  </metadata>
  <files>
    <file src="tools\chocolateyInstall.ps1" target="tools" />
    <file src="tools\chocolateyUninstall.ps1" target="tools" />
    <file src="AlkamiManifest.xml" target="AlkamiManifest.xml" />
    <file src="**\*.*" target="src" exclude="**\obj\**\*.*;**\.vs\**\*.*;**\bin\**\*.*;**\packages\**\*.*;**\.nuget\**\*.*;**\.git\**\*.*;**\.gitignore;**\node_modules\**\*.*;**\.suo;**\.user;**\Tests\**\*.*;**\Test\**\*.*;**\UnitTest\**\*.*;**\UnitTests\**\*.*;**\tools\**\chocolatey*.ps1" />
    <file src="bin\USBFI.Client.Widget.ToyFair.*" target="lib" exclude="**\*.config"/>
    <file src="**\Scripts\" target="content\Areas\App" />
    <file src="**\Styles\" target="content\Areas\App" exclude="**\*.scss" />
    <file src="**\Views\" target="content\Areas\App" />
    <file src="**\Images\" target="content\Areas\App" />
    <file src="**\_SiteText\" target="content\Areas\App" exclude="**\*.xx.xml"/>
  </files>
</package>
```

Tags are PascalCase, single-word preferred; use one shared tag (for example `Authentication`) across a family of related packages (widget, microservice, module). `projectUrl`, `copyright`, `authors`, `owners` must name the owning organization.

Sources: Widget Coding Guidelines (https://confluence.alkami.com/spaces/SDKC/pages/53743045)

---

## 4. Microservice Coding Guidelines

Updated 2025-04-22. Microservices created before **February 1, 2019** are grandfathered; new ones must comply.

### 4.1 Naming

```
<well-known identifier>.MS[.Processor].<Component>[.<Purpose>][.<vendor/client>]
```

`[ ]` marks optional segments. Note the deliberate **absence of `.Service`** and the shortening of `.MicroService` to `.MS` to keep paths short. Examples: `Alkami.MS.AtLogin`, `Alkami.MS.UserLookup`, `Alkami.MS.CardManagement.SymConnect`, `USBFI.MS.BranchManagement.NorthAmerica`, `USBFI.MS.CardManagement`.

- Names must be clear and consistent; abbreviations are not preferred; full names should be **less than 64 characters** where possible.
- `RootNamespace` and `AssemblyName` must match the csproj name. **The csproj name is also the Chocolatey package ID.**
- Do not leave `Alkami` as the identifier. When in doubt, ask the SDK or Architecture team.

**Display name** (`configurator.SetDisplayName(...)`) drops `.MS`, `.Service`, `.Host`, and the word "Microservice":

```
<well-known identifier> <Component>[ <Purpose>][ <vendor/client>]
```

| Project name | Don't do this | Do this |
| --- | --- | --- |
| Alkami.MicroServices.QuickApply.Service.Host | Alkami Quick Apply Microservice | Alkami QuickApply |
| Alkami.MicroServices.QuickApply.ESB.Desert.Service.Host | Alkami DSFCU Quick Apply Microservice | Alkami QuickApply ESB Desert |
| Alkami.MicroServices.Rules.Content.Service.Host | Alkami Rules Micro Service | Alkami Rules Content |
| Alkami.MicroServices.Authentication.AD.Service.Host | Alkami Active Directory Authentication Service | Alkami Authentication AD |
| Alkami.MicroServices.CardManagementProviders.Corelation.Host | Alkami Corelation Card Management Provider Microservice | Alkami CardManagement Corelation |

### 4.2 Shorter path names

Windows path APIs fail beyond **260 characters** and most build automation does not use long-path APIs. A layout such as `Alkami.MicroServices.CardManagementProviders/...SymConnect/...SymConnect.Service.Host/...SymConnect.Service.Host.csproj` is already 246 characters, and 290 once the Bamboo workspace `D:\Bamboo\Workspaces\CardManagementProviders` is prepended.

### 4.3 Solution and project organization

Solution: `<well-known identifier>.MS[.Processor].<Component>[.<Purpose>].sln`. Valid: `Alkami.MS.AtLogin.sln`, `USBFI.MS.BranchManagement.sln`. Invalid: `Alkami.AtLogin.Service.sln`, `BranchManagement`. Single target `AnyCPU`. Root also has `.git`, `.gitignore`, `README.md`, `SolutionInfo.cs` (sem.ver). Projects (`N` = `<well-known identifier>.MS[.Processor].<Component>[.<Purpose>]`):

| Folder | Project | Notes |
| --- | --- | --- |
| `Contracts\` | `N.Contracts.csproj` (sem.ver) | No logic; request/response contracts only. Versioned independently of the host. |
| `Client\` | `N.Client.csproj` (sem.ver) | Very lightweight; nearly no logic beyond the client structure. Versioned independently. |
| `Data\` | `N.Data.csproj` | Limited logic; data types separate from contracts. |
| `Helpers\` (opt.) | `N.Helpers.csproj` | Helpers shared across implementations, migrations, etc. |
| `Host\` | `N.csproj` | Runs as a Windows service. `AssemblyName` = `N`. `<Prefer32Bit>false</Prefer32Bit>`. `Program.cs`: `configurator.SetDisplayName("<well-known identifier> [Processor] <Component> [<Purpose>]");` and `configurator.SetServiceName("N");`. `Alkami.MicroServices.Installer.Logic.nuspec` or `Alkami.MicroServices.Installer.Database.nuspec` references all non-test, non-client projects. |
| `Implementations\<Vendor>\` (opt.) | `N.<vendor/client>.Host.csproj` (sem.ver) | Vendor-specific hosts; usually replaces `Host\` unless providers are sub-projects. Often have their own migrations and test projects. |
| `Migrations\` (opt.) | `N.Migrations.csproj` | Configurable microservices usually have migrations; pairs with MigrationTests. |
| `Service\` (opt.) | `N.Service.csproj` | Default implementation for the host. |
| `Validations\` (opt.) | `N.Validations.csproj` | Validation for Data project classes. |
| `Tests\` | `N.Tests.csproj`, `Tests\IntegrationTests\N.IntegrationTests.csproj`, `Tests\MigrationTests\N.MigrationTests.csproj` | |

Sources: Microservice Coding Guidelines (https://confluence.alkami.com/spaces/SDKC/pages/53743063)

---

## 5. Chocolatey and NuGet nuspec metadata defaults

A `.nuspec` builds both a NuGet and a Chocolatey package. Standard metadata for any Alkami package (the `<files>` section follows the same pattern as the widget nuspec in 3.6):

```xml
<metadata>
  <id>{Package ID: the namespace of the C# startup project}</id>
  <version>$version$</version>                      <!-- replaced by the build script -->
  <title>{Human readable title}</title>
  <authors>Alkami Technology, Inc</authors>         <!-- or Financial Institution name; same for owners, copyright -->
  <owners>Alkami Technology, Inc</owners>
  <projectUrl>https://confluence.alkami.com/sdk</projectUrl>   <!-- or relevant FI location -->
  <iconUrl>https://www.alkami.com/files/alkamilogo75x75.png</iconUrl>   <!-- or FI logo -->
  <licenseUrl>http://alkami.com/files/orblicense.html</licenseUrl>     <!-- or FI license url -->
  <requireLicenseAcceptance>false</requireLicenseAcceptance>
  <description>{Another developer or product owner should read this and know the purpose of the component}</description>
  <releaseNotes>The initial re</releaseNotes>
  <tags>ConfigurableMicroservice</tags>
  <copyright>Copyright (c) {YearCreated} Alkami Technology, Inc</copyright>
</metadata>
```

`projectUrl` references a Confluence wiki page: after creating the component, create a page under the appropriate wiki section with the Package ID as the page header. Only `$version$` is filled by the build; set every other value.

**Which files to include**

- Library packages: include `.pdb` and `.xml` files for every assembly.
- Microservice packages: specify the assembly name to include all necessary assemblies from the bin folder.
- Componentized packages (widgets, providers): include your component's assemblies and all referenced microservice clients (with their contracts, data, and validations assemblies). **Do not include assemblies from the Orb Shared folder**, such as `Alkami.Ioc.dll`, `Alkami.Services.Subscriptions.ParticipatingClient.dll`, `Alkami.Services.Subscriptions.Resolver.dll`, `Alkami.MicroServices.Settings.ProviderBasedClient.dll`.

Sources: Chocolatey and NuGet Nuspec Metadata Defaults (https://confluence.alkami.com/spaces/SDKC/pages/78843917)

---

## 6. Logging Best Practices

### 6.1 Logger declaration

Every class declares a `static readonly` logger; loggers are always `readonly`. This ties the class name to each entry for filtering.

```csharp
private static readonly ILog Logger = LogManager.GetLogger<ClassBeingLogged>();        // non-static classes
private static readonly ILog Logger = LogManager.GetLogger(typeof(ClassBeingLogged));  // static classes
```

**Use the Common.Logging namespaces (via `Alkami.Utilities`), never log4net directly.** The only exception is setting context properties with `log4net.GlobalContext` or `log4net.LogicalThreadContext` at the request/action level. Use Common.Logging 2.1+ (lambda support).

### 6.2 Methods and enabled checks

| Level | Methods | Enabled check |
| --- | --- | --- |
| TRACE | `Logger.Trace()`, `TraceFormat()`, `LogTrace()` | `Logger.IsTraceEnabled` |
| DEBUG | `Logger.Debug()`, `DebugFormat()`, `LogDebug()` | `Logger.IsDebugEnabled` |
| INFO | `Logger.Info()`, `InfoFormat()`, `LogInfo()` | `Logger.IsInfoEnabled` |
| WARN | `Logger.Warn()`, `WarnFormat()`, `LogWarn()` | `Logger.IsWarnEnabled` |
| ERROR | `Logger.Error()`, `ErrorFormat()`, `LogError()` | `Logger.IsErrorEnabled` |
| FATAL | `Logger.Fatal()`, `FatalFormat()`, `LogFatal()` | `Logger.IsFatalEnabled` |

`Logger.IsEncryptionEnabled()` is a method (include the parentheses); when false, PII must be sanitized rather than encrypted.

### 6.3 Level guidance

- **FATAL**: the application or a thread is about to die. Rare; always investigated.
- **ERROR**: unexpected condition (assertion failure, network problem), usually an exception; not a user error. Log the full stack trace using the `Alkami.Utilities` helpers such as `ErrorCallStack(string)`; do not build it yourself.
- **WARN**: concerning but the operation continues (a bank setting was missing and a default was assumed).
- **INFO**: normal operation; production logs INFO and above. Must be useful to an operator ("[Who] transferred [Amount] from [Where] to [Where]") and not chatty.
- **DEBUG**: off in production by default. Use freely; log whatever you would need to debug from log files alone. Never do expensive work for a DEBUG entry unless DEBUG is enabled.
- **TRACE**: most detailed; external system requests and responses, every minute decision. Logging you would otherwise delete after development belongs at TRACE. Common library methods called many times per request must log at TRACE.

### 6.4 Formatting rules

- Use placeholders, not concatenation: `Logger.DebugFormat("User ID: {0}", user.ID)`, not `Logger.Debug("User ID: " + user.ID)`.
- **`*Format` methods throw if the string contains curly brackets that are not placeholders (JSON).**

```csharp
Logger.TraceFormat(jsonString);          // WRONG: throws
Logger.TraceFormat("{0}", jsonString);   // correct
Logger.Trace(jsonString);                // correct
```

- Wrap placeholders in brackets so empty values are visible: `Logger.DebugFormat("Executing task under context [{0}]...", (null == context ? "null" : context.Description));` prints `[]` for an empty description.
- Include data and description: `Logger.WarnFormat("Unknown message type. Message ID:[{0}], Type:[{1}]", message.ID, message.GetType());` not `Logger.Warn("Unknown message type");`. No magic numbers.

### 6.5 Efficiency

Arguments are evaluated before the log call even when the level is off. Guard expensive arguments with `Is*Enabled` or, preferably, a lambda. Vary detail by level with `IsTraceEnabled` (no `IsInfoEnabled` check; INFO is assumed always on).

```csharp
if (Logger.IsDebugEnabled)
    Logger.DebugFormat("Data with expensive ToString method = {0}", data.ToString());

Logger.Debug(m => m("My Class = {0}", myClass));   // preferred: evaluated only when enabled

if (Logger.IsTraceEnabled)
    Logger.Trace("Processing ids: {0}", requestIds);
else
    Logger.Info("Processing ids size: {0}", requestIds.size());
```

### 6.6 Method arguments, return values, external systems

Log entry and exit (arguments and return values) at DEBUG or TRACE for every method that accesses an external system (including the database), blocks, or waits. An "Entering" without a "Leaving" exposes deadlocks and starvation.

```csharp
public string PrintDocument(Document doc, Mode mode)
{
    Logger.TraceFormat("Entering PrintDocument(doc={0}, mode={1})", doc, mode);
    String id = DoSomeLengthyOperation();
    Logger.TraceFormat("Leaving PrintDocument(): {0}", id);
    return id;
}
```

Treat logging like unit tests: no part of the system should be without it. Log every request and response to an external service at DEBUG (everything in and out at TRACE). Use `System.Diagnostics.Stopwatch` and log `Elapsed` for external calls.

### 6.7 Exceptions and side effects

- **Log, or wrap and throw (preferred), never both**; otherwise the same stack trace appears twice.
- Log an exception only when you handle it and the reason is useful ("XXX service threw an exception. Displayed Service Down Error to user"), passing the exception to `Logger.Error()`. Otherwise let it propagate and be logged higher up.
- Log statements must not change behavior when toggled (example: a log entry that initialized a lazy NHibernate collection), and arguments must tolerate nulls (a chained `request.GetUser().getId()` inside a log lambda throws if any link is null).

### 6.8 PCI / PII

Never log passwords, tax IDs, card numbers, PINs, or other confidential data without masking, redacting, or encrypting. Decryption keys for staging and production are held by Security.

**Sanitization** (`Alkami.Utilities.Formatting.Utilities.StringUtilities`, NuGet package `Alkami.Utilities`):

- `SanitizeData()`: string extension that masks patterns matching credit card numbers, tax IDs, and private numbers. Use it when logging any response from a third party or core.
- `MaskCardNumbers()`: masks a string leaving the first 2 and last 4 digits.

```csharp
using Alkami.Utilities.Formatting.Utilities.StringUtilities;
Logger.TraceFormat("<My Service Response>: Response [{0}]", response.SanitizeData());
Logger.LogDebug(m => m("SSN: {0}", socialSecurityNumber.SanitizeData()));
```

`SanitizeData()` regexes (full patterns on the source page): credit cards as 2 leading digits plus 7-13 digits, spaced quads, dashed quads, or Amex 4-6-5, ending in 4 digits; tax IDs as `\b([0-9]{1,3})([ -]?)([0-9]{2,3})([ -]?)([0-9]{4})([a-zA-Z]?)\b`; private numbers as standalone runs of 2 to 19 digits.

**Encryption** (when masking is unreasonable): `ILog` extensions in `Alkami.Common.LogUtility` and `Alkami.Utilities.LogUtility` encrypt with a certificate public key: `LogInfoEncrypted()`, `LogWarnEncrypted()`, `LogErrorEncrypted()`, `LogDebugEncrypted()`, `LogTraceEncrypted()`. Core helpers that log an encrypted `CREQ` entry (core method name + string): `CreateCoreRequestEntry()` / `CreateCoreResponseEntry()` (XML), `CreateCoreRequestJsonEntry()` / `CreateCoreResponseJsonEntry()` (JSON). Lower level: `Alkami.Utilities.Cryptography.AsymmetricEncryption.Encrypt(payload, X509 certificate)` and `.Decrypt(payload)` (private key found by the thumbprint in the payload; fails if unavailable), plus `AsymmetricEncryption.Encryptor` / `.Decryptor`. Encryption is toggled in app settings (Alkami.Utilities.Configuration, Confluence page ID 32246281).

```csharp
if (Logger.IsTraceEnabled)
{
    if (Logger.IsEncryptionEnabled())
        Logger.LogTraceEncrypted(responseWithPII);
    else
        Logger.Trace(responseWithPII.SanitizeData());
}
```

### 6.9 Operational configuration (log4net.config)

- Log locally to files; each service or process logs to a **different file**; appenders watch the config file and reload without restart; use rotation and retention. Format includes date/time to the millisecond, level, class name, thread ID, message, stack trace. Ship PDBs so stack traces have line numbers. Logs are aggregated centrally (Splunk).
- **Never lower the `root` logger to DEBUG or below** (an NHibernate bug breaks the system). Change only the `Alkami` logger or your own named logger. Loggers match by namespace prefix.

IIS applications under `C:\Orb` each have a `log4net.config` in their root (for example `C:\Orb\WebClient\log4net.config`). Change the `Alkami` logger from `ERROR` (default) to `DEBUG` (`TRACE` is noisier), adding it if absent, and add a logger for your namespace (a full namespace such as `USBFI.Client.Widget.MyMoney` narrows it to one widget):

```xml
<logger name="Alkami" additivity="false">
  <level value="DEBUG" />
  <appender-ref ref="StandardRollingFileAppender" />
  <appender-ref ref="DebugAppender" />
</logger>
<logger name="USBFI" additivity="false">
  <level value="DEBUG" />
  <appender-ref ref="StandardRollingFileAppender" />
  <appender-ref ref="DebugAppender" />
</logger>
```

Microservices: `C:\ProgramData\chocolatey\lib\<package id>\tools\log4net.config`, for example `C:\ProgramData\chocolatey\lib\Alkami.MicroServices.Authorization.Service.Host\tools\log4net.config`. Your own service's logger already carries your project namespace; set it to `DEBUG` the same way.

Sources: Logging Best Practices (SDK) (https://confluence.alkami.com/spaces/SDKC/pages/51352129)

---

## 7. Unit and integration testing

Stack: **NUnit 3** and **Moq** (examples also use FluentAssertions). Widget tests inherit `Alkami.Client.Common.Test.ClientUnitTestBase` (section 3.5). Install the NUnit3 Test Adapter in Visual Studio (https://docs.nunit.org/articles/vs-test-adapter/Adapter-Installation.html), then `Install-Package Moq -Version 4.14.5` in Package Manager Console.

The microservice template may use `[SetUp]` where the example uses `[OneTimeSetUp]`; **`[OneTimeSetUp]` is more performant and should be used** for fixture-level setup. Both code files are attached to the Confluence page (attachments not visible here).

### 7.1 Service unit test pattern

Mocks are created in `[OneTimeSetUp]` and injected through the service's static factory hooks (`ScopeFactory`, `ContextFactory`, `SettingServiceFactory`, ...). Pass `.Object` when a mock is a constructor argument or you get an object reference error. Namespaces: `Alkami.Data.Access`, `Alkami.Data.Validations`, `Alkami.Security` (`AlkamiClaimTypes`), `Alkami.Test` (`PermissionHelpers`), `Alkami.Test.EntityFrameworkHelpers` (`CreateFakeDbSet`), `Alkami.MicroServices.Settings.Contracts[.Requests|.Responses]`, `FluentAssertions`, `Moq`, `NUnit.Framework`.

```csharp
namespace USBFI.MicroServices.Accounts.Service.Tests.AccountImplTests
{
    [TestFixture]
    public class AccountTests
    {
        private AccountServiceImp _accountServiceImpl;
        private Mock<IDataScope> _dataScopeMock;
        private Mock<DbConnection> _dbConnectionMock;
        private Mock<AccountsContext> _accountContextMock;
        private Mock<ISettingsServiceContract> _settingServiceMock;
        private Guid[] accountIdentifiers = { Guid.Parse("9C6B80ED-9242-4D96-8D25-CD7B5ABA8FC5"), /* ... */ };

        [OneTimeSetUp]
        public void Setup()
        {
            PermissionHelpers.AccountGuidsToUse = accountIdentifiers.ToList();
            EntityValidator.AddValidator(new AccountValidator());
            _accountServiceImpl = new AccountServiceImp();

            _dbConnectionMock = new Mock<DbConnection>();
            _dbConnectionMock.Setup(x => x.ConnectionString).Returns("MockConnectionString");
            _accountContextMock = new Mock<AccountsContext>(_dbConnectionMock.Object);
            _accountContextMock.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(0));
            _dataScopeMock = new Mock<IDataScope>();
            _dataScopeMock.Setup(x => x.Connection).Returns(_dbConnectionMock.Object);

            AccountServiceImp.ScopeFactory = request => _dataScopeMock.Object;
            AccountServiceImp.ContextFactory = context => _accountContextMock.Object;
            AccountServiceImp.AccountAddOrUpdateFactory = (set, account) => { };

            _settingServiceMock = new Mock<ISettingsServiceContract>();
            _settingServiceMock.Setup(x => x.GetItemsAsync(It.IsAny<GetItemRequest>()))
                               .Returns(() => Task.FromResult(new ItemResponse()));
            AccountServiceImp.SettingServiceFactory = () => _settingServiceMock.Object;

            PermissionHelpers.SetPrincipalWithSpecificClaims(new List<Claim>
                { new Claim(AlkamiClaimTypes.AccountPermission + accountIdentifiers[0], "3") });
        }

        [Test]
        public async Task AddOrUpdateAccountAsync_Returns_Saved_Accounts()
        {
            var account1 = new Account();
            var account2 = new Account();
            var accounts = new List<Account> { account1, account2 };
            var request = new AddOrUpdateAccountRequest { ItemList = accounts };

            var dbSetAccountMock = _accountContextMock.CreateFakeDbSet(accounts);
            _accountContextMock.Object.Accounts = dbSetAccountMock.Object;

            PermissionHelpers.InjectClaimsIntoRequest(request);
            var result = await _accountServiceImpl.AddOrUpdateAccountAsync(request);

            result.HasError.Should().BeFalse();
            result.ValidationResults.Count.Should().Be(0);
            Assert.That(result.Accounts.Contains(account1));
            Assert.That(result.Accounts.Contains(account2));
        }
    }
}
```

### 7.2 Service integration test pattern

The fixture inherits `DatabaseBasedTests` (`Alkami.Test`), calls `MockResolver.Use()`, configures log4net from `log4net.config` in the test directory (`log4net.Config.XmlConfigurator`), replaces the broker subscriber with `EmptySubscriber` (`Alkami.Broker.Base`), starts the real host and stops it in `[OneTimeTearDown]`, and tests through the real service client. `[SetUp]` runs before every test and resets `ScopeFactory` to `CreateMockedDataScope()` so the "tester" database is copied and the original stays unmodified.

```csharp
namespace USBFI.MicroServices.Accounts.Service.IntegrationTests
{
    [TestFixture]
    public class AccountsIntegrationTests : DatabaseBasedTests
    {
        private readonly IAccountServiceContract _testClass;
        private readonly DistributedAccountService _host;
        private readonly Mock<ISettingsServiceContract> _settingsServiceContract;

        public AccountsIntegrationTests()
        {
            MockResolver.Use();

            var file = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "log4net.config"));
            if (file.Exists) { XmlConfigurator.Configure(file); }

            _settingsServiceContract = new Mock<ISettingsServiceContract>();
            _settingsServiceContract.Setup(x => x.GetItemsAsync(It.IsAny<GetItemRequest>()))
                                    .Returns(() => Task.FromResult(new ItemResponse()));
            AccountServiceImp.SettingServiceFactory = () => _settingsServiceContract.Object;

            Alkami.Broker.App.Subscription.SubscriberFactory = () => new EmptySubscriber(CancellationToken.None);

            _host = new DistributedAccountService();
            _host.OnStart();
            _testClass = new AccountServiceClient();
        }

        [SetUp]
        public void Setup()
        {
            AccountServiceImp.ScopeFactory = request => CreateMockedDataScope();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _host.OnStop(TimeSpan.FromSeconds(10));
        }

        [Test]
        public async Task AccountsMicroservice_GetAccountsAsync_ReturnsAccounts()
        {
            var maxResults = 9;
            var accountsRequest = new GetAccountRequest
            {
                Mapping = { IncludeAccountType = true, IncludeRoutingInfo = true },
                Filter = { IncludeAggregate = true, IncludeExternal = true },
                MaxResults = maxResults,
            };
            SetupFixture.AugmentBaseRequest(accountsRequest, SetupFixture.Defaults.Users[1]);

            var accountResponse = await _testClass.GetAccountAsync(accountsRequest);

            Assert.IsNotNull(accountResponse);
            Assert.IsFalse(accountResponse.HasError);
            Assert.AreEqual(maxResults, accountResponse.Accounts.Count);
            foreach (var account in accountResponse.Accounts)
            {
                List<ValidationResult> results;
                Assert.IsTrue(account.Validate(out results));
            }
        }
    }
}
```

Sources: Unit and Integration testing guidelines and examples (https://confluence.alkami.com/spaces/SDKC/pages/95497639); Widget Coding Guidelines (https://confluence.alkami.com/spaces/SDKC/pages/53743045)

---

## 8. Cryptographic architecture guidelines

- Follow NIST guidelines, particularly **NIST SP 800-57**.
- Data in transit: at minimum **TLS 1.2**. Data at rest: at minimum **AES 256**.
- Publicly signed certificates: Alkami follows the CA/Browser Forum and limits them to a maximum of **398 days**. Other certificate types follow industry best practice.

Sources: Cryptographic Architecture Guidelines (https://confluence.alkami.com/spaces/SDKC/pages/282928359)

---

## 9. SDK pattern overview: widget plus microservice pairing and SSO

From "SDK Pattern Documentation - Basics and Overview" (updated 2026-04-10), a high-level pattern guide. **Where it conflicts with the detailed guideline pages above, those pages are authoritative for naming and layout; conflicts are flagged.**

### 9.1 Pairing pattern

Every SDK integration is two independently deployed artifacts: a **Client Widget** (all UI rendering and user interaction inside the Alkami digital banking shell: Razor views, JS, CSS) and a **Microservice Host** (all backend logic, external API calls, data transformation). The widget calls the microservice over HTTP via a configured endpoint and renders the result.

Naming on the pattern page: `{ClientCode}.Client.Widget.{FeatureName}` (e.g. `ACME.Client.Widget.SkipAPay`) and `{ClientCode}.MS.{FeatureName}.Service.Host` (e.g. `ACME.MS.SkipAPay.Service.Host`). Conflict: the Microservice Coding Guidelines (2025) specify **no** `.Service` or `.Host` suffix; follow section 4 for new package IDs and confirm with the SDK team if unsure.

Packaged widget layout on the pattern page (matches the nuspec `target` values in 3.6): `content/Areas/App/{Scripts,Styles,Images,_SiteText}`, `content/Areas/App/Views/ACMESkipAPay/` with `Index.cshtml` and `Error.cshtml` (both required) plus `Mobile/Index.cshtml` (required) and `Mobile/Error.cshtml`; `lib/` (the page says ".NET 4.6.2 dependencies"; the Widget Coding Guidelines target .NET 4.8); `src/Controllers/` (`ACMESkipAPayController.cs`, `ACMESkipAPayControllerBase.cs`, `MobileACMESkipAPayController.cs`), `src/Models/`, optional `src/Services/` and `src/Extensions.cs`, `src/AlkamiManifest.xml`. Conflict: the pattern page names the shared controller `<Name>ControllerBase`; the Widget Coding Guidelines use `Base<url slug>Controller`.

### 9.2 Controller, model, and view pattern

Shared logic goes in an abstract base; desktop and mobile controllers only delegate. The `Mobile`-prefixed controller resolves to `Mobile/` views automatically. Models are plain data containers with no business logic. Views use `@model`, a `widget-container` wrapper div, and jQuery `$.post('@Url.Action("Submit")', ...)`.

```csharp
public abstract class ACMESkipAPayControllerBase : AlkamiController
{
    private readonly ISkipAPayService _skipAPayService;
    protected ACMESkipAPayControllerBase(ISkipAPayService s) { _skipAPayService = s; }

    protected async Task<ActionResult> IndexCore()
    {
        var model = new ACMESkipAPayModel
        {
            EligibleLoans = await _skipAPayService.GetEligibleLoansAsync(CurrentUser.UserId)
        };
        return View(model);
    }
}
// ACMESkipAPayController and MobileACMESkipAPayController derive from the base and only expose
// public async Task<ActionResult> Index() => await IndexCore();  (same for [HttpPost] Submit => SubmitCore)
```

### 9.3 AlkamiManifest.xml as illustrated on the pattern page

Every widget must include `AlkamiManifest.xml` to register with the platform. The page shows `<AlkamiManifest><Widget>` with `Name` (must match the controller name prefix exactly, e.g. `ACMESkipAPay`), `FriendlyName`, `Description`, `Version`, `ServiceEndpoint`, `RequiredRole` (e.g. `Member`), `DesktopController`, `MobileController`. Caution: this is an illustration; verify element names against the "Adding or Updating an AlkamiManifest" page (microservices topic) before writing a manifest.

### 9.4 SSO integration pattern

SSO widgets authenticate the current Alkami user and redirect to a partner (insurance, wealth management, rewards, e-documents) with no second login. Flow: member clicks widget; the widget controller reads `CurrentUser` and calls the auth microservice; the microservice validates, builds a SAML assertion or OAuth token, signs it with the partner certificate or secret, and returns a redirect URL; the widget redirects; the partner validates the token.

The widget controller is intentionally thin: `IndexCore()` builds an `SSORequest` from `CurrentUser.UserId`, `Email`, `FirstName`, `LastName`, `InstitutionId`, calls `_authService.GetSSORedirectUrlAsync(ssoRequest)`, returns `View("Error")` if the URL is empty, else `Redirect(redirectUrl)`.

- **SAML 2.0** (enterprise partners such as Ameriprise, FIS, Lively): build attributes (email, firstName, lastName, partner-required fields; **never SSN, account numbers, or raw passwords**), create the assertion with issuer, audience, nameId = user ID, signing certificate, `validFor: TimeSpan.FromMinutes(5)`, Base64-encode it, return `{PartnerAcsUrl}?SAMLResponse={escaped assertion}`. `SamlConfig`: `Issuer`, `Audience`, `PartnerAcsUrl`, `SigningCertificate` (thumbprint only; loaded from the store).
- **OAuth 2.0 / JWT** (fintech partners such as Narmi, Envoy, AmpliFI): POST `grant_type=client_credentials`, `client_id`, `client_secret`, `user_id` to the token endpoint, return `{PartnerLaunchUrl}?token={access token}`. `OAuthConfig`: `ClientId`, `ClientSecret` (from a secrets manager at runtime), `TokenEndpoint`, `PartnerLaunchUrl`.
- The page's microservice samples use .NET Core idioms (`IOptions<T>`, `appsettings.json`). SDK microservices per the detailed pages are .NET Framework Windows services; treat the samples as illustrating the flow, not drop-in code.

**Security rules, non-negotiable for every SSO integration:**

- DO pull user identity exclusively from `CurrentUser`; store all secrets (certs, client secrets, API keys) in environment variables or a secrets manager; set token/assertion TTL to 5 minutes maximum; validate the partner redirect domain against an allowlist; log SSO initiation events (user ID plus timestamp, no PII).
- DO NOT pass SSN, full account numbers, card numbers, or passwords in any SSO payload; hardcode secrets, certificates, or API keys in source or appsettings.json; trust user-supplied redirect URLs without validation; log full SAML assertions or OAuth tokens; reuse tokens across sessions.

Sources: SDK Pattern Documentation - Basics and Overview (https://confluence.alkami.com/spaces/SDKC/pages/556876036)

---

## 10. Technical Guide templates

Two near-identical templates document a finished feature or vendor integration: "Technical Guide Template" (2021) and "Vendor Technical Guide Template" (2022, newer). Both open with a header table (`Document Status: DRAFT`, `Primary Vendor Contact`) and a table of contents, then: Overview; Architecture and Design; Account Management (products to purchase, dependencies on existing Alkami products); Implementation (Setup) containing an Implementation Status table (Widget or Microservice | In Testing | In Beta | In GA | Release Versions | Notes; In Testing = internal teams, In Beta = external clients, GA = in production), Bounces and Cache (is a bounce needed, what exactly, which providers or microservices do not expire the cache), How to Configure (Prerequisites, Setup, Configuration Scripts, Additional Details; the vendor template references "How to Install Microservices" and "Choco Usage Guidelines and Customizations" and says to ask the Product Owner for microservice versions), a Configuration Options table (Setting Name | Feature | Required | Description | Input Needed From Customer | Format | Default | Accepted Values | Example, plus Setting Type), How to Test, Limitations; Troubleshooting (Common Issues with Issue / Solution); FAQs; Related Links.

Sources: Technical Guide Template (https://confluence.alkami.com/spaces/SDKC/pages/145498597); Vendor Technical Guide Template (https://confluence.alkami.com/spaces/SDKC/pages/200247962)
