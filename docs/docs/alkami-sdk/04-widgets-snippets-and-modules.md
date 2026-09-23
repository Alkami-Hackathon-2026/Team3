# Widgets, Snippets, and Modules

**What this covers.** How to create, configure, package, deploy, and extend the UI plug-ins of the Alkami platform: client widgets, admin widgets, snippets, dashboard modules, and AtLogin modules. It also covers widget display and native display settings, registering a widget to users and packages, widget versioning, pre-compiled Razor views, the Widget Header JavaScript API, Navigation Builder in a local SDK, Spanish SiteText, theme and logo changes, iframes/pop-ups and idle-timeout suppression, MyAccountsV2 sub-nav tabs, the deprecated Login Refresh install, and optional feature packages.

---

## 1. Widget platform basics and conventions

A widget is an ASP.NET MVC application (an MVC Area) that plugs into the base Alkami platform. Client widgets deploy to the member site (`C:\Orb\WebClient\Areas\<areaName>`); admin widgets deploy to the admin portal (`C:\Orb\WebClientAdmin`). Both are packaged as Chocolatey packages through the `Alkami.Installer.Widget` and `Alkami.Installer.Widget.PostBuild` NuGet packages, which add a `Tools` folder, an `AlkamiManifest.xml`, and a `.nuspec` (already present with `Alkami.SDK.Templates` 1.11.0 or greater).

Conventions:

- Naming: `<FIPrefix>.Client.Widget.<Name>`, `<FIPrefix>.Admin.Widget.<Name>`, `*.Client.Snippet.*` (required for snippet registration). Tutorials use the fictional FI prefix `USBFI`.
- The `areaName` (for example `USBFIMyMoney`) is the MVC Area name, the `Areas` folder name, and the URL slug: `https://developer.dev.alkamitech.com/<areaName>`.
- Templates: `choco install Alkami.SDK.Templates -y` (close Visual Studio first). Samples: `choco upgrade Alkami.SDK.Samples -y`, installed under `C:\AlkamiSDK\Samples`.
- Visual Studio must run as Administrator to deploy and debug locally. Feed credentials are the same as for `https://feeds.alkamitech.com/`.
- Target framework: the client widget page (2024) says .NET Framework 4.7.2; the admin widget page (2023) says .NET Framework 4.8.
- Alkami prefers TypeScript (v3.5.3 per the 2023 admin page); admin widgets commonly use TypeScript with Knockout.js, and Vue.js layouts exist.
- Each widget needs a `core.Widget` row before the platform will load its DLL at startup. Locally, run `Tools\install_widget.sql`; it ships in rollback mode, so swap the rollback line for the commit line before executing.
- Rebuilding the project deploys the widget locally. Watch the Visual Studio output for packaging errors.

### The .nuspec file

- Metadata must be completely filled out with fully qualified URLs and values for every field (author, projectUrl, iconUrl, licenseUrl, description). Blank values make packaging fail (snippets: compilation error).
- The bin-files line ("Line 30", commented out by default) must be un-commented or the package is empty. Restrict it to your assemblies, for example `src="bin\USBFI.*"`, so you do not deploy every referenced assembly into `C:\Orb\WebClientAdmin\bin` (easily corrupted; delete any strays).
- If the widget depends on microservice client/data DLLs, list those too.

```xml
<files>
  <file src="tools\chocolateyInstall.ps1" target="tools" />
  <file src="tools\chocolateyUninstall.ps1" target="tools" />
  <file src="AlkamiManifest.xml" target="AlkamiManifest.xml" />
  <file src="**\*.*" target="src" exclude="**\obj\**\*.*; ... redacted/>
  <file src="bin\USBFI.*" target="lib" exclude="**\*.config"/>
  <file src="**\Scripts\" target="content\Areas\App" />
  <file src="**\Styles\" target="content\Areas\App" exclude="**\*.scss" />
  <file src="**\Views\" target="content\Areas\App" />
  <file src="**\Images\" target="content\Areas\App" />
  <file src="**\_SiteText\" target="content\Areas\App" exclude="**\*.xx.xml"/>
</files>
```

### AlkamiManifest.xml (widget)

Add your FI's `bankIdentifier` GUID and set `iconName` and `displaySettings`. Read `AlkamiManifest_explained.txt` (opened by the template) for the full rules. `displayName` must match `@WidgetDisplayName` in `install_widget.sql`.

```xml
<?xml version="1.0"?>
<packageManifest>
  <version>1.0</version>
  <general>
    <creatorCode>USBFI</creatorCode>
    <compound>USBFI_SDK</compound>
    <element>USBFI.Client.Widget.MyMoney</element>
    <componentType>Widget</componentType>
    <bankIdentifiers>
      <bankIdentifier name="USBFI">12DBFCD2-4133-41D1-B0DE-D0EABE11D7A0</bankIdentifier>
    </bankIdentifiers>
  </general>
  <widgetManifest>
    <widgetName>USBFI.Client.Widget.MyMoney</widgetName>
    <displayName>My Money</displayName>
    <description>The My Money financial solution you've always needed.</description>
    <widgetInstall>Client</widgetInstall> <!-- Client|Admin|Generic; Generic installs in both with v1.0 manifests -->
    <areaName>USBFIMyMoney</areaName>
    <assemblyInfo>USBFI.Client.Widget.MyMoney</assemblyInfo>
    <displaySettings>All</displaySettings> <!-- Desktop|Mobile|Tablet|DesktopMobile|All -->
    <iconName>piggy</iconName>
  </widgetManifest>
</packageManifest>
```

