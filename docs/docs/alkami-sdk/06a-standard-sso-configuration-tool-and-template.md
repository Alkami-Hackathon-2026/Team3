# Standard SSO: Configuration Tool (Jarvis), Template, and Samples

**What this covers.** Alkami's Standard SSO (nicknamed "Jarvis") is a configuration-driven single sign-on engine: a JSON document in `sso.Configuration` tells the `Alkami.MicroServices.StandardSSO.Service.Host` microservice and the `Alkami.Client.Widget.StandardSso` widget how to build an HTTP GET/POST, form post, SAML assertion, or JWT for a third party with no custom code. This consolidates the legacy Jarvis page tree (2022 to 2025): schema, expressions, ConfigTool, Card Management and Transaction integration, deployment and SDK testing, SAML certificates, JWT/JWE, client redirects, the "Cert & Config install" submission, and the code-based alternatives (My First SSO Provider, Generic SSO Microservice, `Alkami.Utilities.Saml`). A newer 2026 "Standard-SSO" page tree is documented in the sibling file `06b-standard-sso-2026.md`; where the two conflict, prefer the 2026 pages.

## 1. Integration types and Standard SSO FAQ

The SDK "Integration Types" page lists the models to pick from before submitting an SDK Project Proposal (Feature Request Template): **Standard SSO** (predefined fields, one account at a time, no-code, several protocols via a configuration script; ask the third party for a sample URL, request body, or assertion first); **SDK Widget and Microservice Integration** (business logic before calling an external system; sample MyMoney); **SDK Provider Integration** (for example a card management provider; sample Working with Card Management); **SDK SSO Integration** (an SSO needing logic or fields unavailable in Standard SSO, such as bill pay checking account permissions; samples My First SSO Provider and Generic SSO Microservice, section 16); **SDK with API**, **API**, and **Custom Integration**.

Standard SSO FAQ: it builds most SSO integrations without a microservice or widget, via a SQL script holding all settings the Standard SSO microservice and widget need; it is available in the SDK; it supports SAML, HTTP GET, HTTP POST, and so on; it can send anything in the Expression Glossary; no code deployment is needed but Alkami must run the SQL script in the Alkami databases; signing and encryption certificates are supported.

