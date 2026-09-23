# SDK Releases, Changelog, Breaking Changes, and Events

**What this covers.** The release history of the Alkami SDK (ORB platform) from 2018.01 through 2025.2 as documented in the SDK Confluence space: what changed per release, what was breaking, and which packages, commands, or config edits an upgrade required. It also covers the Changelog page (package diffs for 2024.4 to 2025.2), three migration guides (BusinessAchProcessing MS deprecation, MFA widget requirements for 2019.01, SymConnect widget changes for 2022.6 and later), and a catalog of developer webinars, client conference developer-track sessions, and hackathon materials, keeping the package names, templates, and code those pages contain.

## 1. Release process and upgrade commands

- Install and upgrade steps live on SDK Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/40403249) and Alkami Platform Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/48800506). Every release page from 2022.4 on says those instructions "are frequently updated" and must be re-read before each upgrade.
- Feature-level release notes are on Midas (https://midas.alkami.com/category/release-notes/); the SDK release pages only document what an SDK developer must change.
- Maintenance branches: 2019.01.0.709 / 2019.01.1.875 (Maintenance) was the last maintenance branch. From 2019.02 on there are no maintenance SDK releases (this supersedes the 2018.07.2.809 promise of one maintenance release per iteration).
- Upgrade command by era:
  - Through 2020.3: `choco upgrade alkami.machinesetup.sdk -y`
  - 2020.4 and later: `choco upgrade alkami.machinesetup.sdk.features -y`. Alkami.MachineSetup.SDK became a dependency of Alkami.MachineSetup.SDK.Features; the base package alone is the "Minimum Viable Platform" (Authentication, Settings, empty DashboardV2).
  - 2022.4 and later: a new installation process; use the current Machine Setup page, not commands from older release notes.
- Stop services around an upgrade (2019.4.0.3):

```powershell
Stop-IISAndServices
choco upgrade alkami.machinesetup.sdk -y   # alkami.machinesetup.sdk.features from 2020.4 on
Start-IISAndServices
# if the functions are missing:
choco install Alkami.PowerShell.IIS -y
```

- Before each submission, upgrade widget NuGet packages but first check NuGet Package Requirements (What Versions Are Current) (https://confluence.alkami.com/sdk/nuget-package-requirements-what-versions-are-current-82794502.html): some packages are pinned and must not be taken to latest (2020.6.0.8).

Sources: Releases (https://confluence.alkami.com/spaces/SDKC/pages/55348748), 2018 Releases (https://confluence.alkami.com/spaces/SDKC/pages/55357563), 2019 Releases (https://confluence.alkami.com/spaces/SDKC/pages/55357565), 2020 Releases (https://confluence.alkami.com/spaces/SDKC/pages/82780220), 2021 Releases (https://confluence.alkami.com/spaces/SDKC/pages/115509671), 2022 Releases (https://confluence.alkami.com/spaces/SDKC/pages/172118230), 2023 Releases (https://confluence.alkami.com/spaces/SDKC/pages/255773406), 2024 Releases (https://confluence.alkami.com/spaces/SDKC/pages/400211323), 2019.4.0.3 (https://confluence.alkami.com/spaces/SDKC/pages/67612381), 2020.6.0.8 (https://confluence.alkami.com/spaces/SDKC/pages/102550578).

## 2. Chronological release table

Dates are Confluence page dates; Alkami did not publish GA dates on these pages.

| Version | Page date | Key changes | Breaking / required action |
|---|---|---|---|
| 2018.01.304 | 2019-01 | 64-bit microservices; new icon guide; "More" link / WidgetSummary removed | `Prefer32Bit` = false in Service.Host.csproj; `core.Item` gained `LastUpdate`, widget SQL scripts must change (3.1) |
| 2018.02.351 | 2019-01 | MVC 5; WIF 3.5 removed; Settings MS and Message Center contract changes | SettingDescriptor namespace moved; providers inherit `ProviderBasedService<T>`; Settings packages >= 4.1.x (3.1) |
| 2018.03.412 | 2019-01 | No major changes | None |
| 2018.04.464 | 2019-01 | Native Chocolatey SDK install (Beta); feed tiers; Authorization widget componentized | VM image supported only through 2018 |
| 2018.05.535 / 2018.06.0.599 | 2019-02 | New local-deploy script; expandable permissions preview | Remove `LightInjectHttpModule` line from C:\Orb\WebClient\web.config or get HTTP 500 |
| 2018.07.0.641 / 2018.07.2.809 | 2019-01 | Componentization begins; maintenance release | None |
| 2019.1.0.709 (+2019.1.1.875) | 2019-05-17 | RPSTS componentized on Authorization MS; Expandable claims; Message Center componentized | RPSTS and Authorization MS must run; Message Center separate install; MFA widgets (3.2); last maintenance branch |
| 2019.2.0.723 | 2019-03-25 | .NET 4.7.2 minimum; themes as Alkami.Orbital; Snippets Runtime 2.0.0; new Risk suite | .NET 4.7.2 Developer Pack; `choco install Alkami.Orbital -y`; JetBrains.Annotations removed; Alkami.Modules.RiskEvaluation 2.0.2 |
| 2019.3.0.768 | 2019-05-17 | Platform and MS on .NET 4.7.2; Authorization MS 1.4 | Install .NET 4.7.2 Developer Pack first |
| 2019.4.0.3 | 2019-08-01 | Upgrade guidance | `Stop-IISAndServices` before upgrade |
| 2019.5.0.6 | 2019-09-13 | Legacy Sync Users; RiskEvaluation 2.3.0 needs StepUpManager; Common.SymConnect NuGet; Admin API package | Install Alkami.Legacy.Sync.Users 1.0.2 or Radium fails; replace Alkami.App.Providers.Shared.SymConnect |
| 2019.6.0.8 | 2019-11-08 | Admin Flavors widget; Message Center Management MS; Subscription Service port 50002 | Install Alkami.Admin.Widget.Flavors 1.0.1, Alkami.MS.MessageCenterManagement.Service.Host 1.0.1; reserve ports |
| 2020.1.0.13 | 2020-01-14 | Knockout 3.5.0 (XSS fix); Alkami.Legacy.Sync.Transactions 1.0.0 | Template bindings must name an existing template; default "loading" template per view |
| 2020.2.0.9 | 2020-07-10 | No contract changes | Move projects to .NET Framework 4.8 |
| 2020.3.0.11 (Important) | 2020-05-21 | Common.Logging binding redirect; new.web.config files | Replace 15 web.config files, delete RP-STS Alkami.Utilities.dll (3.4); Symitar must upgrade SymConnect packages |
| 2020.4.0.13 | 2020-07-10 | jQuery 3.5.0; security scans; WidgetName in AlkamiManifest.xml; SDK Features package | `choco upgrade alkami.machinesetup.sdk.features -y`; AlkamiManifest.xml WidgetName = display name |
| 2020.5.0.26 | 2020-08-25 | Event Management MS removed from SDK Features; temporary Radium jobs | Install Event Management MS separately if needed |
| 2020.6.0.8 | 2020-11-05 | No new instructions | Check pinned NuGet versions before upgrading |
| 2021.1.0.8 | 2020-12-29 | No SDK changes | None |
| 2021.3.0.12 | 2021-04-30 | Auto-deploy on build | `choco upgrade Alkami.PowerShell.IIS -y` |
| 2021.5.0.8 | 2021-08-24 | New login workflow on by default | Brady password and OTP from the Login Refresh page |
| 2022.2.0.13 | 2022-03-14 | Services left on manual start; VS 2022 templates | `Set-SDKServiceStartupType ...` (3.5); `choco upgrade Alkami.SDK.templates -y` |
| 2022.4.x.x / 2022.5 | 2022-07 / 2022-09 | New installation process | Follow current Machine Setup page |
| 2022.6 | how-to 2023-02 | SymConnect widget packages | SymConnectMultiplexer >= 2.13 (3.6) |
| 2023.1 | 2023-03-09 | Matches Alkami Stage; IIS cleanup | Remove stray IIS sites; keep WebClientAdmin, WebClient, IPSTS |
| 2024.3 | deprecation page | AchTemplates MS gains BusinessACHProcessing endpoints | Migrate (3.7) |
| 2024.4 | 2024-11-04 | .NET 6.0 and 8.0 required; Subscriptions pinned 5.4.0; TrustServerCertificate; StepUp/Risk condensed | See 3.8 |
| 2025.1 | Changelog 2025-05 | Nexus, Session, CoreProxy, Sidekick added; MicroServiceTester removed; Subscriptions 6.0.3; BusinessACHProcessing service gone | Section 4 |
| 2025.2 | Changelog 2025-05 | Version bumps only | Section 4 |

## 3. Release details

### 3.1 2018 releases

**2018.01.304.** All microservices must build 64-bit: set `<Prefer32Bit>false</Prefer32Bit>` in both debug and release property groups of `Service.Host.csproj`. `DeveloperDynamic.core.Item` gained a `LastUpdate` column: widget install scripts must add `NewLastUpdate DateTime` and `OldLastUpdate DateTime` to the `@itemSettingOutput` table variable, and the `MERGE core.ItemSetting` statement must use an explicit column list, `INSERT (ItemID, Name, Value, CreatedUTC, Version) VALUES (...)`, instead of a bare `INSERT VALUES`. The "More" link (WidgetSummary) on Widget Options was removed; drop `WidgetSummary` at your convenience.

**2018.02.351.** MVC 5 upgrade for widgets: Alkami.Webtoolkit, Alkami.WebToolkit.Extensions, Alkami.Webtoolkit.Scripts to 2.0.0; Microsoft.AspNet.Mvc 5.2.3; Microsoft.AspNet.WebPages 3.2.3; uninstall Mvc3Futures and MvcContrib.Mvc3-ci; delete any web.config under Views; add `.ToString()` to boolean model values in Razor. WIF 3.5 removed (first mandatory WebExtension module). Settings MS breaking change: `SettingDescriptor` moved from `Alkami.TrackableObjects.SettingDescriptor` to `Alkami.MicroServices.Settings.ProviderBased.Contracts.SettingDescriptor`; provider services inherit `ProviderBasedService<T>` (`T : Alkami.TrackableObjects.Plugins.Plugin`) instead of `DistributedServiceBase` and override the `ServiceContract` property. Minimums: Alkami.ConfigurationDrivenObjects and Alkami.ConnectorsAndProcessors >= 4.1.2, Alkami.MicroServices.Settings.Contracts and .Client >= 4.1.0, Alkami.MicroServices.Settings.ProviderBased.Contracts and Alkami.MicroServices.Settings.ProviderBasedService >= 4.1.2. Message Center: retest code using Alkami.App.MessageCenter.Contracts or Alkami.Client.Services.MessageCenter against the assemblies in C:\Orb\Shared.

**2018.04.464.** Native Chocolatey install (Beta; minimum Windows 8.1/10, VS 2015/2017, SQL Server 2012/2016/2017, PowerShell 5.0). Feeds: ThirdParty, choco.canary (deprecated after Q3 2018), choco.dev, choco.qa, choco.stage, choco.prod; prefer stage and prod. Unsupported natively: Nag alerts, SMTP, SMS, Entrust, BillPay, Wires/ACH, admin reporting.

**2018.05.535 / 2018.06.0.599.** After upgrading from 2018.05, remove the `add name="LightInjectHttpModule"` line from C:\Orb\WebClient\web.config or WebClient returns Server Error 500.

Sources: 2018.01.304 (https://confluence.alkami.com/spaces/SDKC/pages/55348800), 2018.02.351 (https://confluence.alkami.com/spaces/SDKC/pages/55348819), 2018.03.412 (https://confluence.alkami.com/spaces/SDKC/pages/55348822), 2018.04.464 (https://confluence.alkami.com/spaces/SDKC/pages/55348825), 2018.05.535 (https://confluence.alkami.com/spaces/SDKC/pages/55348829), 2018.06.0.599 (https://confluence.alkami.com/spaces/SDKC/pages/55348831), 2018.07.0.641 (https://confluence.alkami.com/spaces/SDKC/pages/55348869), 2018.07.2.809 (https://confluence.alkami.com/spaces/SDKC/pages/55348873).

### 3.2 2019 releases and the MFA widget update

**2019.1.0.709 / 2019.1.1.875.** RPSTS is componentized and uses the Authorization microservice; four claim types replace the hard-coded 144-permission limit: `UserPermissionsExpandable`, `EntityPermissionsExpandable`, `AccountPermissionsExpandable`, `AllGrantedPermissionsExpandable`. RPSTS and Authorization MS must be installed and running. Message Center is a separate feature install; some admin views depend on it.

**MFA (2FA) widgets for 2019.01** (optional in 2019.01, included from 2019.02):
- Web server: Alkami.Modules.RiskEvaluation 2.0.0, Alkami.Modules.LegacyAuthClient 1.1.1, Alkami.Modules.LegacyAuthAdmin 1.1.1. App server: Alkami.MicroServices.Risk.Management.Service.Host 1.3.0, Alkami.MicroServices.Risk.Alkami.Service.Host 1.3.0/1.3.1, Alkami.MicroServices.Audit.Service.Host 6.6.0.
- Widget: install NuGet Alkami.RiskEvaluation.WebExtension; remove the `<div id="second_factor" class="modal fade" ...>` block that renders `~/Areas/API/Views/SecondFactorAuthentication/(Mobile)SecondFactorAuthentication.cshtml` from desktop and mobile views; keep `@Html.IncludeSiteTextScript()` in the view; replace the `shouldprompt2fa` call with a risk request for your ActionType:

```csharp
var request = new ChangePasswordRiskRequest(){
    //SessionInformation = new Dictionary<string, string>()
};
if (!this.IsAllowedByRiskEvaluation(request)) { return null; }
```

**2019.2.0.723.** Alkami.Client and Alkami.Common 2019.02 require .NET Framework 4.7.2 (Developer Pack, not just runtime: https://dotnet.microsoft.com/download/thank-you/net472-developer-pack). Check:

```powershell
Get-ChildItem "HKLM:SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" | Get-ItemPropertyValue -Name Release | ForEach-Object { $_ -ge 461808 }
```

Themes moved out of Alkami.Client.WebClient/Orbital: `choco install Alkami.Orbital -y` (required). JetBrains.Annotations was removed from Alkami.Common: delete the annotations or install JetBrains.Annotations 10.0.0 from the ThirdParty feed. Upgrade Alkami.Modules.RiskEvaluation to 2.0.2. Widget TargetFrameworkVersion 4.7.2 is optional and deployable only on 2019.02+.

**2019.3.0.768.** Platform and microservices build on .NET 4.7.2. Authorization MS 1.4 requires IPAddress and UserAgent to generate API tokens.

**2019.5.0.6.** Radium requires Alkami.Legacy.Sync.Users 1.0.2 or it will not start. Alkami.Modules.RiskEvaluation 2.3.0 depends on Alkami.MicroServices.StepUpManager.Service.Host 1.11.0. Providers referencing `Alkami.App.Providers.Shared.SymConnect` must move to `Alkami.App.Providers.Core.Common.SymConnect` (1.1.4) or `Alkami.MicroServices.SymConnectMultiplexer.Client` (2.3.4). Admin API controllers moved to Alkami.Admin.API 1.0.2.

**2019.6.0.8.** Alkami.Admin.Widget.Flavors 1.0.1 (admin path /Flavors/Setup). Alkami.MS.MessageCenterManagement.Service.Host 1.0.1 is required. The Subscription Service needs port 50002; reserve ports.

Sources: 2019.1.0.709 (https://confluence.alkami.com/spaces/SDKC/pages/55348895), Update MFA (2FA) widget requirements for ORB 2019.01 (https://confluence.alkami.com/spaces/SDKC/pages/52595162), 2019.2.0.723 (https://confluence.alkami.com/spaces/SDKC/pages/55348901), 2019.3.0.768 (https://confluence.alkami.com/spaces/SDKC/pages/58458744), 2019.5.0.6 (https://confluence.alkami.com/spaces/SDKC/pages/71992057), 2019.6.0.8 (https://confluence.alkami.com/spaces/SDKC/pages/78844094).

### 3.3 2020 releases

**2020.1.0.13.** Knockout 3.5.0 asserts on a template binding without a name instead of failing silently; every binding must reference an existing template and each view needs a default "loading" template. Alkami.Legacy.Sync.Transactions 1.0.0 was added to Alkami.MachineSetup.SDK.

**2020.2.0.9.** No contract changes; update projects to .NET Framework 4.8 to build against newer Alkami packages.

**2020.3.0.11 (Important).** The Common.Logging binding redirect changed and the machinesetup upgrade does not overwrite web.config. Run `stop-iisandservices`, then either delete web.config before upgrading or copy the shipped `new.web.config` over `web.config` afterward in each of C:\Orb\WebClient, WebClientAdmin, AuditService, BankService, ContentService, CoreService, ExceptionService, IPSTS, MessageCenterService, NagConfigurationService, NotificationService, SchedulerService, SecurityManagementService, STSConfiguration, SymConnectMultiplexer; also `del C:\Orb\RP-STS\bin\Alkami.Utilities.dll`; then IISRESET. Two .bat files are attached to the page. If Subscriptions will not start, reserve ports per Hardware and Software Requirements:

```
netsh int ipv4 Add excludedportrange protocol=tcp startport=12345 numberofports=2
netsh int ipv4 Add excludedportrange protocol=tcp startport=50000 numberofports=30
netsh http add iplisten 0.0.0.0
netsh http add iplisten ::
```

Stage-match Symitar sites must also run `choco upgrade Alkami.App.Providers.Multiplexer.Client`, `choco upgrade Alkami.App.Providers.Core.SymConnect`, `choco upgrade Alkami.MicroServices.SymConnectMultiplexer.Service.Host`.

**2020.4.0.13.** jQuery 3.5.0 (https://jquery.com/upgrade-guide/3.5/). C# and JavaScript security scans are part of submission review. `AlkamiManifest.xml` "Widget Name" must be the human-friendly display name (missing values get "Suggestion" now, "NeedsWork" later). Configurations are no longer submitted; Alkami creates them. New command `choco upgrade alkami.machinesetup.sdk.features -y`; SDK Features includes Forms, Flags, Contacts, Holidays, Images, Notifications, Event Management, Transactions, Transaction Enrichment, Secure Message Center, My Accounts, Risk Management, Step Up, and the admin Flavors, Package Assignment, User Management, API Applications widgets.

**2020.5.0.26.** Event Management MS is no longer a dependency of Alkami.MachineSetup.SDK.Features (install it yourself; section 7). Base package temporarily gained Alkami.App.Providers.Radium.BillPay and Alkami.App.Providers.Radium.ApproveACHBatch.

Sources: 2020.1.0.13 (https://confluence.alkami.com/spaces/SDKC/pages/82780222), 2020.2.0.9 (https://confluence.alkami.com/spaces/SDKC/pages/94996059), 2020.3.0.11 - Important! (https://confluence.alkami.com/spaces/SDKC/pages/89662270), 2020.4.0.13 (https://confluence.alkami.com/spaces/SDKC/pages/94996063), 2020.5.0.26 (https://confluence.alkami.com/spaces/SDKC/pages/95017716).

### 3.4 2021 to 2023 releases

- **2021.3.0.12**: `choco upgrade Alkami.PowerShell.IIS -y` (not in MachineSetup upgrades) so widgets auto-deploy on build. **2021.5.0.8**: new login workflow on by default; credentials and OTP are on Add the New Login Refresh Feature (https://confluence.alkami.com/sdk/add-the-new-login-refresh-feature-131006687.html). With the new workflow, challenge questions and the `DisableSecurityQuestions` bank setting are ignored unless you stop the authentication workflow service and revert to legacy login (June 2021 webinar).
- **2022.2.0.13**: after `choco upgrade Alkami.MachineSetup.SDK.Features -y;` fix startup types, and update templates for VS 2022:

```powershell
Set-SDKServiceStartupType -ServiceName @('Alkami.MS.MessageCenterManagement.Service.Host','Alkami.Services.Subscriptions.Host','Alkami.MS.Username.Service.Host') -StartupType 'Automatic'
choco upgrade Alkami.SDK.templates -y;
```

- **2022.4.x.x / 2022.5**: new installation process; follow the current Machine Setup page.
- **2023.1**: upgrade promptly to match Alkami Stage. If a stage-match site breaks, check IIS bindings and certificate selection; remove IIS sites named admin-developer.dev.alkamitech.com, developer.dev.alkamitech.com, ip.dev.alkamitech.com and keep WebClientAdmin, WebClient, IPSTS. Symitar: upgrade the SymConnect package. Troubleshooting: https://confluence.alkami.com/sdk/common-support-solutions-216493829.html.

Sources: 2021.1.0.8 (https://confluence.alkami.com/spaces/SDKC/pages/115509676), 2021.3.0.12 (https://confluence.alkami.com/spaces/SDKC/pages/135996232), 2021.5.0.8 (https://confluence.alkami.com/spaces/SDKC/pages/150653814), 2022.2.0.13 (https://confluence.alkami.com/spaces/SDKC/pages/189080667), 2022.4.x.x (https://confluence.alkami.com/spaces/SDKC/pages/217552340), 2022.5 (https://confluence.alkami.com/spaces/SDKC/pages/227902737), 2023.1 (https://confluence.alkami.com/spaces/SDKC/pages/255773408), 2021-06 Webinar June 2021 (https://confluence.alkami.com/spaces/SDKC/pages/145492299).

### 3.5 Widgets calling SymConnect: 2022.6 and later

- Use Alkami.MicroServices.SymConnectMultiplexer packages version 2.13 or later. Alkami.MicroServices.SymConnectMultiplexer.Utilities 2.13.0 also requires Alkami.MicroServices.Accounts.Contracts 2.27.0 (the page prose says "2.2.7", a typo; its packages.config sample pins 2.27.0) and Alkami.App.Providers.Core.Common.SymConnect 1.14.0 (stated minimums; the page's sample uses newer).
- The page carries the full working packages.config of the SampleSymConnect widget (net48): Alkami.Client and Alkami.Common 2023.1.0.2, Alkami.App.Multiplexer 1.7.0, Alkami.App.Providers.Core.Common.SymConnect 1.40.0, Alkami.MicroServices.Accounts.Contracts 2.27.0, Alkami.MicroServices.SymConnectMultiplexer.Client and .Contracts 2.14.0, .Utilities 2.13.0, Alkami.MicroServices.Settings.* 5.0.0, Alkami.WebToolkit 4.1.2, Microsoft.AspNet.Mvc 5.2.7, Newtonsoft.Json 8.0.3, among others.
- The .nuspec must ship the medium-versioned assemblies:

```xml
<file src="bin\Sample.Client.Widget.SymConnect.*" target="lib" exclude="**\*.config"/>
<file src="bin\Alkami.Microservices.SymConnectMultiplexer.*" target="lib" exclude="**\*.config"/>
<file src="bin\Alkami.Microservices.Accounts.Data.*" target="lib" exclude="**\*.config"/>
<file src="bin\Alkami.Microservices.Settings.Contracts.*" target="lib" exclude="**\*.config"/>
<file src="bin\Alkami.Microservices.Settings.Data.*" target="lib" exclude="**\*.config"/>
<file src="bin\Alkami.Microservices.Settings.Service.*" target="lib" exclude="**\*.config"/>
<file src="bin\Alkami.App.Providers.Core.Common.SymConnect.*" target="lib" exclude="**\*.config"/>
```

- `Alkami.App.Providers.Core.Common.SymConnect.Utility.IsSymConnectResponseSuccess(...)` no longer works the same way. Either adopt the full Common.SymConnect approach for RepGen calls or rely on the status code only:

```csharp
// old: HasError/StatusCode check combined with IsSymConnectResponseSuccess((ICallSymConnectResponse) response, true, out string moreDetails)
// new:
if (response.HasError || response.StatusCode != SymConnectStatusCodes.Success)
{...}
```

Sources: Updates To Widgets Calling SymConnect - 2022.6 and later (https://confluence.alkami.com/spaces/SDKC/pages/250893513).

### 3.6 Deprecation: BusinessAchProcessing MS

The backing service for Alkami.MicroServices.BusinessACHProcessing.Client and Alkami.MicroServices.BusinessACHProcessing.Data is deprecated by ORB 2025.1; the replacement endpoints on AchTemplates exist only from ORB 2024.3.
1. Remove references to Alkami.MicroServices.BusinessACHProcessing.Client and Alkami.MicroServices.BusinessACHProcessing.Data.
2. Add or update Alkami.MicroServices.AchTemplates.Service.Client and Alkami.MicroServices.AchTemplates.Contracts.
3. The same endpoints exist on the AchTemplates client. If the build breaks: request properties were altered, `Range` type references were removed, and filter properties were split into `Start` and `End`.

Sources: Deprecate BusinessAchProcessing MS (https://confluence.alkami.com/spaces/SDKC/pages/369626205).

### 3.7 2024.4

- Microsoft .NET 6.0 and .NET 8.0 SDKs are required (installers sdk-6.0.406-windows-x64 and sdk-8.0.400-windows-x64; recent VS 2022 may already include them).
- Check `clist subscriptions -lo`. If Alkami.Services.Subscriptions.Host is above v5.4.0: stop it in Services, run `choco install Alkami.Services.Subscriptions.Host --version 5.4.0 -fy`, start it, and set Automatic (Delayed Start). (2025.1 then moves it to 6.0.3.)
- Update tenant connection strings or newer services will not run migrations or register:

```sql
use AlkamiMaster
update dbo.Tenant set ConnectionString = ConnectionString + 'TrustServerCertificate=true;'
where not (ConnectionString like '%TrustServerCertificate=true;%')
```

- Condensed services: Alkami.MicroServices.StepUpManager.Service.Host, Alkami.MicroServices.StepUp.Alkami.Service.Host, Alkami.MicroServices.StepUp.Entrus.Service.Host are replaced by Alkami.MS.StepUp.Service.Host; Alkami.MicroServices.Risk.Management.Service.Host, Alkami.MicroServices.Risk.Alkami.Service.Host, Alkami.MicroServices.Risk.Entrust.Service.Host are replaced by Alkami.MS.Risk.Service.Host.
- Stage-match sites: manually upgrade core-specific choco packages.

Sources: 2024.4 (https://confluence.alkami.com/spaces/SDKC/pages/400211327).

## 4. Changelog page (2024.4 to 2025.2)

The Changelog page (updated 2025-05-29) lists package diffs per SDK release. Full lists are on the page; the developer-relevant entries:

**2024.4 to 2025.1.** New: Alkami.Services.Nexus.Host 2.3.2, Alkami.Services.Session 1.5.1, Alkami.MS.CoreProxy.Service.Host 2.5.0, Alkami.Sidekick 1.5.3. Removed: Alkami.MicroServiceTester (last 1.0.16), Internal.Services.Session (last 1.4.5). Updated: Alkami.MachineSetup.DatabaseCore and Alkami.MachineSetup.OrbCore 2024.4.0.9 -> 2025.1.0.6; Alkami.Services.Subscriptions.Host 5.4.0 -> 6.0.3; Alkami.MicroServices.Accounts.Service.Host 2.53.2 -> 2.55.0; Alkami.MicroServices.Transactions.Service.Host 1.25.2 -> 1.25.3; Alkami.MicroServices.Settings.Service.Host 5.1.7 -> 5.2.4; Alkami.MicroServices.EventManagement.Service.Host 3.11.0 -> 3.11.3; Alkami.MicroServices.Security.Service.Host 2.43.0 -> 2.44.0; Alkami.MicroServices.AchTemplates.Service.Host 2.140.234 -> 2.146.7; Alkami.MS.StepUp.Service.Host 1.33.6 -> 1.33.12; Alkami.MS.Risk.Service.Host 1.22.5 -> 1.24.3; Alkami.Modules.RiskEvaluation 2.20.0 -> 2.21.0; Alkami.Platform.DeveloperKit 1.0.18 -> 1.0.19; Alkami.PowerShell.SDK 1.4.4 -> 1.5.4; Alkami.PowerShell.Choco 3.40.1 -> 3.43.4; Alkami.PowerShell.Common 3.47.0 -> 3.50.1; Alkami.SDKRelease.Manifests 1.2.2 -> 1.3.2; Alkami.Api.OrbFX 1.54.2 -> 1.61.1; Alkami.Api.AFX 1.54.8 -> 1.61.2; Alkami.Api.CUFX 1.54.8 -> 1.61.4; Alkami.Client.Widgets.API 2.54.0 -> 2.61.1; Alkami.Apps.MyAccountsV2 3.22.6 -> 3.22.14; Alkami.Apps.DashboardV2 1.10.1 -> 1.11.2; Alkami.WebApps.Isotope 1.14.17 -> 1.15.0.

**2025.1 to 2025.2** (updates only). Alkami.MachineSetup.DatabaseCore and OrbCore 2025.1.0.6 -> 2025.2.0.12; Alkami.SDK.Samples 2.33.0 -> 2.34.2; Alkami.Services.Subscriptions.Host 6.0.3 -> 6.0.5; Alkami.MicroServices.Accounts.Service.Host 2.55.0 -> 2.55.2; Alkami.MicroServices.Transactions.Service.Host 1.25.3 -> 1.26.0; Alkami.MicroServices.AchTemplates.Service.Host 2.146.7 -> 2.149.10; Alkami.MicroServices.Notifications.Service.Host 1.13.4 -> 1.14.0; Alkami.MicroServices.Security.Service.Host 2.44.0 -> 2.45.0; Alkami.MicroServices.UserInterface.Service.Host 1.43.2 -> 1.48.3; Alkami.MS.CoreProxy.Service.Host 2.5.0 -> 2.6.0; Alkami.MS.Risk.Service.Host 1.24.3 -> 1.26.2; Alkami.Sidekick 1.5.3 -> 1.6.1; Alkami.PowerShell.SDK 1.5.4 -> 1.5.5; Alkami.SDKRelease.Manifests 1.3.2 -> 1.3.3; Alkami.Api.OrbFX 1.61.1 -> 1.62.4; Alkami.Api.AFX 1.61.2 -> 1.62.4; Alkami.Api.CUFX 1.61.4 -> 1.62.5; Alkami.Client.Widgets.API 2.61.1 -> 2.62.1; Alkami.MicroServices.Audit.Service.Host 6.76.3 -> 6.79.0.

Sources: Changelog (https://confluence.alkami.com/spaces/SDKC/pages/442832619).

## 5. Developer webinars

Each webinar page holds a slide deck (PDF) attachment; the Confluence text is only the topic list plus the notes below.

| Webinar | Topics | Notes on the page |
|---|---|---|
| 2019-05 | Conference report; use .NET 4.7.2 and VS 2019; RPSTS componentized in 2019.02; `choco install Alkami.SDK.Samples -y` installs to C:\AlkamiSDK; validate all input; use `account.DisplayAccountNumber`, not custom masking; new widget packaging | |
| 2019-06 | 2019.03 current; .NET 4.7.2, VS 2019; Microservice Tester | Slides only |
| 2019-07 | Frontend caching: ICache<T>, SessionCache | Sample code in Alkami.SDK.Samples 2.2.2 |
| 2019-09 | Training version 2019.04; Jira types (SDK Support Incident vs Alkami Support SP); add yourself as watcher; required Alkami.Broker 2.9.2, Alkami.ConfigurationDrivenObjects 4.4.2, updated Installers; widget deps Newtonsoft 8.0.3, Common.Logging 3.3.1, MVC 5.2.3 | |
| 2019-11 | SDK Submission Standard and Checklist; code freeze 12/13 to 1/12; reserving TCP ports for Subscriptions; step-up auth; VS 2019; 2019.06 widget and admin widget templates | |
| 2020-01 | 2020 required updates; roadmap (Admin widget config self-service, Proxy MS, SSO Provider / Slim MS / Check Imaging / eDocs VS templates, Project Albus, AWS SQS processor workflows); VS 2019; .NET 4.8; use NuGet packages, not c:\orb\shared, for reference assemblies | |
| 2020-03 | Async controller methods; Configurable Microservices and `GetScopeAsync(request)`; IDisposable | |
| 2020-05 | Check Imaging MS template; 2020.3 special steps; VS 2019; .NET 4.8; package sources; Slim Microservice template | Support ticket video https://www.youtube.com/watch?v=v8ziBm2gEEE |
| 2020-07 | SDK base vs Features package; 2020.4 changes; MS provider and widget configurations; troubleshooting Subscriptions; Generic Proxy MS | |
| 2020-10 | 2020.6; twice-monthly SDK deployments; Admin Package Manager; Jira/Confluence account suspension; no secrets in packages; remove unused code; native app testing | |
| 2021-02 | Project proposals; keep SDK current; Microsoft.AspNet.Mvc 5.2.7 allowed but then Alkami WebToolkit 4.x required; support process; widget area vs display names; renaming pitfalls; Windows Server 2016 tips | |
| 2021-04 | Project proposals live; testing native apps against Stage/Prod/SDK; admin rights; getting packages into production; no secrets in source | SQL and choco commands below |
| 2021-06 | Standard SSO config tool; Login Refresh; Snippets 2.0; Log4Net in MS; external accounts and card info; performance | DisableSecurityQuestions note (3.4) |
| 2021-10 | 2021 conference recap; Standard SSO (Jarvis); template updates; Snippets 2.0; Dashboard module dependencies; SQL Server Service Broker | JW Player example |
| 2022-03 | NuGet updates; Log4Net fix; new Snippets template; VS 2022; pre-compiled views; Iris 2 and Iris Vue; one DLL folder per widget package; do not use the ORB/Images folder | Pre-compiled views mostly cut web server CPU |
| 2022-07 | New SDK installer; Fortify scanner; Get-SDKSupport diagnostic tool; Nav Builder; remote access to the SDK | |
| 2022-10 | Pre-compiled views; Iris Vue; caching changes; AugmentRequest; PDFs in native apps; code freeze dates | |
| 2023-02 | AlkamiManifest.xml; including required DLLs in a widget package; matching service version to MS contract version; "New Nav"; choco installers; VS 2019 no longer supported; Accounts MS filters limited to 1000 IDs | Links to adding-or-updating-an-alkamimanifest-235930406, updating-your-local-navigation-builder-configurations-241342114, standard-sso-cert-config-install-241348346, sdk-project-proposal-using-the-feature-request-template-141532322 (under https://confluence.alkami.com/sdk/) |

April 2021 addenda (API and mobile apps in the SDK):

```sql
use DeveloperDynamic
Go
update dbo.RegisteredApplications set ClientKey = 'YourKeyHere', ClientSecret ='YourSecretHere'
where name = 'iOS_ORB' or name = 'ANDROID_ORB'
```

```
choco install Alkami.Api.OrbFX
choco install Alkami.Api.AFX
choco install Alkami.Api.CUFX
choco install Alkami.MicroServices.ApplicationSettings.Service.Host
# CUFX API also needs:
choco install Alkami.MicroServices.Payments.Service.Host
choco install Alkami.MS.AccountsOrchestration.Service.Host
```

Sources: Developer Webinars (https://confluence.alkami.com/spaces/SDKC/pages/60622901) and the individual webinar pages 61736511, 62697935, 67611060, 78848978, 78652172, 82783548, 87004561, 87026477, 95489599, 101794856, 114214715, 135990865, 145492299, 162736525, 190161914, 216465975, 232198884, 250892222 (https://confluence.alkami.com/spaces/SDKC/pages/<ID>).

## 6. Client conference developer tracks

### 6.1 2019 (playlist https://www.youtube.com/playlist?list=PLvJMPO8Jet6tc1TAfjfi3Zrpo5E5bjg9W; PDF and PPTX on each page)

**Extending Card Management** (Vinod Srinivasan, Trevor Brown). Prerequisites: `Alkami.Apps.CardManagementV2` plus its SQL script, `Alkami.MicroServices.CardManagement.Service.Host` (Prism), optional `Alkami.Admin.CardManagement`. Install `Alkami.Template.CardProvider` (also in Alkami.SDK.Templates), create the project (generates classes, provider install script, capability mapping script), implement `GetProviderCapabilities`, `GetCardNumbers`, `GetCards`, `InitiateCardAction`, run the provider SQL, run the MS from Visual Studio, configure the capability mapping table. Capabilities: GetCardNumbers, GetCards, ActivateCard, BlockCard, UnblockCard, EnableForeignTransactions, DisableForeignTransactions, RaiseLimit, LowerLimit, ReplaceCard, LostCard, StolenCard, MisplacedCard, DamagedCard, CloseCard, GetTravelNotices, AddOrUpdateTravelNotice, DeleteTravelNotice, SubmitBalanceTransfer, AddOrUpdateAlertRegistration, RemoveAlertRegistration, AlertsSso, RewardsSso, CardControlUrl, AddOrUpdateCardAlerts.

**Event Management and Notifications** (Paul Palmer). Template `Alkami.Template.EventProcessor`. Sub-pages:
- *Alkami.Microservices.Alerts*: proxy to Notifications that resolves contactIds to email/sms/push and logs for reporting. Categories: AccountAlert (extra `AccountId`), BillPaymentAlert, BudgetAlert, GeneralAlert, SecurityAlert, ThirdPartyAlert, TransactionAlert (extra `TransactionId`), UserAlert. Properties: `Payload` (same type as at template registration), `TemplateName` (registered with Notifications first), `OriginatorName`, exactly one of `DestinationUserId` / `DestinationUserIdentifier`, `ContactIds`, `FallbackToPrimaryEmail`, `TemplateTransformType`, `AlertDate` (UTC).
- *Alkami.Microservices.Notifications*: `RegisterTemplate` takes Template objects keyed by TemplateName, MediumType (Email, Sms, Push), LocaleId (1033, 21514), TemplateTransformType (Razor, XSLT). Razor templates must not declare `@model` (the model arrives as JSON turned into a dynamic; cast properties before use). Exclude the global header/footer with `@* ExcludeDefaultGlobalTemplate() *@` or `<!-- <xsl:exclude-global-template /> -->`. The "Sending a notification" section is empty on the page.
- *Alkami.MicroServices.EventManagement.Processors*: base classes for processor microservices. Trigger interfaces: `IEventProcessor` (`public Events EventType => Events.AddressChanged;`), `ICustomEventProcessor` (`public string CustomEventType => "SampleEvents::MyFirstEvent";`, names must be unique), `ICronProcessor` (`public string DefaultCronExpression { get => @"0 0 2 1/1 * ? *";}`); all inherit `IProcessorType` (`DefaultDisplayName`, `DefaultDescription`). Raise custom events with `EventingServiceFactory().TriggerCustomEventAsync(new TriggerCustomEventRequest { BankIdentifier, CorrelationId, CustomEventType, EventData, UserId, UserIdentifier })`; put the subscription `UserId` in `EventData` because BaseRequest properties are not passed to processors. Opt-in interfaces: `IIsNotificationProcessor` (`DefaultCategory`, `DefaultWidgetCategoryName`), `IValidateSubscription` (`ValidateUserSubscriptionAsync`), `IIsMandatory` / `IAllowsIsMandatorySetting`, `IHasExtendedDefinitions` (`GetExtendedDefinition` returning `ExtendedDefinition { AccountIds }`), `IDefinitionHasSections` (`Sections` of `Column` with DataType Account, Custom, Money), `IAdditionalSettings` (`AdditionalDefaultSettings`, `AdditionalDescriptors`, `Validate`), `IHasParentProcessor` (`DefaultParentProcessorName`; hidden from the settings widget), `ITimeToLive` (`TimeToLiveSeconds` setting). Every processor overrides `Task<bool> InternalExecuteAsync(DoWorkRequest request)`; Event Management checks only mandatory/subscribed, so account-level filtering must happen inside it (`request.MetaData.UserSubscription.UserId`).

**Core Integration** (Paul Sawey): core integration overview, referencing the core common NuGet package, building requests and core calls. Slides only.

**Click Click (Quick) Apply** (Brian Harboldt). Template `Alkami.Template.QuickApplyProvider`. Prerequisites: `choco install Alkami.Apps.QuickApply` plus its SQL (inserts `core.Widget` Name 'QuickApply', AssemblyInfo 'Alkami.Client.Widgets.QuickApply', IconName 'quick-apply' and `core.FlavorWidget` rows); `choco install Alkami.MicroServices.Audit.Service.Host` (must precede Quick Apply because of audit schema dependencies); `choco install Alkami.MicroServices.QuickApply.Service.Host`; `choco install Alkami.Admin.Quickapply`. The template implements `IQuickApplyProviderContract` with logic in the BusinessLogic folder; run the generated script in ProviderScripts, run the MS, configure Category and Product in Quick Apply Admin. Capabilities: GetJointOwnerAction, GetProductsAction, GetSupportedFieldsAction, OpenAccountAction (response carries Decision, AccountNumber, ConfirmationNumber), StatusAction (false hides the provider), ValidateProductAction.

**Iris and Vue, Two Peas in a Pod** (Lance Turri): Iris design-system components through Vue. **SignalR Integration** (Lance Turri, Justin Collins): filtering by user and account, connecting the JavaScript front end, subscribing to events and registering keys. Both slides only.

### 6.2 2021 and 2022

Recordings are on Alkami University (au.myabsorb.com) as "Alkami Co:Lab 2021 SDK Tutorials" and "Alkami Co:Lab 2022 SDK Tutorials". 2021: Alkami Architecture; Automated Testing for SDK; Standard SSO for SDK; Login Refresh: At Login Module; Native Debugging for SDK; Typescript 101. 2022: SDK for Non-Programmers; Customizing Transaction Disputes; Future Vision of HTTP Services; Building an SSO with Jarvis; Mobile Best Practices; SDK Configurable Settings; Creating Custom Alerts Using Event Management.

Sources: Client Conference (https://confluence.alkami.com/spaces/SDKC/pages/58461842), Client Conference 2019 - Developer Track (https://confluence.alkami.com/spaces/SDKC/pages/58461845), Extending Card Management (https://confluence.alkami.com/spaces/SDKC/pages/58463161), Event Management and Notifications (https://confluence.alkami.com/spaces/SDKC/pages/58463162), Alkami.Microservices.Alerts (https://confluence.alkami.com/spaces/SDKC/pages/58463163), Alkami.MicroServices.EventManagement.Processors (https://confluence.alkami.com/spaces/SDKC/pages/58463164), Alkami.Microservices.Notifications (https://confluence.alkami.com/spaces/SDKC/pages/58463165), Core Integration (https://confluence.alkami.com/spaces/SDKC/pages/62697492), Click Click (Quick ) Apply (https://confluence.alkami.com/spaces/SDKC/pages/62697501), Iris and Vue, Two Peas in a Pod (https://confluence.alkami.com/spaces/SDKC/pages/62697511), SignalR Integration (https://confluence.alkami.com/spaces/SDKC/pages/62697518), Client Conference 2021 - Developer Track (https://confluence.alkami.com/spaces/SDKC/pages/163197674), Client Conference 2022 - Developer Track and other sessions (https://confluence.alkami.com/spaces/SDKC/pages/189103631).

## 7. Hackathon materials (2024, 2025)

The 2024 Hackathon tree has three tracks (Simple SSO, Money Movement, Innovation) plus Get Started. The container pages and "Hackathon 2025 References" were not fetched.

**Before you start (Get Started):**

```
choco install Alkami.SDK.Templates --version 1.38.0 --source https://feeds.alkamitech.com/nuget/choco.dev/
choco install Alkami.SDK.Samples --version 2.31.0 --source https://feeds.alkamitech.com/nuget/choco.dev/
```

**Integrating a Microservice with Widget.** Add Alkami.MicroServices.Transactions.Client and Alkami.MicroServices.Transactions.Contract, then use the delegate factory pattern:

```csharp
using Alkami.MicroServices.Transactions.Contracts;
using Alkami.MicroServices.Transactions.Contracts.Requests;
using Alkami.MicroServices.Transactions.Contracts.Responses;
using Alkami.MicroServices.Transactions.Service.Client;

public static Func<ITransactionServiceContract> TransactionServiceFactory = () => new TransactionServiceClient();

var transRequest = new GetTransactionsRequest();
this.AugmentRequest(transRequest);
var transactionsResponse = AsyncHelper.RunSync(() => TransactionServiceFactory().GetTransactionsAsync(transRequest));
```

The widget must carry the service DLLs into its Chocolatey package: add `<file src="bin\Alkami.MicroServices.Transactions.*" target="lib" exclude="**\*.config"/>` beside the existing `<file src="bin\USBFI.*" target="lib" exclude="**\*.config"/>` in the .nuspec.

**Account MicroService.** Needs Alkami.MicroServices.Accounts.Service.Host in IIS and references to Alkami.MicroServices.Accounts.Contracts and Alkami.MicroServices.Accounts.Service.Client (IAccountServiceContract.cs, 2.45.0), plus Alkami.MicroServices.Security.Client and Alkami.MicroServices.Security.Contracts. Factories: `Func<IAccountServiceContract> accountsService = () => new AccountServiceClient();` and `Func<ISecurityServiceContract> securityService = () => new SecurityServiceClient();`. First get the user's accounts with `securityService().GetUserAsync(new GetUserRequest { UserId = CurrentUser.Id, Mapping = new UserMapper { ShouldIncludeUserAccounts = true, ShouldIncludeExternalUserAccounts = true, ShouldIncludePermissions = true, ShouldIncludeUserAccountPermissions = true } })`, check `IsDeleted`, read `Users.FirstOrDefault().UserAccounts`. Then `accountsService().GetAccountAsync(new GetAccountRequest { Filter = new AccountFilter { Ids = new List<long> { accountId }, IncludeExternal = true }, Mapping = new AccountMapper { IncludeAccountType = true, IncludeRoutingInfo = true, AccountMaskSettings = new AccountMaskSettings { JoinAccountHolderNumberFormatString = BankSettings.GetSettingOrDefault(BankSettingName.JoinAccountHolderNumberFormatString, "{0}{1}") } } })`. Always `this.AugmentRequest(request)` first. See Sample.Client.Widget.Accounts in SDK.Samples.

**Transaction MicroService.** Needs Alkami.MicroServices.Transactions.Service.Host in IIS (ITransactionServiceContract.cs, 1.25.0). Build `GetTransactionsRequest { Filter = new TransactionFilter { AccountIds, StartDate, DateSearchField = TransactionDateFields.PostingDate }, Mapping = new TransactionMapper { IncludeSplits, IncludeTransactionCategories, IncludeAdditionalInfo, IncludeTransactionType } }` and page with `MaxResults = 1000` and `Page` until `results.Count >= response.TotalResults`. See Sample.Client.Widget.GetTransaction in SDK.Samples.

**Event Management Overview.** Alkami.MicroServices.EventManagement.Service.Host is deployed as part of the Event Management compound and extended by processors (example: Security Alerts Processor) built from the Event Management Visual Studio Processor Templates. Event types: Broker (Login, Transfer Succeeded), Custom (WCF call to TriggerCustomEvent), Cron (one processor fires). Since 3.10.0 alert subscriptions live in Alkami.MS.MessageSubscription.Host (Event Management proxies for compatibility) and new development should use the newer Message Management processor template. Lifecycle (3.10.x): registrations in `event.Configurations`; `UseLegacySubscription` true uses `event.Subscription` and sends to Alkami.MS.EventManagement.Dispatcher; false uses `event.Configurations.WorkflowSteps` (null means Alkami.MS.MessageSubscriptions.Host then the Dispatcher). Install or upgrade (trailing period as written on the page):

```
choco install Alkami.MicroServices.EventManagement.Service.Host. -y
choco upgrade Alkami.MicroServices.EventManagement.Service.Host. -y
```

The sub-pages Event Management Processors, Event Management Template, and Event Management Processors - Test are include macros that rendered empty or as permission errors; the processor content is in 6.1.

**Project guides.**
- *Unified Rewards App (Simple SSO)* and *Digital Payment Dashboard (Money Movement)*: start Visual Studio as admin, use the Alkami Client Widget template, tick "Add Code for simple External Redirect" (SSO) or "Add Code for For Money Movement" (Iris Vue layout with unwired buttons), use bank identifier 78554577-9DE6-43CD-9085-5868977156D1 (DeveloperDynamic) if you have none. The template auto-runs install_widget.sql (run it manually if the DeveloperDynamic DB shows no row); a clean build in admin mode installs the widget. The SSO target is the `SsoURL` widget setting in Admin. Reference USBFI.*.MyMoney (`GetUserAccounts()`) and Sample.Client.Widget.GetTransactions.
- *Simple SSO Example Implementation*: node OAuth2 fake auth server (node-oauth2-server-example.zip attachment; `npm install`, `npm start`, http://localhost:3000). Token: `POST http://localhost:3000/oauth/token` with `grant_type=client_credentials` and `Authorization: Basic Y29uZmlkZW50aWFsQXBwbGljYXRpb246dG9wU2VjcmV0` (confidentialApplication:topSecret). SlimSSOService in SDK Samples (`GetAccessToken()`) builds to USBFI.MS.SlimSSOService.Service.Host; install with `choco install USBFI.MS.SlimSSOService.Service.Host -s <repo>\USBFI.MS.SlimSSOService.Service.Host\Installer`; expose the Client and Data packages via a local NuGet source; add `<file src="bin\USBFI.MS.*" target="lib" exclude="**\*.config"/>` to the widget .nuspec; use `Func<ISlimSSOServiceServiceContract> ssoServiceFactory = () => new SlimSSOServiceServiceClient();` and a `SimpleRedirect()` action that calls `GetAccessToken`, reads `GetWidgetSetting("SsoUrl")`, and redirects with `access_token` in the query string.
- *Dream Catcher (Innovation)*: start from "My First Microservice", "My First Client Widget", and the MyMoney sample (client widget, admin widget, provider service; Alkami microservices, GenericProxy service, Vue and Iris in mobile). No core connectivity at the hackathon.

Sources: Important - please read! (https://confluence.alkami.com/spaces/SDKC/pages/349145286), Integrating a Microservice with Widget (https://confluence.alkami.com/spaces/SDKC/pages/334082865), Account MicroService (https://confluence.alkami.com/spaces/SDKC/pages/334082867), Transaction MicroService (https://confluence.alkami.com/spaces/SDKC/pages/334082869), Event Management Overview (https://confluence.alkami.com/spaces/SDKC/pages/334082872), Event Management Processors (https://confluence.alkami.com/spaces/SDKC/pages/334082901), Event Management Template (https://confluence.alkami.com/spaces/SDKC/pages/334082917), Event Management Processors - Test (https://confluence.alkami.com/spaces/SDKC/pages/428578204), Project Guide - Unified Rewards App (https://confluence.alkami.com/spaces/SDKC/pages/334082859), Example Implementation (https://confluence.alkami.com/spaces/SDKC/pages/349144988), Project Guide - Digital Payment Dashboard (https://confluence.alkami.com/spaces/SDKC/pages/339890564), Project Guide - Dream Catcher (https://confluence.alkami.com/spaces/SDKC/pages/339890580), Hackathon (https://confluence.alkami.com/spaces/SDKC/pages/334085806), 2024 Hackathon Instructions (https://confluence.alkami.com/spaces/SDKC/pages/325355525), Hackathon 2025 References (https://confluence.alkami.com/spaces/SDKC/pages/470391837).