Sources: My First Client Widget (https://confluence.alkami.com/spaces/SDKC/pages/78655984); My First Admin Widget (https://confluence.alkami.com/spaces/SDKC/pages/71995307)

---

## 2. My First Client Widget

Sample: `Alkami.SDK.Samples` under `MyMoney`. Test site `https://developer.dev.alkamitech.com`, login `mike.brady` / `12345`, OTP `560142`.

1. `choco install Alkami.SDK.Templates -y` (Visual Studio closed).
2. Open Visual Studio as Administrator. New project, filter on the Alkami Platform type or search "widget", choose Alkami Client Widget.
3. Name: `USBFI.Client.Widget.MyMoney`, target .NET Framework 4.7.2. Build and verify.
4. Open `Tools\install_widget.sql`, set `@WidgetDisplayName` to `My Money`, un-comment Line 27 and comment out Line 26 (rollback to commit mode), execute.
5. Edit the `.nuspec` metadata and un-comment Line 30 (section 1).
6. Edit `AlkamiManifest.xml`: bankIdentifier, iconName, displaySettings.
7. Rebuild. Browse to the site, log in, and replace `DashboardV2` in the URL with `USBFIMyMoney`.

Sources: My First Client Widget (https://confluence.alkami.com/spaces/SDKC/pages/78655984)

---

## 3. My First Admin Widget

Prerequisite: My First Provider Service (https://confluence.alkami.com/display/SDKC/My+First+Provider+Service). Admin site: `https://admin-developer.dev.alkamitech.com`.

1. `choco install Alkami.SDK.Templates -y`; run Visual Studio as Administrator.
2. New project, search "admin", choose Alkami Admin Widget. Name `USBFI.Admin.Widget.MyMoney`, .NET Framework 4.8. Build.
3. Open `insert_menu_item.sql` at the project root, set the display name to `My Money`, execute against the local `DeveloperDynamic` database. Admin menus are permission driven and can differ per environment.
4. If not already present, add NuGet packages `Alkami.Installer.Widget` and `Alkami.Installer.Widget.PostBuild` (PostBuild depends on and pulls in Installer.Widget).
5. Edit the `.nuspec` (metadata, un-comment Line 30, Line 29 restricted to `bin\USBFI.*`).
6. Edit `AlkamiManifest.xml`.
7. Rebuild. In the admin site, Setup > My Money.

### Call the My Money service from the admin widget

Add the My Money service Client and Data NuGet packages. Model, inheriting `Alkami.Admin.WebClient.Models.BaseModel`:

```csharp
using Alkami.Admin.WebClient.Models;
using USBFI.MS.MyMoney.Data.ProviderSettings;
public class USBFIMyMoneyModel : BaseModel
{
    public List<Setting> Settings { get; set; }
}
```

Controller (usings: `Alkami.Admin.WebClient`, `Alkami.Admin.WebClient.Controllers`, `USBFI.MS.MyMoney.Contracts`, `USBFI.MS.MyMoney.Contracts.Requests`, `USBFI.MS.MyMoney.Service.Client`); note `AugmentRequest(req)` before the call:

```csharp
[Authorize]
public class USBFIMyMoneyController : BaseController
{
    public static Func<IMyMoneyServiceContract> MyMoneyServiceProxy = () => new MyMoneyServiceClient();

    [HttpGet]
    public async Task<ViewResult> IndexAsync()
    {
        var req = new GetSettingsRequest();
        AugmentRequest(req);
        var res = await MyMoneyServiceProxy().GetSettingsAsync(req);
        var myMoneyModel = new USBFIMyMoneyModel { Settings = res.ItemList };
        return await Task.FromResult(View(myMoneyModel));
    }
}
```

### Iris UI

Admin Views are split into the widget folder and `Shared` (layout plus left-side navigation). Layouts live in `C:\Orb\WebClientAdmin\Views\Shared`; use `~/Views/Shared/_Layout.Iris.cshtml` for Iris. Each layout defines sections that `Index.cshtml` must match.

`Views/Shared/_SideBar.cshtml`:

```html
<h4>My Money Administration</h4>
<div class="sidebar"><div>
  <a href="@Url.Action("Index", "USBFIMyMoney", new { area = "USBFIMyMoney" } )">Home</a>
</div></div>
```

`Views/Shared/_Layout.cshtml`:

```cshtml
@{ Layout = "~/Views/Shared/_Layout.Iris.cshtml"; }
@section HeadTitle{ @RenderSection("Title", true) }
@section BodySubNav { @Html.Partial("_SideBar") }
@section BodyJS{ @RenderSection("Scripts", false) }
@section HeadCSS{ @RenderSection("Styles", false) }
@RenderBody()
```

`Index.cshtml` head; the body is an `iris-content-header` row (columns Name, Current Value, Default Value using `iris-content-header__item`) and a `@foreach (var setting in @Model.Settings)` loop emitting one `<div class="iris-record" data-spacing="comfortable">` per setting with `@setting.Name`, `@setting.Description`, `@setting.CurrentValue`, `@setting.DefaultValue`:

```cshtml
@using Alkami.Client.Framework.Utility
@model USBFI.Admin.Widget.MyMoney.Models.USBFIMyMoneyModel
@section Title { USBFI My Money (Admin) }
@section Scripts { @Html.ScriptWithCacheExpiration("~/Areas/USBFIMyMoney/Scripts/usbfimymoney.js") }
@section Styles { @Html.CssWithCacheExpiration("~/Areas/USBFIMyMoney/styles/usbfimymoney.css") }
<h4 class="iris-content-header__title">Provider Settings</h4>
```

Sources: My First Admin Widget (https://confluence.alkami.com/spaces/SDKC/pages/71995307)

---

## 4. Snippets

A snippet is an MVC application like a widget, but it renders as a tab at the top of an existing widget. It is registered in the admin portal and attached to one or more widgets that have a tab dashboard (for example the Settings widget). Constraints:

- Desktop only. Cannot extend Alkami's BillPay module.
- Requires choco package `Alkami.Utilities.WebToolKit.Snippets.Runtime` version 2.1.0 or later.
- Project name must contain `*.Client.Snippet.*`.

### My First Snippet (2025)

1. `choco upgrade Alkami.SDK.Samples -y` and `choco upgrade Alkami.SDK.Templates -y`. Sample under `Snippets`.
2. Use the Alkami Snippet template; replace the `Alkami` prefix with your FI name and the part after `Snippet.` with a descriptive name.
3. `.nuspec`: author, projectUrl, iconURL, licenseUrl, description required. Uncomment line 29 (dependencies) and list every external dependency, including microservice bin files.
4. Run `Tools > install_snippet.sql` (creates the snippet's tables). Build; the output window shows the WebClient deployment.
5. Admin site: Setup > User Interface Settings > Snippets; click the pencil to attach the snippet to a widget. The widget on the member (Orb) site then shows the new tab.

### Upgrade Snippets 1.0 to 2.0 (2022; all new snippets follow 2.0)

1. Name contains `*.Client.Snippet.*`.
2. Reference `System.Web`.
3. Install `Alkami.Installer.Widget` and `Alkami.Installer.Widget.PostBuild`.
4. Upgrade `Alkami.Client` and `Alkami.Common` NuGet packages to the latest builds.
5. Install ASP.NET MVC 5.27 or greater.
6. Delete `WidgetDescription.cs`; add `SnippetsAreaRegistration.cs` that maps desktop and mobile routes and inserts them at the front of the route table:

```csharp
public class SnippetsAreaRegistration : AreaRegistration
{
    public const string _areaName = "NewNewSnippet";
    public override string AreaName => _areaName;

    public override void RegisterArea(AreaRegistrationContext context)
    {
        var desktopRoute = RouteTable.Routes.MapRoute(
            name: $"{AreaName}Route", url: AreaName + "/{action}/{id}",
            defaults: new { area = AreaName, controller = AreaName, action = "Index", id = UrlParameter.Optional },
            namespaces: new[] { typeof(NewNewSnippetController).Namespace + ".*" });
        var mobileRoute = RouteTable.Routes.MapRoute(
            name: $"Mobile{AreaName}Route", url: $"Mobile/{AreaName}/" + "{action}/{id}",
            defaults: new { area = AreaName, controller = $"Mobile{AreaName}", action = "Index", id = UrlParameter.Optional },
            namespaces: new[] { typeof(MobileNewNewSnippetController).Namespace + ".*" });
        var addedRoute = RouteTable.Routes[$"{AreaName}Route"];
        var addedMobileRoute = RouteTable.Routes[$"Mobile{AreaName}Route"];
        desktopRoute.DataTokens["area"] = AreaName;
        mobileRoute.DataTokens["area"] = AreaName;
        RouteTable.Routes.Remove(addedRoute);
        RouteTable.Routes.Remove(addedMobileRoute);
        RouteTable.Routes.Insert(0, addedMobileRoute);
        RouteTable.Routes.Insert(0, addedRoute);
    }
}
```

7. On every controller add `[FlavorAuthorize("AreaName")]` and inherit from the snippets base controller (spelled `SnippestBaseController` on the page) instead of `BaseController`.
8. Build; confirm the nuspec includes all DLLs needed to render.
9. Replace `installer_widget.sql` with the following and execute it:

```sql
use YOUR-DATABASE
go
insert into ui.Snippets (AreaName,FriendlyName,BankId,Description)
values ('YOUR AREA NAME','NICKNAME',30,'Snippets 2.0 Description')
select * from ui.Snippets       -- registered snippets
select * from ui.Registrations  -- widgets the snippet is attached to
```

10. Admin: Setup > User Interface Settings > Snippets > pencil icon, attach to a widget with a tab dashboard.
11. Verify a user can open the snippet on the client site.

Sources: My First Snippet (https://confluence.alkami.com/spaces/SDKC/pages/207852733); How To Upgrade From Snippets 1.0 To Snippets 2.0 (https://confluence.alkami.com/spaces/SDKC/pages/150658248)

---

## 5. Dashboard modules

A dashboard module is a partial view in one of your widgets. Desktop only.

1. Controller returns `PartialView("ExamplePartial", model);`. The partial has no layout sections, for example `@model Sample.Client.Widget.DashboardModule.Models.SampleDashboardModuleModel` followed by plain markup using `@Model.BannerMessage`.
2. Admin: User Interface Settings > Modules tab > Add Module. Name (shown above the module), platform Desktop, Path (URL slug that returns the partial). Save.
3. Layout tab > Edit > Add Module, check the module, save; drag it by its title to position, save the layout. Takes effect immediately.

Sources: Creating a Dashboard Module (https://confluence.alkami.com/spaces/SDKC/pages/69999110)

---

## 6. Widget display settings and native display settings

`DisplaySettings` is a bit flag in `core.Widget` and `core.FlavorWidget`, set via `@WidgetDisplaySettings` in `install_widget.sql` (or `displaySettings` in the manifest): Hidden 0, Desktop 1, Mobile 2, Desktop and Mobile 3, Tablet 4, All 7.

```sql
SELECT @WidgetName = 'USBFCUTravelNotes' --required
,@WidgetAssemblyInfo = 'USBFCU.Client.Widget.TravelNotes' --required
,@WidgetDisplayName = 'Travel Notes'
,@WidgetDescription = 'A widget for traveling notes.'
,@WidgetIconName = 'list'
,@WidgetDisplaySettings = '0' -- HIDDEN
```

Native apps use a separate `NativeDisplaySetting`:

```csharp
public enum NativeDisplaySetting
{
    Unknown = 0x00, IosPhone = 0x01, IosTablet = 0x02, AndroidPhone = 0x04, AndroidTablet = 0x08,
    Phone = IosPhone | AndroidPhone, Tablet = IosTablet | AndroidTablet,
}
```

Sources: Set The Widget Display Setting (https://confluence.alkami.com/spaces/SDKC/pages/62692173); Adding A Widget To Users (https://confluence.alkami.com/spaces/SDKC/pages/82791336)

---

## 7. Adding a widget to users (Stage/Prod)

**Widget must be on the native app menu:** open a Client Services ticket (below) with widget name, display name, description, assembly info, `DisplaySettings`, `NativeDisplaySetting`, target package(s) (flavors), and whether it is Added or Favorited by default for new and existing users.

**Widget not needed in native apps:** self-serve.

1. First deployment: admin site (Stage or Prod) > Setup > Widget Settings > Widget Registration, using the same values as your SDK `install_widget.sql`. Do this before the maintenance window that deploys the package: the widget DLL is not loaded unless the configuration is in the database before the environment restart.
2. Add the widget to a package and create a package assignment rule to move users into it; package settings (Added, Favorite) then apply. Guide: https://community.alkami.com/customer-service-61/packages-and-automatic-package-assignment-product-guide-v2-09-636
3. To test rules in the SDK, `choco install` `Alkami.MicroServices.Rules.Service.Host`, `Alkami.MicroServices.Rules.Package.Service.Host`, and `Alkami.MicroServices.Rules.DataSheet.Service.Host`.

**Requesting a package change:** no CCR required, but there is a queue (work is done at night). Open a Client Services ticket of type "Task", summary "Request for package change"; describe widget name, package(s), existing versus new users, and mobile/native access (iOS only, Android only, all). Add a comment mentioning `@sdksupport` asking them to monitor. Do not word it as if the SDK team will execute the request; they cannot, and the ticket may sit unworked.

Sources: Adding A Widget To Users (https://confluence.alkami.com/spaces/SDKC/pages/82791336)

---

## 8. Widget versioning (SDK submission)

Alkami rejects a submission whose version matches any prior submission (all history, not only the latest). Two files must stay in sync: `sem.ver` at the project root and `Properties/AssemblyInfo.cs`.

```json
{ "Version": { "Major": 1, "Minor": 2, "Patch": 4 } }
```

```csharp
[assembly: AssemblyVersion("1.2.4.0")]
[assembly: AssemblyFileVersion("1.2.4.0")]
```

Leave the fourth .NET segment at 0. Follow semver: Major for breaking changes, Minor for backward-compatible features, Patch for fixes and resubmissions (most common). Rebuild after changing `AssemblyInfo.cs`; the version in the compiled DLL is what is checked. Common mistakes: updating one file only; reusing, going backwards, or sideways on a version; not rebuilding.

Sources: How to Increase your Widget Version (https://confluence.alkami.com/spaces/SDKC/pages/572895377)

---

## 9. Pre-compiled Razor views

Speeds the first request per web server after a restart and greatly reduces web-server CPU. Video: https://youtu.be/C_bhc2FZBpM

1. Ensure every `.cshtml` is included in the `.csproj`.
2. Back up the project directory including `bin` (to compare DLL size).
3. Install `RazorGenerator.MsBuild` and `WebActivatorEx` from the Alkami Third Party feed (https://feeds.alkamitech.com/feeds/ThirdParty).
4. Pre-build event: `if EXIST "$(ProjectDir)\obj\CodeGen\" (rmdir /s /q "$(ProjectDir)\obj\CodeGen\")`
5. Add `exclude="**\obj\**\*.*"` to the nuspec so the generated `CodeGen` folder is not packaged.
6. Build in RELEASE mode; `.cshtml.cs` files appear in `$(ProjectDir)\obj\CodeGen\`.
7. Fix view errors: replace `@Html.Image(source, alt)` with `<img src="source" alt="alt">`; any `Alkami.Client.WebClient.Helpers.ThemeHelper` reference needs SDK Team help; add `System.Web` (and possibly `System.Core`); Iris.Vue projects need `Alkami.WebAssets.Helpers` (missing `IrisVueScriptsSnippet` / `IrisVueLinksSnippet`). Close all `.cshtml` tabs before compiling.
8. Confirm the new DLL is larger than the backup, then verify the widget renders in WebClient.
9. Development impact: editing a deployed view and refreshing no longer works. To verify precompiled views are in use, empty a deployed `.cshtml` but keep the file. Never delete `.cshtml` files; they are still used for routing and the widget fails without them.
10. SDK-style csproj: add the comment `<!-- System.Web.Mvc, Version=5 - This line only exists so that RazorGenerator picks this up as an MVC project -->` to the csproj.
11. After validation, restrict precompilation to Release so Debug builds keep editable views, by changing the RazorGenerator import condition to `Condition="'$(Configuration)' == 'Release' AND Exists('..\packages\RazorGenerator.MsBuild.2.5.0\build\RazorGenerator.MsBuild.targets')"`. Build in Release and test locally before submitting the choco package.

Put `@using Alkami.Client.WebClient.Shared.Helpers`, `@using System.Web.Mvc.Html`, `@using Alkami.Client.Framework.Utility` at the top of desktop and mobile views. If errors persist, upgrade NuGet packages (check https://confluence.alkami.com/sdk/nuget-package-requirements-what-versions-are-current-82794502.html for version limits).

Sources: Widget Pre-Compiled Views (https://confluence.alkami.com/spaces/SDKC/pages/184428253)

---

## 10. Managing the Widget Header (JavaScript API)

The Widget Header has a Title (the widget DisplayName), Navigation (tabs), and Actions (widget-wide buttons such as "Add Payee"). Control it with CSS (see `C:\AlkamiSDK\Samples\Widgets\Sample.Client.Widget.CustomCssAndJs` for hiding elements) or programmatically through the `WidgetHeaderService` (`Alkami.WidgetHeader`, but prefer the events). Events fire on the header element and bubble to `document`. Register listeners outside any ready/`DOMContentLoaded` handler or you may miss them.

```js
// Title: use widgetHeaderInitAfter (header fully initialized)
document.addEventListener('widgetHeaderInitAfter', function(event) {
  event.detail.header.title = 'My Custom Title';
});

// Navigation item
document.addEventListener('widgetHeaderInitAfter', function(event) {
  const service = event.detail;
  service.header.navigation.append(new service.NavigationItem({ title: 'Custom Tab', url: 'Widget/Custom/Path' }));
});

// Action button: use widgetHeaderInitBefore (actions need no header data)
document.addEventListener('widgetHeaderInitBefore', function(event) {
  const service = event.detail;
  const actionItem = new service.ActionButtonItem({ iconName: 'dollar', buttonId: 'money_making_action_item' });
  actionItem.element.addEventListener('widgetActionItemActivated', function(e) { /* handle */ });
  service.header.actions.append(actionItem);
});

// Skip the server round trip (for example an unregistered SSO widget iframing content)
document.addEventListener('widgetHeaderInitBefore', function(event) {
  event.detail.preventDefaultFetch = true;
});
```

SPA usage: pass `preventClick: true` on each `NavigationItem` to avoid full page refreshes, then on `header.navigation.element` handle `widgetNavigationItemActivated` (build UI, then `navigation.selectedItem = navigationItem`) and `widgetNavigationItemSelected` (display the view). Set `preventDefaultFetch = true` and `header.title` in `widgetHeaderInitBefore`; append routes in `widgetHeaderInitAfter`.

API:

- `WidgetHeaderService`: events `widgetHeaderInitBefore` (header empty; add actions) and `widgetHeaderInitAfter` (header populated; set title/navigation); `event.detail` is the service. Constructors `ActionButtonItem`, `ActionItem`, `NavigationItem`. Properties `currentWidgetName`, `header`, `headerId`, `preventDefaultFetch` (default false). Functions `fetchHeaderInfo()`, `storeHeaderInfo(info)`, `resetHeaderInfoStorage()`, `initHeader()`. Header info is cached in session storage.
- `WidgetHeader`: `actions`, `element`, `navigation`, `title` (custom HTML not supported).
- `WidgetNavigation` and `WidgetActions`: `element`, `items`, `length`; `append`, `getItem(index)`, `getItemIndex(item)`, `insert(item, index)`, `prepend`, `refreshUI(rebuildList)`, `remove(item)`. Navigation also has `selectedItem` and events `widgetNavigationItemActivated` / `widgetNavigationItemSelected` (`event.detail.navigation`, `.navigationItem`).
- `NavigationItem(options)`: required `title`, `url` (unique unless `id` given); optional `id`, `preventClick`. Properties `element`, `id`, `linkElement`, `selected`, `title`.
- `ActionButtonItem(options)`: `buttonId`, `containerId`, `href`, `iconName` (Iris icon class minus `font-icon-`), `text`, `buttonAttributes`. Event `widgetActionItemActivated` (`event.detail.actionItem`). Properties `buttonElement`, `buttonId`, `containerElement`, `containerId`, `element`, `id`, `visible`. `ActionItem(element)` wraps an arbitrary element (`element`, `id`, `visible`).

Sources: Managing The Widget Header (https://confluence.alkami.com/spaces/SDKC/pages/266837211)

---

## 11. Navigation Builder (new navigation) in the local SDK

With the 2022.6 or later SDK (`Install-SDKRelease -Latest`) new navigation is already present; the steps below describe what was installed or how to add it to an older SDK. The local SDK cannot create/edit menus through the Navigation Builder UI; you edit the JSON configuration files by hand.

### Admin side

Environment variables: `ALKAMI_SERVICE_HOSTNAME_OVERRIDE=local.dev.alkamitech.com` (formerly `_HOST_`; update it), `ALKAMI_SSL_CERTIFICATE_NAME=*.dev.alkamitech.com`, `ALKAMI_API_GATEWAY_URL=https://pcpg5exr6l.execute-api.us-east-1.amazonaws.com/dev`, `ALKAMI_MASTER_DB_CONNECTION_STRING=data source=localhost;Integrated Security=SSPI; Database=AlkamiMaster`, `ALKAMI_REDIS_CONNECTION_STRING=redis-18620.redis.corp.alkamitech.com:18620,keepAlive=60,password=gamehendge2020`, `ALKAMI_STAGE=dev`, `AWS_REGION=us-east-1`, `ALKAMI_ENVIRONMENT_IS_DEVBOX=true`, `ALKAMI_CLOUDFRONT_URI=https://dev-assets.alkami.net/`. Hosts file: `127.0.0.1 local.dev.alkamitech.com`. In Manage Computer Certificates give "Local Service" full control of the `*.dev.alkamitech.com` certificate.

```powershell
choco upgrade Alkami.Services.NavigationOrchestration Alkami.Admin.Widget.NavigationBuilder Alkami.Modules.Navigation Alkami.MS.Flags.Service.Host Internal.Services.Session -y
Set-SDKServiceStartupType -ServiceName 'All' -StartupType 'Automatic'
Set-SDKServiceStartupType -ServiceName 'Internal.Services.Session' -StartupType 'Automatic'
Restart-SDKServices
```

Enable `EnableNavigationBuilder` in the `feature.flag` table (missing: update `Alkami.MS.Flags.Service.Host`). Give your admin role the "Manage Navigation" permission (Staff > Admin Roles).

### Client side (local files instead of S3)

Base path `C:\Orb\WebClient\s3`:

- Static assets: `C:\Orb\WebClient\s3\cdn\navigation\alkami.modules.nav.min.css` and `alkami.modules.nav.min.js` (same for all FIs; attached to the page).
- Configs: `C:\Orb\WebClient\s3\navigations\<BankInstanceIdentifier>\<FlavorId>\<LocaleId>\<device>.json`. BankInstanceIdentifier is in `AlkamiMaster.dbo.Tenant`; FlavorId is the test user's package (default folder `3107`, Adults package in DeveloperDynamic); LocaleId usually `1033`; file is `desktop.json`, `mobileweb.json`, `native.json` (native v4x), or `tablet.json` (unused). Pull your FI's config from Stage by capturing the `desktop.json?` (or `mobileweb.json?`) response in DevTools; with a stage match, do both tenants. Default base files are attached to the page. Admin template files: not currently supported.

Set `C:\ProgramData\chocolatey\lib\Alkami.Services.NavigationOrchestration\lib\appsettings.json`:

```json
{ "ServicePath": "navigation-orchestration", "ServiceName": "Alkami.Services.NavigationOrchestration",
  "NewRelic.AgentEnabled": "false", "NavigationOrchestrationSettings": { "UseLocalConfigStorage": true } }
```

Verify `Internal.Services.Session` and `Alkami.Services.NavigationOrchestration` are running and the JSON is in place, then open the member site. If it fails, create an SDK Support Incident.

### Updating local configurations

Files are minified; beautify with the Notepad++ JSTool plugin (JS Format). Match the GUID folder to `BankInstanceIdentifier`; duplicate `3107` for other FlavorIds and `1033` for other locales. A menu item in a section's `SectionItems` has `DisplayName` (menu text), `WidgetName` (matches the `WebClient\Areas` folder), and `URL` (relative path). A section (for example "USBFI" after "Tools") has `DisplayName`, `Description`, `Icon` (not currently shown), `SectionItems`. Configs differ per device type: copy individual `SectionItems` between them, not whole files; whole config sets can be copied between flavors and locales.

Sources: How to Set Up Navigation Builder Locally (https://confluence.alkami.com/spaces/SDKC/pages/241342070); Updating Your Local Navigation Builder Configurations (https://confluence.alkami.com/spaces/SDKC/pages/241342114)

---

## 12. Spanish language and SiteText

1. Enable es-US (locale Id `21514`; see `select * from Contact.Locale`):

```sql
USE DeveloperDynamic
GO
UPDATE [Contact].[Locale] SET [IsAvailable] = 1 WHERE [Id] = 21514
```

2. Optional "XX" test culture (shows every SiteText spot): if `select * from Contact.Locale where Id = 99999` is empty, run `insert into Contact.Locale values (99999, 'Test Culture', 'xx-xx', Getdate(), 'xx', null, 1)`.
3. Add a Spanish DisplayName and Description (otherwise the area name is shown). The page's script copies the English `core.Item` row named `'Widget.' + @WidgetName` to a new row with `SecondaryId = 21514`, then inserts or updates `core.ItemSetting` rows named `DisplayName` and `Description` for that ItemId:

```sql
insert into core.Item (ItemType, ParentId, SecondaryId,Name,CreatedUtc,Version,Deleted,LastUpdate)
select ItemType, ParentId, 21514,Name,CreatedUtc,Version,0,GetDate() from core.Item where name = 'Widget.' + @WidgetName
Set @ItemId = SCOPE_IDENTITY ()
insert into core.ItemSetting (ItemId,Name,[Value],CreatedUtc,Version,LastUpdate)
Values(@ItemId,'DisplayName',@WidgetDisplayName,GETUTCDATE(),'1.0.0.1',getdate())  -- and 'Description'
```

4. Add Spanish (United States) to the browser's languages.
5. `restart-sdkservices` in PowerShell (or reboot). Language buttons appear at the bottom of SDK pages.
6. SiteText files: the template file has `en` before the extension; add `es` for Spanish and `xx` for the test culture. `en` and `es` package automatically; `xx` is excluded by the nuspec (`exclude="**\*.xx.xml"`), must not be submitted to Alkami, and is copied manually to the widget's `_SiteText` folder under the deployed area (`c:\orb\areas\WidgetName` as written on the page).
7. Admin Setup > Integration Settings has the SiteText tool; per-language database overrides win over the XML defaults (decision diagram on the page).

Sources: Add Spanish Language To Your SDK (https://confluence.alkami.com/spaces/SDKC/pages/119753202)

---

## 13. Theme, ProbePath, and logo updates

Themes ship in Orbital: `choco upgrade alkami.orbital -y`.

- ProbePath is the FI directory in `C:\Orb\WebClient\Orbital` (for example `SalesDemo`); set it in admin Setup > Integrations > Bank Settings (search "orbital") or with `UPDATE core.itemsetting SET value = 'NewProbePath, Default' WHERE name = 'OrbitalThemeProbePath'`. Changes are not immediate: reset IIS and clear the browser cache.
- Themes live in `C:\Orb\WebClient\Orbital\<ProbePath>\Themes\<ThemeName>` (SalesDemo has Business, SalesDemo, SalesDemoV2) and are registered in `core.theme`. Change the default in the admin Package widget or: `update core.theme set IsDefault = 0 where IsDefault = 1` then `UPDATE core.theme SET Name = 'NewThemeName' WHERE IsDefault = 1`. If no row exists, insert into `core.theme (BankId, Name, Active, IconImage, IconContentType, CreateDate, IsDefault)` with BankId 30, Active 1, `image/png`, IsDefault 1 inside a transaction (the page has a PNG binary literal; the SDK team can supply one).
- Logo: admin Setup > General Setting > Institution Info > Branding > Replace, then Save. 240 x 65 pixels (required for package logos). Stored in a binary column of `core.bank`. Afterwards: shift-refresh, kill the WebClient process, refresh, clear cache.
- Package logos: Setup > Packages > edit (eye icon) > Basic Info > Replace > Save; stored in `core.Flavor.LogoImage`; visible only after login.

Sources: Updating the Default Theme of the Website (https://confluence.alkami.com/spaces/SDKC/pages/82783808)

---

## 14. Pop-ups, iframes, and the "Are you still here" timeout

For SSO content prefer Alkami Standard SSO (https://confluence.alkami.com/spaces/SDKC/pages/141527390). Custom iframes may hit CORS restrictions; native app webviews add cross-domain limits.

Desktop: `<iframe src="https://alkami.com"></iframe>`; pop-up script inside `@section JavaScriptIncludeContentPlaceholder{ ... }`. Mobile: script inside `@section BodyScripts{ ... }`, setting the native nav bar first: `window.nativeHook.setNavBar({ text: '@Html.SiteText("MobileTitle")', left: { button: 'drawer' } });`. For PDFs in mobile append `?nativeDocType=pdf` (for example `/Areas/SamplePopups/Images/SamplePDF.pdf?nativeDocType=pdf`). The pop-up itself is `newwindow = window.open(url, 'name', 'height=200,width=150'); if (window.focus) { newwindow.focus() } return false;` invoked from `<a href="..." onclick="return popitup('...')">`.

**Idle timeout reset from an iframe.** Before ORB 2021.05 there was no way to reset the timer from inside an iframe. Since 2021.05 the parent page listens for a `postMessage` of `'idletimeout-reset'`. In the iframed page:

```js
function IframeEventHandler() { window.parent.postMessage('idletimeout-reset'); }
['keydown', 'mousedown', 'touchstart', 'touchend'].forEach((eventName) => {
  document.addEventListener(eventName, IframeEventHandler);
});
```

`setInterval(eventHandler, 30000)` also suppresses the prompt but is for demonstration only; do not use `setInterval` in production.

Sources: Pop-up Windows and IFrames using the SDK (https://confluence.alkami.com/spaces/SDKC/pages/241347224); Preventing are you still here pop within in IFrame (https://confluence.alkami.com/spaces/SDKC/pages/141547565); Prevent "Are you still here" on client (https://confluence.alkami.com/spaces/SDKC/pages/159222335)

---

## 15. MyAccountsV2 sub-nav tab

Adds a link on the desktop MyAccountsV2 account page via widget setting `external_information_account_types` (JSON array). Desktop only; the icon left of the label cannot be changed. The URL can point to a custom widget and automatically receives the `accountIdentifier` of the displayed account. `CoreName` is a RegEx matched against the account type name.

```sql
declare @widgetId as bigint
select @widgetId=ID from core.widget where name = 'MyAccountsV2'
if EXISTS(Select * from core.WidgetSetting where WidgetID = @widgetId and [Name] = 'external_information_account_types')
  update core.WidgetSetting set value = '[{"CoreName": "VCHK","Url": "/DemoHelloWorld","Label": "Rates", "UsexUrl": "false"}]'
  where WidgetID = @widgetId and [Name] = 'external_information_account_types'
ELSE
  Insert into core.WidgetSetting (WidgetID,[Name],[Value]) Values (@widgetId,'external_information_account_types',
  '[{"CoreName": "VCHK","Url": "/DemoHelloWorld","Label": "Rates", "UsexUrl": "false"}]')
```

Sources: Add a Sub-Nav Tab to MyAccountsV2 (https://confluence.alkami.com/spaces/SDKC/pages/216485405)

---

## 16. AtLogin modules

AtLogin modules (SDK template "AtLogin module") run after a successful login and before the Dashboard. The project looks like a widget with the same packaging and installer (create a client widget first if you have not). The difference is `StepAreaRegistration.cs`, required for MVC routing; do not alter it unless Alkami directs you to.

Optional controller members (model inherits `Alkami.Client.Framework.Mvc.BaseModel`; the page's `Index.cshtml` form example is a placeholder):

```csharp
public override List<string> IncludeStyles => new List<string>() { /* "~/Areas/TrainingHelloWorldModule/Styles/MyStyles.css" */ };

[HttpPost]
public async Task<ActionResult> Submit(Models.TrainingHelloWorldModuleModel model)
{
    return Complete(); // this will create the historical record
}
```

Remove a module from the SDK: delete its folder under `c:/orb/webclient/areas` (iisreset if files are locked), run the following, then `restart-sdkservices`:

```sql
select @StepId = ID from login.Step where Name = @ModuleName
delete from login.UserStep where StepId = @StepId
delete from login.ScenarioStep where StepId = @StepId
delete from login.Step where ID = @StepId
```

Removal from Stage/Production requires an SDK Support Incident and can take days to weeks. Do not submit an AtLogin module to Alkami just to test it; submit only fully tested modules intended for long-term use.

Sources: Work with the AtLogin Template (https://confluence.alkami.com/spaces/SDKC/pages/211521351)

---

## 17. Login Refresh (deprecated)

The page is marked "Deprecated!! Do not follow these instructions." The new login is installed by default. Still useful: static credentials `mike.brady` / `12345`, SMS code `560142`; challenge questions are not used with the new login. Product guide: https://midas.alkami.com/product-guide/registration-security-and-authentication-product-guide/

The old install (assumed 2021.4) force-installed `Alkami.Apps.Authentication --version 1.17.0`, force-upgraded the Authentication.Workflow/Static, StepUp, AtLogin, Username and UserContacts packages, ran a tenant SQL script registering the `Static Authentication Provider` and `SmsCode`/`EmailCode` step-up options plus `OneTimePasswordTemplate` notification templates, then `Restart-SDKServices`. Troubleshooting on the page: set bank settings `BankSettingName.UseEmailCodeAuthentication` and `BankSettingName.UseSMSAuthentication` to true; reinstall `alkami.microservices.authentication.workflow.service.host` with `-fy` (other microservices started first).

Sources: Add the new Login Refresh feature (https://confluence.alkami.com/spaces/SDKC/pages/131006687)

---

## 18. Adding optional features to the SDK

The base SDK install is minimal. Optional packages:

```
# Content Management System (CMS)
choco install Alkami.MicroServices.CMS.Service.Host
choco install Alkami.Admin.Cms
choco install Alkami.Modules.Cms

# Package Assignments (Rules); make sure these services are running
choco install Alkami.MicroServices.Rules.Service.Host
choco install Alkami.MicroServices.Rules.DataSheet.Service.Host

# Client Fragment Manager (safer replacement for Html.Raw with dangerous content)
choco install Alkami.MicroServices.ClientFragmentManager.Service.Host
choco install Alkami.Admin.ClientFragmentManagement
choco install Alkami.Modules.ClientFragmentInjector
```

The Adding A Widget To Users page also lists `Alkami.MicroServices.Rules.Package.Service.Host` for package assignment rules.

Sources: Adding additional features to the SDK (https://confluence.alkami.com/spaces/SDKC/pages/89659272); Adding A Widget To Users (https://confluence.alkami.com/spaces/SDKC/pages/82791336)
