# Alkami Embedded and Native App Integration

**What this covers.** Alkami Embedded is the Flutter package set that lets a financial institution host the complete Alkami mobile banking experience inside its own Flutter app: machine setup, starter-app contents, initialization and JWT authentication, theming with Iris Toolkit, loading widget override, push notification interceptors, Firebase analytics, and the per-release dependency version tables (Flutter, Xcode, Gradle, Java, Cloudsmith package versions). It also covers the older but still relevant how-to material for anyone whose .NET widgets run inside the Alkami native apps: WebView UX query parameters and JavaScript hooks, testing the Alkami native apps against a local SDK (Proxyman and Charles Proxy procedures, the `RegisteredApplications` and `core.Widget` SQL), injecting navigation JSON into the Flutter app, and streaming a PDF that the native app will open with sharing support. Alkami Embedded pages were updated in 2026; the native-app how-to pages date from 2022 to 2025 and reference older tooling (Charles Proxy, dl.alkami.com builds).

## 1. Alkami Embedded overview

Alkami Embedded is delivered as a Flutter starter app plus Alkami packages hosted on Cloudsmith (`https://dart.cloudsmith.io/alkami/flutter/`). The host app registers its dependency services in GetIt, initializes `AlkamiEmbeddedImpl`, selects an API environment, starts an auth session with a JWT, and then either navigates into Alkami routes or embeds the Alkami widget tree via `alkamiWidget(...)`.

Getting-started pages: App Setup, App Contents, Initializing Alkami Embedded. Services and extensibility pages: Push Notification Interceptors, Firebase Analytics Control, Custom Loading Widget, Theming with Iris. Versions: Release Versions and Mobile Release Versions. The reference client throughout the docs is Hanscom FCU (Jira project `HNSCMFCU`).

Sources: Alkami Embedded https://confluence.alkami.com/spaces/SDKC/pages/534088607

## 2. App Setup (machine and dependency setup)

### 2.1 Toolchain

Versions must match the latest app release version page (see section 9). Search the release page for `flutter_version`, `gradle_version`, `java_version`, and `xcode_version`.

