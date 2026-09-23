# CLAUDE.md: Alkami SDK development

This repo holds Alkami SDK projects (widgets, snippets, microservices, SSO providers) that extend the Alkami digital banking platform ("ORB"). Everything here is .NET Framework 4.8, Visual Studio 2022, Windows only, built with Alkami's VS templates and NuGet/Chocolatey packages from Alkami's private feeds. The SDK runs on a local Windows VM with the full platform installed; it does not run on macOS.

Detailed reference material lives in `docs/alkami-sdk/` (15 topic docs distilled from Alkami's SDK Confluence space, with source page URLs). Read the relevant doc before doing non-trivial work; this file is the short version. The raw Confluence page exports are in `docs/alkami-sdk/source/` if you need to grep the original text.

## Fill in for this FI

- Well-known identifier (Jira Client Services / Delivery project prefix): `<FI>` (replace everywhere; drop a trailing `AM` if the Jira project code ends in it). Ask sdksupport@alkamitech.com if unsure.
- Local SDK version: check `C:\Orb\WebClient\version.txt`.
- Core: Symitar/SymConnect unless noted otherwise.

## Which doc to read

| Task | Read |
| --- | --- |
| Platform concepts, how widgets talk to services, CurrentUser, SymConnect, business banking | `01-platform-overview-and-architecture.md` |
| Installing/upgrading the local SDK, feeds, ports, hosts file, PowerShell cmdlets, stage match | `02-machine-setup-and-local-environment.md` |
| Naming, folder layout, nuspec, logging, testing, crypto rules | `03-coding-guidelines-and-standards.md` |
| Building a client/admin widget, snippet, dashboard module, widget header, nav builder | `04-widgets-snippets-and-modules.md` |
| Building a microservice, contracts/client/host, settings, AlkamiManifest, Sidekick, GenericProxy | `05-microservices.md` |
| Standard SSO (Jarvis) config JSON, expressions, JWT/SAML, template, testing | `06a-...` (tool/template) and `06b-...` (2026 service, admin, REST) |
| Transfers, card management, 2FA/risk, check imaging, ACH/wires, Quick Apply, AFX/CUFX | `07-feature-integrations-and-samples.md` |
| Submission checklist, package pins, Jira/One Click flow, deployment windows, release process | `08-submission-and-release-process.md` |
| Front-end: Iris, `Alkami.*` JS API, widget header, Chart.js, polyfills | `09a-frontend-api-and-iris.md` |
| Vue widget structure and testing; Albus build tool | `09b-vue-project-guide.md`, `09c-albus-build-tool.md` |
| Alkami Embedded (Flutter), native app testing, PDFs in native | `10-alkami-embedded-and-native-apps.md` |
| Errors, FAQ, reusable code snippets, support process | `11-support-faq-and-troubleshooting.md` |
| Release history, breaking changes, deprecations | `12-releases-changelog-and-events.md` |

## Platform model (30 seconds)

- ORB = ASP.NET MVC web apps (`C:\Orb\WebClient`, `C:\Orb\WebClientAdmin`) + legacy WCF/IIS services + many standalone Windows-service microservices, over SQL Server (`AlkamiMaster` for tenant routing, `DeveloperDynamic` as the local tenant DB) and Redis. Logs go to `C:\OrbLogs`.
- The SDK adds to the platform, it never modifies it. Extension points: client widgets (MVC Areas), admin widgets, snippets (widget extensions), dashboard modules, microservices (provider-based or logic), SSO providers. A feature is usually a widget + microservice pair.
- Widgets call microservices through generated clients; requests carry user/bank context. Always `this.AugmentRequest(request)` in a widget controller before calling a service, and `req.CopyBaseFrom(request)` when a microservice calls another.
- Dev loop: build in VS (run as Administrator), deploy to the local ORB, restart IIS web processes (sometimes clear ASP.NET temp files), refresh the browser. `Alkami.Services.Subscriptions.Host` must be running or no microservice will start.
- SDK code never reads or writes `AlkamiMaster`/`core.*` tables directly. Persistent config goes through Settings (provider settings, widget settings, bank settings).

## Hard rules (these fail code review or submission)

1. Prefix every solution, project, namespace, assembly, package id and title with the FI identifier. `Alkami.*` names are for illustration only; a submission named `Alkami.Client.Widget.X` is cancelled.
2. Never put `Mobile` in a project or widget name (breaks Alkami routing). Widget names: letters and digits only, no spaces, periods, underscores; a name cannot start with a digit. No spaces anywhere in project or directory names (breaks the build).
3. 64-bit only: `Platform target = Any CPU`, `<Prefer32Bit>false</Prefer32Bit>` in Debug and Release. One build target, `AnyCPU`.
4. Target .NET Framework 4.8 (required for Alkami Release 2020.1+). Some older template output targets 4.7.2; bump it.
5. Packages come only from `https://feeds.alkamitech.com/nuget/nuget.dev` and `https://feeds.alkamitech.com/nuget/ThirdParty` (Chocolatey: `https://feeds.alkamitech.com/nuget/choco.dev`). Never nuget.org, never Microsoft Offline Packages. Alkami reference assemblies may not be more than two minor versions behind the deployment target.
6. Widget `bin` (and the package `lib`) contains only: your own assemblies, the client/contracts/data/validations assemblies of microservices you call, and custom assemblies not already in ORB. Set every other reference to Copy Local = false. Never ship anything from `C:\Orb\shared` (`Alkami.Common`, `Alkami.Ioc.dll`, `Newtonsoft.Json.dll`, `Alkami.Services.Subscriptions.*`, `Alkami.MicroServices.Settings.ProviderBasedClient.dll`). Duplicate assemblies are the number one cause of the 500 "controller has no parameterless public constructor" error.
7. Every `IDisposable` in a `using`. Every service call in `try/catch` with error-result validation (`response.HasError`, validation results) and exception logging.
8. Security: no public-internet calls from widget code (route through a microservice or GenericProxy with a whitelisted host); no PII in URLs or query strings; no secrets, keys or connection strings in source, `appsettings`, or SQL scripts (read from configuration/settings); never override TLS validation; TLS 1.2+ in transit, AES-256 at rest; no `console.log` left in shipped JS; never log passwords, tax IDs, card numbers or PINs unmasked (`SanitizeData()` / `MaskCardNumbers()` from `Alkami.Utilities`).
9. Never reference `CurrentClaimsIdentity`, `CurrentClaimsToken`, `IClaimsUtility.CheckAccess`, or `IClaimsUtility.AddAccountClaimFromService` from widget code. Use the value members exposed on `BaseController` (`CurrentUser`, etc.).
10. Discouraged and flagged in review: mixing sync and async, `ViewBag`, `Html.SiteText` inside `Html.Raw`, hard-coded configuration. DI is LightInject.
11. Do not edit `chocolateyInstall.ps1` / `chocolateyUninstall.ps1` generated by the installer packages, and do not hand-edit a built `.nupkg` (OTS bankIdentifier edits are the one documented exception).
12. Versions: `sem.ver` and `Properties/AssemblyInfo.cs` (`AssemblyVersion`/`AssemblyFileVersion`) stay in sync, fourth segment always `0`, semver (major = breaking, minor = feature, patch = fix or resubmit). Every submission needs a version never used before, and the compiled DLL version is what gets checked, so rebuild after bumping.

## Widgets

- Naming: `<FI>.Client.Widget.<Slug>` (admin: `<FI>.Admin.Widget.<Slug>`, snippets contain `.Client.Snippet.`). `RootNamespace`, `AssemblyName`, csproj, sln and nuspec id all equal that name. `<Slug>` is the URL segment and the Areas folder name.
- Required: `WidgetDescription.cs` (class `WidgetDescription : Alkami.Client.Framework.Mvc.WidgetDescription`; `Name` MUST return the slug, `Title` is display text), `<Slug>Controller` (optional `Base<Slug>Controller` for shared logic, `Mobile<Slug>Controller` for mobile), `Views\<Slug>\Index.cshtml`, `Views\<Slug>\Mobile\Index.cshtml`, `Error.cshtml`, `_SiteText\<name>.sitetext.en.xml`, `AlkamiManifest.xml`, `tools\` from `Alkami.Installer.Widget`, `.nuspec`. Admin widgets use `Area.cs` instead of `WidgetDescription.cs`.
- Folders: `Controllers`, `Models`, `Views`, `Scripts` (TypeScript preferred, served as `/<Slug>/Scripts/...`), `Styles`, `Images`, `_SiteText`. Return `View()` or `View("Name")`, never a full path. `WidgetSummary.cshtml` is legacy. No `web.config` in Views.
- Controllers inherit the Alkami base controller and get `CurrentUser` (`UserId`, `FirstName`, `LastName`, `Email`, `TaxId`, `FlavorId`, `GetUserContact<T>()`). Shared logic in the base controller; desktop and mobile controllers only delegate. Models are plain data.
- Registration: the widget must exist in `core.Widget` before its DLL loads. Locally run `Tools\install_widget.sql` (switch from `ROLLBACK` to `COMMIT`). `AlkamiManifest.xml` `displayName` must equal `@WidgetDisplayName` in that script; `widgetInstall` is `Client|Admin|Generic`; `displaySettings` is `Desktop|Mobile|Tablet|DesktopMobile|All`. `DisplaySettings` bits: Hidden 0, Desktop 1, Mobile 2, Tablet 4, All 7.
- Pre-compiled views: never delete `.cshtml` files (routing still needs them).
- Snippets: need `Alkami.Utilities.WebToolKit.Snippets.Runtime` 2.1.0+, desktop only, cannot extend BillPay. Snippets 2.0: no `WidgetDescription.cs`, add `SnippetsAreaRegistration.cs`, `[FlavorAuthorize("AreaName")]` on every controller, inherit the snippets base controller, register in `ui.Snippets`. Return a static `ContentResult` from Index to avoid per-request recompiles.
- Dashboard modules are desktop-only partial views registered in Admin > User Interface Settings > Modules.
- Iframes: reset the idle timeout with `window.parent.postMessage('idletimeout-reset')` (ORB 2021.05+); the `setInterval` sample is demo-only. Do not implement the deprecated "Login Refresh" page.
- SiteText: all user-facing strings through SiteText (`Alkami.Localization.SiteText.get(key, args)` in JS), including Spanish (`.es.xml`).

## Microservices

- Naming: `<FI>.MS[.Processor].<Component>[.<Purpose>][.<Vendor>]` (no `.Service`, no `Alkami`, under 64 chars, watch the 260-char path limit). The csproj name is the Chocolatey package id. Display name (`configurator.SetDisplayName`) drops `.MS`, `.Service`, `.Host` and the word Microservice: `<FI> <Component> [<Purpose>]`.
- Template choice (`choco install Alkami.SDK.Templates -y`, 1.11.0+): Provider Service when admins need configurable settings; Logical Service for no persistence; Slim Logical for a minimal service + client; Single Sign On Service for custom SSO; also Event Processor, Facts Provider, Card Management, Quick Apply templates.
- Layout the template generates: `Contracts` (`[ServiceContract]` interface with `[OperationContract] Task<TResponse> XAsync(TRequest)`; `Requests`/`Responses` deriving from `Alkami.Contracts.BaseRequest` / `BaseResponse<T>`), `Data` (`[DataContract]` POCOs, `[DataMember(EmitDefaultValue = false)]`, `ProviderSettings\SettingNames`), `Validations` (`EntityValidatorImpl<T>`), `Service` (`ServiceImp`, partials `.Headers.cs` with `StaticName`/`StaticProviderType`/`StaticProviderName` and `.SettingManagement.cs` with `DefaultSettings()`/`SettingDescriptors()`/`ValidateChangedSetting`), `Service.Client` (`ProviderBasedClient<IContract>`, each method `ProxyCall((op, inner) => op.XAsync(inner), request)`), `Service.Host` (`DistributedService : ProviderBasedService<IContract, ServiceImp>`, `Program.cs`, `Installer` with nuspec, `ProviderScripts`), `Tests`.
- Adding an operation means three edits: Contracts interface, `ServiceClient.cs`, `DistributedService.cs`.
- `OnStart()` must call `EntityValidator.AddValidator(...)`, `Alkami.Broker.ZeroMq.Setup.PublishUsingZeroMqLocally()`, `SubscribeUsingZeroMqLocally(...)`, `Alkami.Broker.App.Subscription.InitializeSubscriber()`, then `base.Start()`.
- Client `ProviderType` == `StaticProviderType` in `ServiceImp.Headers.cs` == `@providerTypeName` in the provider SQL script == `<provider>` in `AlkamiManifest.xml`. Never change `ItemType` ("Connector") on a configurable service. Read settings with `using (var scope = await GetScopeAsync(request)) { scope.GetSettingOrDefault<string>(SettingNames.X); }`.
- `app.config`: `NewRelic.AppName` must equal the service name (never `REPLACEME`) and `NewRelic.AgentEnabled` must not be false. Host assembly major.minor must match the Contracts assembly (HostVersionCheck).
- `AlkamiManifest.xml` at project root and package root (`<file src="../AlkamiManifest.xml" target="/" />` for services). `<version>` is always `1.0` (manifest schema version). Delete empty `<dependencies>` and `<migrations>` elements. `bankIdentifier` is a bare GUID. Generate with `New-AlkamiManifest -Type Service|Widget|Provider`, check with `Test-AlkamiManifest -f <path>` (must print `True`).
- Local run: `Service.Host` as startup project, VS as Administrator, `Alkami.Services.Subscriptions.Host` running. A choco-installed copy of your service auto-starts and blocks F5; stop or uninstall it first. Test with Sidekick (`choco install Alkami.Sidekick -y`; Microservice Tester is deprecated as of 2025.1).
- Wiring a widget to your service: add the Client/Contracts(/Data) packages, add `<file src="bin\<FI>.MS.X.*" target="lib" exclude="**\*.config"/>` lines to the widget nuspec, call through a `Func<IContract>` factory (`() => new XClient()`), `AugmentRequest` first, wrap sync MVC actions with `AsyncHelper.RunSync(() => ...)`.
- External HTTP from the platform goes through your microservice or `Alkami.MS.GenericProxy.Service.Host` with the host whitelisted in Admin > Setup > Integration Settings > Providers > GenericProxy (production whitelists need Networking/security approval). Secrets live in settings, not code.
- SymConnect: use `MultiplexerClient` via `Alkami.MicroServices.SymConnectMultiplexer.*` (>= 2.13 on 2022.6+, 2.15 recommended, all on the same version) with `Alkami.MicroServices.Accounts.Contracts` >= 2.27.0; check `response.HasError || response.StatusCode != SymConnectStatusCodes.Success`. `CallCustomAPIRepository.CallCustomAPI` is legacy.

## Standard SSO

- Prefer Standard SSO (config-driven JSON in `sso.Configuration`, engine `Alkami.MicroServices.StandardSSO.Service.Host`, widget `Alkami.Client.Widget.StandardSso`) over a custom SSO microservice whenever the partner accepts GET/POST, form post, SAML (Alkami is always the IdP, IdP-initiated only) or JWT with predefined fields. Write a custom Generic SSO Provider (`ISsoProvidersContract`: `GetOptions`, `GetSso`, `CanProcess`) only when logic cannot be expressed in config.
- Config: `BankIdentifier`, `ProviderName` (also the widget slug `StandardSso/<ProviderName>`; mismatch gives `InvalidJson: Configuration was not provided`), `HttpRequest` (`DisplayLocation` Inline|NewWindow, `Uri`, `Method`, `ContentType`, `FormFields`, `Headers`, `QueryParameters`), optional `Services[]` (Http/Saml/Jwt), `Methods`, `AccountConfiguration`, `AccountLoop`.
- Expressions: `{{ FirstName }}` style, no space between the paired braces (`{ {` breaks), whitespace inside the expression is fine, everything is a string, no conditionals, `@Name` references a Method. Wrap nullable inputs with `Coalesce(...)` or evaluation throws.
- JWT: HS256/384/512 (secret >= 32/48/64 bytes), RS/PS/ES (need `CertificateThumbprint`). SAML: signing cert is an Alkami-generated `.pfx`, encryption cert is the vendor's public cert; new SAML integrations must set `EncryptionParameters.KeyEncryptionAlgorithm` to `http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p`. Never share certs across FIs. Never put SSN, account numbers, card numbers or passwords in an SSO payload; TTL 5 minutes max; validate redirect domains against an allowlist.
- Test locally with the `StandardSSO Configuration Widget Template.sql` (flip `ROLLBACK` to `COMMIT`), the `Alkami.Microservices.StandardSSO.ConfigTool`, or the 2026 Admin widget (`Alkami.Admin.Widget.StandardSSO`, feature flag `EnableStandardSSOAdmin`, REST `POST /v1/sso/validate|evaluate`). Where the older Jarvis pages and the 2026 Standard-SSO pages disagree, the 2026 pages win.

## Front end

- Iris 2 / Iris Vue is the design system (Iris Classic is end of life). Load Vue and Iris from Alkami's copies: `@Html.IrisVueLinksSnippet()`, `@Html.ScriptWithCacheExpiration("~/lib/vue/vue.min.js")`, `@Html.IrisVueScriptsSnippet()`. Never load Vue, Chart.js or other libs from a CDN or re-bundle libraries already in `C:\Orb\WebClient\lib` or `\javascripts`; disclose extra third-party libs in `package.json`.
- Use the platform JS API instead of hand-rolling: `Alkami.Helpers.ajax(options)` (rejects on non-2xx, always `.catch`), `Alkami.Security.post` for MFA-gated calls (user cancel = rejection), `Alkami.Helpers.createDialog`/`createYesNoDialog`/`createConfirmationDialog`, `Alkami.Helpers.postLink` for sensitive links, `Alkami.Dom.*` (show/hide/fade/slide, `onDocumentReady`, `sanitizeHTML`, `passiveEventListener`), `Alkami.Localization.SiteText.get`, `Alkami.Utils.{CookieHelper,CurrencyHelper,DateHelper,StorageHelper,StringHelper}`, `Alkami.FlashBanner.showError|showSuccess|...`.
- Widget header: listen for `widgetHeaderInitBefore` (actions) and `widgetHeaderInitAfter` (title/navigation) and register those listeners outside any ready/DOMContentLoaded handler.
- Co-browsing: mark sensitive elements with `data-cobrowsing-hidden|masked|disabled`; dispatch a `conceal-elements` event after late rendering.
- Mobile titlebar: support both the HTML titlebar (`@section Header`, `titlebar-button left|right`, `<h1 id="page_title">`) and `window.nativeHook` (`isNativeApp()`, `setNavBar`, `updateNavBar`).
- JavaScript target ES2019; TypeScript is the supported transpiler; rely only on the documented polyfills; avoid `Promise.race/allSettled/any` and smooth-scroll options. Chart.js from `~/lib/chartjs/chart.min.js` with `role="img"` and `aria-label` on canvases.
- Vue widgets: Vue 3 with `@alkami/albus-preset-vue3`, Pinia, `vue-router@4`; source under `Scripts/{api,components,directives,interfaces,store,styles,utils,views}` plus `app.ts`, `App.vue`, `router.ts`; PascalCase SFCs with `<script setup lang="ts">` and scoped SCSS (`::v-deep` for nested selectors); mount element id `app`; interfaces named `IThing` one per file; Jest specs under `tests/unit/*.spec.ts`; Cypress e2e with `data-cy` selectors and a MirageJS mock server. Build with Albus: `npx albus build -C <project>` (`catalyst`, `transmute`, `saturate`, `panopticon`, `substantiate`, `evaporate`, `scry`), config in `albus.config.js` (`tokens.orbWidgetName`, `projectNamespace`, `clientFolderName`, `areaFolderName`), npm registry `@alkami:registry=https://feeds.alkamitech.com/npm/npm.dev/` in `.npmrc`.

## Logging

- One `private static readonly ILog Logger = LogManager.GetLogger<T>()` per class through Common.Logging (`Alkami.Utilities`); never call log4net directly except for context properties.
- Levels: FATAL (process dying), ERROR (unexpected, usually an exception; log the stack via the `Alkami.Utilities` helpers), WARN (continued with a default), INFO (production baseline, operator-useful, not chatty), DEBUG (off in prod), TRACE (external request/response bodies, per-call detail).
- Use `*Format` with placeholders in brackets (`"[{0}]"`), never string concatenation; `*Format` throws on JSON braces, so `Logger.Trace(json)` or `TraceFormat("{0}", json)`. Guard expensive arguments with `Is*Enabled` or lambdas (`Logger.Debug(m => m("x = {0}", x))`).
- Log entry/exit around every external or blocking call (with `Stopwatch` elapsed). Log an exception once: log it where handled, or wrap and throw, never both.
- PII: `response.SanitizeData()`, `MaskCardNumbers()`, or the `Log*Encrypted()` helpers when `Logger.IsEncryptionEnabled()`.
- Local config: edit `C:\Orb\WebClient\log4net.config` (and the microservice's `C:\ProgramData\chocolatey\lib\<pkg>\tools\log4net.config`); set the `Alkami` logger and your `<FI>` logger to DEBUG. Never lower the `root` logger.

## Testing

- NUnit 3 + Moq (FluentAssertions ok). Widget tests inherit `Alkami.Client.Common.Test.ClientUnitTestBase` (dynamic BankIdentifier/UserIdentifier, never assume static values). Test project `<name>.Tests`.
- Service unit tests: mocks in `[OneTimeSetUp]` injected through the static factory hooks (`ScopeFactory`, `ContextFactory`, `SettingServiceFactory`); `PermissionHelpers` from `Alkami.Test` for claims; `CreateFakeDbSet` for EF.
- Integration tests inherit `DatabaseBasedTests`, call `MockResolver.Use()`, use `EmptySubscriber`, start the real host, reset `ScopeFactory = request => CreateMockedDataScope()` per test.
- Submission pins: `Alkami.Test` 4.2.0, `Moq` 4.16.1 (check `08-...` for current values).
- Before submitting, `choco install` the built package on the local SDK and exercise it; a VS deploy is not enough.

## Packaging and submission

- `.nuspec` builds both the NuGet and Chocolatey package; fill every metadata field (empty fields fail packaging). Widget nuspec `files`: tools scripts, `AlkamiManifest.xml`, `src` (everything except bin/obj/tests/node_modules), `lib` (`bin\<FI>.Client.Widget.X.*` only, `exclude="**\*.config"`), and `Scripts`, `Styles` (no `.scss`), `Views`, `Images`, `_SiteText` to `content\Areas\App`. Remove the `<dependencies>` element (NuspecDependenciesCheck). No `Web.config` in the package.
- Package pins as of 2022.6 (verify in `08-...`): `Newtonsoft.Json` 8.0.3, `NHibernate` 3.0.0.4000, `NetMQ` <= 3.3.3.4, `Alkami.Subscriptions.ParticipatingClient` >= 3.7.2; never `Alkami.MicroServices.Accounts.*` 2.26.0 or `Alkami.MicroServices.SSOProviders` 1.5.0; do not include `Alkami.MicroServices.Accounts.WebApi.Host`.
- Every new project starts with an SDK Project Proposal in Jira (issue type `Feature Request`, label `sdk_project_proposal`, Team and Component `SDK`, all seven sections, data flow diagram for any external integration; 5 business day review; approval to proceed is not approval to deploy).
- Submission flow for SDK clients (2026): Merlin One Click Submission (feeds login) with pipeline SUBMITTED > SECURITY SCAN (~15 min) > CODE REVIEW (0 to 2 business days) > DEPLOY READY; staging must succeed before production unlocks; hold-to-confirm deploy is irreversible. Legacy/partner flow: upload `.nupkg` to `<fi>.choco.dev` on feeds.alkamitech.com, then a Jira `SDK Submission` (Summary = name + version, SDK Component Type, FI Contact, Package URL). Cancel any open ticket for a prior version first.
- Windows: Staging Mon to Thu 7:00pm CT (cutoff 4:00pm CT); Production Sun/Tue/Thu 10:30pm CT (transition by 2:00pm CT). New widgets in production also need PSO, a pod bounce and 7 to 10 days notice.
- A widget broken by an Alkami platform update ships as a new major version; resubmissions forced by Alkami changes are not billable, FI-initiated changes are.

## Local environment cheat sheet

- Requirements: Windows 11/Server, VS 2022 (ASP.NET + .NET desktop workloads, .NET Framework 4.8, .NET 8 SDK), SQL Server 2022 Developer/Enterprise (default instance, Windows auth, sysadmin; Express/Standard do not work), Chocolatey 1.4.0 exactly (`$env:chocolateyVersion = '1.4.0'`; 2.x breaks installs), Postgres 16 (2025.1+), Redis. Launch VS once as admin before installing templates.
- Install/upgrade: `choco upgrade Alkami.Powershell.SDK -y; Import-Module Alkami.Powershell.SDK -Force; Install-Nexus; Install-SDKRelease -Latest; choco upgrade Alkami.SDK.Templates -y; choco upgrade Alkami.SDK.Samples -y` (Merlin GUI installer is the alternative). Upgrade every 2 to 3 months. 2025.4 needs `Alkami.Powershell.SDK` >= 1.5.7. After upgrading from 2025.1 or earlier, add `TrustServerCertificate=true;` to every `AlkamiMaster.dbo.Tenant.ConnectionString`.
- Cmdlets: `Get-SDKSupport`, `Start-SDKServices`, `Stop-SDKServices`, `Restart-SDKServices`, `Restart-SDKWebClients -ClearTemp`, `Stop-IISAndServices`, `Start-IISAndServices`, `Ping-AlkamiWebsites`, `Remove-SDK [-Hard]`.
- Sites: https://developer.dev.alkamitech.com (user `mike.brady` / `12345`, MFA `560142`), https://admin-developer.dev.alkamitech.com (`admin.developer.dev`).
- Port reservations are mandatory: `netsh int ipv4 Add excludedportrange protocol=tcp startport=12345 numberofports=2` and `startport=50000 numberofports=30`; port 808 must belong to `SMSvcHost` only.
- Symitar stage match: `choco install Alkami.SDK.Core.Symitar --version <Major.Minor>` matching the SDK, then `choco upgrade Alkami.SDK.Features.Core.SymConnect -y` after each SDK update; provider `MultiplexerMode=2`, `HOSTIPADDRESS` as an IP, `_MS` suffix on the SOCKETLIST machine name.
- SQL scripts shipped by Alkami default to `ROLLBACK TRAN`; switch to `COMMIT TRAN` deliberately.

## Quick fixes

| Symptom | Fix |
| --- | --- |
| 500 "controller has a parameterless public constructor" | Extra/duplicate DLLs in widget bin; Copy Local = false, list dependencies in nuspec `lib` |
| Release-only `CssWithCacheExpiration` missing | Add `@using Alkami.Client.WebClient.Shared.Helpers`, `System.Web.Mvc.Html`, `Alkami.Client.Framework.Utility` to the view |
| 503 | WebClient app pool stopped; start it in IIS |
| Widget changes not showing | `IISRESET /stop`, delete ASP.NET Temp `root`, `IISRESET /start` (or `Restart-SDKWebClients -ClearTemp`) |
| Services will not start | Start `Alkami.Services.Subscriptions.Host`; check Redis env var `ALKAMI_REDIS_CONNECTION_STRING`, hosts entries, port reservations |
| DashboardV2 500 after upgrade | `update core.FlavorWidget set NativeDisplaySetting = 0 where NativeDisplaySetting is null` |
| Login loop / session expired after upgrade | `choco upgrade Alkami.MicroServices.Authentication.Workflow.Service.Host -y`; reinstall `Alkami.App.Service.Scheduler.Host` |
| Multiplexer "no compatible service ISettingsServiceContract" | Restart Settings, wait 30 s, restart Multiplexer |
| Templates missing in VS | Close VS, `choco install Alkami.SDK.Templates -f`, or run the VSIX in `C:\ProgramData\chocolatey\lib\Alkami.SDK.Templates\tools` |
| Submission "Manifest Error" | `AlkamiManifest.xml` must be at the package root |
| `Keyset does not exist` (SSO certs) | Grant the service accounts (`IIS_IUSRS`, `dev.*`) read on the private key |

## Version notes to keep in mind

- Latest documented SDK release: 2025.2 (changelog 2025-05-29); 2025.4 exists (PowerShell module requirement). Confirm against `version.txt` and the Releases page before assuming behavior.
- 2025.1: Nexus/Postgres required, Subscriptions Host 6.x, Sidekick replaces Microservice Tester, `BusinessACHProcessing` removed (use `Alkami.MicroServices.AchTemplates.Service.Client/Contracts`). 2024.4: .NET 6 and 8 SDKs, StepUp and Risk services consolidated into `Alkami.MS.StepUp.Service.Host` / `Alkami.MS.Risk.Service.Host`. 2022.6: SymConnect Multiplexer >= 2.13. 2022.4+: install process changed, ignore older upgrade commands.
- Vue 2 / Iris Classic are deprecated. Iris Vue was not Vue 3 compatible as of late 2023; check before upgrading a widget that depends on it.
- Alkami Embedded (Flutter host apps) is a separate track with its own toolchain pins (Flutter 3.38.6, Xcode 26.1.1 for 4020.x to 4021.x); see doc 10.

## Working style for Claude in this repo

- Before generating a new widget or service, confirm the FI identifier, the target SDK version, and whether a template already produced the skeleton. Prefer editing template output over hand-writing project scaffolding.
- Cite the doc section you relied on when a rule matters (naming, packaging, security). If the docs conflict (they do in a few places: `.Service.Host` suffix on the pattern page vs the coding guideline, .NET 4.7.2 vs 4.8, Jarvis vs 2026 SSO pages), say so and pick the newer guidance.
- When something is not covered here or in `docs/alkami-sdk/`, say it is not documented rather than guessing; the answer is usually sdksupport@alkamitech.com or an `SDK Support Incident` in Jira.
- Windows-only tooling: do not propose macOS or Linux build steps, Docker for the platform itself, or nuget.org packages.