Sources: Integration Types (https://confluence.alkami.com/spaces/SDKC/pages/273422381); Standard SSO - FAQs (https://confluence.alkami.com/spaces/SDKC/pages/334072176)

## 2. Components and packages

| Component | Package | Notes |
|---|---|---|
| Microservice | `Alkami.MicroServices.StandardSSO.Service.Host` (choco.dev) | Reads `sso.Configuration`, evaluates expressions, calls services. 1.6.6+ for Card Management. |
| Widget | `Alkami.Client.Widget.StandardSso` (choco.dev) | Slug `StandardSso/<ProviderName>`. 1.2.0+ for Card Management. |
| Configuration tool | `Alkami.Microservices.StandardSSO.ConfigTool` (choco.dev) | Design, validate, evaluate, export configs. 1.3.0+ for card features. |
| Validation library | `Alkami.MicroServices.StandardSSO.Engine` (nuget.dev) | .NET Standard 2.0; same validation as the service; must be kept in sync with the version inside the microservice. |
| Generic SSO contracts | `Alkami.MicroServices.SSOProviders.Contracts`, `Alkami.MicroServices.SSOProviders.Service.Client` | Code-based SSO providers (section 15). |
| SAML utility | `Alkami.Utilities.Saml` 1.0.2 (nuget.dev) | Code-based SAML (section 16). |

(The Card Management page's prerequisite table swaps the links for the widget and MS rows; the IDs above are correct.)

Sources: Standard SSO - Test In SDK Environments (https://confluence.alkami.com/spaces/SDKC/pages/156932234); Build a microservice using the Standard SSO template (https://confluence.alkami.com/spaces/SDKC/pages/203889369); Standard SSO - Card Management Integration (https://confluence.alkami.com/spaces/SDKC/pages/203894289)

## 3. Configuration JSON schema

Top level (required: `BankIdentifier`, `ProviderName`, `HttpRequest`):

| Property | Purpose |
|---|---|
| `BankIdentifier` | FI bank identifier GUID (`core.Bank.BankIdentifier`). |
| `ProviderName` | SSO name; also the widget name and URL segment `/StandardSso/<ProviderName>`. |
| `Description` | Free text. |
| `AccountConfiguration` | Restricts eligible accounts (3.1). |
| `BusinessUserConfiguration` | Restricts business sub users (3.2). |
| `CardConfiguration` | Card Management integration (section 9). |
| `HttpRequest` | The request the browser makes to the third party (ClientHttpRequest). |
| `Services` | Array of named server-side calls (`Http`, `Saml`, `Jwt`) used from expressions. |
| `Methods` | Map of named expressions computed once, reused as `@Name`. |
| `HashMethods`, `EncryptMethods`, `DeriveKeyMethods` | Named crypto definitions for `Hash`, `Encrypt`, `DeriveKey`. |

**ClientHttpRequest** (required `DisplayLocation`, `Uri`, `Method`, `ContentType`): `DisplayLocation` = `Inline` (iframe) or `NewWindow` (popup); `Uri` (relative or absolute, may be an expression); `QueryParameters`, `Headers`, `FormFields` (string maps); `Method` = `Get` or `Post` (default `Post`); `ContentType` (default `application/json`; use `application/x-www-form-urlencoded` with `FormFields`); `Body` (string). Real-world configs also set `"AutoLaunch": true`.

**Service** (required `Key`) holds one of:

- `Http`: `Request` (same as HttpRequest without DisplayLocation; required `Uri`, `Method`, `ContentType`) and `Response` with `ResponseType` (`Json`, `Xml`; real-world configs also use `Text`) and `OnSuccess` (JSONPath such as `$.access_token`, or XPath such as `/*[local-name()='Envelope']/...`) selecting the value returned by `Http('Key')`.
- `Saml`: `Request` (required `Issuer`, `Subject`, `Audiences`, `SigningParameters`, `AttributeFormat`) with `Recipient`, `Issuer`, `Domain`, `Subject`, `Audiences` (array), `AuthnContext`, `Attributes` (map), `AttributeFormat` (`Basic` default, `Uri`), `InitialTimestampShift` (int, default `-60`), `SigningParameters` (required `Thumbprint`, `SignatureLevel` = `Response` default or `Assertion`; optional `IncludeTransform`, `SignAssertionsOnly`, `CanonicalizationAlgorithm`; the Cenlar example also sets `SignatureMethod` `http://www.w3.org/2001/04/xmldsig-more#rsa-sha256` and `DigestMethod` `http://www.w3.org/2001/04/xmlenc#sha256`), `EncryptionParameters` (required `Thumbprint`; optional `IncludeCertificateInfo`, `SymmetricEncryptionAlgorithm`, `KeyEncryptionAlgorithm` such as `http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p`).
- `Jwt`: section 13.

**HashMethod** (required `Key`, `Encoding`, `Algorithm`): `SharedKey`; `Encoding` = `Base64`/`Hex`; `Algorithm` = `SHA256`, `SHA384`, `SHA512` (the older Jarvis schema also listed `MD5` and `SHA1`; the 2023 template page and glossary do not, so treat SHA256/384/512 as supported).

**EncryptMethod** (schema-required: `Key`, `Algorithm`, `SharedKey`, `IV`, `Mode`, `PaddingMode`, `KeyDerivationMethod`, `IVGenerationMode`; the RSA example omits most): `Algorithm` = `AES`/`TripleDES`/`RSA`; `KeySize`; `Mode` = `CBC`/`ECB`/`OFB`/`CFB`/`CTS`; `PaddingMode` = `None`/`PKCS7`/`Zeros`/`ANSIX923`/`ISO10126`; `KeyDerivationMethod` = `Guess`/`RawUTF8String`/`Base64String`/`RawBytes`; `IVGenerationMode` = `None`/`Binary`/`ASCII`; `InputEncoding` = `UTF8`/`Base64`; `Encoding` = `Base64`/`Hex`; for RSA, `Thumbprint` and `RSAPaddingMode` (`Pkcs1`, `OaepSHA1`, `OaepSHA256`, `OaepSHA384`, `OaepSHA512`). `SharedKey` and `IV` may be expressions.

### 3.1 AccountConfiguration

- `AllowedRelationshipTypes`: the schema declares enum names but the example uses numbers. `Unknown` 0, `PrimaryOwner` 1, `JointOwner` 2, `Linked` 3, `Aggregated` 4, `BusinessAccount` 5.
- `CoreNames`: core account type names, for example `["AlkamiCheckings", "AlkamiSavings"]` or `["SRL-EXT"]`.
- `Permissions`: account permissions, for example `["ViewStatements", "ViewTransactions"]`.
- `IncludeAccountTypes` / `ExcludeAccountTypes`: values must be from `AutoLoanAccount`, `BalanceOnlyAccount`, `BusinessAccount`, `CertificateAccount`, `ClosedEndedLoanAccount`, `CoreOnlyScheduledTransfer`, `CreditAccount`, `CreditCardAccount`, `DemandDepositAccount`, `ExternalAccount`, `MoneyMarketAccount`, `MortgageLoanAccount`, `OpenEndedLoanAccount`, `PayoffCalculatorDisplayed`, `SavingsAccount`, `InvestmentAccount`, `IRAAccount`, `HSAAccount`. Validation fails if a value is outside this list or in both lists. An account that is both an included `BusinessAccount` and an excluded `SavingsAccount` is excluded.

AccountConfiguration is applied twice: the account dropdown lists only accounts meeting the condition, and a preselected account (a redirect from MyAccountsV2 or CardManagement) is processed only if it meets the condition.

### 3.2 BusinessUserConfiguration

`RestrictSubUserAccess` (bool): if true, business sub users get an "Insufficient Permissions" error. `RolePermissions` (string list, used with the `ViewSummary` permission): all-or-nothing, the sub user must have ALL listed permissions. Example: `"BusinessUserConfiguration": { "RestrictSubUserAccess": "false", "RolePermissions": ["ViewEDocuments", "Rewards"] }`.

### 3.3 Expression-capable locations

`HttpRequest.Uri`, `HttpRequest.Body`, values of `HttpRequest.FormFields`/`Headers`/`QueryParameters`; values of `Methods`; `Services[].Http.Uri`, `.Body`, values of `.FormFields`/`.Headers`/`.QueryParameters`, `.Thumbprint`; `Services[].Saml.Request.Subject` and values of `.Attributes`. Examples also use expressions in `EncryptMethods[].SharedKey`/`IV` and in JWT field-level encryption.

Sources: Alkami Standard SSO Configuration Tool (Jarvis) (https://confluence.alkami.com/spaces/SDKC/pages/141527390); Build a microservice using the Standard SSO template (https://confluence.alkami.com/spaces/SDKC/pages/203889369)

## 4. Expression syntax

An expression is an operation to evaluate or a constant string; every expression produces a string.

- Operations are delimited by `{{` and `}}` with no spacing between the two braces. Text outside is literal: `Who is {{ FirstName }}?` gives `Who is John?`.
- Strings use single ticks or double quotes: `{{ 'Single Tick' }} vs. {{ "Double Tick" }}`. Escape by doubling: `{{ 'Bob said ''Does that work?''' }}`; or mix quote types: `{{ "Bob said 'Does that work?'" }}`.
- Function names are case-insensitive (`Append` = `append`); white-space tolerant (`{{Append('A','B')}}` = `{{ Append ( 'A' , 'B' ) }}`).
- Numbers are treated as strings: `{{ 12345 }}` gives `12345`.
- `@Name` references a `Methods` entry; the name must exist, be at least one character, and is case-insensitive. Methods are computed once and may reference other methods and services.
- Valid hash algorithms: SHA256, SHA384, SHA512. Valid encodings: Hex, Base64.
- Inside a JSON `Body` string, escape quotes: `"Body": "{\"First\":\"{{ FirstName }}\",\"Last\":\"{{ LastName }}\"}"`.

The 2022 (200260619) and 2024 (145509109) syntax pages are identical except the 2024 copy adds the hash algorithm and encoding lists.

Sources: Standard SSO - Expression Syntax (https://confluence.alkami.com/spaces/SDKC/pages/145509109); Standard SSO - Expressions Syntax (https://confluence.alkami.com/spaces/SDKC/pages/200260619)

## 5. Expression glossary (consolidated)

The 2024 Jarvis glossary (145509115) is a superset of the 2022 template glossary (200260612), which lacks `AccountDisplayName`, `CombineBase64Strings`, `CoreName`, `Master*`, `QueryString`, `RegexReplace`, `Relationship`, `SessionId`, `UserId`. This is the union. Date format strings follow C# DateTime format standards. "Account" expressions require a selected or preselected account; "card" expressions a selected card; "transaction" expressions the Transaction parameter (section 8).

**User** (primary contact preferred): `{{ UserId }}`, `{{ UserName }}`, `{{ DisplayName }}`, `{{ FirstName }}`, `{{ MiddleName }}`, `{{ LastName }}`, `{{ BirthDate('MM-dd-yyyy') }}`, `{{ TaxId }}` (unmasked, 9 digits), `{{ Email }}`, `{{ HomePhone }}`, `{{ MobilePhone }}`, `{{ WorkPhone }}`, `{{ PrimaryPhone }}` (falls back to the main phone contact), `{{ AddressLine1 }}`, `{{ AddressLine2 }}`, `{{ City }}`, `{{ State }}`, `{{ PostalCode }}`, `{{ Country }}`, `{{ MemberIdentifier }}` (use this for the Symitar MemberNumber, per FAQ), `{{ MemberNumber }}` ("MemberNumber" extended property), `{{ PersonId }}` ("ExternalKey" extended property), `{{ UserExtendedProperty('PropertyName') }}`, `{{ IpAddress }}` (`AAAA.BBBB.CCCC.DDDD`), `{{ SessionId }}` (from claims), `{{ ProductCategory }}` (input to the call), `{{ QueryString('ParameterName') }}` (query string passed to the widget).

**Business master user** (Business Master Users and Sub-users only): `{{ MasterUserId }}`, `{{ MasterUserName }}`, `{{ MasterDisplayName }}`, `{{ MasterFirstName }}`, `{{ MasterMiddleName }}`, `{{ MasterLastName }}`, `{{ MasterBirthDate('MM-dd-yyyy') }}`, `{{ MasterTaxId }}`, `{{ MasterEmail }}`, `{{ MasterPrimaryPhone }}`, `{{ MasterHomePhone }}`, `{{ MasterMobilePhone }}`, `{{ MasterWorkPhone }}`.

**Account**: `{{ AccountNumber }}` (unmasked), `{{ MaskedAccountNumber }}`, `{{ AccountHolder }}` (holder number), `{{ AccountDisplayName }}`, `{{ AccountExtendedProperty('PropertyName') }}`, `{{ CoreName }}` (for example `S:0001`), `{{ Relationship }}` (numeric, see 3.1), `{{ MicrNumber }}`, `{{ MortgageNumber }}`, `{{ CreditCardNumber }}`, `{{ CreditLimitAmount }}`, `{{ NextPaymentAmountDue }}`, `{{ NextPaymentDate('MM-dd-yyyy') }}`, `{{ PastDueAmount }}`, `{{ LateChargeAmountDue }}`.

**Card** (Card Management): `{{ CardNumber }}` (not the same as `CreditCardNumber`), `{{ CardDescription }}`, `{{ CardType }}` (unknown/debit/credit), `{{ CardHolderDisplayName }}`, `{{ CardActivationDate('MM-dd-yyyy') }}`, `{{ CardExpirationDate('MM-dd-yyyy') }}`, `{{ CardIsActivated }}`, `{{ CardIsBlocked }}`, `{{ CardIsChipped }}`, `{{ CardIsRegistered }}`, `{{ CardForeignTransactionsEnabled }}`, `{{ CardPinUpdateAttemptsLeft }}`.

**Transaction**: `{{ TransactionAmount }}`, `{{ TransactionBatchNumber }}`, `{{ TransactionCheckNumber }}`, `{{ TransactionCode }}`, `{{ TransactionCoreSequence }}`, `{{ TransactionCreateDate('MM-dd-yyyy') }}`, `{{ TransactionEffectiveDate('MM-dd-yyyy') }}`, `{{ TransactionPostingDate('MM-dd-yyyy') }}`, `{{ TransactionImageSequence }}`, `{{ TransactionImageSide }}` (needs the ImageSide parameter), `{{ TransactionMerchant }}`, `{{ TransactionMicr }}` (MICR from the transaction entry), `{{ TransactionSourceMicr }}` (MICR from the parameters passed in), `{{ TransactionPostingSequence }}`, `{{ TransactionRoutingNumber }}` (needs the RoutingNumber parameter), `{{ TransactionTraceNumber }}`.

**String and formatting functions**

| Expression | Behavior |
|---|---|
| `{{ Append('A', 'B', AccountHolder) }}` | Concatenate two or more arguments |
| `{{ Coalesce(City, State, 'Unknown') }}` | First non-null/empty/whitespace argument |
| `{{ Format('{0} {1}', 'Hello', 'World') }}` | String.Format semantics |
| `{{ Left('Input', 4) }}`, `{{ Right('Input', 4) }}` | N left/right chars (N > 0); shorter input returned as-is |
| `{{ Lower(x) }}`, `{{ Upper(x) }}`, `{{ Trim(x) }}`, `{{ TrimStart(x) }}`, `{{ TrimEnd(x) }}` | Case and whitespace |
| `{{ PadLeft('Input', 10) }}`, `{{ PadLeft('Input', 10, '-') }}`, `{{ PadRight(...) }}` | Pad to minimum length (>= 0), default pad char space |
| `{{ Mask('Input') }}`, `{{ Mask('Input', '#', 4, 8) }}` | Mask char (default `*`), chars kept on the right (default 4, > 0), total length (default 0 = unlimited, else must exceed arg 3) |
| `{{ Replace(CoreName, ':', '') }}` | Literal replace (`S:0001` becomes `S0001`) |
| `{{ RegexReplace(UserName, '[^a-zA-Z0-9_.]+', '') }}` | Regex replace (`UserName123!@#` becomes `UserName123`) |
| `{{ UrlEncode(x) }}`, `{{ Base64Encode(x) }}`, `{{ Base64Decode(x) }}`, `{{ HexEncode(x) }}`, `{{ HexDecode(x) }}` | Encodings (decode to UTF-8) |
| `{{ CombineBase64Strings('B64a', 'B64b', Encrypt('EM', X)) }}` | Decode each argument to bytes, concatenate the byte arrays, re-encode as one base64 string |
| `{{ FormatNumber('000-00-0000', TaxId) }}` | .NET numeric format (also `'0.00;0.00'`) |
| `{{ FormatDateTime('MM-dd-yyyy') }}`, `{{ FormatDateTime('MM-dd-yyyy', 'Central Standard Time') }}`, `{{ FormatDateTime('O') }}` | Current UTC time formatted; optional timezone id; `'O'` gives ISO 8601 |
| `{{ DateTimeOffset('MM-dd-yyyy', 'Day', 3, 'yyyyMMdd') }}` | Offset a datetime; unit is Year, Month, Day, Minute, or Second; arg 4 is the output format |
| `{{ UtcDateTime }}` | ISO 8601 UTC now (`2020-06-23T22:48:04.9851859Z`) |
| `{{ Guid }}` | Random GUID |
| `{{ RandomString(10) }}` | Random string of the given length (glossary misspells it `RandomsString`) |
| `{{ XPath('<xml><test>alkami</test></xml>', '/xml/test') }}` | XML node value (`alkami`); arg 1 may be an expression |

**Crypto and services**: `{{ Hash('HashName', Input) }}` (named HashMethods entry), `{{ Encrypt('EncryptName', Input) }}` (named EncryptMethods entry), `{{ DeriveKey('DeriveKeyName', 'Secret', 16, 32) }}` (key of size arg 3 from the secret and named DeriveKeyMethods entry; optional arg 4 offsets by that many bytes), `{{ Http('ServiceName') }}` (value selected by `Response.OnSuccess`), `{{ Saml('ServiceName') }}` (base64 SAML response), `{{ Jwt('ServiceName') }}` and `{{ Jwt('ServiceName', Expr) }}` (section 13).

Sources: Standard SSO - Expression Glossary (https://confluence.alkami.com/spaces/SDKC/pages/145509115); Standard SSO - Expressions Glossary (https://confluence.alkami.com/spaces/SDKC/pages/200260612); Standard SSO - Transaction Integration (https://confluence.alkami.com/spaces/SDKC/pages/200260917)

## 6. Configuration examples

The Jarvis page and the ConfigTool ship these named examples: Query Parameters and Headers, Business Configuration (two variants), Body, FormFields, AccountConfiguration, CardConfiguration, Method, Http Service, Saml Service, Hash, Encrypt (Base64 and Hex), Complex, SOAP, RSA Encrypt, Derive Key, AccountType. Fragments below share `"BankIdentifier": "27a6c31a-2b8f-48e9-869a-71b4cafcad93", "ProviderName": "Test Provider"`.

GET with query parameters and headers:

```json
{ "BankIdentifier": "27a6c31a-2b8f-48e9-869a-71b4cafcad93", "ProviderName": "Test Provider",
  "HttpRequest": { "DisplayLocation": "NewWindow", "Uri": "http://www.alkami.com", "Method": "Get", "ContentType": "application/json",
    "QueryParameters": { "FN": "{{ FirstName }}", "LN": "{{ LastName }}" },
    "Headers": { "H1": "{{ Mask('123456789') }}", "H2": "{{ '123456789' }}" } } }
```

Form post with chained methods:

```json
"HttpRequest": { "DisplayLocation": "NewWindow", "Uri": "http://www.alkami.com", "Method": "Post",
  "ContentType": "application/x-www-form-urlencoded",
  "FormFields": { "UserInfo": "{{ @UserInfo }}", "AccountInfo": "{{ @AccountInfo }}" } },
"Methods": { "Salt": "SaltValue", "SaltEncoded": "{{ UrlEncode(@Salt) }}",
  "UserInfo": "{{ Append(FirstName, @SaltEncoded, LastName) }}",
  "AccountInfo": "{{ Append(AccountNumber, @SaltEncoded, AccountHolder) }}" }
```

HTTP service (JSON) feeding a query parameter:

```json
"HttpRequest": { "DisplayLocation": "NewWindow", "Uri": "http://www.alkami.com", "Method": "Post", "ContentType": "application/json",
  "QueryParameters": { "Token": "{{ Http('http') }}" } },
"Services": [ { "Key": "http", "Http": {
  "Request": { "Uri": "http://alk-ncover/feeds/api/Package/Alkami.Api.CUFX", "Method": "Get", "ContentType": "application/json" },
  "Response": { "ResponseType": "Json", "OnSuccess": "$.packageId" } } } ]
```

SOAP service: `Request` with `"ContentType": "text/xml"`, a `"SOAPAction"` header, and an escaped `<s:Envelope ...>` body; `Response` with `"ResponseType": "Xml"` and `"OnSuccess": "/*[local-name()='Envelope']/*[local-name()='Body']/*[local-name()='SignOnResponse']/*[local-name()='SessionToken']"`.

SAML service posted as a form field:

```json
"HttpRequest": { "DisplayLocation": "NewWindow", "Uri": "http://www.alkami.com", "Method": "Post",
  "ContentType": "application/x-www-form-urlencoded", "FormFields": { "SamlResponse": "{{ Saml('SamlService') }}" } },
"Services": [ { "Key": "SamlService", "Saml": { "Request": {
  "Recipient": "recipient", "Issuer": "issuer", "Domain": "domain", "Subject": "subject",
  "Audiences": [ "audience1", "audience2" ], "AuthnContext": "authncontext", "Attributes": { "att1": "value1", "att2": "value2" },
  "SigningParameters": { "Thumbprint": "232b868ae474115ab5d7a299799f2228a84d41d6", "SignatureLevel": "Response", "SignAssertionsOnly": false },
  "EncryptionParameters": { "Thumbprint": "232b868ae474115ab5d7a299799f2228a84d41d6", "IncludeCertificateInfo": true },
  "AttributeFormat": "Basic", "InitialTimestampShift": -60 } } } ]
```

The real-world Cenlar config posts `SAMLResponse` plus `RelayState` to a Shibboleth `/Shibboleth.sso/SAML2/POST` endpoint, uses `AuthnContext` `urn:oasis:names:tc:SAML:2.0:ac:classes:unspecified`, and builds `Subject` as `{{ Append('C', @TaxId, @PreAccountNumber) }}` with `"TaxId": "{{ Right(@FullTaxId, 4) }}"` and `"PreAccountNumber": "{{ PadLeft(@PadAccountNumber, 10, '0') }}"`.

Hash, encrypt, RSA envelope, and derive key:

```json
"FormFields": { "TaxHash": "{{ Hash('MyHashKey', TaxId) }}", "EncryptMe": "{{ Append(Encrypt('EM', 'happy'), FirstName, LastName) }}",
  "EncryptedKey": "{{ Encrypt('RSA', @RandomKey) }}", "EncryptedAccount": "{{ Encrypt('TripleDES', AccountNumber) }}" },
"Headers": { "Authorization": "{{ DeriveKey('MyDeriveKeyMethod', 'SecretPassword', 16, 32) }}" },
"Methods": { "RandomKey": "{{ RandomString(32) }}", "RandomIV": "{{ RandomString(8) }}" },
"HashMethods": [ { "Key": "MyHashKey", "SharedKey": "SuperSecret", "Encoding": "Base64", "Algorithm": "SHA256" } ],
"EncryptMethods": [
  { "Key": "EM", "Algorithm": "TripleDES", "SharedKey": "SharedKey", "IV": "12345678", "KeySize": 192, "Mode": "CBC",
    "PaddingMode": "PKCS7", "KeyDerivationMethod": "RawUTF8String", "IVGenerationMode": "None", "Encoding": "Base64" },
  { "Key": "RSA", "Algorithm": "RSA", "Thumbprint": "aa845e7cb2f1ccaf1f966539e0ec459b7517b2c1", "RSAPaddingMode": "Pkcs1" },
  { "Key": "TripleDES", "Algorithm": "TripleDES", "SharedKey": "{{ @RandomKey }}", "IV": "{{ @RandomIV }}", "KeySize": 192,
    "Mode": "CBC", "PaddingMode": "PKCS7", "KeyDerivationMethod": "RawUTF8String", "Encoding": "Base64" } ],
"DeriveKeyMethods": [ { "Key": "MyDeriveKeyMethod", "Salt": "AlkamiTechnology", "SaltEncoding": "UTF8", "Iterations": "1000", "HashEncoding": "Base64" } ]
```

Complex chaining from the Complex example: `"Method3": "{{ Append(Mask(AccountNumber), '-', Mask(AccountHolder, '#', 3, 7)) }}"`, `"Method4": "{{ Coalesce(@Method5, Http('http')) }}"`, `"Method5": "{{ AccountExtendedProperty('not_present') }}"`. Account type filter: `"AccountConfiguration": { "IncludeAccountTypes": [ "AssetAccount", "CertificateAccount" ], "ExcludeAccountTypes": [ "DemandDepositAccount" ] }` with `"Uri": "https://www.alkami.com?Test={{ AccountNumber }}"`.

Real-world patterns on the Jarvis page: Equishare check images (GET with `{{ TransactionSourceMicr }}`, `{{ FormatNumber('0.00;0.00', TransactionAmount) }}`, `{{ TransactionCheckNumber }}`, `{{ TransactionImageSide }}`); CashPlease (a `token` service returning `$.access_token` used as `"authorization": "Bearer {{ Http('token') }}"` on a second service returning `$.RedirectURL_CP`, which becomes the top-level `"Uri": "{{ Http('http') }}"`); FICS eStatus (SOAP); InstantOpenV2 (`"ResponseType": "Text"` service whose body is the redirect URL); UAS Student Loan (`"CoreNames": ["SRL-EXT"]`, `"loan_id": "{{ AccountNumber }}"`, `$.result.url`). Placeholders are written `<< Replace Me >>`.

Sources: Alkami Standard SSO Configuration Tool (Jarvis) (https://confluence.alkami.com/spaces/SDKC/pages/141527390); Build a microservice using the Standard SSO template (https://confluence.alkami.com/spaces/SDKC/pages/203889369)

## 7. The Configuration Tool (ConfigTool)

`Alkami.Microservices.StandardSSO.ConfigTool` provides a visual designer, validation using the Standard SSO Engine (checking syntax with it is highly recommended), evaluation with real or mocked responses (Input Data tab), bundled example configurations, and export of the resulting request as cURL (importable into Postman). If the result's URI is relative it uses the Request's `RelativeUriHost` from the Input Data tab to build an absolute URI.

```
choco install Alkami.Microservices.StandardSSO.ConfigTool -y
choco upgrade Alkami.Microservices.StandardSSO.ConfigTool -y
```

Add `--version {versionNumber}` to pin a version. To test: open the tool (any sample request file can be opened), open the sample JSON under the Raw JSON tab, paste your config and click Validate (shows Success), add values under Input Data, click Execute to create the SSO Url, then paste the Url into a browser or Copy to Clipboard and test in Postman. Engine validation note: `HttpRequest.Uri` allows relative or absolute URIs, so validation only checks it is not null or whitespace.

Sources: Standard SSO - Configuration Tool (https://confluence.alkami.com/spaces/SDKC/pages/200260539); Alkami Standard SSO Configuration Tool (Jarvis) (https://confluence.alkami.com/spaces/SDKC/pages/141527390)

## 8. Transaction integration (check imaging)

Built for check imaging providers. The Transaction data source is NOT available from the Standard SSO widget; it works only when the microservice is called directly with the `Transaction` request parameter:

```csharp
public class Transaction
{
    [DataMember(EmitDefaultValue = true)] public long Id { get; set; }
    [DataMember(EmitDefaultValue = true)] public string Micr { get; set; }
    [DataMember(EmitDefaultValue = true)] public string RoutingNumber { get; set; }
    [DataMember(EmitDefaultValue = true)] public string ImageSide { get; set; }
}
```

Example: `"Transaction": { "Id": 1111, "Micr": "2222", "RoutingNumber": "111222233", "ImageSide": "Front" }`. Prerequisites: Standard SSO MS, ConfigTool, and `Alkami.MicroServices.Transactions.Service.Host`; base install requirements are on How to Install Microservices, versions from the Product Owner spreadsheet. A working-session video is linked (Google Drive).

Sources: Standard SSO - Transaction Integration (https://confluence.alkami.com/spaces/SDKC/pages/200260917)

## 9. Card Management integration

Card Management has two retrieval methods. **GetCards** (used by the Card Management Widget) aggregates all providers, stores cards in the database, and populates `CardIdentifier`. **GetCardNumbers** calls only providers supporting the `CardNumbers` attribute, does no aggregation, does not store cards, and does not populate `CardIdentifier`; supported core: SymConnect only. Only the SymConnect provider maps cards to accounts via `AccountIdentifier`; it filters by `AccountIdentifiers` when present, otherwise by the `SUPPORTEDCARDTYPES` item setting.

`CardConfiguration` (all optional):

| Setting | Default | Values | Notes |
|---|---|---|---|
| `CardSource` | `GetCards` | `GetCards`, `GetCardNumbers` | Schema enum says `GetCardsAsync`/`GetCardNumbersAsync`; examples use `GetCardNumbers`. Use `GetCards` when arriving from the Card Management Widget; `GetCardNumbers` when the FI does not use the widget. |
| `CardDesignator` | `CardIdentifier` | `CardIdentifier`, `ExternalReference` | Key returned with masked card numbers and matched against PRISM. `CardIdentifier` unavailable with `GetCardNumbers`; `ExternalReference` only with SymConnect. |
| `FilterOnAccounts` | `true` | `true`, `false` | `GetCards` does not support account filtering: set `false` with `GetCards` (SymConnect falls back to `SUPPORTEDCARDTYPES`). |

Prerequisites: `Alkami.MicroServices.CardManagement.Service.Host` 7.0.0+ (required), `Alkami.MicroServices.CardManagementProviders.SymConnect.Host` 6.0.0+ (required), Standard SSO MS 1.6.6+ (required), widget 1.2.0+ (optional), ConfigTool 1.3.0+, `Alkami.Apps.CardManagement` widget (optional). Required Symitar RepGens: `SYC.GETCARDINFO.ALKAMI.V1` (2021.05.28), `SYC.GETCARDINFO.ALKAMI.PRO` (2021.03.22), `ALKAMI.COMMON.PRO`, `ALKAMI.COMMON.DEF` (bitbucket CORECUSTOM/symitarrepgens). An outdated custom RepGen needs Product involvement and a Carbon dev ticket.

Example: `"CardConfiguration": { "CardSource": "GetCardNumbers", "CardDesignator": "ExternalReference", "FilterOnAccounts": true }` with form fields such as `"CardNumber": "{{ CardNumber }}"`, `"CardType": "{{ CardType }}"`, `"CardExpirationDate": "{{ CardExpirationDate('yyyy-MM-dd') }}"`. Sequence diagrams (Lucidchart) and a training recording are linked; related Jira DEV-113999, DEV-111514, DEV-115279.

Sources: Standard SSO - Card Management Integration (https://confluence.alkami.com/spaces/SDKC/pages/203894289)

## 10. Deployment and testing in the SDK

Environment setup: install the widget and microservice, then run "StandardSSO Configuration Widget Template.sql" (attachment on the Deployment page; reproduced in full on the Test In SDK Environments page). It adds rows to `sso.Configuration` (JSON config), `sso.ConfigurationHistory`, and `core.Widget` (Slug and matching Name).

```
choco install Alkami.MicroServices.StandardSSO.Service.Host
-- then set this service to run automatically if you want to
choco install Alkami.Client.Widget.StandardSso
```

What the template script does (database `DeveloperDynamic`; it ends with `ROLLBACK` and a commented `--COMMIT`, so switch to commit once the output is right):

- Variables: `@ProviderName` (example `'SampleSSO'`), `@JarvisWidgetAssembly = 'Alkami.Client.Widget.StandardSso'`, `@BankIdentifier = (SELECT BankIdentifier FROM core.Bank)`, `@Display` (`NewWindow`/`Inline`), `@WidgetDisplayName`, `@WidgetDescription`, `@WidgetDisplaySetting INT = 7`, `@WidgetIconName = 'generic-icon-coins'`, `@AddedByDefault = 1`, `@FavedByDefault = 0`, `@InitialConfig BIT = 1` (0 also inserts `core.UserWidget` rows for existing users).
- `@Value NVARCHAR(MAX)` holds the JSON; the script checks `ISJSON(value)` and raises `'BAD JSON! BAD!!'` if invalid.
- MERGEs into `sso.Configuration` (`ProviderName`, `Value`, `CreatedDate`, `LastModifiedDate`); `core.Widget` (Name = `@ProviderName`, AssemblyInfo = `Alkami.Client.Widget.StandardSso`, Slug = `'StandardSso/' + @ProviderName`, DisplaySettings 7, WidgetType 0); `dbo.LocalizableResource` (`'Widget.' + @ProviderName`, `'SiteText.Alkami.Client.Widget.StandardSso.' + @ProviderName`); `core.Item` (ItemType `'Localizable Resource'`, SecondaryId 1033); `core.ItemSetting` (widget `DisplayName`, `Description`, and SiteText keys `SSO.Button.Back`, `SSO.Button.Open`, `SSO.Error.*` (Title, Message, Button.Text, EmailMissing, NoEligibleAccounts), `SSO.Index.Button.Text`, `SSO.Index.Header`, `SSO.Index.Message`, `SSO.ThirdPartyCookieLink.*`, `SSO.Title.Text`); `core.FlavorWidget` for every flavor except `Master`.

Minimal config used by the script: `"HttpRequest": { "DisplayLocation": "NewWindow", "Uri": "http://google.com/", "Method": "Get", "ContentType": "application/json" }` with the standard `BankIdentifier`/`ProviderName`.

Access in the member site: add a navigation item manually (see "Updating your local navigation builder configurations", page 241342114) or browse to `https://developer.dev.alkamitech.com/StandardSSO/<ProviderName>`.

Sources: Standard SSO - Deployment (https://confluence.alkami.com/spaces/SDKC/pages/200260545); Standard SSO - Test In SDK Environments (https://confluence.alkami.com/spaces/SDKC/pages/156932234)

## 11. Display and navigation query parameters

When linking to a Standard SSO from another widget (Accounts "Pay Now", Quick Apply, Card Management):

- `displayMethod` overrides the config's `DisplayLocation`: `SameWindow` (same tab, `_self`; only a spinner shows in the widget; recommended), `NewWindow` (`_blank`), `Inline` (iframe). On native, or with a preselected account, the display method is always SameWindow except Inline with a preselected account; add the parameter anyway to get only the spinner.
- `backUrl=WidgetToGoBackTo` returns the user to the originating widget on back/close. Chromium browsers do not add quickly-redirected pages to history, but keep `backUrl` for other browsers. The Flutter native app uses `webviewActive`/`webviewInactive` events for this, but the 2023 page notes it does not work as expected on iOS/Android (DEV-118319); the 2024 Jarvis page says not applicable to native as of 3/3/2021.
- iOS: add the query parameter `"target": "browser"` in the config to open in the browser instead of the in-app WebView.

```html
<a href="/StandardSso/JarvisTest?displayMethod=SameWindow&backUrl=MyAccountsV2" target="_blank"></a>
<a href="/StandardSso/JarvisTest?displayMethod=SameWindow&backUrl=MyAccountsV2" target="_self"></a>
<a href="/StandardSso/JarvisTest?displayMethod=Inline&backUrl=MyAccountsV2" target="_blank"></a>
```

Recommended iPhone/iPad `mobile_pay_now_account_types` entry: `[ { "CoreName": "T:MORT", "Url": "/Mobile/StandardSso/JarvisTest?displayMethod=SameWindow&backUrl=MyAccountsV2", "Target": "_self", "AccountParams": "MortgageNumber" } ]`.

FAQ: the widget always auto-launches unless an account-selection step is required; multiple buttons to different sites is a Quick Links use case (or separate Standard SSO widgets or a custom widget).

Sources: Alkami Standard SSO Configuration Tool (Jarvis) (https://confluence.alkami.com/spaces/SDKC/pages/141527390); Build a microservice using the Standard SSO template (https://confluence.alkami.com/spaces/SDKC/pages/203889369)

## 12. Client redirects (iframe SSOs)

Client redirects let an SSO hosted in an iframe navigate the user elsewhere in online banking in response to `postMessage` events. Supported only for the `Inline` display method. Configuration lives entirely in the widget setting `ClientRedirects` (a JSON list of ClientRedirect objects, no limit; no JSON config change). All four properties are required, no additional properties: `EventName` (any string, case-insensitive), `Origin` (host only such as `www.ssourl.com`, case-insensitive), `WebDestination` (OLB subpath such as `/MyAccountsV2`; `/Mobile` and leading slash are added if needed), `NativeDestination` (`Accounts` = app home, `LastPage` = close SSO and return to prior page).

```json
[ { "EventName": "redirectToDashboard", "Origin": "www.ssourl.com", "WebDestination": "/DashboardV2", "NativeDestination": "LastPage" },
  { "EventName": "redirectToAccounts", "Origin": "www.ssourl.com", "WebDestination": "/MyAccountsV2", "NativeDestination": "Accounts" } ]
```

Vendor side: `window.parent.postMessage` with the data being just the event name and the target being the entire URL of the SSO widget. This page (updated 2026-04-09) mirrors the 2026 Standard-SSO client redirects page in 06b.

Sources: Standard SSO - Client Redirects (https://confluence.alkami.com/spaces/SDKC/pages/557260262)

## 13. JWT and JWE services

A `Services` entry with `Jwt.Request` produces a token referenced as `{{ Jwt('Key') }}`.

Signed JWT: `Payload` (required; string key/value map), `Headers` (optional; default header has `alg` = SigningAlgorithm and `type` = `jwt`), `SigningAlgorithm` (required: `HS256`, `HS384`, `HS512`, `RS256`, `RS384`, `RS512`, `PS256`, `PS384`, `PS512`, `ES256`, `ES384`, `ES512`), `TokenExpiryInMinutes` (required), `CertificateThumbprint` (required for RS/PS/ES), `HMACSecretKey` (required for HS; minimum length HS256 32, HS384 48, HS512 64). Signing certificates should be generated by Alkami (private key .pfx proves the issuer).

JWE with a vendor JWK (`kty`, `kid`, `use`, `alg`, `e`, `n`; accepted `alg`: `RSA1_5`, `RSA_OAEP`, `RSA_OAEP_256`) and required `JweEncryptionAlgorithm` (`A128CBC_HS256`, `A192CBC_HS384`, `A256CBC_HS512`, `A128GCM`, `A192GCM`, `A256GCM`):

```json
"HttpRequest": { "DisplayLocation": "NewWindow", "Uri": "https://onboard-api-uat.cotribute.co/prefill-requests?", "Method": "Post",
  "ContentType": "application/json", "QueryParameters": { "JWT": "{{ Jwt('JwtToken') }}" } },
"Services": [ { "Key": "JwtToken", "Jwt": { "Request": {
  "Payload": { "prefillTemplateUuid": "SomePayloadValue", "completionRedirectURL": "url.com" },
  "JWK": { "kty": "RSA", "kid": "AnExampleKid", "use": "enc", "alg": "RSA_OAEP", "e": "AQAB", "n": "AnNValueHere" },
  "TokenExpiryInMinutes": "60", "JweEncryptionAlgorithm": "A256GCM" } } } ]
```

JWE with a certificate: `Payload`, `CertificateThumbprint` (signing and encryption), `CertKeyType` (`RSA1_5`, `RSA_OAEP`, `RSA_OAEP_256`), `JweEncryptionAlgorithm`, for example `"CertificateThumbprint": "kjhhdsfhah32hdjshfjh3jh", "CertKeyType": "RSA_OAEP_256", "JweEncryptionAlgorithm": "A256GCM"`.

Field-level encryption: pass the value as a second argument to encrypt only that field, for example `"ssn": "{{ Jwt('JwtToken', TaxId) }}"`, `"birthday": "{{ Jwt('JwtToken', BirthDate('yyyy-MM-dd')) }}"`, `"zip": "{{ Jwt('JwtToken', PostalCode) }}"` in `FormFields`.

Sources: Standard SSO - JWT (https://confluence.alkami.com/spaces/SDKC/pages/482971236)

## 14. Certificates for SAML in your SDK

Standard SSO identifies certificates by thumbprint. For SAML, Alkami is the IdP, so you need a certificate with a private key for signing; the SP usually supplies a public-key-only certificate for encryption. Video: YouTube eBVPL77JFQM, or Alkami University > SDK Tutorials > Certificates for SAML SSO.

1. Install OpenSSL for Windows (https://slproweb.com/products/Win32OpenSSL.html; the page used 3.0.0.0) and open the Win64 OpenSSL Command Prompt.
2. Generate keys (730-day expiry; Common Name used: `developer.dev.alkamitech.com`). Give the public key file to the SP; protect the private key.

```
openssl req -new -newkey rsa:4096 -nodes -sha256 -keyout "C:\SSO\Alkami_SampleSso_signing_private_key.crt" -x509 -days 730 -out "C:\SSO\Alkami_SampleSso_signing_public_key.crt"
```

3. Combine into a password-protected PFX:

```
openssl pkcs12 -export -in C:\SSO\Alkami_SampleSso_signing_public_key.crt -inkey C:\SSO\Alkami_SampleSso_signing_private_key.crt -out C:\SSO\Alkami_SampleSso_Full_Certificate.pfx
```

4. `mmc` > File > Add/Remove Snap-in > Certificates > Computer account > Local computer. Personal > Certificates > All Tasks > Import the .pfx (enter the password; marking exportable is optional and must NOT be done for production keys on an unsecured machine), into the Personal store.
5. Self-signed: copy the certificate from Personal into Trusted Root Certification Authorities.
6. Right-click the certificate > All Tasks > Manage Private Keys and add the local computer's `Users` group (on a domain, change the search location to the computer).
7. Vendor encryption certificate (.crt, public key only, no password): import into Personal, then copy into Trusted People. No private-key permissions needed.
8. Thumbprint: certificate Details tab > Thumbprint; copy into the configuration.

The "Work with SAML" page's variant for code-based SAML requires SHA256, RSA 2048, and CSP `Microsoft Enhanced RSA and AES Cryptographic Provider` when building the PKCS12 archive (note the key filename mismatch between the two commands on that page):

```
openssl.exe req -x509 -nodes -sha256 -days 3650 -subj "/CN=Local" -newkey rsa:2048 -keyout Local.key -out MyLocalCert.crt
openssl.exe pkcs12 -export -in MyLocalCert.crt -inkey MyLocalCert.key -CSP "Microsoft Enhanced RSA and AES Cryptographic Provider" -out MyLocalCert.pfx
```

Sources: Certificates for SAML SSO configuration in your SDK (https://confluence.alkami.com/spaces/SDKC/pages/172102677); Work with SAML (https://confluence.alkami.com/spaces/SDKC/pages/87017032)

## 15. Submitting a Standard SSO: Cert & Config install

For NEW implementations only (updates use a standard support ticket). Covers certificate creation/install and running the Jarvis configuration script; it cannot deploy custom code.

1. Create the ticket like a standard microservice issue with the Standard SSO component type. If a package URL is required, enter any URL; it is not used.
2. Attach the SQL statement containing the Jarvis config as `init.Jarvis.sql`. Automation scans it (about 30 minutes) and comments with instructions.
3. Automation determines needed certificates and posts naming instructions (example prefix CSTEST is a test project; use your own). Alkami provides the signing certificate; the vendor provides the encryption certificate if needed.
4. Internal Alkami tickets are created for other departments; they may take several days and may not be visible to you.

SAML endpoints must be whitelisted: staging is restricted by egress rules, so supply the host names your SAML implementation calls with your SDK project submission.

Sources: Standard SSO Cert & Config install (https://confluence.alkami.com/spaces/SDKC/pages/241348346); Work with SAML (https://confluence.alkami.com/spaces/SDKC/pages/87017032)

## 16. Code-based alternative: My First SSO Provider and Generic SSO Microservice

When Standard SSO cannot express the logic, build an SSO Provider microservice implementing the Generic SSO contract. Widgets call it to log users into associated applications; settings are managed in the Admin Portal under Setup > Integration Settings > Providers.

1. Close Visual Studio, then `choco install Alkami.SDK.Templates -y` (runs VisualStudioInstaller.exe).
2. Run Visual Studio as Administrator (needed to attach the debugger). New project, search "SSO", select **Alkami Generic SSO Service**. Name per the Microservice Coding Guidelines (example `USBFI.MS.MyCustomSSO`) in a path near `C:\`.
3. Build (restores NuGet; feed credentials are the same as https://feeds.alkamitech.com/). Service.Host must be the Startup Project.
4. Run `insert_provider_setting.sql` (Service.Host project) against `DeveloperDynamic`. Local only: submit a Provider Configuration ticket to the SDK team for stage/production.
5. `Alkami.Services.Subscriptions.Host` must be running. Break on the first line of `SsoServiceImp.GetOptions()` in `SsoServiceImp.cs`, press F5 (Topshelf console host).
6. `choco install Alkami.MicroServiceTester -y`; open microservicetester, double-click the service, select `GetOptions`, Send Message; execution pauses on the breakpoint.
7. Unit tests need the NUnit 3 Test Adapter. The template test `GetSso_WithValidProviderID_Success()` uses `DistributedService.cs` to call `GetSso()` and asserts a non-empty `Url` and a `DisplayMethod`.
8. Update `SsoServiceImp.cs` so `GetSso` and `GetOptions` build the vendor's SSO URL; rebuild; verify with the Tests project and the Microservice Tester.

Contract (`Alkami.MicroServices.SSOProviders.Contracts`):

```csharp
public interface ISsoProvidersContract
{
    Task<GetOptionsResponse> GetOptions(GetOptionsRequest request);
    Task<GetSsoResponse> GetSso(GetSsoRequest request);
    Task<CanProcessResponse> CanProcess(CanProcessRequest request);
}
```

Request types derive from `SsoBaseRequest : BaseRequest` (`long? ProviderId`, `string ProviderKey`): `CanProcessRequest` (no extra members), `GetOptionsRequest` (`IDictionary<string, string> Parameters`), `GetSsoRequest` (`Parameters`, `Guid? AccountIdentifier`, `string ClientIpAddress`). Response types derive from `SsoBaseResponse : BaseResponse` (`long ProviderId`): `CanProcessResponse` (`bool CanProcess`), `GetOptionsResponse` (`IDictionary<Guid, string> Parameters`), `GetSsoResponse` (`string Url`, `IDictionary<string, string> Parameters`, `DisplayMethod DisplayMethod`). `enum DisplayMethod { Popup, Iframe }`.

Troubleshooting: verify the implementation against the vendor spec and the setting values; contact the provider for Unauthorized, Access Denied, or File Not Found errors. A certificate problem logs `Certificate named '57315D3356625180E1204D88FCB2B07A228EB7E0' was not found in Personal Store Local Machine`: confirm it is installed, not expired, and the private key has permissions. If the site cannot be reached, paste the Base URL from provider settings into a browser.

Generic SSO Microservice page: several Alkami widgets call this contract and several Alkami microservices implement it; build a custom widget calling an Alkami SSO microservice, or a custom microservice from the template. Samples attached to that page: `Sample.Client.Widget.GenericSsoDemo.7z`, `Sample.MS.MyCustomSSO.7z`. A package in the feed is not automatically permitted in production; some have contractual or licensing requirements (check with your CSM).

Sources: My First SSO Provider (https://confluence.alkami.com/spaces/SDKC/pages/223323903); Generic SSO Microservice (https://confluence.alkami.com/spaces/SDKC/pages/250893074)

## 17. Code-based SAML: Alkami.Utilities.Saml

Optional library (built on OIOSAML.NET) for custom SAML providers; for configuration-driven SAML use Standard SSO. `nuget install Alkami.Utilities.Saml` (1.0.2). Before calling `GetSamlResponseXml()`: verify inputs, load provider settings (at minimum Recipient base URL, Issuer, Subject, SAML Audience), and build signing/encryption parameters from thumbprints. Third-party certs that error in Alkami's implementation should be discussed with Alkami.

```csharp
using Alkami.Utilities.Saml;
var signingCert = Saml20Utility.GetCertificateForSigning(X509FindType.FindByThumbprint, settings[Provider.SettingKeys.CertThumbPrintForSigning], true);
var encryptionCert = Saml20Utility.GetCertificateForEncryption(X509FindType.FindByThumbprint, settings[Provider.SettingKeys.CertThumbPrintForEncrypt], true); // optional, null if not configured
var signingParameters = new Saml20Utility.SigningParamaters { Certificate = signingCert, SignatureLevel = Saml20Utility.SignatureType.Response };
var encryptionParameters = encryptionCert == null ? null
    : new Saml20Utility.EncryptionParameters { Certificate = encryptionCert, KeyEncryptionAlgorithm = EncryptedXml.XmlEncRSAOAEPUrl };
var samlResponse = Saml20Utility.GetSamlResponseXml(
    settings[Provider.SettingKeys.BaseUrl], settings[Provider.SettingKeys.Issuer], settings[Provider.SettingKeys.Domain],
    Guid.NewGuid().ToString() /* Subject */, new List<string> { settings[Provider.SettingKeys.Domain] } /* Audience */,
    settings[Provider.SettingKeys.AuthnContxt], samlAttributes, signingParameters, encryptionParameters,
    SamlAttribute.NAMEFORMAT_URI); // UTC time offset defaults to -60 sec
var base64Saml = Convert.ToBase64String(Encoding.UTF8.GetBytes(samlResponse));
```

Sources: Work with SAML (https://confluence.alkami.com/spaces/SDKC/pages/87017032)

## 18. Relationship to the 2026 Standard-SSO pages

The pages consolidated here were updated between 2022 and 2025 (Client Redirects, 2026-04-09, is the exception). The 2026 "Standard-SSO" tree (page 556889778 and children) covers the Standard-SSO Admin widget, SSO templates, REST endpoints, encryption, rollout, troubleshooting, and refreshed copies of the glossary, syntax, Card Management, client redirects, JWT, and SAML pages. Use `06b-standard-sso-2026.md` for the admin UI and current operational procedures; use this document for the JSON schema, expressions, ConfigTool, SDK testing, certificate setup, and code-based alternatives. Where they disagree, prefer the 2026 pages.
