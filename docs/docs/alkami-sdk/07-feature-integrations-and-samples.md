# Feature Integrations and Sample Projects

**What this covers.** The Alkami SDK "Samples and Tutorials" and "How-to Articles" pages about integrating SDK code with specific platform features: the sample catalog and MyMoney sample, Transfers V2 setup and deep linking, the Transfers CoreProxy contract, building a Transfer Payment Provider (RTP/FedNow), Card Management providers, 2FA/risk evaluation, fake external accounts, check imaging providers, Business ACH and Wires, Quick Apply providers, AFX/CUFX/OAuth API enablement, and the Prizeout API. Package IDs, SQL values, contract names, and settings keys are kept exactly; long boilerplate SQL reproduced on the source pages is summarized by its key values with a pointer to the page. Where two copies of a page existed, the newer copy is preferred and differences are noted.

## 1. Samples and Tutorials catalog

Sample source ships in the `Alkami.SDK.Samples` Chocolatey package (needs NuGet feed access and a licensed 2022 SDK):
```
choco install Alkami.SDK.Samples --source https://feeds.alkamitech.com/nuget/choco.dev/
```
Sample widgets live in `C:\AlkamiSDK\Samples\Widgets`, namespaced `Sample.Client.Widget.[Name]`, deploying to `C:\Orb\WebClient\Areas` as `Sample[Name]`. Build each project to deploy to the ORB folder and run each sample's `.sql` script. The package also includes the `Alkami.Client` solution source as a patterns reference.

Sample services: Caching (`Sample.MS.Caching`; `SessionCache` and `ICache`), Exchange Rates (`RestClient` call to the public GDAX API), Get Doc (return a local PDF to a widget). Microservice-integrated widgets: Accounts (Security + Accounts microservices), Caching, Exchange Rates, Get Settings, View Documents (render a PDF), Web Fetch, Change Address (UserContacts microservice). Standalone widgets: Before Login (unauthenticated), Custom CSS and JS, Dashboard Module (partial view on Dashboard v2), Disclosure, File Upload (chunked JavaScript upload reassembled in the controller because the platform upload limit is very small), Interstitial Page, Mobi Nav, OAuth, Secure Message, Send An Email (SMTP), Site Text, SymConnect (RepGen, tracking records), Tab Navigation, Travel Notifications, NativeSamples (native/desktop SSO only), Still Here (suppress "are you still here" in an IFrame), Savings Pod (Vue components, vueChart).

How-to Articles index (label `kb-how-to-article`, entries through May 2026): Standard-SSO - How to Troubleshoot StandardSSO; How To Rollout a Standard-SSO Implementation For New and Existing SSO Providers to a Client's Production Environment; Stage-match core connections for SDK; Adding A Widget To Users; Package A Microservice Installer; Deprecate BusinessAchProcessing MS; Resolving Package Validations; Using Vue.js in an SDK Widget; Update *.dev.alkamitech.com certificate; Preventing are you still here pop within in IFrame; Adding additional features to the SDK; Adding or Updating an AlkamiManifest; Chocolatey and NuGet Nuspec Metadata Defaults; Logging Best Practices (SDK); 64 bit only (SDK); How to Set Up Navigation Builder Locally; Access your SDK from the public Internet; Integrate a widget with a microservice; Pop-up Windows and IFrames using the SDK; Package Assignment Rules and Fact Providers. Only TransfersV2 Deep Linking and Work with External Accounts are covered in this document.

