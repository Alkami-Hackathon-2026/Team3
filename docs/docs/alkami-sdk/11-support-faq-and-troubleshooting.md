# Support, FAQ, Common Errors, and Code Snippets

**What this covers.** How to get help from the Alkami SDK team (support email, Jira issue types, what a ticket must contain), the complete SDK FAQ condensed, common error messages and fixes, the SDK team's "Common Support Solutions" runbook, the Microservice Update Guide for the newer package-validation rules (AlkamiManifest.xml and the Service installer), common code snippets, and the documentation-file and team pages at the top of the SDK Confluence space. Source pages date from 2022 to 2026; out-of-date items are flagged.

---

## 1. Getting support

- Email **sdksupport@alkamitech.com**, or (preferred) open an **SDK Support Incident** in Alkami's Jira (https://jira.alkami.com). The SDK team opens any further internal tickets so the incident stays the single reference. Video: https://www.youtube.com/watch?v=v8ziBm2gEEE. The Support page attaches `Alkami_SDK_Support_Workflow.mp4` ("SDK Support Workflow (new in 2021)").
- Search the Confluence space first.
- Staging problems: SDK Support Incident with full detail; the team can pull staging logs. The SDK team has **no** access to stage/production servers for environmental problems (e.g. oAuth); open a general support issue for those.
- Local run-time errors: paste and complete this checklist in the Description: detailed explanation and steps to reproduce; ORB browser error: attach WebClient and WebClientAdmin log files; service error: attach the service log file(s); ORB Version from https://developer.dev.alkamitech.com/version.txt.
- General questions: describe the larger project's requirements.

Sources: Support, https://confluence.alkami.com/spaces/SDKC/pages/62695666

---

## 2. SDK issue types in Alkami's Jira server

Developers need access to their Client Services or Delivery project in Alkami's Jira. Atlassian/Extranet credentials come from the Jira server; reset passwords at https://jira.alkami.com ("Reset Password"), not from the Extranet.

**SDK Submission** (submit a Chocolatey package for review and scheduled deployment):

| Field | Description |
|---|---|
| Summary | At minimum package name and version. |
| SDK Component Type | "Widget" or "Microservice" most commonly; also "Provider", "Snippet", "Repository". |
| FI Contact / FI Contact Info | Contact and phone/email for escalation. |
| Package URL | URL of the hosted package (see "Add a package to your development feed"). **A malformed or incorrect URL makes automation cancel the submission; a new one is required.** |
| Scheduled Release Time | Set by Alkami automation; comment on the submission to change it. |

**SDK Support Incident** (any SDK question or issue): Summary; Description (detailed: screenshots, repro steps, logs, stack traces; **one issue per ticket**; vague tickets are bounced for more info); FI Contact / FI Contact Info.