- **Flutter**: install the version defined in the latest release. Alkami engineers use Visual Studio Code; follow https://docs.flutter.dev/install/quick. Recommended: manage Flutter versions with FVM (https://fvm.app/).
- **Android Studio**: latest version from https://developer.android.com/studio/install. Match Java and Gradle versions to the release page.
- **Xcode**: install the specific version listed as `xcode_version` in the release page (https://xcodereleases.com/). See section 2.5 for pinning Xcode.

### 2.2 Entitlement tokens (environment variables)

Dependencies for both the starter app and Alkami Embedded require entitlement tokens set as environment variables. The tokens are stored as a secret in Jira ticket `HNSCMFCU-404` (https://jira.alkami.com/browse/HNSCMFCU-404).

Add to `~/.zshrc`:

```
export MAVEN_PROGET_ALKAMI_MOBILEPLATFORM_USER=<insert_feeds.alkamitech.com_username>
export MAVEN_PROGET_ALKAMI_MOBILEPLATFORM_PASS=<insert_feeds.alkamitech.com_password>
export COCOAPODS_CLOUDSMITH_ALKAMI_FLUTTER_PASS=<insert_alkami-flutter-entitlement-token>
export COCOAPODS_CLOUDSMITH_ALKAMI_VENDOR_PASS=<insert_alkami-vendor-entitlement-token>
```

Then reload the shell:

```
exec zsh
```

### 2.3 Flutter dependencies

Add the Flutter entitlement token to Dart's pub token store, then fetch packages from the project base directory:

```
echo '<insert COCOAPODS_CLOUDSMITH_ALKAMI_FLUTTER_PASS here>' | dart pub token add https://dart.cloudsmith.io/alkami/flutter/
flutter pub get
```

Success produces `pubspec.lock`.

### 2.4 iOS and Android dependencies

iOS: from the `ios` directory run `pod install`; success produces `ios/Podfile.lock`.

Android: Maven dependencies are retrieved automatically when the Android app is built (using the `MAVEN_PROGET_ALKAMI_MOBILEPLATFORM_*` credentials).

### 2.5 Xcode version management (Machine Management)

A specific Xcode version is REQUIRED because Xcode is tied to a Swift version and iOS binaries compiled with a given Swift version must run with that Swift version. The required version is in the app-foundation CircleCI config (search for "xcode"): https://gitlab.mgmt.alkami.net/MP/app-foundation/-/blob/main/.circleci/config.yml (line ~1415). The release version pages also list it as `xcode_version`.

Xcode auto-updates itself, so the procedure is to install a versioned copy and rename it:

1. **Download** the specific version from https://developer.apple.com/download/all/?q=Xcode (sign in with an Apple Developer account; Alkami email / SAML credentials are valid). Do not use the App Store, since its version cannot be controlled. Large download and long install.
2. **Rename the existing app** (skip on a first install): while downloading, rename `/Applications/Xcode` to `Xcode-version` (convention `Xcode > Xcode-version`). This prevents auto-updates and allows multiple Xcode versions side by side.
3. **Install**: double-click the downloaded file to unzip/install. Rename the new `Xcode` app in Applications to follow the same convention. Launch it and allow prompts to install additional tools.
4. **Set the active Xcode** (only when multiple versions are installed; required for building to simulators/devices from Xcode, `flutter run`/`flutter build`, or the VS Code debugger):

```
xcodebuild -version
sudo xcode-select -s /Applications/Xcode-{version}.app
xcode-select -p
```

The "Machine Management" parent page itself is empty; Xcode Version Management is its only child.

Sources: App Setup https://confluence.alkami.com/spaces/SDKC/pages/534088625; Machine Management https://confluence.alkami.com/spaces/SDKC/pages/557260394; Xcode Version Management https://confluence.alkami.com/spaces/SDKC/pages/557260403

## 3. App Contents (starter app layout)

- **`pubspec.yaml`**: all packages required by Alkami Embedded are under `dependencies`. The set reflects the client's features (Hanscom FCU example: Vertifi remote deposit capture, card management, Glia video chat); resolved versions are in `pubspec.lock`.
- **`brandedAssets/`**: files that support included features (API keys for packages) and identify the app (images, color schemes).
  - `configuration/`: keys and certificates for Alkami APIs and the different API environments.
  - `configuration/build_configuration.json`: build-time behavior parameters:
    - `isStoreBuild`: when `true`, the production environment is used automatically and the debug menu is disabled.
    - `isDebugEnabled`: enables/disables the debug menu regardless of environment.
    - `appVersion`: the version the app is built with; use it to test or bypass forced-upgrade version checks.
    - `buildDate`: decides whether new runtime configurations must be fetched; a recent enough date means the build is assumed to have up-to-date configurations.
  - `fonts/`: fonts used in place of the default.
  - `images/`: launch image and the logo shown on most pages.
  - `l10n/`: overrides for localized strings used in Alkami Embedded.
  - `theme/`: colors, button shapes, background images, supplemental logos, FDIC logo appearance, etc. This is a backup theme bundled into the build; Alkami Embedded downloads theme tokens from Alkami's ThemeBuilder APIs on app launch.
- **`main.dart`**: app entry point; the initial version demonstrates initialization and push notification interceptors.
- **`global_scaffold.dart`**: a stateful widget acting as an omnipresent event handler for features that manipulate the whole widget tree (bottom sheets, overlays), typically for Digital ID events or app lifecycle state changes.
- **`dependency_service_impl.dart`**: implementation of the Alkami `DependencyService`; prepares features when Alkami Embedded is initialized. Most correspond to `pubspec.yaml` dependencies or client-specific configurations such as CardHub.
- Example files referenced elsewhere: `theming_example.dart`, `loading_widget_example_page.dart`, `analytics_example.dart`.

Sources: App Contents https://confluence.alkami.com/spaces/SDKC/pages/556861046

## 4. Initializing Alkami Embedded and session lifecycle

Initialize as soon as the app starts, right after all dependency services are registered in GetIt. Create an `AlkamiEmbeddedImpl` instance and call:

```dart
Future<void> initialize({required AlkamiEmbeddedOptions embeddedOptions});
```

`AlkamiEmbeddedOptions` constructor parameters:

```dart
// Hide the Alkami Global Navigation (top bar and bottom bar)
bool hideGlobalNavigation;
// Control whether analytics are enabled
bool enabledAnalytics;
// Override any registered Alkami route. `route` is the route about to be shown;
// return a bool indicating whether the route should be prevented or shown.
// typedef RoutingCallback = bool Function(String route);
RoutingCallback? routingCallback;
```

After initialization, select an API environment (Production live data, or Staging/QA test data):

```dart
Future<List<EnvironmentModel>> getEnvironments();
Future<void> selectEnvironment({required EnvironmentModel environment});
```

Start an authenticated session with a JWT. The JWT is exchanged for an Alkami access token that Alkami Embedded uses for API requests for the session (including refreshing it as needed). The method returns the access token so the host can also call Alkami APIs directly:

```dart
/// Starts an authenticated session with Alkami Embedded using a JWT.
/// [jwt] - The JSON Web Token string for authentication
Future<String?> startAuthSession(String jwt);
```

JWT prerequisite: an external OIDC user must already be registered as an Alkami user. OIDC user management is described on the "Hanscom OIDC Provider" page (Confluence pageId 556879312, not part of the SDK space extract).

End the session (logs out of Alkami; re-authentication requires `startAuthSession` again). Note: as of 2026-08-12 the page states this is not fully implemented; tracked as `DEV-220024` (https://jira.alkami.com/browse/DEV-220024).

```dart
/// Ends the current authenticated session with the Alkami eco-system.
Future<void> endAuthSession()
```

Navigate to an Alkami route (requires prior initialize, selectEnvironment, and startAuthSession):

```dart
/// [routeUrl] - The route path to navigate to (e.g., "accounts", "transfers", "more")
/// [arguments] - The arguments to pass to the route (optional)
Future<void> navigate({required String routeUrl, Map<String, dynamic>? arguments});
```

Get the main Alkami widget to embed anywhere in the host widget tree (same prerequisites):

```dart
/// Gets the main Alkami app widget that can be embedded in the host app.
/// The Alkami Embedded must be fully initialized before calling this method.
/// [routeUrl] - The initial route path to load within the Alkami widget
Widget alkamiWidget({required String routeUrl});
```

Required order: register services in GetIt, `initialize`, `selectEnvironment`, `startAuthSession`, then `navigate` / `alkamiWidget`. Push interceptors (section 6) are registered after initialization and environment selection.

Sources: Initializing Alkami Embedded https://confluence.alkami.com/spaces/SDKC/pages/538187050; JWT Authentication https://confluence.alkami.com/spaces/SDKC/pages/594820847

## 5. Theming with Iris and custom loading widget

### 5.1 Iris Toolkit

Alkami Embedded exposes Iris Toolkit, which contains a library of widgets matching Alkami Theme Builder settings and a set of design-token values grouped by color, shape, typography, spacing, etc. Design tokens are populated from data sent to Alkami Embedded by Alkami Theme Builder.

- Iris widgets follow the `Iris*` naming convention: `IrisListHeader`, `IrisButton`, `IrisAvatar`, `IrisSwitch`, `IrisTooltip`, `IrisProgressIndicator`. They share one theme (for example `IrisButton` and `IrisSwitch` have the same color; `IrisButton` and `IrisTooltip` the same border radius).
- For new or adjusted widgets, use tokens from `IrisToolkit` via GetIt:

```dart
final iris = GetIt.instance<IrisToolkit>();

iris.color.affordanceBase        // Color matching the primary brand color
iris.spacing.brandedCompact      // double used for spacing
iris.spacing.insetAllNano        // EdgeInsetsGeometry used for padding
iris.surface.platformRaisedShadowY // double used for elevation / drop shadows
```

See `theming_example.dart` in the starter app. The Flutter package is `iris_flutter` (versions in section 9).

### 5.2 Custom loading widget

The default loading image can be replaced with any Flutter widget by setting `loadingWidget` on the `AlkamiEmbeddedBase` instance from GetIt:

```dart
final alkamiEmbedded = GetIt.instance<AlkamiEmbeddedBase>();
alkamiEmbedded.loadingWidget = const Scaffold(
  body: Column(
    mainAxisAlignment: MainAxisAlignment.center,
    children: [
      Text('Here is my custom loading widget!'),
      SizedBox(height: 12),
      Center(
        child: IrisProgressIndicator(
          type: IrisProgressType.indeterminate,
          circularProgressSize: IrisCircularProgressSize.xlarge,
          indicatorColorOverride: Colors.green,
        ),
      ),
    ],
  ),
);
```

See `loading_widget_example_page.dart`.

Sources: Theming with Iris https://confluence.alkami.com/spaces/SDKC/pages/538181891; Custom Loading Widget https://confluence.alkami.com/spaces/SDKC/pages/538531934

## 6. Push notification interceptors

Alkami Embedded's push notifications are built on Airship. The host app can register callbacks on the `PushNotifications()` singleton. Register them after Alkami Embedded is fully initialized and right after an environment is selected.

Import:

```dart
import 'package:alkami_push_notifications/alkami_push_notifications.dart';
```

Callback typedefs (note the source spelling `OnPushTokenRecievedCallback` for the typedef, while the property is `onPushTokenReceivedCallback`):

```dart
typedef OnChannelCreatedEventCallback = void Function(ChannelCreatedEvent event);
typedef OnPushTokenRecievedCallback = void Function(PushTokenReceivedEvent event);
typedef OnPushReceivedCallback = bool Function(PushReceivedEvent event);
typedef OnNotificationResponseCallback = bool Function(NotificationResponseEvent event);
typedef OnNotificationDeeplinkCallback = bool Function(DeepLinkEvent event);
```

The `bool` callbacks return `false` to allow default handling and `true` to intercept and prevent default handling. Example registration:

```dart
PushNotifications().onChannelCreatedEventCallback = (event) {
  print('Host App received Airship Channel ID: ${event.channelId}');
};

PushNotifications().onPushTokenReceivedCallback = (event) {
  print('Host App received push token: ${event.pushToken}');
};

PushNotifications().onPushReceivedCallback = (event) {
  print('Host App received push notification: ${event.pushPayload.toString()}');
  final embeddedContext = routingService.navigatorKey.currentContext;
  if (embeddedContext != null) {
    ScaffoldMessenger.of(embeddedContext).showSnackBar(
      const SnackBar(content: Text('Push Notification received.')),
    );
  }
  return false; // Return true to intercept and prevent default handling
};

PushNotifications().onNotificationResponseCallback = (event) {
  print('Host App received notification response: ${event.toString()}');
  final embeddedContext = routingService.navigatorKey.currentContext;
  if (embeddedContext != null) {
    ScaffoldMessenger.of(embeddedContext).showSnackBar(
      const SnackBar(content: Text('Notification response received.')),
    );
  }
  return false;
};

PushNotifications().onNotificationDeeplinkCallback = (event) {
  print('Host App received notification deeplink: ${event.deepLink}');
  return false;
};
```

`routingService.navigatorKey.currentContext` gives a `BuildContext` inside the embedded navigator.

Sources: Push Notification Interceptors https://confluence.alkami.com/spaces/SDKC/pages/538193730

## 7. Firebase analytics control

- Analytics events go to the client's Alkami Firebase project. Access to Firebase dashboards is granted on request; supply an email address and desired role (owner, editor, viewer).
- The Firebase instance is exposed through the Flutter Platform package's core dependencies:

```dart
import 'package:flutter_platform/alkami_core_dependencies.dart';
```

- Analytics vs Crashlytics: analytics events are long-term, low-stakes metrics and appear in Firebase only after enough time or volume. Crashlytics events are for fatal and non-fatal errors, include a stack trace, and appear faster and with more detail. A crash inside Alkami Embedded logs a Crashlytics event automatically.
- Analytics can also be switched off globally with `AlkamiEmbeddedOptions.enabledAnalytics` (section 4).
- Debugging incoming events requires native-level environment variables; see Firebase's DebugView docs (https://firebase.google.com/docs/analytics/debugview). See `analytics_example.dart` in the starter app.

Sources: Firebase Analytics Control https://confluence.alkami.com/spaces/SDKC/pages/538517977

## 8. Release versions: what changes and where to look

When Alkami releases a new app version, dependency versions (including `flutter_platform`) change and the host app's `pubspec.yaml` must be updated to keep using Alkami Embedded. Things that may need updating: Alkami dependencies on Cloudsmith, the Flutter version, and native tools (Xcode, Gradle, Java).

Two index pages exist and they disagree on coverage:
- "Release Versions" (updated 2026-08-24) lists 4022.2.0, 4022.1.0, 4022.0.0, 4021.2.0, 4021.1.0, 4021.0.0, 4020.2.0, 4020.1.0, 4020.0.0. No per-version detail for the 4022.x or 4021.2.0/4021.0.0/4020.2.0/4020.1.0 entries was present in the extract.
- "Mobile Release Versions" (updated 2026-04-09) has child pages with full tables for 4021.1.2, 4021.1.1, 4021.1.0, 4020.1.4, 4020.1.3, 4020.1.2, 4020.1.1, 4020.0.0. Those are summarized in section 9. Treat the 4022.x line as newer than anything documented below.

Sources: Release Versions https://confluence.alkami.com/spaces/SDKC/pages/538189394; Mobile Release Versions https://confluence.alkami.com/spaces/SDKC/pages/557260437

## 9. Mobile release version tables (4020.0.0 through 4021.1.2)

### 9.1 Build dependencies (identical across all eight documented releases)

| Dependency | Version |
|---|---|
| flutter_version | 3.38.6 |
| xcode_version | 26.1.1 |
| ios_deployment_target | 15.1 |
| gradle_version | 8.14.4 |
| java_version | 17 |
| android_compile_sdk_version | 36 |
| android_min_sdk_version | 31 |
| android_target_sdk_version | 35 |
| kotlin_version | 2.1.0 |

### 9.2 Flutter package versions (Cloudsmith, `hosted: https://dart.cloudsmith.io/alkami/flutter/`)

Baseline (4020.0.0, identical for 4020.1.1 through 4020.1.4):

```
alkami_accounts 5.84.0            alkami_capture_misnap 5.2.0        alkami_capture_static 3.19.0
alkami_capture_vertifi 7.16.1     alkami_cardhub 9.3.1               alkami_core_dependencies 4.50.0
alkami_gimbal 4.18.0              alkami_glia 7.1.0                  alkami_greenlight 1.8.0+hotfix.1
alkami_migration_service 3.29.0   alkami_money_movement 1.27.0+hotfix.1
alkami_payrailz 1.7.0             alkami_popio 6.23.0                alkami_push_notifications 5.17.1
alkami_push_provisioning 5.15.0   alkami_remote_deposit 4.64.0       alkami_rxp_fiserv 1.19.0
alkami_settings 4.38.1            alkami_subordinate_app_launcher 3.30.0
alkami_subordinate_apps 3.29.0    alkami_zelle 4.27.0                alkami_zelle_fiscoop 1.12.0
alkami_zelle_fiserv 2.28.0        alkami_zelle_jha 1.29.0            alkami_zelle_webview_wrapper 4.32.0
flutter_html 3.0.0-alkamifork.1   flutter_html_table 3.0.0-alkamifork.1
flutter_platform 2.194.0          flutter_system_proxy 3.26.0        gimbal_flutter 4.31.0
glia_flutter 7.2.0                greenlight_flutter 1.10.0+hotfix.1 iris_flutter 10.116.0
payrailz_flutter 1.17.0
webview_flutter, webview_flutter_android, webview_flutter_platform_interface, webview_flutter_wkwebview 6.5.0
zelle_fiscoop_flutter 1.22.0      zelle_fiserv_flutter 2.40.0        zelle_jha_flutter 2.31.0
```

Changes in later releases:

- **4021.1.0**: `flutter_platform` 2.195.0, `iris_flutter` 10.117.0. Everything else unchanged.
- **4021.1.1**: `alkami_capture_vertifi` 7.17.0. The page omits flutter_platform, iris_flutter, alkami_core_dependencies, alkami_accounts, alkami_migration_service, alkami_money_movement, alkami_push_notifications, alkami_remote_deposit, alkami_settings, alkami_zelle_webview_wrapper, flutter_system_proxy, and the webview_flutter packages from its list.
- **4021.1.2**: the page lists only alkami_capture_misnap 5.2.0, alkami_cardhub 9.3.1, alkami_glia 7.1.0, alkami_push_provisioning 5.15.0, flutter_html and flutter_html_table 3.0.0-alkamifork.1, gimbal_flutter 4.31.0, glia_flutter 7.2.0.

The 4021.1.1 and 4021.1.2 pages do not say whether the omitted packages were removed or simply not captured by the scan, so consult `pubspec.lock` of the actual release.

### 9.3 Android (Maven) SDK dependencies by Flutter package

Baseline (4020.0.0):

```
airship_flutter:            com.urbanairship.android:airship-framework-proxy:11.1.0
alkami_capture_misnap:      com.miteksystems.misnap:document:5.7.0, com.miteksystems.misnap:classifier:5.7.0
alkami_capture_vertifi:     :rdcframework-release, :viplibrary-10.3
alkami_cardhub:             com.ondotsystems:cardapp-sdk:25.3.0-5_API35_16kb, com.urbanairship.android:urbanairship-fcm:18.5.0
alkami_popio:               com.drewnoakes:metadata-extractor:2.9.1, io.github.oothp:android-pdf-viewer:3.2.0-beta05,
                            pl.droidsonroids.gif:android-gif-drawable:1.2.29, io.socket:socket.io-client:0.8.3, :ft-sdk-release, :libwebrtc
alkami_push_provisioning:   :VisaPushProvisioning-4.1.3, :VisaInAppCore-4.1.3, :TMXProfiling-7.3-52, :TMXProfilingConnections-7.3-52,
                            :samsungpaysdk-2.6.00, :play-services-tapandpay-18.1.0, com.squareup.okhttp3:okhttp:4.12.0,
                            com.squareup.okhttp3:logging-interceptor:3.14.9, net.minidev:json-smart:2.4.11, commons-io:commons-io:2.19.0,
                            commons-codec:commons-codec:1.16.0, com.nimbusds:nimbus-jose-jwt:8.2.1
alkami_zelle_webview_wrapper: :digital-payments-sdk-2.4.2
apptentive_flutter:         com.apptentive:apptentive-kit-android:6.9.3
flutter_aepcore:            com.adobe.marketing.mobile:core:3.6.0, identity:3.0.2, lifecycle:3.0.2, signal:3.0.1
flutter_aepedge:            com.adobe.marketing.mobile:edge:3.0.2
flutter_pdfview:            io.github.oothp:android-pdf-viewer:3.2.0-beta05
gimbal_flutter:             com.gimbal.android.v4:gimbal-sdk:4.10.0, com.gimbal.android.v4:gimbal-slf4j-impl:4.10.0,
                            com.gimbal.android:airship-adapter:2.0.5, com.urbanairship.android:urbanairship-fcm:18.5.0,
                            urbanairship-automation:18.5.0, urbanairship-message-center:18.5.0
glia_flutter:               com.glia:android-widgets:3.5.0
greenlight_flutter:         me.greenlight:partner:3.0.3
pendo_sdk:                  sdk.pendo.io:pendoIO:3.9.3.8651
zelle_fiscoop_flutter:      :digital-payments-sdk-3.2.0
zelle_fiserv_flutter:       :ZelleSDK_V_1.5.5
zelle_jha_flutter:          :Payment_Toolkit_Android_SDK_Release_V3.4.2
```

Per-release changes:

- **4020.0.0, 4020.1.1, 4020.1.2**: the pages mark `com.miteksystems.misnap:document:5.7.0`, `com.miteksystems.misnap:classifier:5.7.0`, `com.ondotsystems:cardapp-sdk:25.3.0-5_API35_16kb`, and `me.greenlight:partner:3.0.3` as `FAILED` (the dependency scan could not resolve them). No other differences.
- **4020.1.3**: same versions, `FAILED` markers gone.
- **4020.1.4**: `com.squareup.okhttp3:logging-interceptor` moves from `3.14.9` to `4.8.1` (alkami_push_provisioning).
- **4021.1.0**: `flutter_platform` 2.195.0, `iris_flutter` 10.117.0; logging-interceptor 4.8.1.
- **4021.1.1**: `alkami_capture_vertifi` 7.17.0 with `:viplibrary-11.0` (was 10.3); `alkami_zelle_webview_wrapper` Android dep becomes the Maven coordinate `com.alkami.fiscoop.digitalpayments:digital-payments-sdk:2.4.2`.
- **4021.1.2**: local AAR references replaced by Alkami-hosted Maven coordinates: `com.alkami.vertifi.rdcframework:rdcframework:10.3.0`, `com.alkami.vertifi.viplibrary:viplibrary:10.3.0`, `com.alkami.popio.ftsdk:ft-sdk:1.1.20`, `com.alkami.google.libwebrtc:libwebrtc:92.4515.0`, `com.alkami.fiscoop.digitalpayments:digital-payments-sdk:3.2.0` (both alkami_zelle_webview_wrapper and zelle_fiscoop_flutter), `com.alkami.zelle.zellesdk:ZelleSDK:1.5.5`, `com.alkami.jackhenry.paymenttoolkit:Payment_Toolkit_Android_SDK:3.4.2`. `pl.droidsonroids.gif:android-gif-drawable` is listed as `1.2.6` (was 1.2.29). Vertifi is back at 10.3.0 in this page (4021.1.1 said 11.0).

Sources: 4020.0.0 https://confluence.alkami.com/spaces/SDKC/pages/557260438; 4020.1.1 https://confluence.alkami.com/spaces/SDKC/pages/557260439; 4020.1.2 https://confluence.alkami.com/spaces/SDKC/pages/557260440; 4020.1.3 https://confluence.alkami.com/spaces/SDKC/pages/557260441; 4020.1.4 https://confluence.alkami.com/spaces/SDKC/pages/557260442; 4021.1.0 https://confluence.alkami.com/spaces/SDKC/pages/557260443; 4021.1.1 https://confluence.alkami.com/spaces/SDKC/pages/557260444; 4021.1.2 https://confluence.alkami.com/spaces/SDKC/pages/557260445

## 10. WebView UX interactions (query parameters and JS hooks for widgets in the mobile app)

Widget pages rendered in the Alkami Flutter mobile app's WebViews can control native layout and transitions through URL query parameters plus a bi-directional JavaScript hook. The mobile app listens to all URL changes inside its WebViews and intercepts them to apply layout and animation options. Background: a Google Slides presentation and a UX Confluence page (https://confluence.alkami.com/x/a-zaFw) are linked from the source page; a demo video `top_bar_ux.mp4` is attached to it.

Query parameters:

- **`isSubPage=true|false`**: the master switch. `true` tells the app not to render the route in the current WebView but to create a new WebView and apply the other layout/animation options to it. It also hides the main bottom bar navigation on that route, so the user must back out of the flow rather than jump to another section. `false` renders in place with the bottom navigation tab shown. The other parameters are only respected when `isSubPage=true`.
- **`showTopBar=true|false`**: `true` shows the top bar; `false` hides it and the WebView takes the full screen. This ties into existing routing logic, so the existing top bar type and the `refresh-nav` endpoint still work.
- **`transitionAnimation=slideInFromRight|slideInFromBottom|none`**: native transition played when navigating to a new sub-page. No custom CSS/JS is needed and the animation follows the device's current design paradigm.

JavaScript hooks (details on the "Webview JavaScript Actions" page, not in this extract):

- `showTopBarScrim`: event the WebView invokes to tell the Flutter app to draw a scrim over the top bar (if one exists).
- `onTopBarScrimTapped`: event the Flutter app fires into the WebView when the user taps the scrim; the WebView should dismiss its modal/bottom sheet.

Debugging in the app: log in, open the More menu, choose "Developer", then "Packages", then "Webview", then "Webview UX Debug Page". The page is itself a WebView. Its Page Options and Transition Animations sections override any query parameters typed into the URL field (even when unchecked), so use those controls rather than hand-typing parameters.

### 10.1 Troubleshooting: external URL widget missing the bottom navigation bar

Problem (client ICCU): a NAV Builder widget configured as a "custom URL" pointing at an external URL did not show the bottom navigation bar on mobile. Resolution: create the widget using `GenericUrlLauncher` and configure it in NavBuilder as a widget (not custom URL). Key settings:

- Provider StandardSSO: `GenericUrlLauncher`
- Provider Name
- DisplayLocation
- Method: `Get`
- URI

Setting the display setting to "New Window" prevents the external URL from loading in an iframe so the bottom navigation bar stays visible; if the FI prefers an iframe, set the display setting to Inline. Reference: "Configure External URL To Launch In Alkami" (Implementation Services Confluence space).

Sources: WebView UX Interactions https://confluence.alkami.com/spaces/SDKC/pages/538187637

## 11. Testing Alkami native apps against a local SDK

Two pages describe this. The newer "Native App testing in the SDK" (2025-02-25) uses Proxyman and the release app's built-in environment picker. The older "Step-by-Step: Testing Alkami Native apps to an SDK" (2023-10-09) covers network/DNS setup, the `RegisteredApplications` table, and the dl.alkami.com/dev builds. Prefer the 2025 procedure; the 2023 page remains useful for connectivity troubleshooting and for stage-match sites.

### 11.1 Current procedure (Proxyman, 2025)

1. Download Proxyman from https://proxyman.io/ for your platform and install it (allow the firewall prompt).
2. Install the Proxyman certificate on the SDK machine: Certificate > Install Certificate on this Windows, Install & Trust, Yes.
3. Install the certificate on the mobile device: Certificate > Install Certificate on iOS / Install Certificate on Android and follow the Setup Guide. Emulators: iOS https://docs.proxyman.com/debug-devices/ios-simulator, Android https://docs.proxyman.com/debug-devices/android-device.
4. Enable testing in the local SDK. If the SDK was installed fresh at 2024.2 or higher, skip this. For fresh installs before 2024.2, add the Oauth widget to the database (DeveloperDynamic and Stage Match), then run `Restart-SDKServices` in an admin PowerShell window:

```sql
if(exists (select * from core.Widget where AssemblyInfo = 'Alkami.Client.Widgets.Oauth'))
begin
  print 'This has already been added to the core.widget table'
end
else
begin
  insert into core.widget values (30, 'Oauth', 'Alkami.Client.Widgets.Oauth', 0, 1, NULL, NULL, 0, NULL, GETUTCDATE(), 5, 0, NULL)
end
```

5. Download the native app from https://dl.alkami.com/release and install it on the device.
6. Configure the app: on the environment picker landing screen tap the empty space 5 times to open the hidden menu. Tap Developer Tools, then the Debugging tab, then turn the Proxy toggle on (the system proxy is auto-detected; you can edit IP and port, then Save; Proxyman's default port is 9090). Return to the environment picker, choose Custom Environment, pick a default environment for DeveloperDynamic or enter stage-match site information, click "Make it so", then select the environment. The proxy setting persists across force-closes; turn it off in Developer Tools.
7. Test, observing traffic in Proxyman.

Troubleshooting: if login shows "Disclosure not accepted. Please refresh and accept the disclosure to sign in to the app" and the Refresh Disclosure button just loops, install the CMS microservice:

```
choco install alkami.microservices.cms.service.host -y
```

### 11.2 Older step-by-step procedure (2023; connectivity, DNS, RegisteredApplications)

The native apps use OAuth for session auth; session permissions combine what the device type may access and what the end user may access. At the time of writing, native apps could not connect to a stage-match site; use `developer.dev.alkamitech.com`. The DevSdk build from https://dl.alkami.com/dev uses these values (already present in `RegisteredApplications`):

- Client ID `CBA61EA9-DFA1-495E-AE8A-3191A8E28B03`
- Client Secret `0C8730BB-3F3E-4261-B4E2-1452A844CDE3`

Steps (the source page numbers two different steps as "Step 4"):

1. Make a plan: which system, and how network connectivity will work (these instructions target a local SDK environment).
2. Obtain a dedicated test device (not a personal device; get explicit organizational permission for BYOD).
3. Get the app. Production apps from https://dl.alkami.com/release only connect to production (URL is burned in). Development apps from https://dl.alkami.com/dev let you enter the URL; pick the DevSdk copy for the basic SDK site.
4. Resolve connectivity. Confirm `http://127.0.0.1` shows the SDK default page (http, not https, because the cert name will not match). Find the SDK machine IP with `ipconfig`, confirm `http://<ip>` works from the desktop, then from the mobile browser. If the mobile browser fails, the Wi-Fi likely cannot reach the SDK; use a USB-to-Ethernet adapter or a proxy (Charles Proxy).
5. Resolve name resolution for `developer.dev.alkamitech.com` (or a stage-match name such as `usbfi.dev.alkamitech.com`): a static DNS entry in the router, an internal DNS entry from networking, a hosts file entry on the device (`192.168.1.100 developer.dev.alkamitech.com`; usually possible on Android, not iOS), a proxy tool such as Charles Proxy routed through a computer whose hosts file you control, or Charles Proxy DNS Spoofing.
6. Test mobile browser access: open `https://developer.dev.alkamitech.com` on the device and log in with a registered user such as `mike.brady`. This must work before continuing.
7. Add the Registered Application (skip for DevSdk app against developer.dev.alkamitech.com). For an FI-branded native app, add a row to `dbo.RegisteredApplications` with that app's Client Key and Secret. Open an SDK Support incident to request them (Alkamists: https://bitbucket.corp.alkami.net/projects/IOSDEV/repos/fastlane/browse). In the admin site, Setup > Registered Applications, add an application with the same permissions as the `IOS_ORB` application but your key and secret, and select all Application scopes. Or clone the DevSdk row with this script:

```sql
Declare @Your_CLIENT_KEY as uniqueidentifier
Declare @Your_CLIENT_SECRET as uniqueidentifier
Set @Your_CLIENT_KEY='00000000-0000-0000-0000-YourClientKey'
Set @Your_CLIENT_SECRET='00000000-0000-0000-0000-YourClientSecret'

---- Make no changes below this point

Declare @DevSdk_CLIENT_KEY as uniqueidentifier
Declare @DevSdk_CLIENT_SECRET as uniqueidentifier
Set @DevSdk_CLIENT_KEY='CBA61EA9-DFA1-495E-AE8A-3191A8E28B03'
Set @DevSdk_CLIENT_SECRET='0C8730BB-3F3E-4261-B4E2-1452A844CDE3'

if (@Your_CLIENT_KEY = @DevSdk_CLIENT_KEY or @Your_CLIENT_SECRET = @DevSdk_CLIENT_SECRET)
Begin
print 'Your client key and secret must be unique. They cannot match the DevSdk values.'
print 'Update was NOT successful'
return
end

if EXISTS (Select * from [RegisteredApplications] where ClientKey = @Your_CLIENT_KEY and ClientSecret = @Your_CLIENT_SECRET)
Begin
print 'Your client key and secret are already in the RegisteredApplications table.'
print 'Update was NOT successful'
return
end

if NOT EXISTS (Select * from [RegisteredApplications] where ClientKey = @DevSdk_CLIENT_KEY and ClientSecret = @DevSdk_CLIENT_SECRET)
Begin
print 'The devSdk record is missing from the RegisteredApplications table.'
print 'Update was NOT successful'
return
end

insert into RegisteredApplications
SELECT TOP (1)
@Your_CLIENT_KEY as [ClientKey]
,@Your_CLIENT_SECRET as [ClientSecret]
,[Name] ,[Description] ,[FirstUseUtc] ,[IsFullyTrusted] ,[IsEnabled]
,[LogoBytes] ,[TokenLifespan] ,[RefreshEnabled] ,[IsAbsoluteTime]
,[RedirectUris] ,[CreateDate] ,[LogoImageId] ,[RefreshTokenDays]
,[LastUpdate] ,[Settings] ,[ServiceAdminUserID]
FROM [RegisteredApplications]
where [ClientKey] = @DevSdk_CLIENT_KEY and [ClientSecret] = @DevSdk_CLIENT_SECRET

print 'Script finished'
```

8. Verify an http binding on the WebClient site in IIS. The native app requires both an https and an http binding on the WebClient site.
9. Point the app at the URL: fresh-launch the app (close it if running), tap the splash screen 5 times, enter the URL, key and secret, continue, then log in.

Troubleshooting: make sure the `core.widget` record for the Oauth widget exists (see the SQL in 11.1, and the "Alkami Platform Machine Setup" page, https://confluence.alkami.com/spaces/SDKC/pages/48800506).

Sources: Native App testing in the SDK https://confluence.alkami.com/spaces/SDKC/pages/341809878; Step-by-Step: Testing Alkami Native apps to an SDK https://confluence.alkami.com/spaces/SDKC/pages/250878714

## 12. Navigation injection for Flutter mobile apps (Charles Proxy Map Local)

Purpose: replace the navigation JSON returned to the Flutter app with an edited copy so widgets not configured in Nav Builder can be reached in the mobile app. Written for the Flutter apps (2022); untested on older native apps. Assumes Charles Proxy is already set up per "Charles Proxy for Mobile Devices" (Confluence pageId 135988994).

Procedure:

1. In the Flutter app, tap 5 times in the white space above the environment selector, open Developer Tools, Debugging tab, enable the Charles proxy and set your IP address.
2. Ensure Charles Proxy is running and capturing; log in to the app.
3. In Charles, Sequence tab, find the `navigations` request. Copy its response from the JSON Text tab.
4. Edit the JSON to add widget entries. Each item has `displayName`, `widgetName`, `url` (for example `/Mobile/USBFIMyMoney`), and `useNative` (`false` for web widgets); sections also carry `icon`. The structure is `{ "identifier": "<guid>", "sections": [ { "displayName", "description", "icon", "sectionItems": [...] } ], "staticItems": [...], "utilityItems": [...] }`. Example added entry:

```json
{ "displayName": "MyMoney", "widgetName": "MyMoney", "url": "/Mobile/USBFIMyMoney", "useNative": false }
```

   Any widget added must be installed in the target environment or navigation returns a 404.
5. Save the edited JSON locally with a `.json` extension.
6. In Charles, right-click the `navigations` call, choose "Map Local...". In the map editor, reduce the Path to `/navigations/` plus an asterisk (`/navigations/*`) and set "Map To" to the saved file.
7. Close the app, reopen, and log in; the modified navigation appears.

Revert: Charles Tools > Map Local..., uncheck "Enable Map Local" (or uncheck only that mapping).

Sources: New Navigation Injection for Mobile Apps (Flutter) https://confluence.alkami.com/spaces/SDKC/pages/216486927

## 13. Displaying a PDF in the native apps

For the native app to detect a PDF and display it with sharing capabilities, the web page must trigger a navigation request in the embedded WebView, and the URL must carry the query string parameter `nativeDocType=pdf` (the examples use lowercase `nativedoctype=pdf`).

Triggering the navigation request (iOS):

- Link: an `<a>` element with `target="_self"` (the default) so the native navigation delegate sees it.

```html
<a href="/TrainingHelloWorld/GetPdf?nativedoctype=pdf" target="_self">Get PDF</a>
```

- Button: a non-form-submitting button handled in JavaScript normally opens a new window (`_blank`), which the app will not intercept. Pass `_self`:

```html
<input type=button onClick="window.open('/TrainingHelloWorld/GetPdf?nativedoctype=pdf', '_self');" value='Do The Window'>

<input type=button onClick="'location.href='/TrainingHelloWorld/GetPdf?nativedoctype=pdf'" target="'_self'" value='click here'>
```

Controller example that streams a PDF (NuGet package `hiqpdf_x64`):

```csharp
using HiQPdf;

public ActionResult GetPdf()
{
    HtmlToPdf htmlToPdfConverter = new HtmlToPdf();
    //htmlToPdfConverter.SerialNumber = serialNumber;
    htmlToPdfConverter.Document.PageSize = PdfPageSize.A4;
    htmlToPdfConverter.Document.PageOrientation = PdfPageOrientation.Portrait;
    htmlToPdfConverter.Document.PdfStandard = PdfStandard.Pdf;
    htmlToPdfConverter.Document.Margins = new PdfMargins(1, 1, 1, 1);
    htmlToPdfConverter.Document.FontEmbedding = true;
    htmlToPdfConverter.TriggerMode = ConversionTriggerMode.Auto;

    string depPath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath(@"~/Areas/TrainingHelloWorld/bin"), "HiQPdf.dep");
    htmlToPdfConverter.SetDepFilePath(depPath);

    string htmlToConvert = "<html><body>Hello World</body></body>";
    // If the HTML contains an image, the img src must contain the base64 encoded image as data

    byte[] byteInfo = htmlToPdfConverter.ConvertHtmlToMemory(htmlToConvert, "http://127.0.0.1");

    MemoryStream myStream = new MemoryStream();
    myStream.Write(byteInfo, 0, byteInfo.Length);
    myStream.Position = 0;

    return new FileStreamResult(myStream, "application/pdf");
}
```

Package the HiQPdf binaries in the widget `.nuspec`:

```xml
<file src="bin\Hiqpdf.*" target="lib" exclude="**\*.config"/>
```

Sources: Display a PDF in Native Apps https://confluence.alkami.com/spaces/SDKC/pages/261143453