Sources: Samples and Tutorials (https://confluence.alkami.com/spaces/SDKC/pages/62701693); How-to Articles (https://confluence.alkami.com/spaces/SDKC/pages/62699008)

## 2. MyMoney sample

Demonstrates Alkami microservice calls, the GenericProxy service, and Vue/Iris in mobile. Tabs:
- **MFA**: manual MFA from a form posted to a controller for risk evaluation. Requires `Alkami.MicroServices.Risk.Management.Service.Host`, `Alkami.MicroServices.Risk.Service.Host`, `Alkami.MicroServices.StepUpManager.Service.Host` (installed by `Alkami.MachineSetup.SDK.Features`). Challenges occur once per session; to re-challenge, log out or recycle Redis or the WebClient app pool. Locally the challenge is email-style questions (no SMS); with the default authenticator any answer works.
- **Accounts**: Accounts, CardManagement, and Security microservices. Requires `Alkami.MicroServices.Accounts.Service.Host`, `Alkami.MicroServices.CardManagement.Service.Host`, `Alkami.MS.CardAccounts.Service.Host`, `Alkami.MicroServices.CardManagementProviders.Static.Host` (DeveloperDynamic), module `Alkami.Admin.CardManagement`, and Tools scripts `StaticCardManagementProviderConfiguration.sql` and `CardManagementSetup-AdminNavigation.sql` in commit mode. Stage-matched databases need the core-specific CardManagementProvider and SQL (ask the SDK team). `Alkami.MicroServices.CardManagement.Service.Host` v8.0.0+ requires Admin > Setup > Staff > Admin Roles > Preview General Admin > check Manage Card Configuration and Manage Card Images > Save > re-login, then Setup > Card Management > Card Action Mapping and Card Attribute Mapping tabs.
- **Loan Apply**: Iris slide-out drawer form; partial view rendered as full view; requires `USBFI.MS.MyMoney` (sample `ProviderService` folder) running in VS or as a Windows service.
- **Available Features**: FeatureContext microservice; requires `USBFI.MS.FeatureManagement.Service` (sample `SlimService` folder).
- **Generic Proxy** / **My Weather**: GET/POST through `Alkami.MS.GenericProxy`; add `www.google.com`, `schematic-ipsum.herokuapp.com`, `www.7timer.info` to the GenericProxy provider allowed domain list. Any GenericProxy site must also be added to Stage and Production provider settings and approved by security.
- **Mobile**: Iris components in a simple Vue overview.

Sources: MyMoney Sample (https://confluence.alkami.com/spaces/SDKC/pages/141531845)

## 3. Set Up TransferV2

Updated 2025-11-21.
1. `choco install Alkami.Client.Widgets.TransferV2 -y`
2. Register the widget (change `use DeveloperDynamic` for a stage-matched database):
```sql
use DeveloperDynamic
IF NOT EXISTS (SELECT ID FROM core.Widget WHERE Name = 'TransferV2')
BEGIN
DECLARE @BankID bigint
SELECT @BankID = ID FROM core.Bank
INSERT INTO core.Widget (BankID, Name, AssemblyInfo, Salt, Active, IconImage, IconContentType, CanBeRemoved, DisplaySettings, WidgetType, IconName)
VALUES (@BankID, 'TransferV2', 'Alkami.Client.Widgets.TransferV2', 0, 1, null, null, 1, 7, 2, 'transfers')
DECLARE @WidgetID int
SET @WidgetID = @@IDENTITY
INSERT INTO core.FlavorWidget (FlavorID, WidgetID, AddedByDefault, FavedByDefault, Ordering)
SELECT DISTINCT f.ID, @WidgetID, 0, 0, MAX(fw.Ordering) + 1
FROM core.Flavor f JOIN core.FlavorWidget fw ON f.ID = fw.FlavorID
WHERE fw.Ordering < 999 GROUP BY f.ID
END
```
3. Validate installed: `Alkami.Legacy.Sync.Accounts`, `Alkami.Legacy.Sync.Transactions`, `Alkami.Legacy.Sync.Users`, `Alkami.Modules.LegacyAuthAdmin`, `Alkami.Modules.LegacyAuthClient`.
4. `choco install <pkg> -y` for `Alkami.MicroServices.AchTemplates.Service.Host`, `Alkami.MicroServices.ACHTransfersRequests.Service.Host`, `Alkami.MicroServices.LoanPayment.Service.Host`, `Alkami.MicroServices.NachaFileGenerator.Service.Host`, `Alkami.MicroServices.TransferShim.Service.Host`, `Alkami.Admin.Widgets.BusinessBanking` (possibly present), `Alkami.App.Providers.ACH.NachaOutput`, `Alkami.WebExtensions.SignalR` (from MachineSetup).
5. Admin > Setup > Packages > eye icon on Adults > Widgets tab > toggle Transfer V2 on and select its box, toggle old Transfer off, save. For Brady test members (e.g. Mike Brady): Support > Members > member > Features and Access > Package > Change > Kids, save; repeat with Adults. Log the member out and in.

**TransferV2 4.3.0 addendum.** Minimums: SDK 2020.6+; `Alkami.MicroServices.Audit.Service.Host` 6.32.0+; `Alkami.MicroServices.Iav.Service.Host` 1.0.0+; `Alkami.MicroServices.Settings.Service.Host` 4.7.0+. Also install `Alkami.MicroServices.Transfers.Service.Host`, `Alkami.MicroServices.Limits.Service.Host`, `Alkami.MicroServices.LinkedAccounts.Service.Host`, `Alkami.Client.Widget.AccountLinking`.

Widget settings, `alkami.client.widgets.transferv2`: `Use_New_Account_Linking_Experience` (True), `USE_NEW_ACCOUNT_LINKING_EXPERIENCE_WITH_IAV` (False; enables IAV in the linking sheet), `Use_New_Account_Linking_Experience_For_TrialDeposit` (True), `Use_New_Account_Linking_Experience_For_CrossAccount` (True). `alkami.client.widget.accountlinking`: `USERACCOUNTSYNC_RETRY_LIMIT` (1), `USERACCOUNTSYNC_RETRY_INTERVAL` (3 s), `ROUTING_NUMBER_REGEX` (`^[0-9]{8,9}$`), `ACCOUNT_NUMBER_REGEX` (`^[0-9]+$`), `NICKNAME_REGEX` (`^[A-Za-z0-9 ]+$`; special characters cause issues), `CROSSACCOUNT_FIRSTNAME_ENABLED` (false; first name always optional). Bank setting `ACHTransfersEnabled` must be on for ACH features (not owned by this widget). Site text: about 150 keys with defaults on the page, in families `AddAccount.Aggregation|Transfer.*`, `AddTransferAccount.CrossAccount.*`, `AddTransferAccount.Disclosure.*`, `AddTransferAccount.External.Instant|Manual.*` (e.g. `Error.Core.LimitExceeded`, `Error.DisallowedByRisk`, `Error.DisclosureNotAccepted`), `AddTransferAccount.Internal.*`, `AddTransferAccount.MFA.*`, `Header.*`, `Title` ("Account linking").

Sources: Set Up TransferV2 (https://confluence.alkami.com/spaces/SDKC/pages/125436069)

## 4. TransfersV2 deep linking

Dated 2020-06-30. Supported only on the desktop "classic" tab (`/TransferV2/Classic`) and mobile (`/Mobile/TransferV2`); not the "quick" tab. The caller decides desktop vs mobile (MMC has no redirect endpoint). Keys are case-sensitive, values are not: `toAccount`, `fromAccount` (GUID `core.Account.Identifier`); `frequency` (Daily, Weekly, Biweekly, Semimonthly, Every4Weeks, Monthly, EndOfMonth, Quarterly, Every6Months, Annually, EveryOtherMonth, WhenDue); `ending` (`Never`, integer > 0, or `MM/DD/YYYY`); `startDate` (`MM/DD/YYYY`, zero-padded); `amount` (positive, no `$`, max two decimals). Desktop: `amount`, `frequency`, `startDate` need `toAccount`; `ending` needs `toAccount` and `frequency`. Mobile (wizard, provide everything): `toAccount` needs `fromAccount`; `amount` needs both; `frequency`/`startDate` need both plus `amount`; `ending` also needs `frequency`.
```
https://developer.dev.alkamitech.com/TransferV2/Classic?toAccount=0498a1fb-d172-4515-9f31-4205736f42ad&amount=400&startDate=01/03/2021
https://developer.dev.alkamitech.com/Mobile/TransferV2?fromAccount=90e94793-9860-4305-828b-0f2ac62f032a&toAccount=0498a1fb-d172-4515-9f31-4205736f42ad&amount=10.5&frequency=monthly&ending=9&startDate=04/01/2020
```
No guarantee the linked transfer is allowed; validate on the caller side; LPOs unsupported; past dates are accepted in the URL but rejected.

Sources: TransfersV2 Deep Linking Capabilities (https://confluence.alkami.com/spaces/SDKC/pages/89685683)

## 5. Transfers CoreProxy: ITransfersCoreProxyServiceContract

Namespaces `Alkami.MicroServices.Transfers.CoreProxy.Contracts.Requests` / `.Models`.
```csharp
[ServiceContract]
[RequiresPermissions]
public interface ITransfersCoreProxyServiceContract
{
    [OperationContract] Task<AddTransferResponse> AddTransferAsync(AddTransferRequest request);
    [OperationContract] Task<AddTransferCreditResponse> AddTransferCreditAsync(AddTransferCreditRequest request);
    [OperationContract] Task<AddTransferDebitResponse> AddTransferDebitAsync(AddTransferDebitRequest request);
}
```
`AddTransferRequest : BaseCreateOrUpdateRequest<TransferRequest>`, `AddTransferResponse : BaseResponse<TransferResponse>`; same pattern for `TransferCreditRequest`/`TransferCreditResponse` and `TransferDebitRequest`/`TransferDebitResponse`. Items derive from abstract `BaseTransferRequest` / `BaseTransferResponse` (all `[DataContract(IsReference = true)]`).

`BaseTransferRequest`: `Guid RequestIdentifier` (batch correlation); `TransferAccount SourceAccount`, `DestinationAccount`; `string GLAccountNumber` (null = primary core default); `bool IsPriorYearContribution`; `DateTime EffectiveDate`; `TransferType TransferType`, `PaymentType? PaymentType` (serialized via private `int TransferTypeValue` / `int? PaymentTypeValue`); `decimal Amount` (principal + escrow + interest + fees); `decimal? AdditionalPrincipalAmount`, `AdditionalEscrowAmount`, `InterestAmount`; `string Description`, `TransferKey`; `bool ShouldTriggerAccountSync`, `ShouldTriggerTransactionSync`; `Guid? UserIdentifier` (SourceAccount owner; null = requester). `TransferAccount`: `Guid? AccountIdentifier`, `string TransactionCode`, `string AccountNumber` (required only for crypto: on the source for `CryptoCredit`, destination for `CryptoDebit`; the identifier on that side is ignored). `BaseTransferResponse`: `RequestIdentifier`, `IsSuccessful`, `TransferKey`, `IsInNightMode` (success = will not post immediately), `CoreMessage` (technical), `List<TransferError> TransferErrors` (`ErrorCode` via `int ErrorCodeValue`, `ErrorDescription`), `HasError`.

`PaymentType`: MinimumDue=0, CurrentBalance=1, LastStatementBalance=2, Standard=3, PayOff=4, Other=5, PrincipalOnly=6, EscrowOnly=7, AmountDue=8, AmountPastDue=9, PrePay=10, VariableMinimumDue=11, VariableLastStatementBalance=12, VariablePayOff=13. `TransferType`: Unknown=0, Loan=1, Standard=2, ACHCredit=3, ACHDebit=4, Wire=5, Autodraft=6, CryptoCredit=7, CryptoDebit=8, RTPCredit=9, RTPDebit=10, DebitCardCredit=11, DebitCardDebit=12, InternalFunding=13, ExternalFunding=14.

`AddTransferCreditAsync` is a one-sided credit from a GL (incoming ACH, crypto sell); `AddTransferDebitAsync` a one-sided debit to a GL (outgoing ACH, crypto buy). With `GLAccountNumber` null, `ACHCredit`/`ACHDebit` use `ACHGLAccountNumber` and `CryptoCredit`/`CryptoDebit` use `CryptoGLAccountNumber`. Build `ItemList` of items, call `AugmentRequest(request)`, then `await client.AddTransferAsync(request)` (or Credit/Debit). Sample values: internal `TransferType.Standard` with `TransactionCode` "9999" (destination) and "1111" (source); ACH debit with both `AccountNumber = "897798"` (BatchOffsetAccountNumber), codes "123" (TransfersOut) / "321" (TransfersIn), `GLAccountNumber = "00000000001"`. Responses carry `ItemList[]` of `BaseTransferResponse` plus standard `BaseResponse` fields.

Sources: Transfers CoreProxy - ITransfersCoreProxyServiceContract (https://confluence.alkami.com/spaces/SDKC/pages/273419024)

## 6. Create a Transfer Payment Provider (RTP / FedNow / debit card)

Audience Alkami Partner; updated 2025-05-12. Offers Instant External Transfers via RTP and FedNow through your API. A .NET Core `dotnet new` template on the `nuget.dotnetcore` feed (not `Alkami.SDK.Templates`):
```
nuget sources add -name "Alkami Nuget DotNetCore" -source https://feeds.alkamitech.com/nuget/nuget.dotnetcore -user emailAddress -password secret
dotnet new install Alkami.Templates
```
Visual Studio > Create a new project > search `TransferPayments` > name e.g. `TransferPayments.TestProvider` > Create > Build Solution > Test > Run All Tests.

Controllers: base controllers in `Alkami.Providers.TransferPayments.Contracts` already define routes, validation, HTTP method, and response codes; implement only the methods. Required: `RealTimePaymentsController.Submit` (response must include `Identifier` and `Status`; Identifier and Reference trace the request, Status updates Alkami's transfer record and is shown to the user; fill the rest too), `RoutingNumbersController.Get` (Boolean `IsEligibileForRealTimeTransfers`, sic), `StatusController.Get` (whether Alkami may call you; sets `SupportedPaymentTypes`, unlisted types are hidden, and `ProviderLoanPaymentTypes`). Optional: `DebitCardPaymentsController.Submit` (same Identifier/Status rules), `DebitCardVerificationsController.Verify` (return `Status`; if Approved, the supported transfer types). Flow: after auth/validation, get the request context (`IRequestContext`, gives `Identity.UserId`), then delegate to your handler.

Validators check presence and non-default values (e.g. not an empty GUID), not ownership. Base: `DebitCardAddressValidator`, `DebitCardPaymentSubmitRequestValidator`, `DebitCardVerifyRequestValidator`, `RealTimePaymentSubmitRequestValidator`. Custom validators in the validator folder under models are auto-registered; implement `IValidator<T>` with `Errors Validate(T model)` and `errors.Add(nameof(model.Field), "Must be non-null and non-empty")` (namespace `Alkami.Providers.TransferPayments.Contracts.Models.Validators`). `Startup.cs` registers defaults; add interfaces there or in `IServiceCollectionExtensions.cs`. Logging: Serilog via injected `ILogger<T>` with structured params, e.g. `_logger.LogTrace("Retrieving User {userId}.", userId);`.

Testing: run Visual Studio as administrator (otherwise it fails to run); the console prints the URL as "Host Determined"; open `<url>/swagger/index.html`. JWT from Sidekick: Services Request > New WCF Request > Auth tab > bank identifier and user > Get JWT > Swagger Authenticate (401 = re-authenticate). Integration tests extend `IntegrationTestBase`, mock `IWcfFactory<IAccountServiceContract>` / `IWcfFactory<ISecurityServiceContract>` (`CreateAsync`, `ApplyClaimsIdentity`), set `_jwtBuilder` (`UserId`, `UserIdentifier`, `BankIdentifier`, `BankInstanceIdentifier`, `BankUrl`, `SessionId`), register `AccountService`, `SecurityService`, `LegacyServiceCallRPCWrapper<>` via `CreateClient(services => ...)`, and POST a `RealTimePaymentSubmitRequest` (`SourceAccountIdentifier`, `DestinationAccountIdentifier`, `Amount`, `FeeAmount`, `Reference`, `Date`) to `/real-time-payments` with `.SignAndApplyJwt(this, _jwtBuilder)`.

WCF calls use Filter (e.g. `GetAccountRequest.Filter.AccountIdentifiers`), Mapper (e.g. `IncludeRoutingInfo = true`), Sorter (`OrderByFields`, avoid non-indexed columns). Example: `ISecurityService.GetUserAsync(new GetUserRequest { Filter = new UserFilter { Ids = new List<long> { requestContext.Identity.UserId.Value } }, Mapping = new UserMapper { ShouldIncludeUserAccounts = true, ShouldIncludeExternalUserAccounts = true } })` in a `[Trace]` command.

Register the provider: the page's script (set `@ServicePath`) creates provider type `transfer-payments` ("Transfer Payments Provider"), a `core.Provider` named `@ServicePath` with `ServiceTypeId` from `core.ProviderServiceTypes` where `Description = 'Http'` and `ServicePath = @ServicePath`, and a `Connector` `core.Item` (`1.0.0.0`); it raises `No Item Found` on failure. A companion script deletes the item and provider. Both build a `<ServicePath>_role` CREATE/DROP ROLE statement in `@Sql` but never execute it and do not substitute `'@ServicePath'`; verify the role manually.

Sources: Create a Transfer Payment Provider (https://confluence.alkami.com/spaces/SDKC/pages/306061420)

## 7. Card Management

Merged from "Work with Card Management" (2023-05-31), "Copy of Working with Card Management" (2024-10-31; adds `CardBinsToInclude`, `GenericReward`, obsoletes `CardRewards`, adds CVV/PIN transport), and "Set Up Card Management" (2025-11-11). Prerequisites: Card Management Product Guide and the three "My First" tutorials. An FI can build its own card provider microservice while using Alkami's Card Management widget as UI.

**Set up the static provider (2025).** Needs a clean validated SDK with services running and page attachments `cardmanagement_packages.ps1`, `StaticCardManagementProviderConfiguration.sql`, `Card_Mapping.sql`, `CardManagementV2WidgetConfiguration.sql`, `Sample_CardJson.txt`, `CustomControlCardManagementProviderConfiguration.sql`. If the `card.*` tables are missing, the page attaches `card_CardAccount.sql`, `card_CardAlertContact.sql`, `card_CardSyncSource.sql`, `card_CardAttributesMapping.sql`, `card_CardDisplayMapping.sql`, `card_CardProviderCapabilityMapping.sql`, `card_UserCardActionType.sql`, `card_UserCardAction.sql`, `card_Users.sql`. After any reboot: `Restart-SDKServices;Ping-AlkamiWebsites`.
1. Run `CardManagementV2WidgetConfiguration.sql` with `COMMIT TRAN` uncommented and `ROLLBACK TRAN` commented.
2. Run `Card_Mapping.sql`.
3. Admin > Setup > Staff > Admin Roles > General Admin > check Manage Card Configuration and Manage Card Image > log out and in.
4. Copy `cardmanagement_packages.ps1` to `C:\`; admin PowerShell `cd\` then `.\cardmanagement_packages.ps1` (ignore warnings). Installs `Alkami.Admin.CardManagement`, `Alkami.Apps.CardManagementV2`, `Alkami.MicroServices.CardManagement.Service.Host`, `Alkami.MicroServices.CardManagementProviders.Static.Host`, `Alkami.MS.CardAccounts.Service.Host`.
5. Verify services `CardManagementService.Host`, `CardManagementProviders.Static.Host`, `CardAccounts.Service.Host` run; wait a minute.
6. Run `StaticCardManagementProviderConfiguration.sql` with `COMMIT TRAN`.
7. Run `CustomControlCardManagementProviderConfiguration.sql` (creates "Custom Control Card Management Provider" with `NumberOfControlsForCredit` = 20, `NumberOfControlsForDebit` = 20).
8. Admin > Settings > Integration Settings > Providers > CardManagement > Static Card Management Provider > Edit > paste `card_json.txt` into `CardsJSON` > Save Change.
9. Admin > Setup > Packages > Edit package > Widgets > Card ManagementV2 on.
10. `Restart-SDKServices;Ping-AlkamiWebsites` (about 10 minutes), then open `https://developer.dev.alkamitech.com/CardManagementv2`.

2023 page: `choco install Alkami.Apps.CardManagementV2 --version <n>`; register Area Name `CardManagementV2`, Assembly Info `Alkami.Client.Widgets.CardManagementV2`. Custom HTML display: `choco install Alkami.MS.CardManagementProviders.CustomControl.Host -y`; its script registers provider type `CardManagement`, provider `Custom Control Card Management Provider`, Connector item `2.0.0.0`, and MERGEs `@ProviderSettings` into `core.ItemSetting` (ships with `ROLLBACK TRAN` active).

**Custom provider.** `choco upgrade Alkami.SDK.Templates`; use the Card Management Provider VS template; in `ProviderScripts` run `insert_provider_setting.sql` (provider + settings) and `ExampleMappingConfiguration.sql` (capability mapping per card type; unmapped capabilities are unavailable to users). Verify one `core.Provider` row, two `core.Item` rows (`Localizable Resource`, `Connector`), one `core.ItemSetting` per setting. Build, debug (connects to the Subscription Service), test like the static provider.

**Contract methods.**
- `GetProviderCapabilitiesAsync(GetProviderCapabilitiesRequest) -> GetProviderCapabilitiesResponse`: required by the Admin Capability Mapping tab; Prism (orchestration) filters by mapping; capabilities map to `InitiateCardAction` actions.
- `GetCardsAsync(GetCardsRequest{CardTypes bitmap, Attributes bitmap, AccountIdentifiers List<Guid>}) -> GetCardsResponse{Cards}`; 2024 copy recommends a `CardBinsToInclude` BIN filter. Sample: `new ProviderRequest.GetCardsRequest { Attributes = configuration.CardAttributes, CardTypes = configuration.CardTypes, AccountIdentifiers = accountIdentifiers }`.
- `GetSupportedAttributesAsync(GetSupportedAttributesRequest) -> GetSupportedAttributesResponse{ProviderId, ItemList}`: attributes the provider can retrieve; Admin maps them to card types.
- `InitiateCardActionAsync(InitiateCardActionRequest) -> InitiateCardActionResponse{Card, TravelNotice[], BalanceTransfer, GeneratedCardControlUrl}`: `CardAction` (enum `CardActions`; Unknown throws) and `Card` (null throws) mandatory; `LimitKey`/`LimitAmount` (limit changes), `ReplacementReason` ("Lost", "Damaged", "Stolen"), `LastUsedDate`, `LostStolenDate`, `AreaLostCode` (two-digit state), `TravelNotice` (mandatory for `AddOrUpdateTravelNotice`), `BalanceTransfer` (mandatory for `TransferBalance`: `TransactionId`; mandatory `CardIssuerAccountNumber`, `TransferAmount`, `AddressLine1`, `Name`, `City`, `State`, `ZipCode`; optional `AddressLine2`), `AlertPreferences` (mandatory for `AddOrUpdateAlertRegistration`: `IsUpdate`, `ContactIds`). Derived: `AddOrUpdateCardAlertsActionRequest` (`CardAction` = `CardActions.AddOrUpdateCardAlerts` (25), `Card`, `CardAlerts`), `UpdatePinCardActionRequest` (mandatory strings `ExpirationDate`, `SecurityCode`; PIN read from cache).
- `PopulateCardsAsync(PopulateCardsRequest{Cards*, VisaPushProvisioningNonce (Push Provisioning), Attributes}) -> PopulateCardsResponse{Cards, ProviderId}`: Prism calls it when the mapping lacks card number/type attributes but maps others.

**Card model** (* mandatory): `CardIdentifier` (`CardAccount` maps CardNumber to it), `ExternalReference`*, `AccountIdentifier` (`core.Account`; required for Balance Transfer), `CardNumber`*, `CardType`*, `CardHolderDisplayName`, `CardTypeDescription`, `CustomCardImage` (`CardImage`), `ExpirationDate`*, `ActivationDate`, `IsBlocked`*, `ForeignTransactionsEnabled`, `IsChipped`, `IsRegistered`, `IsActivated`*, `AlertContactIds`, `Rewards` (`CardRewards`, obsolete: use `GenericRewards`), `GenericRewards` (`List<GenericReward>`), `CardLimits`, `TravelNotices`, `CardAlerts`, `CardControls`* (at least one control or the UI shows no actions even with capability mappings), `PinUpdateAttemptsLeft`, `AllowedTransactionRegions`, `Transactions`, `BillingAddress` and `CardEncryptedString` (required for Push Provisioning), `HasNaturalReissuance`, `PinUpdateCompleted`, `UnverifiedCardAttributes`. Sub-models: `GenericReward` (`FieldName`* enum `GenericRewardField` Field1..Field5 via SiteText, `DefaultFieldNameValue`, `Unit`* enum `GenericRewardUnit` Unit1..Unit5, `Value`*, `IsCurrency`); obsolete `CardRewards` (`Fields`*, `PrimaryRewardField`*, `HasRewards`) and `RewardField` (`FieldName`* `RewardFieldNames` PointsEarned=0, PointsRedeemed=1, PointsPending=2, RedeemablePoints=3; `Value`*; `DisplayName`); `CardLimit` (`LimitKey` e.g. "ATM"/"PerTransaction", `LimitName`, `LimitAmount`*, `LowerLimit`*, `UpperLimit`*, `LimitDisabled`*); `TravelNotice` (`ExternalReference`*, `DepartureDate`*, `ReturnDate`*, `Status` `TravelNoticeStatus` 1 Scheduled, 2 Active, 3 Completed, 4 Canceled, 0 removes; `Destinations`* of `TravelDestination` {`IsInternational`*, `CountryCode` ISO 3166-1 alpha-3, `StateCode`, `Destination`}; multiple only with `AllowMultipleDestinations` on `TravelNoticesControl`); `CardAlert` (`CardIdentifier`*, `Type` `CardAlertType`); `CardControl` (`RequiresRegistration`, `CardControlCategoryId`, `SupportedCardActionIds`; e.g. `AlertsRegistrationControl`, `AlertsControl`, `ActivationControl`); `TransactionRegion` (`ExternalReference`*, `RegionName`, `Coordinates` {`Latitude`*, `Longitude`*}, `Radius` for circle with one coordinate, `RegionType`; only Circle); `CardTransaction` (`TransactionDate`, `CardTransactionStatus`, `Amount`, `Description`, `Merchant`, `MerchantCity`, `MerchantState`, `MerchantCountry`); `BillingAddress` (`AddressLine1`, `AddressLine2`, `City`, `State`, `Country`, `PostalCode`).

**CVV and PIN (2024).** Both travel in `Card.CVV` (a PIN field is planned), encrypted with the key from the **"Alkami Mutual Client"** certificate and decrypted with the same key. Fetch CVV: provider encrypts into `Card.CVV`, UI decrypts. Update PIN: UI controller encrypts into `Card.CVV`, provider decrypts and updates.

Sources: Work with Card Management (https://confluence.alkami.com/spaces/SDKC/pages/241342567); Copy of Working with Card Management (https://confluence.alkami.com/spaces/SDKC/pages/400211549); Set Up Card Management (https://confluence.alkami.com/spaces/SDKC/pages/306057766)

## 8. 2FA and risk evaluation

Updated 2026-05-05. Prerequisites: Alkami Platform Machine Setup and the MFA Product Guide (https://midas.alkami.com/product-guide/multi-factor-authentication-mfa-product-guide/), which configures per-user risk evaluation. The widget builds a risk request and calls the controller extension `IsAllowedByRiskEvaluation()`; `Alkami.RiskEvaluation.WebExtension` (https://feeds.alkamitech.com/feeds/nuget.dev/Alkami.RiskEvaluation.WebExtension) renders the 2FA views.

Risk request types (`Alkami.MicroServices.Risk.Contracts.Requests`): `AccessP2PRiskRequest`, `AddAchAccountRiskRequest`, `AddBillPayPayeeRiskRequest`, `AddCrossAccountRiskRequest`, `ChangePasswordRiskRequest`, `ChangeUsernameRiskRequest`, `LoginRiskRequest`, `MakeBillPaymentRiskRequest`, `TransferRiskRequest`, `UpdateContactRiskRequest`, `UpdateProfileRiskRequest`, `UpdateSecurityRiskRequest`, `WireTransferRiskRequest`, `BusinessAuthorizeACHTemplateRiskRequest`, `BusinessAuthorizeTransferBaseRequest`, `BusinessAuthorizeExternalTransferRiskRequest`, `BusinessAuthorizeInternalTransferRiskRequest`, `BusinessAuthorizeWireTransferRiskRequest`, `BusinessManagementBaseRiskRequest`, `BusinessPayeeManagementRiskRequest`, `BusinessRoleManagementRiskRequest`, `BusinessSubmitACHTemplateRiskRequest`, `BusinessSubmitWireTransferRiskRequest`, `BusinessUpdateACHTemplateRiskRequest`, `BusinessUserManagementRiskRequest`.

Add NuGet `Alkami.RiskEvaluation.WebExtension` to the widget project and ship the libraries in the `.nuspec` (page versions, may be newer: `Alkami.MicroServices.Risk.Client` 1.31.0, `Alkami.MicroServices.Risk.Contracts` 1.31.0, `Alkami.RiskEvaluation.WebExtension` 2.30.1):
```xml
<file src="bin\Alkami.RiskEvaluation.WebExtension.*" target="lib" exclude="**\*.config"/>
<file src="bin\Alkami.MicroServices.Risk.*" target="lib" exclude="**\*.config"/>
```
View: include `@Html.IncludeSiteTextScript()`, a button `<button class="iris-button iris-button--primary" id="auth_2fa">Authorize 2FA</button>`, and on click call `ajaxSecurePost("/MySDKWidget/Auth2FA", '{}', success, error, function(){}, null, 'application/json');`. Controller:
```csharp
using Alkami.RiskEvaluation.WebExtension;
using Alkami.MicroServices.Risk.Contracts.Requests;

[HttpPost]
public JsonResult Auth2FA()
{
    try
    {
        var request = new UpdateContactRiskRequest()
        {
            //SessionInformation = new Dictionary<string, string>()   // extra parameters for the risk microservice
        };
        if (!this.IsAllowedByRiskEvaluation(request))
            return null;
        return Json(new { success = true });
    }
    catch (Exception e)
    {
        Logger.Error("Unable to Auth2FA", e);
        return Json(new { success = false, error = e.Message });
    }
}
```
In the SDK dev environment the prompt is questions (answer = last word of the question); other environments may use phone or SMS. No further prompt in that session.

Sources: Work with 2FA and Risk Evaluation (https://confluence.alkami.com/spaces/SDKC/pages/95496119)

## 9. Fake external accounts (DeveloperDynamic only)

Dated 2021-12-15. For developers without a stage-matched core-connected database (stage-match users sync external accounts from their core; Gold Partners use their test core).

Add script (against `DeveloperDynamic`; full column list on the page): `@AccountIdentifier = NEWID()`, `@AccountTypeID = 1534` (ALKAMIACHCHK), `@AccountNumber = '1111111111111111'`, balances 1234.56, `@InterestRate = 0.0000`, `@AccountHolder = 443459` (Mike Brady), `@CoreProviderID = 1`, `@MemberID = 4858`, `@DisplayName = 'External Test'`, `@ThemeColorIndex = 3`, `@RoutingNumber = '11111111111'`, `@BankName`, `@BankState = 'TX'`, `@BankCity = 'Dallas'`, `@DateOfLastRevision = '106170'`, `@ExternalAccountType = 0`. Steps: (1) insert `[core].[Account]` with `Discriminator = 'ExternalAccount'`, `Status` 0, `Locked` 0, dates `GETDATE()`, other columns NULL (the minimum set); `@AccountID = @@Identity`. (2) insert `[core].[UserAccount]` `(UserID, AccountID, AccessLevelID, DisplayName, MobileDisplayName, ThemeColorIndex, HasMasterRights, AddedByCore, BillPayAccountKey, CreateDate, Ordering, Deleted, Relationship, HideFromEndUser, HiddenByEndUser, RelationshipDescription, IsHiddenFromBillPay, IsBalancePeekEnabled, LastUpdate)` = `(@MemberID,@AccountID,NULL,@DisplayName,@DisplayName,@ThemeColorIndex,1,1,NULL,GETDATE(),7,0,3,0,0,NULL,0,0,NULL)`. (3) insert `[core].[RoutingNumberInfo]` `(RoutingNumber, BankName, CreateDate, RoutingNumberInfoType, NewRoutingNumber, IsBlockedForACH, IsBlockedForWires, BankShortName, BankState, BankCity, DateOfLastRevision, Deleted)` = `(@RoutingNumber,@BankName,GETDATE(),0,NULL,0,0,NULL,@BankState,@BankCity,@DateOfLastRevision,0)`; capture `@RoutingNumberInfoID`. (4) insert `[core].[ExternalAccount] (ID, RoutingNumberInfoID, ExternalAccountType)`. (5) `UPDATE [core].[Account] SET RemoteIdentifier = @RoutingNumberInfoID WHERE ID = @AccountID`.

Validate with MyMoney: in `USBFIMyMoneyController` build `new GetAccountRequest { Filter = new AccountFilter { Ids = accountIds, IncludeExternal = true }, Mapping = new AccountMapper { IncludeAccountType = true, IncludeRoutingInfo = true, AccountMaskSettings = new AccountMaskSettings { FormatOnlyAccountHolder = false, JoinAccountHolderNumberFormatString = BankSettings.GetSettingOrDefault(BankSettingName.JoinAccountHolderNumberFormatString, "{0}{1}"), PadOutput = false } } }`, `this.AugmentRequest(accountRequest)`, `AsyncHelper.RunSync(() => accountsService().GetAccountAsync(accountRequest))`, then `accountsResponse.Accounts.OfType<ExternalAccount>()`.

Remove (only for script-created accounts; otherwise a full SDK wipe may be needed): find `@AccountID` by `Number` and `@RoutingNumberInfoID` from `core.ExternalAccount`, then delete from `core.ExternalAccount`, `core.RoutingNumberInfo`, `core.UserAccount`, `core.Account`. The page's script selects `UserAccount` where `[AccountID] = @AccountNumber` (number, not id); use `@AccountID`.

Sources: Work with External Accounts (https://confluence.alkami.com/spaces/SDKC/pages/171909547)

## 10. Check imaging

Dated 2023-05-31. Prerequisites: Accounts Product Guide (https://midas.alkami.com/downloads/accounts-product-guide/) and the three "My First" tutorials. Images are hosted outside Alkami; a custom check imaging microservice retrieves them.
1. Enable the field for all account types:
```sql
IF NOT EXISTS (SELECT ID FROM CORE.TransactionDetailGeneralField WHERE Field = 'CheckImage')
insert into core.TransactionDetailGeneralField (field, ordering, CreateDate) values('CheckImage', 5, GETUTCDATE())
```
To restrict, insert `CheckImage` rows in `[core].[AccountTypeTransactionDetailField]` per `AccountTypeId` or `[core].[AccountTypeClassTransactionDetailField]` per class id.
2. Set `MyAccountsV2` widget setting `transaction_information_sections` (JSON in `CORE.WidgetSetting`; page script inserts or updates) to `[ { Name: "Summary", Title: "Summary", AccountTypeClassFilter: "*", Sections: [ { Title: "", ColumnsPerLine: "2", Columns: [ { Title: "Description", ExtendedPropertyName: "SpecificDescription", ExtendedPropertySource: "Transaction", FormatString: "", Order: "1" }, { Title: "Transaction ID", ExtendedPropertyName: "Id", ExtendedPropertySource: "Transaction", FormatString: "{0:G}", Order: "2" } ] } ] }, { Name: "CheckImages", Title: "Check Images", AccountTypeClassFilter: "*", Sections: [] } ]`. `Name` values are referenced by code; do not change.
3. Generic provider (passthrough from the legacy Bank Service to the proxy; translates `CheckImageDetail` to legacy `CheckImage`): `choco upgrade Alkami.App.Providers.CheckImaging.Generic -y`, then insert into `core.Provider` `(ProviderTypeID 10, Name 'Generic', Description 'Connect to the Check Imaging Proxy Microservice', AssemblyInfo 'Alkami.App.Providers.CheckImaging.Generic.Provider, Alkami.App.Providers.CheckImaging.Generic', CreateDate GETUTCDATE())` if not exists by AssemblyInfo.
4. Proxy (logical, no database; aggregates providers): `choco upgrade Alkami.MS.CheckImagingProxy.Service.Host -y`.
5. Static provider (stock images): `choco upgrade Alkami.MS.CheckImaging.Static.Service.Host -y`; run the page's registration script in commit mode: provider type `CheckImaging` (do not change), display `Check Imaging Provider`, provider `Alkami.MS.CheckImaging.Static`, description `Alkami MS CheckImaging Static`, empty AssemblyInfo, ticket `SDKCustom`, Connector item `1.0.0.0`.
6. `Stop-IISAndServices` then `Start-IISAndServices`.

Test: Microservice Tester > `ICheckImagingProxyContract` > `GetCheckImages` with at least one long in `Filter` (no filter fails validation); each item returns front, back, URI, transaction id, provider name. Expected warnings: missing Filter validator, empty sorter. End to end: `select * from core.Transactions where CheckNumber is not null` (descriptions contain "draft"); log in as carol.brady, Accounts widget, search "draft", open a transaction.

Custom provider: `choco upgrade Alkami.SDK.Templates -y`, check imaging VS template, run `insert_provider_setting.sql` from the `Service.Host` project unchanged (reuses the `CheckImaging` type), build, run. Admin integration settings list Generic, Static, and yours. Settings are optional; expect `BASEURL` plus auth; template settings `RequesterID` (`RQSTRID`), `InstitutionId`, `CUID`; remove unused ones. Test by stopping the "Alkami MS CheckImaging Static" service, restarting the proxy, and calling `GetCheckImages` through the proxy with a breakpoint. Default template output: a stock public image URI plus two empty image objects. To show URI-linked images set bank setting `OverrideCheckImagesWithUri` = true (test uses https://placeholder.com); the UI shows a link opening a new tab. Get Static working first; return binary or URI; calling a provider directly skips proxy aggregation.

Sources: Work with Check Imaging (https://confluence.alkami.com/spaces/SDKC/pages/241340179)

## 11. Business ACH and Wires

Updated 2024-10-03. After install, reboot and run `Restart-SDKServices`.

Business ACH:
```
choco upgrade -y Alkami.Apps.BusinessAdmin Alkami.Client.Widgets.BusinessAdmin.Users Alkami.MS.Username.Service.Host Alkami.Admin.Widgets.Operations Alkami.Admin.Widgets.OperationsSidebar Alkami.MicroServices.BusinessReportsData.Service.Host Alkami.MicroServices.BusinessEvents.Service.Host Alkami.MicroServices.Limits.Service.Host Alkami.MS.BusinessPayees.Service.Host Alkami.MS.PaymentCompany.Service.Host Alkami.Admin.Widgets.RetailACH Alkami.MicroServices.Reports.Export.Service.Host Alkami.MS.Reporting.BusinessReports.Service.Host Alkami.Apps.BusinessACH Alkami.MicroServices.AchTemplates.Service.Host Alkami.MicroServices.BusinessAchConfiguration.Service.Host Alkami.MicroServices.BusinessACHProcessing.Service.Host Alkami.MicroServices.NachaFileGenerator.Service.Host Alkami.App.Providers.ACH.NachaOutputBusiness Alkami.App.Providers.ACH.NachaOutput Alkami.MS.EM.Processor.ScheduledACH.Host Alkami.MS.OrbProxy.Service.Host
```
(The How-to index lists "Deprecate BusinessAchProcessing MS", June 2024; `Alkami.MicroServices.BusinessACHProcessing.Service.Host` may be deprecated.)

Business Wires (overlaps on purpose so either list alone is complete):
```
choco upgrade -y Alkami.Apps.BusinessAdmin Alkami.Client.Widgets.BusinessAdmin.Users Alkami.MS.Username.Service.Host Alkami.Admin.Widgets.Operations Alkami.Admin.Widgets.OperationsSidebar Alkami.MicroServices.BusinessEvents.Service.Host Alkami.MicroServices.Limits.Service.Host Alkami.MS.BusinessPayees.Service.Host Alkami.MS.PaymentCompany.Service.Host Alkami.Admin.Widgets.RetailACH Alkami.MicroServices.Reports.Export.Service.Host Alkami.MS.Reporting.BusinessReports.Service.Host Alkami.Services.Tenant Alkami.Services.BusinessSecurity Alkami.App.Processor.Wire.FedwireOutput Alkami.Apps.BusinessWires Alkami.MS.BusinessWires.Service.Host Alkami.MS.WireProcessor.WireXChange.Service.Host Alkami.MS.EM.Processor.ScheduledWire.Host Alkami.MS.EM.Processor.WireStatusChange.Host Alkami.MS.WireCustomization.Service.Host Alkami.MS.OrbProxy.Service.Host
```

Widget conversion: the page gives one script three times (in `DeveloperDynamic`, `ROLLBACK TRAN` active; uncomment `COMMIT TRAN`). It inserts or updates `core.Widget`, creates `dbo.LocalizableResource` `Widget.<Name>` with a `Localizable Resource` `core.Item` (SecondaryId 1033) holding `DisplayName` and `Description` item settings, optionally a `core.WidgetNotificationAction`, repoints `core.WidgetSetting`, `core.FlavorWidget`, `core.UserWidget`, `notify.UserWidgetNotificationPreference` from the old widget, then deletes the old widget and its changesets, item settings, item, and localizable resource.

| | Business ACH | Business Wires | Business Admin |
| --- | --- | --- | --- |
| `@OldWidgetName` | `AchPayments` | `WirePayments` | `BusinessAdministration` |
| `@WidgetName` | `BusinessACH` | `BusinessWires` | `BusinessAdmin` |
| `@WidgetDisplayName` | Business ACH | Business Wires | Business Admin |
| `@WidgetAssemblyInfo` | `Alkami.Client.Widgets.BusinessACH` | `Alkami.Client.Widgets.BusinessWires` | `Alkami.Client.Widgets.BusinessAdmin` |
| `@WidgetDescription` | Create and Manage ACH Payment Templates | Create Business Wires | Create admin roles, add users and payees. |
| `@WidgetIconName` | `ach-payments` | `wire-transfers` | `business-admin` |
| `@WidgetDisplaySettings` / `@WidgetType` | 7 / 7 | 7 / 4 | 7 / 0 |
| `@WidgetNotificationExists` / `IsBusinessOnly` | 1 / 1 | 1 / 1 | 0 / 1 |

Then Setup > Packages > Business Banking flavor > enable Business ACH and Business Wire widgets.

Providers: two scripts of the same shape (create `core.ProviderType` if missing, `core.Provider` with NULL `ServicePath`/`ServiceTypeId`, and a `core.Item` of ItemType `Processor`, version `1.0.0.0`; uncomment `COMMIT TRAN`). No output means already present; verify at Setup > Integration Settings > Providers.

| | Business ACH | Business Wires |
| --- | --- | --- |
| `@ProviderTypeName` | `ACHOutputFile` | `WireOutputFile` |
| `@ProviderTypeDisplayName` | ACH Output File Provider | Wire Output File Processor |
| `@ProviderName` | NACHA Business ACH Output Processor | Fedwire Output Processor |
| `@ProviderAssemblyInfo` | `Alkami.App.Providers.ACH.NachaOutputBusiness.Provider, Alkami.App.Providers.ACH.NachaOutputBusiness` | `Alkami.App.Processor.Wire.FedwireOutput.Processor,Alkami.App.Processor.Wire.FedwireOutput` |

Permissions: Admin > Setup > Staff > Admin Roles > General Admin > select all permissions on every tab > save > re-login. Business Master User (briefcase and crown icon, e.g. Alina Adamec): Support > Members > view details > Business Info > ACH Transaction Types > select all > save.

Sources: Set Up Business ACH and Wires (https://confluence.alkami.com/spaces/SDKC/pages/366021903)

## 12. Quick Apply

Dated 2023-05-30. Read the Quick Apply Product Guide (https://midas.alkami.com/product-guide/quick-apply-product-guide/) completely; the three "My First" tutorials are required. A custom provider is a microservice handling each workflow step; the client widget talks only to the aggregating Quick Apply microservice.
1. `choco upgrade Alkami.Apps.QuickApply`; register like TransferV2 with `core.Widget` values `(@BankID, 'QuickApply', 'Alkami.Client.Widgets.QuickApply', 0, 1, @icon, 'image/png', 1, 3, 0, 'quick-apply')` plus `core.FlavorWidget` rows.
2. `choco upgrade Alkami.Admin.Quickapply`.
3. `choco upgrade Alkami.MicroServices.QuickApply.Service.Host`; register provider `Quick Apply Service` under type `QuickApply`, AssemblyInfo `'Alkami.App.Providers.QuickApply, Alkami.App.Providers.QuickApply'`, with a `Connector` `core.Item` (`1.0.0.0`). The page script uses `@Debug BIT = 1` (rolls back); set 0 to commit.
4. `choco upgrade Alkami.MicroServices.QuickApply.Dynamic.Service.Host`; register `Dynamic Quick Apply Provider`, AssemblyInfo `'Alkami.App.Providers.QuickApply.Dynamic.Provider, Alkami.App.Providers.QuickApply.Dynamic'`, plus Connector item.
5. Test via product guide sections 3 (grant the SDK admin user application management), 4, 6, 8 (https://midas.alkami.com/product-guides/).

Custom provider: `choco upgrade Alkami.SDK.Templates`, Quick Apply Provider template, run `ProviderScripts\QuickApplyProvider.sql`, verify three providers, build and debug. The service implements **`IQuickApplyProviderContract`** (mandatory), delegating to `BusinessLogic` classes; settings use the `Data` folder pattern (none required). Actions: `GetJointOwnerAction` (optional), `GetProductsAction` (filtered products), `OpenAccountAction` (after workflow and disclosure; response `Decision`, `AccountNumber`, `ConfirmationNumber`), `StatusAction` (false hides products), `GetSupportedFieldsAction` (`AllowAllFields` true; leave it), `ValidateProductAction` (validate free-form `ProductID` against the core).

Sources: Work with Quick Apply (https://confluence.alkami.com/spaces/SDKC/pages/87015664)

## 13. AFX, CUFX, OrbFX and the OAuth workflow demo

Updated 2025-05-14; since SDK 2021.5 these may already be present. AFX V2 needs the Windows Hosting Bundle for ASP.NET Core 8.0 Runtime (8.0.16 at writing). Required for any API using Alkami API data (admin PowerShell): `choco install` each of `Alkami.Api.AFX`, `Alkami.Api.OrbFX`, `Alkami.Api.CUFX`, `Alkami.Api.AFX.V2`, `Alkami.Api.Admin`, `Alkami.Api.Fdx`, `Alkami.Services.Afx.User`, `Alkami.Services.Afx.Business`, `Alkami.Services.Afx.Account`. Swagger: https://developer.dev.alkamitech.com/CUFX/swagger/ui/index, https://developer.dev.alkamitech.com/AFX/V1/swagger/ui/index, https://developer.dev.alkamitech.com/OrbFx/swagger/ui/index. As needed: `Alkami.MicroServices.ApplicationSettings.Service.Host`, `Alkami.MicroServices.Payments.Service.Host`, `Alkami.MS.AccountsOrchestration.Service.Host`, `Alkami.Client.Widgets.OAuth`, `Alkami.Admin.Widgets.ApiApplications` (Registered Applications admin widget). Admin role: https://admin-developer.dev.alkamitech.com > Setup > Staff > Admin Roles > General Admin > Setup group > Manage Registered Applications > Save (repeat per role).

OAuth demo (2023-05-23): grant types `authorization_code` (RFC 6749 4.1) and `refresh_token` (RFC 6749 6). Third parties authorize the registered application, generate tokens, call the API, refresh tokens (OAuth 2.0 Product Guide: https://midas.alkami.com/product-guide/oauth-2-0-product-guide/). Before the demo install `Alkami.Admin.Widgets.ApiApplications` and `Alkami.Client.Widgets.OAuth`, run the attached `InsertAdminWidgetMenuWithWidgetCheck.sql` against DeveloperDynamic, reset IIS. Attachments not readable here: `OAuth Workflow.mp4`, `OAuth Workflow Demo.postman_collection.json`, `OAuth Workflow Demo.pptx`, the SQL script.

Sources: AFX Development (https://confluence.alkami.com/spaces/SDKC/pages/55346039); Enable AFX and CUFX in your Local Environment (https://confluence.alkami.com/spaces/SDKC/pages/135989624); OAuth Workflow Demo (https://confluence.alkami.com/spaces/SDKC/pages/55346046)

## 14. Prizeout API

Updated 2026-01-28. Base path `/third-party/v1`; one endpoint `GET /third-party/v1/prizeout` (Swagger tag `prizeout`, `[Authorize]`, header `Authorization: Bearer {token}`, no body). Creates a Prizeout session and returns a scoped URL, JWT, cashback balance, and widget info. 200 returns `ApiResponse<GetPrizeoutResponse>`; 401/403/500 return `ApiResponse` with `error`. `ApiResponse`: `isValid`, `statusCode`, `result`, `error` (`ErrorDetails`: `message`, `code` e.g. `AUTH_ERROR`), `timestamp` (UTC), `correlationId`. `GetPrizeoutResponse`: `productName` ("CashBack+"), `availableBalance` (string dollars), `prizeoutLogoUrl`, `fullPageUrl` (redirect), `layoutName` (e.g. `STACKED_CASHBACK_BALANCE_TEXT`), `offerItems` (`OfferItemDto[]`: `badge`, `imageUrl`, `headline`, `subject`, `progressBar` {`completionPercentage`, `label`}, `progressMetric` {`value`, `label`}, `footer`), `ctaLabel`. Implementation: `PrizeoutController`. The page reads as generated from source and documents only this endpoint.

Sources: Prizeout API Documenation (https://confluence.alkami.com/spaces/SDKC/pages/538186940)