Also referenced in the FAQ: a **Support (SP)** ticket for production-side requests (enable production logging, change a deployed widget's DisplayName); a **CCR** (change request via your Account Executive / Client Success Manager) for professional-services work (environment syncs, custom icons, custom fonts, NativeDisplaySetting in upper environments).

Sources: SDK Issue Types in Alkami's JIRA Server, https://confluence.alkami.com/spaces/SDKC/pages/67609047; FAQ, https://confluence.alkami.com/spaces/SDKC/pages/62695668

---

## 3. FAQ (all questions, condensed; page last updated 2022-12-22)

### Environment, installation, tooling

- **SiteText "Detected NULL key for sitetext with Default Value []"**: the key's database entry is null. Inspect the SiteTexts column (query as written on the page, non-standard syntax: `SELECT SiteTexts FROM [AlkamiMaster].[sitetext].[Namespace] like 'Sample.Client.Widget.ChangeAddress'`) and fix badly formatted items.
- **Widget settings not updating after the Widget Settings admin screen**: 15 minute cache. Stage/prod: change, wait 15 minutes, member logs out and in. Locally: recycle the **CoreService** in IIS Manager.
- **Certificate store private key permission errors on install**: set IIS Application Pool identities to run as a local user.
- **VS will not authenticate with the feed server**: known VS NuGet bug; restart VS.
- **NuGet package sources** (Tools > Options > NuGet Package Manager > Package Sources). Alkami projects: do **not** enable public sources (nuget.org, Microsoft Visual Studio Offline Packages); use `https://feeds.alkamitech.com/nuget/nuget.dev` and `https://feeds.alkamitech.com/nuget/ThirdParty`; a local folder such as `c:\LocalPackageFeed` is handy for packages you build. Non-Alkami projects: `https://api.nuget.org/v3/index.json`, `C:\Program Files (x86)\Microsoft SDKs\NuGetPackages`, Alkami sources off. A per-project `nuget.config` with `<packageSources><clear /></packageSources>` overrides machine-wide sources.
- **Build error "C:\xxxxx : The term 'C:\xxxxx' is not recognized as the name of a cmdlet..."**: a space in the path (e.g. `c:\xxxxx sdk`). **SDK project directory and project names must not contain spaces.**
- **"Cannot load Counter Name data because an invalid index was read from the registry"** on microservice start: cmd, `cd c:\windows\SysWow64`, `lodctr /r`.
- **Microservice crashes when debugging in VS**: run VS as Administrator; `Alkami.Services.Subscription.Host` must be running; the service being debugged must not also be running as a Windows service (stop it in Services).
- **Subscriptions service will not stay started**: confirm port reservations (bottom of Hardware and Software Requirements, https://confluence.alkami.com/display/SDKC/Hardware+and+Software+Requirements); reinstall the latest `Alkami.Services.Subscriptions.Host` and reboot; else try an older version (3.3.9).
- **Expired SDK certificate**: reinstall `Alkami.DeveloperKit.Certificates.Wildcard.Dev`.
- **Microservices Tester**: `choco install Alkami.MicroServiceTester` (desktop shortcut or search "microservicetester"). **Deprecated** from 2025.1 in favor of Alkami Sidekick (section 5).
- **Upgrade the SDK**: Alkami Platform Machine Setup (https://confluence.alkami.com/display/SDKC/Alkami+Platform+Machine+Setup).
- **Sync local with staging/production**: possible via snapshot scripts; requires a CCR (Account Manager / Delivery Project Manager).
- **Connect the SDK to a test core**: SDK Support Incident with "Request for stage-match database scripts". Requires a working stage environment; Alkami attaches a zip of scripts and helps run them.
- **Let coworkers reach your SDK site**: hosts entries (`C:\Windows\System32\drivers\etc\hosts`) or DNS, e.g. `192.168.1.10 ip.dev.alkamitech.com`, `192.168.1.10 developer.dev.alkamitech.com`, `192.168.1.10 admin-developer.dev.alkamitech.com`. Stage-match site (e.g. usbfi.dev.alkamitech.com): `ip.dev.alkamitech.com`, `usbfi.dev.alkamitech.com`, `admin-usbfi.dev.alkamitech`.
- **Test a widget in the native mobile app**: the QA build of the app accepts a custom URL (https://developer.dev.alkamitech.com). Requires the CUFX packages (search choco.dev for 'CUFX') and device DNS resolution of `developer.dev.alkamitech.com` (proxy or controlled DNS).
- **Detect the environment in code** (widget needs the `Alkami.Utilities` package): `if (ApplicationConfiguration.Environment.Type == EnvironmentType.Production) { ... }`. Simulate locally with `<add key="Environment.Type" value="Production" />` in `<appSettings>` of `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Config\machine.config`, or admin PowerShell `Set-AppSetting "Environment.Name" "Development";`.
- **Unlock a user**: admin site > Support > Members > click "Locked". Not needed for 2021.6 and later; the page also has a SQL fallback that sets `core.ItemSetting` `%InvalidCredentialCount%` to '0' and `core.Users.islockedout = 0` for the user's `STSID` item.
- **Remove a widget from the SDK**: the SDK Samples package includes a cleanup SQL script (the FAQ also attaches `RemoveWidgetScriptTemplate_v2.sql`); run it against the right database, then delete the widget folder from `c:\orb\WebClient\areas` and/or uninstall its Chocolatey package.
- **Package Assignments in the SDK admin site**: `choco install Alkami.MicroServices.Rules.DataSheet.Service.Host -y; choco install Alkami.MicroServices.Rules.Package.Service.Host -y; choco install Alkami.MicroServices.Rules.Service.Host -y; choco install Alkami.Modules.PackageAssignment -y`
- **MFA / standard Settings widget in SDKs before 2020.03**: WebClient log `Could not find a compatible service for Alkami.MicroServices.StepUpManager.Contracts.IStepUpManagerServiceContract.` Upgrade to 2020.03 or install `Alkami.MicroServices.StepUp.Alkami.Service.Host`, `Alkami.MicroServices.StepUpManager.Service.Host`, `Alkami.Microservices.ExtendedProperties.Service.Host`.
- **2FA / step-up in a widget**: install choco `Alkami.Modules.RiskEvaluation` 2.1.0, `Alkami.Modules.LegacyAuthClient` 1.1.1, `Alkami.Modules.LegacyAuthAdmin` 1.1.1, `Alkami.MicroServices.Risk.Management.Service.Host` 1.4.1, `Alkami.MicroServices.Risk.Alkami.Service.Host` 1.4.1, `Alkami.MicroServices.Audit.Service.Host` 6.7.0. In the widget: NuGet `Alkami.RiskEvaluation.WebExtension` 2.1.0; remove the old `<div id="second_factor" ...>@Html.RenderPartial("~/Areas/API/Views/SecondFactorAuthentication/(Mobile)SecondFactorAuthentication.cshtml")</div>` block from desktop and mobile views; include `@Html.IncludeSiteTextScript()`; replace the `shouldprompt2fa` call with a risk request for your ActionType, e.g. `var request = new ChangePasswordRiskRequest(){ /* optional SessionInformation = new Dictionary<string, string>() */ }; if (!this.IsAllowedByRiskEvaluation(request)) { return null; }`. Ship these DLLs in bin and the choco package: `Alkami.RiskEvaluation.WebExtension.2.1.dll`, `Alkami.MicroServices.Risk.Contracts.1.4.dll`, `Alkami.MicroServices.Risk.Data.1.4.dll`, `Alkami.MicroServices.Risk.Data.Validations.1.4.dll`, `Alkami.MicroServices.Risk.Service.Client.1.4.dll`.
- **Firewall/endpoint access from a microservice**: SDK Support Incident. Alkami public IPs: https://confluence.alkami.com/display/SRE/Alkami+Technology+Public+IP+Addressing.
- **Install a microservice locally as a Windows service**: https://confluence.alkami.com/spaces/SDKC/pages/92224158.
- **Add settings to a microservice**: make it a "provider based service" (https://confluence.alkami.com/spaces/SDKC/pages/64946486).

### Widget development

- **Detect Iris loaded**: `document.addEventListener('iris.init.end', function() { console.log("Iris has finished loading."); });`
- **SMTP**: yes, standard setup.
- **Native mobile development**: not possible; SDK widgets are server-side MVC widgets in app web views. No location or push services. No custom fonts in the native app (mobile web views: theme Change Request).
- **jQuery upgrade**: official jQuery guides.
- **FlashMessage / FlashError customization**: not possible; use Iris notifications (https://iris.alkamitech.com/web/components/notification.html). No mobile `FlashSuccessMessage()`; use Iris.
- **"Make sure that the controller has a parameterless public constructor"**: extra assemblies deployed into the widget's Area/bin; WebClient loads all of them and a duplicate aborts loading. **Set "Copy Local" = False on all library references.**
- **Display-name collision with an Alkami widget**: widget name and routing are always prefixed with a unique identifier; display names need not be unique. Same display name means the SDK widget replaces the standard one (which is disabled); otherwise rename.
- **Claims references broke my widget**: never reference claims classes. Good (value-type-only, stable): `Alkami.Client.Framework.Mvc.BaseController` `EntityIdentifier`, `IsBusinessUser`, `BusinessName`, `CurrentBankId`, `CurrentBankName`, `CurrentMasqueradeIdentifier`, `CurrentThemeName`, `CurrentAdminDisplayName`, `CurrentUserIdentifier`, `HashedUserIdentifier`, `CurrentUserLocaleId`, `CurrentUserTimeZoneInfo`, `CurrentUserName`, `ImagePortraitSignature`, `CurrentUserDateTime`, `GetLocaleId`; `Alkami.Security.Common.IClaimsUtility` `CurrentBankId`, `CurrentBankIdentifier`, `CurrentBankName`, `CurrentBankTimeZoneInfo`, `CurrentUserTimeZoneInfo`, `CurrentThemeName`, `CurrentThemeId`, `IsAuthenticated`, `CurrentUserId`, `CurrentUserIdentifier`, `CurrentUserIPAddress`, `CurrentUserLocaleId`, `CurrentUserDisplayName`, `CurrentMasqueradingIdentifer`, `CurrentAdminDisplayName`, `CurrentUserDateTime`, `HasAccountPermission`, `CurrentEntityIdentifier`, `CurrentClientApp`, `CurrentUserIsBusinessMasterUser`, `CurrentUserIsBusinessSubUser`, `HasEntityPermission`, `HasBusinessRolePermission`. **Bad**: `CurrentClaimsIdentity`, `CurrentClaimsToken` (on both), `IClaimsUtility.CheckAccess`, `IClaimsUtility.AddAccountClaimFromService`. Missing abstraction: email sdksupport@alkamitech.com.
- **Force 2FA on custom OAuthController endpoints**: not supported; talk to your Account Manager.
- **Widget links open desktop in mobile web view/app**: the widget must define its own desktop vs mobile redirects; mobile views link only to mobile views.
- **Custom widget icons**: check Iris iconography (https://iris.alkamitech.com/foundations/iconography.html) first. Otherwise attach a simple black-and-white glyph sample to an SDK Support Incident; Alkami builds a second FI-specific icon font loaded with the theme. Needs a CCR, has a nominal cost, ships with the next theme release.
- **Change icon/settings of a deployed widget**: since 2019.06 self-serve in admin > Settings > "Widget Settings" (DisplaySettings is on the Widget Registration screen). **DisplayName is not self-serve**: Support (SP) ticket plus CCR.
- **Widget settings**: example in `C:\AlkamiSDK\Samples\MyMoney\ClientWidget`. Base controller collection `UserWidgetSettings` merges global and per-user settings. **Code can write only the current user's settings, never global ones.**
- **Help button/content**: install `Alkami.Admin.WidgetSettings` (Setup menu or https://admin-developer.dev.alkamitech.com/WidgetSettings); add widget setting `help_content_name`; Content > Manage > Help: create a record with that System Name (pick the language). Wait for cache; `iisreset` may be needed.
- **SiteText**: examples in `C:\AlkamiSDK\Samples\MyMoney\ClientWidget` and `C:\AlkamiSDK\Samples\Widgets\Sample.Client.Widget.SiteText`. Encode HTML in SiteText XML (`&lt;`, `&gt;`); Razor re-encodes, so use `@Html.Raw(Html.SiteText("MyKey"))`.
- **Detect "View as User"**: `if (CurrentMasqueradeIdentifier != null) { /* VAU active */ }`.
- **Nest widgets**: not directly; render a partial view from one widget inside another (Dashboard Modules do this).
- **Widget not displaying after build**: install SQL script run in commit mode; latest Installer.Widget package; check build output (missing required .nuspec field); .nuspec includes `<file src="bin\.....` (see `nuspec.changes.readme`); build in Debug (Clean first if Release); rebuild; check the Areas folder / DLL timestamps; full restart: `IISRESET /stop`, delete the "root" directory in ASP.NET Temp files, `IISRESET /start`.
- **Platform fonts on another site**: `@font-face { font-family: 'Alkami'; src: url("https://YourFiName.orb.alkamitech.com/stylesheets/fonts/Alkami-font.woff") format("woff"), url("https://YourFiName.orb.alkamitech.com/stylesheets/fonts/Alkami-font.ttf") format("truetype"); }` and likewise `'AlkamiLogos'` from `AlkamiLogos.woff` / `AlkamiLogos.ttf` (write `@@font-face` in Razor).
- **Printing in mobile/native**: generate a PDF server-side and send it; web views cannot be reliably printed in native apps.
- **HttpClient**: consider RestSharp (Alkami NuGet feed) or https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong.
- **Upgrade a widget to MVC 5**: 1) `Alkami.Webtoolkit`, `Alkami.WebToolkit.Extensions`, `Alkami.Webtoolkit.Scripts` to 2.0.0; 2) `Microsoft.AspNet.Mvc` 5.2.3; 3) `Microsoft.AspNet.WebPages` 3.2.3; 4) uninstall `Mvc3Futures` (ask Architecture if `Microsoft.AspNet.Mvc.Futures` 5.0.0 is still needed); 5) uninstall `MvcContrib.Mvc3-ci`; 6) delete any web.config under Views.
- **SSO: microservice required?** Traditional token-exchange SSO (confidential data sent server-to-server, token used in a pop-up/iFrame): **yes**. Only the app tier (microservices) can reach the Internet or your data center; widgets in the web tier are firewalled from everything outside Alkami. Merely opening a frame with non-sensitive query-string parameters: widget only. Sensitive data with the frame approach needs encryption or a scripted form post; compliance decides. Where funds or PII are reachable (billpay) the microservice approach is the only acceptable one.

### User, account, session data

- **User email**: `var user = CustomerRepository.Get(); var emailContact = user.GetUserContact(x => x.IsPrimary).FirstOrDefault() ?? user.GetUserContact().FirstOrDefault(); var emailAddress = emailContact.Email;` (`ICustomerRepository CustomerRepository` property, `using Alkami.Client.Services.Bank.Repository;`). Still null-check.
- **CUFX user token**: standard oAuth; AFX Development (https://confluence.alkami.com/spaces/SDKC/pages/55346039).
- **"Member Number"**: core-dependent; inspect `CurrentUser` and accounts in the debugger on a stage-match DB. See also section 7 (ExtendedProperties).
- **Client browser IP**: `IPAddressUtility.GetUserIpAddress(IPAddress.Loopback).ToString()` (never read Request directly; default "127.0.0.1").
- **Business master user**: `if (u.Entity != null) isMasterBizUser = u.Entity.MasterUserIdentifier == u.UserIdentifier;` (null Entity = retail).
- **Username, session id, sub-user** (declare `public static Func<IClaimsUtility> ClaimsFactory = () => new ClaimsUtilityWrapper();`): `CurrentUser.LoginId`; `ClaimsFactory().CurrentClaimsToken.SessionId`; `claims.CurrentUserIsBusinessSubUser`; `claims.CurrentUserIsBusinessMasterUser`.
- **Force an account sync for new core accounts** (account-opening only, never "just in case"): `AccountRepository.Get(new GetAccountsParameters(){ IsNewAccountSync = true }); SessionSecurityTokenUtility.ReissueSessionSecurityToken();`
- **Account Relationship Types**: `Unknown = 0`, `PrimaryOwner = 1`, `JointOwner = 2`, `Linked = 3`, `Aggregated = 4`, `BusinessAccount = 5`.
- **Account type**: `AccountRepository.Get(new Alkami.Client.Services.Bank.Models.GetAccountsParameters { PermissionNames = new List<string> { PermissionNames.ViewSummary } })`, then per account use the "Is" properties, e.g. `a.IsDemandDepositAccount` (checking), `a.IsAutoLoanAccount`.
- **`core.FlavorWidget.NativeDisplaySetting`** bit mask: `IosPhone = 0x01`, `IosTablet = 0x02`, `AndroidPhone = 0x04`, `AndroidTablet = 0x08`; 15 = all, 3 = iOS only. Not self-serve in stage/prod (professional services via Client Success Analyst).

### Packaging, versioning, deployment, access

- **Bank identifier**: SDK Support Incident.
- **Changing an image or .css**: always a new package and submission.
- **Packaging docs**: Alkami.Installer.Widget and You (https://confluence.alkami.com/display/SDKC/Alkami.Installer.Widget+and+You); Alkami.Installer.Provider and You (https://confluence.alkami.com/display/SDKC/Alkami.Installer.Provider+and+You).
- **Update the version**: latest installer package; set `sem.ver`, `.nuspec` (if hardcoded), and `assemblyinfo.cs`; build. **Keep package and assembly versions in sync.**
- **Which version is deployed**: put `version.txt` in the widget root; check the stage/production feeds at https://feeds.alkamitech.com (an open SDK Submission may mean the latest feed version is awaiting a window).
- **Source for standard components**: `choco install SdkSrc --source https://feeds.alkamitech.com/nuget/choco.dev/`. Standard widget source: latest `Alkami.SDK.Samples`, then `C:\AlkamiSDK\Samples\ExampleSource\Alkami.Client`; else SDK Support Incident.
- **Ready for production**: Zero Downtime release process (https://confluence.alkami.com/sdk/zero-downtime-release-schedule-114208190.html); comment on your SDK Submission. ORB deployment dates: ask your AE (Lane/Pod).
- **Breaking changes**: regression-test on the latest SDK (dedicated VM); a broken widget gets a **new major version** (v1.0.8 to v2.0.0), standard review, timing coordinated with the AE and SDK teams. **Hotfix** while v2.0.0 waits in Stage: submit v1.0.9 with special approval straight to Production.
- **Logs in Alkami environments**: no direct access. Stage: SDK Support Incident with search criteria and date/time. Production: Support (SP) ticket to enable logging, test, request research.
- **Add an employee to feeds.alkamitech.com**: SDK Support Incident with full name, email, role (developer only / can submit packages), requested by a recognized manager or someone with that access.
- **Supported mobile OS**: https://midas.alkami.com/downloads/browser-and-device-support-policy/ (April 2020: Android 6.0+, iOS last 2 major).

Sources: FAQ, https://confluence.alkami.com/spaces/SDKC/pages/62695668

---

## 4. Common error messages

- **Server Error 500**: usually a missing widget dependency. WebClient log: `An error occurred when trying to create a controller of type '...Controller'. Make sure that the controller has a parameterless public constructor. ---> System.Reflection.ReflectionTypeLoadException`. Check the nuspec: every bin dependency must be listed with `target="lib"` (all bin files in that single folder, else deployments are incomplete). The reference `USBFI.Client.Widget.MyMoney` nuspec includes `tools\chocolateyInstall.ps1` and `tools\chocolateyUninstall.ps1` (target `tools`), `AlkamiManifest.xml`, the source tree to `src` (excluding obj, .vs, bin, packages, .nuget, .git, node_modules, tests), then `bin\USBFI.*`, `bin\Alkami.MS.GenericProxy.*`, `bin\Alkami.RiskEvaluation.WebExtension.*`, `bin\Alkami.MicroServices.Risk.*`, `bin\Alkami.Microservices.Accounts.*`, `bin\Alkami.Microservices.Security.*`, `bin\Alkami.Microservices.Transactions.*`, `bin\Alkami.Microservices.CardManagement.*`, `bin\Alkami.Microservices.CardManagementProviders.*`, `bin\Alkami.MS.UserContacts.*` (all `target="lib" exclude="**\*.config"`), and content `Scripts`, `Styles` (excluding `*.scss`), `Views`, `Images`, `_SiteText` (excluding `*.xx.xml`) to `content\Areas\App`.
- **Server Error 503**: WebClient app pool stopped; restart in IIS Manager.
- **Cannot log in after fresh install/upgrade** (Developer Dynamic or Stage Match): `Alkami.MicroServices.Authentication.Workflow.Service.Host.log` shows `There was no endpoint listening at net.tcp://scheduler/SchedulerService/SchedulerService.svc`. Fix: force-reinstall the **same version** of `Alkami.App.Service.Scheduler.Host` already installed.
- **Release build: `'HtmlHelper<dynamic>' does not contain a definition for 'CssWithCacheExpiration'`**: Release pre-compiles views; add to the top of every `.cshtml`:

```cshtml
@using Alkami.Client.WebClient.Shared.Helpers
@using System.Web.Mvc.Html
@using Alkami.Client.Framework.Utility
```

Sources: Common Error Messages, https://confluence.alkami.com/spaces/SDKC/pages/216477733

---

## 5. Common support solutions (SDK team runbook, updated 2026-03-23)

### PowerShell, feeds, tooling

- **`Get-SDKSupport` not recognized**: attach `clist alkami -lo` output; fix with `choco upgrade Alkami.Powershell.SDK -y`.
- **Reset feeds / feed password**: `nuget sources remove -name "Alkami Dev"`, `nuget sources remove -name "Alkami Third-Party"`, then `New-SDKMachineSetup` (prompts for username, i.e. email for non-Alkami staff, and feeds password).
- **Private NPM feed**: `npm config set @alkami:registry=https://feeds.alkamitech.com/npm/npm.dev/ --location global` plus a user `.npmrc` (else 401):

```
@alkami:registry=https://feeds.alkamitech.com/npm/npm.dev
//feeds.alkamitech.com/npm/npm.dev:always-auth=true
//feeds.alkamitech.com/npm/npm.dev:_auth=<Base64 of username:password>
```

- **Unable to resolve dependency for OpenTelemetry.Instrumentation.Wcf**: `nuget sources update -name "Alkami Third-Party Core" -username emailAddress -password secret`; in the NuGet manager (source "All", "Include prerelease") install `OpenTelemetry.Instrumentation.Wcf` **v1.12.0-beta.1** then `Alkami.OpenTelemetry.Instrumentation.Wcf` **v1.0.9** in the test and host projects; uncheck prerelease.
- **Microservice Tester shows no services**: deprecated. Use Alkami Sidekick (in the SDK from 2025.1): `choco install Alkami.Sidekick -y`; guide https://confluence.alkami.com/sdk/using-sidekick-to-test-microservices-378932012.html
- **Debug a widget**: Debug > Attach to Process, "Show processes for all users", filter `w3wp`, attach to user `IIS_APPPOOL\WebClient`.
- **Cannot debug a Generic SSO template service**: uncheck "Prefer 32-bit" on the Host project, rebuild.
- **Request/response logs locally**: in the SymConnect (or other core) `log4net.config` after the last appender add `<logger name="Alkami.MicroServices.TracedMessages" additivity="false"><level value="TRACE" /><appender-ref ref="Messages" /></logger>`.

### WebClient / Admin site

- **Dev Admin inaccessible (System.Memory in stack trace)**: stale unversioned DLLs in both the shared folder and `webclient/bin` / `webclientadmin/bin`. After 2022.6: keep newer `Alkami.Contracts.dll` and `System.Memory.dll` in the shared folder and delete them from `c:\orb\webclientadmin\bin`.
- **New Navigation not showing**: restart `Alkami.Services.NavigationOrchestration`; the Session service (`Intenal.Services.Sesssion` as printed on the page) **must** run before it, else the legacy Left Navigation shows. Check `C:\Orb\WebClient\s3\navigations\{BankIdentifier}\3107\1033` exists. Adding widgets to new nav: https://confluence.alkami.com/sdk/updating-your-local-navigation-builder-configurations-241342114.html
- **No theme / auth form missing**: enable IIS **Static Content** (Windows Features).
- **500 on DashboardV2 after update**: `update core.FlavorWidget set NativeDisplaySetting = 0 where NativeDisplaySetting is null`.
- **Legacy login screen (username only)**: ensure `Alkami.MicroServices.Authentication.Workflow.Service.Host` is running; restart, wait 1 minute. After upgrading to **2025.4** with WebClient log `Authentication Workflow ms returned error ... Failed to get isotope definitions for bank url`: `clist Settings -lo`; if `Alkami.MicroServices.Settings.Service.Host` is **5.5.21**, stop it and run `choco install Alkami.MicroServices.Settings.Service.Host --version 5.2.4 -fy`, set Automatic, start.
- **Login bounces back with "session expired"**: `choco upgrade Alkami.MicroServices.Authentication.Workflow.Service.Host -y` (v1.40.1 at writing), set Automatic, start, wait 30 seconds.
- **CloudFlare Turnstile failing on an EC2 SDK exposed externally** (local SDK only): admin > Setup > Integration Settings > Providers > `AuthenticationWorkflow`, uncheck `TurnstileEnabled`, save, `Restart-SDKServices`.

### Windows services and certificates

- **"No Provider Type found from..."**: run `insert_provider_setting.sql` from the Host project's `ProviderScripts` folder (older projects: Host root). Also after a hard reset or new machine.
- **`Alkami.MicroServices.Broker.Host` will not start on EC2 (Permissions Denied)**: with the Alkami Issued Token / Mutual Client / Mutual Service / RPSTS certs present, run `choco install Alkami.platform.developerkit`. If it fails with `ERROR: This does not go on the REDx and QA servers...`, edit `C:\ProgramData\chocolatey\lib\Alkami.Platform.DeveloperKit\tools\ChocolateyInstall.ps1`, comment out lines 344 and 356-358, run `.\ChocolateyInstall.ps1` as admin from that folder, then `Restart-SDKServices`.
- **SDK install failures on AWS EC2 Server 2019 Datacenter (new nav)**: comment out the `Test-IsAWS` conditional in that same `chocolateyInstall.ps1` (lines 349 and 367 per the page) and run it in PowerShell ISE (Admin); `Stop-IISAndServices`; empty `C:\ProgramData\Chocolatey\lib-bkp` and `lib-bad`; delete all Alkami packages from `C:\ProgramData\Chocolatey\lib` **except** `Alkami.Installer.*`, `Alkami.Powershell.*`, `Alkami.Services.Subscriptions.Host`, `Alkami.Orbital`, `Alkami.Platform.DeveloperKit`, `Alkami.MachineSetup.DatabaseCore`, `Alkami.MachineSetup.OrbCore`; `Install-SDKRelease -Latest`; `choco install Alkami.SDk.NavBuilder -y`; `ALTER TABLE feature.Flag ADD IsOverrideAllowedBelowPackageLevel bit NULL;`; insert a `feature.Flag` row (`CategoryName = 'Internal.Navigation'`, `Name = 'EnableNavigationBuilder'`, `IsEnabled = 1`, `IsOverrideAllowed = 1`, `IsOverrideAllowedBelowPackageLevel = NULL`, dates `GETDATE()`); restart `redis-master`; refresh and log in.
- **Subscriptions "Access is denied", Event Viewer: TCP port 50001 in use**: port reservations missing (fresh install or after `Remove-SDK -Hard`). `netstat -ano | findstr 50001`, kill the PID (check 12345 too), then `netsh int ipv4 Add excludedportrange protocol=tcp startport=12345 numberofports=2` and `netsh int ipv4 Add excludedportrange protocol=tcp startport=50000 numberofports=30`.
- **Many services "Access is Denied" while Subscriptions runs**: `NetTcpPortSharing` is broken. Uninstall it (Windows Features), reboot, reinstall, reboot; then `Restart-SDKServices` if not all services (excluding radium) start.
- **Service running as "Local System" instead of "Local Service"**: `clist workflow -lo` for the version; `Invoke-SCExe @('delete', 'Alkami.Microservices.Authentication.Workflow.Service.Host')` (single quotes required); delete the package folder under `C:\ProgramData\chocolatey\lib`; `choco install Alkami.Microservices.Authentication.Workflow.Service.Host --version 1.33.1 -y`; set Automatic, start, wait 30 to 60 seconds.
- **Repeated "Bad user name" failures from `CN=Alkami Mutual Client`**: MMC Certificates (Computer account) > Alkami Mutual Client > Properties > enable only **Server Authentication**. Repeat for other Alkami certs that start erroring.

### Stage-match database

- **Stage-match WebClient login fails (DevDynamic and Admin work)**: `Alkami.App.Bank.Host` log `The account type 'YCK' could not be located for the user with identifier`. Admin > Account Type Mappings > +Add each missing type with values from the real stage Admin.
- **Bank Host error: missing `DisplayNameSetByUser`**: the migration only runs on DeveloperDynamic. Run the page's script against the stage-match DB in commit mode (it ships with `ROLLBACK TRAN`): it adds `DisplayNameSetByUser bit not null` (default 0, constraint `DF_UserAccount_DisplayNameSetByUser`) to `core.UserAccount` and recreates `dbo.vw_UserAccount` including that column (one variant for `BillPayAccountKey`, one for `BillPayAccountKeyDeprecated`).
- **Stage-match Admin: "Exception calling Authorization Microservice: Incorrect number of users returned"**: the third stage-match script ("3 - users and registration...sql") was not run or `[user].useridentity` lacks the update. The page's script (edit DB name and REPLACEME values; it contains a literal `admin.CrescentBank.dev`): zero `WeightForAdminRegistration`, `WeightForRegistration`, `IsRequiredForRegistration`, `IsUsedDuringRegistration` in `core.CoreProviderRegistrationField` where `RegistrationType NOT IN (21,4)`; set `core.itemsetting` 'REGISTRATION FIELD WEIGHT THRESHOLD' and 'REGISTRATION FROM ADMIN FIELD WEIGHT THRESHOLD' to '0' and `QAChallengeCount` to '2'; replace `admin.developer.dev` with `admin.<site>.dev` in `core.ItemSetting` (Name 'STSID'), `core.STSID`, and `[user].useridentity.STSID`; set `[notification].[DefaultTemplates].BankIdentifier` from `core.bank` where it equals '78554577-9DE6-43CD-9085-5868977156D1'.
- **"The socket connection has been disposed"** on stage-match Admin: re-run the **second** stage-match script in commit mode.
- **Registration fields missing in Admin** (`The provider named "" could not be found for bank '<BankIdentifier>'`): query `core.Provider where ProviderTypeID = 2`, its `core.Item` rows (`ItemType = 'Connector'`), their `core.ItemSetting`, and `core.CoreProviderRegistrationField`; make `CoreProviderRegistrationField.CoreProviderID` match the primary core's `core.Item` ID. Confirm provider packages are installed (records survive a soft reset).
- **Registration issues in a stage match**:
  - *Wrong fields*: `Select * from lookup.UserIdentifyingField uif where uif.UserIdentifyingFieldGroupId = (Select Id from lookup.UserIdentifyingFieldGroup where Name = 'Admin_RetailRegistration_default')`; `SELECT * FROM lookup.UserIdentifyingFieldType`. Delete: `Delete from lookup.UserIdentifyingField Where IdentifyingFieldType in (12,17) and UserIdentifyingFieldGroupId = <UserIdentifyingFieldGroupId>`. Change Email (22) to MemberNumber (21): `Update lookup.UserIdentifyingField Set IdentifyingFieldType = 21 Where IdentifyingFieldType = 22 and UserIdentifyingFieldGroupId = <UserIdentifyingFieldGroupId>`. **Always include `UserIdentifyingFieldGroupId`.**
  - *"Unable to locate the specified record"*: check the `Alkami.Microservices.UserLookup.Service.Host` log (core connectivity/config).
  - *Fails after the member is found*: `Alkami.MS.Registration.Service.Host` log `Error when trying to create user on IDP for ...`. List providers: `select * from core.[Provider] prov join core.ProviderType provtype on prov.ProviderTypeId = provtype.ID join core.Item item on prov.Id = item.ParentId where provtype.[Name] = 'Registration'`. Typically Entrust has `IsDeleted = 0` and "Alkami Registration Service" is deprecated. Run the page's script, which inserts (if missing) `core.ProviderType` 'Registration', `core.Provider` 'Alkami Static Registration Service' with AssemblyInfo `Alkami.MicroServices.Registration.Static.Service.Host.DistributedStaticRegistrationService, Alkami.MicroServices.Registration.Static.Service.Host`, and a `core.Item` Connector row (SecondaryId = bank Id, Version '1.0.0.0'). Then `Update core.item Set Deleted = 1 Where name like '%Alkami Entrust Registration%'`, restart `Alkami.MS.Registration.Service.Host`, wait a minute.

Sources: Common Support Solutions, https://confluence.alkami.com/spaces/SDKC/pages/216493829

---

## 6. Microservice Update Guide (deployment validation rules, 2024-01-11)

Alkami's deployment process now validates packages; failing submissions are cancelled.

**"Manifest Error: No AlkamiManifest.xml was detected in the root".** `AlkamiManifest.xml` must be at the **root of the Chocolatey package**. For existing projects replace the Host project Pre-Build event with:

```
if $(ConfigurationName) == Release (
if exist "$(ProjectDir)InstallerOverrides\$(ProjectName).nuspec" (
xcopy "$(ProjectDir)InstallerOverrides\$(ProjectName).nuspec" "$(ProjectDir)Installer" /Y
)
if exist "$(ProjectDir)AlkamiManifest.xml" (
xcopy "$(ProjectDir)AlkamiManifest.xml" "$(SolutionDir)$(SolutionName)" /Y
)
)
```

and add `<file src="..\..\AlkamiManifest.xml" target="AlkamiManifest.xml" />` to the Host nuspec files section (sample nuspec on the page: `..\bin\release\*` to `tools`, `*.ps1` and `*.psm1` to `tools`, `..\InstallerOverrides\*.ps1` to `tools`, the manifest, and the source tree to `src`; tag `ConfigurableMicroservice`).

**"The chocolateyInstall.ps1 file tools/ChocolateyInstall.ps1 does not match the install file template so this submission will be cancelled."** Packages must use the **Service installer** rather than the Logic installer. For projects still referencing `Alkami.MicroServices.Installer.Logic` / `Alkami.MicroServices.Choco.Installer.Logic`:

1. Open the Host project.
2. Click "Show All Files".
3. Drag `chocolateyinstall.ps1` and `chocolateyuninstall.ps1` from `Installer` to `InstallerOverrides`.
4. Delete `ChocolateyBeforeModify.ps1`, `Config.ps1`, `Library.ps1` from `Installer`.
5. Delete `configureInstaller.ps1` from the Host root.
6. In the Host nuspec: change `<file src="..\bin\release\*" target="tools" />` to `target="app"`; add `<file src="..\AlkamiManifest.xml" target="AlkamiManifest.xml" />`; remove `<dependency id="Alkami.MicroServices.Choco.Installer.Logic" version="2.4.8" />` (or `Alkami.MicroServices.Installer.Logic`).
7. Manage NuGet Packages on the Host project.
8. Installed tab, search "installer".
9. Uninstall `Alkami.MicroServices.Installer.Logic` if present.
10. Drag the two ps1 files back to `Installer`.
11. Open both.
12. Ignore the "do not modify" header; replace contents with `process { & C:\ProgramData\Alkami\Installer\Services\install.ps1 $PSScriptRoot; return; }` (install) and `process { & C:\ProgramData\Alkami\Installer\Services\uninstall.ps1 $PSScriptRoot; return; }` (uninstall).
13. Host project > Properties > Build Events.
14. Pre-Build: `if $(ConfigurationName) == Release ( if exist "$(ProjectDir)AlkamiManifest.xml" ( xcopy "$(ProjectDir)AlkamiManifest.xml" "$(SolutionDir)$(SolutionName)" /Y ) )`. Post-Build: `if "$(ConfigurationName)" == "Release" choco pack $(ProjectDir)Installer\$(ProjectName).nuspec -OutputDirectory $(ProjectDir)Installer`. Save All.
15. Rebuild in **Release**. **Bump the version.**
16. Open the Host folder in File Explorer.
17. Open the `.nupkg` in `Installer` with 7-Zip; view `tools\chocolateyinstall.ps1` and `chocolateyuninstall.ps1`.
18. Verify they match step 12; resubmit.

Sources: Microservice Update Guide, https://confluence.alkami.com/spaces/SDKC/pages/273428030

---

## 7. Common code snippets (page updated 2026-08-13; code verbatim)

**Force an account refresh from the core** (a just-created account must be used immediately):

```csharp
// Make sure that you have the appropriate declarations in the controller
using Alkami.Client.Services.Bank.Models;
using Alkami.Client.Common.Utility;

...

AccountRepository.Get(new GetAccountsParameters() { IsNewAccountSync = true });
SessionSecurityTokenUtility.ReissueSessionSecurityToken();
```

**Current member identifier from the primary core:**

```csharp
// Within a Provider Service
user.CoreProviderUsers.FirstOrDefault(x => x.IsPrimary).MemberIdentifier

// Within a Widget
var member = CustomerRepository.Get();
var coreIdentifier = member.Model.CoreIdentifier;
```

**Master Account Identifier (validate sub-user access):** `var masterUserIdentifier = CurrentUser.Entity.MasterUserIdentifier;`

**Paged transaction results** (the service enforces an upper limit; 1000 may be passed in `request.MaxResults`):

```csharp
var results = new List<Transactions>();
var page = 0;

TransactionResponse response;
var transactionService = TransactionServiceFactory();

do
{
transRequest.MaxResults = 100;
transRequest.Page = page;
response = await transactionService.GetTransactionsAsync(transRequest);

if (response.Transactions.Count > 0)
{
results.AddRange(response.Transactions);
}

page++;
} while ((response.Transactions.Count > 0) && (results.Count < response.TotalResults));

return results;
```

**Client IP address (limit an SSO token to that IP):** `IPAddressUtility.GetUserIpAddress(IPAddress.Loopback).ToString()`

**Bank name from Core.Bank:**

```csharp
var bankInfo = BankInfoRepository.Get(CurrentUrl());
var bankName = bankInfo.Model.Name;
```

**Current member's package (flavor):**

```csharp
// Add the functional delegate after the instantiation of the logger at the top of the controller
public static readonly Func<ICoreManagementServiceContract> coreManagementService = () => new CoreManagementServiceClient();

// Add this code to get the current logged in members assigned package name
var flavors = coreManagementService().GetBankFlavors("", new Paging());
var userFlavor = flavors.Results.Where(x => x.Id == (long)CurrentUser.FlavorId).Select(x => x.Name).FirstOrDefault();
```

**Extended Properties via the microservice.** Key names vary per FI; inspect `XmlData` with `SELECT TOP (1000) [EntityId],[EntityName],[XmlData] FROM [dbo].[ExtendedProperties] where EntityName = 'User'`. Add NuGet `Alkami.Microservices.ExtendedProperties.Client` and `Alkami.Microservices.ExtendedProperties.Contracts` to the widget, then:

```csharp
using Alkami.Microservices.ExtendedProperties;
using Alkami.Microservices.ExtendedProperties.Contracts.Filters;
using Alkami.Microservices.ExtendedProperties.Contracts.Requests;
using Alkami.Microservices.ExtendedProperties.Contracts.Responses;
using Alkami.Microservices.ExtendedProperties.Data;

...

public ActionResult Index() {
...
var memberNumber = GetMemberNumber();
var externalKey = GetExternalKey();
...
}

public static class Constants
{
public const string ExternalKey = "ExternalKey";
public const string MemberNumberKey = "MemberNumber";
}

protected string GetMemberNumber()
{
string memberNum = string.Empty;
var extPropInfo = GetExtendedPropertiesInfo;
try
{
if (extPropInfo != null)
{
memberNum = extPersInfo.ItemList.Find(x => x.Key.EqualsIgnoreCase(Constants.MemberNumberKey))?.Value;
}
if (memberNum.IsNullOrEmpty())
Logger.Error($"Member number is empty for this member. {CurrentUser.UserIdentifier}");
}
catch (System.Exception ex)
{
Logger.Error($"Exception at GetMemberNumber Method.", ex);
}
return memberNum;
}

protected string GetExternalKey()
{
string extKey = string.Empty;
var extPropInfo = GetExtendedPropertiesInfo;
try
{
if (extPropInfo != null)
{
extKey = extPersInfo.ItemList.Find(x => x.Key.EqualsIgnoreCase(Constants.ExternalKey))?.Value;
}
if (extKey.IsNullOrEmpty())
Logger.Error($"External Key is empty for this member. {CurrentUser.UserIdentifier}");
}
catch (System.Exception ex)
{
Logger.Error($"Exception at GetExternalKey Method.", ex);
}
return extKey;
}

protected GetExtendedPropertiesResponse GetExtendedPropertiesInfo
{
get
{
long userId = CurrentUserId.Value;
Logger.Debug("GetExtendedPropertiesInfo...");

var request = new GetExtendedPropertiesRequest
{
Filter = new ExtendedPropertyFilter
{
Ids = new List<long> { userId },
Type = ExtendedPropertyType.User,
Keys = new List<string> { Constants.MemberNumberKey }
}
};
this.AugmentRequest(request);
Logger.Trace($"GetExtendedPropertiesInfo Request: {request}");

using (ExtendedPropertiesServiceClient extendedPropertiesServiceClient = new ExtendedPropertiesServiceClient())
{
var response = AsyncHelper.RunSync(() => extendedPropertiesServiceClient.GetAsync(request));
Logger.Trace($"GetExtendedPropertiesInfo Response: {response}");
if (response.HasError)
{
Logger.Error($"Error at GetExtendedPropertiesInfo extendedPropertiesServiceClient method.");
return null;
}
else
{
if (response.ItemList.Any())
{
return response;
}
}
}

return null;
}
}
```

Note: as written on the page the methods reference `extPersInfo` while the local is `extPropInfo`, and `Keys` includes only `MemberNumberKey` although `GetExternalKey` also reads the result; correct both when adapting.

Sources: Common Code Snippets, https://confluence.alkami.com/spaces/SDKC/pages/334074343

---

## 8. Documentation files and other resources

The page (2022-08-26) holds attachments only; their contents are not in the transcript.

- Alkami Platform Overview for SDK Developers; Notes and tips for using the SDK; Alkami Browser and Device Support Policy; Send Notifications Business-to-Business API Product Guide (attachments).
- **Remove unneeded widgets from `core.widget`**: the initial SDK database has `core.widget` rows for unused widgets or ones absent from the "areas" folder, causing many WebClient startup log errors. Attachment `RemoveUnusedWidgetsFromSDK.sql` removes them.
- **Adding New Contract Methods Classes** (example `.cs.txt` attachments for extending a microservice contract): contract classes `LoanDecisionDetail`, `MemberApplicationDetail`, `GetLoanDecisionRequest`, `LoanDecisionResponse`; validators `MemberApplicationDetailValidator`, `GetLoanDecisionRequestValidator`; contract `IMyMoneyServiceContract`; client `MyMoneyServiceClient`; host `DistributedService`; implementation `ServiceImp.LoanDecision`.

Sources: Documentation Files And Other Resources, https://confluence.alkami.com/spaces/SDKC/pages/135989539

---

## 9. SDK team

| Name | Role |
|---|---|
| Ritesh Mishra | Director of Platform Engineering (Extensibility, Core) (older Support page: Senior Engineering Manager) |
| Allen Cheslik | Product Manager |
| Bryan Green | Team Lead, Staff Software Engineer |
| Alex Pham | Staff Software Engineer |
| Hillman Chen | Sr. Staff Software Engineer |

Use sdksupport@alkamitech.com or an SDK Support Incident rather than individual addresses.

Sources: Meet the SDK Team, https://confluence.alkami.com/spaces/SDKC/pages/132708438; Support, https://confluence.alkami.com/spaces/SDKC/pages/62695666
