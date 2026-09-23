# Standard-SSO Service, Admin, and Operations (2026 documentation)

**What this covers.** Standard SSO (previously named Jarvis) is Alkami's configuration-driven SSO framework: the microservice `Alkami.MicroServices.StandardSSO.Service.Host` plus the widget `Alkami.Client.Widget.StandardSso` evaluate a JSON configuration against the current user, account, or card and produce a launch payload (URL, form post, SAML response, JWT, or encrypted fields). This document consolidates the 2026 "Standard-SSO" Confluence tree: schema and options, HTTP/JWT/SAML services, methods, encryption, certificates, REST API, Configuration Tool, deployment and rollout, troubleshooting, account looping, dropdown customization, client redirects, card management integration, and the Standard-SSO Admin widget. The older Jarvis-era pages (Jarvis tool, microservice template, SDK test environments, samples) are in the sibling file `06a`; where a topic appears in both, this document is current.

## 1. Overview and terminology

- Standard SSO integrates with many SSO vendors through configuration only. SAML implementations support SAML 2.0. Older pages, logs, and FAQs still say "Jarvis".
- Components: `Alkami.MicroServices.StandardSSO.Service.Host` (log `Alkami.MicroServices.StandardSSO.Service.Host.log`); `Alkami.Client.Widget.StandardSso` (log `Alkami.Client.Widget.StandardSso.log`); `Alkami.MicroServices.StandardSSO.Engine` (.NET Standard 2.0 validation/evaluation library on the `nuget.dev` feed, usable for validation outside the service but must match the version inside the service); `Alkami.Microservices.StandardSSO.ConfigTool` (desktop tool, choco); `Alkami.Admin.Widget.StandardSSO` (FI Admin widget).
- Source: https://gitlab.mgmt.alkami.net/WIDD/alkami.microservices.standardsso (older links use bitbucket.corp.alkami.net/projects/WIDD).
- Storage: `sso.Configuration` (JSON in `Value`), audit `sso.ConfigurationHistory` (populated by a DB trigger, DEV-108601; the stored procedure `[sso].[AddOrUpdateConfiguration]` is obsoleted), templates `sso.Template`. `sso.Configuration.ProviderName` must match the second segment of the `core.Widget` slug (widget URL `/StandardSso/<ProviderName>`). Configuration is not cached; DB changes take effect immediately.
- Not tied to ORB releases; platform must be 2020.6 or higher (DSTs for new providers use fix version `2020.6`).
- Questions: Slack `#eng-ask-carbon` (https://alkami.slack.com/archives/CD3DEG4V9) or `#sme-carbon`.
- Stub pages: "What is Standard-SSO mean & FAQ's?" contains only "Standard SSO (Previously known as Jarvis)". "Configuration Repository" only points to the ISG page "SSO - Providers: Standard SSO Configuration Cheat Sheet" (real provider configs). "Transaction Integration" only points to "Standard SSO Transaction Image Provider" (the 06a page has the transaction content).

Sources: Standard-SSO (https://confluence.alkami.com/spaces/SDKC/pages/556889778); What is Standard-SSO mean & FAQ's? (https://confluence.alkami.com/spaces/SDKC/pages/556889927); Standard-SSO - Configuration Repository (https://confluence.alkami.com/spaces/SDKC/pages/556889891); Standard-SSO - Transaction Integration (https://confluence.alkami.com/spaces/SDKC/pages/556889926); Standard-SSO - How to Troubleshoot StandardSSO (https://confluence.alkami.com/spaces/SDKC/pages/556889908).

## 2. Service architecture and runtime workflow

The service returns a renderable launch payload, an input state asking for more data, or a typed failure state. Layers: Service Orchestration (`DistributedStandardSSOServiceHost`, `StandardSSOService`, contract `IStandardSSOContract`: loads config by ProviderName, invokes the engine, maps states); Rules and Expression Engine (`Engine.Evaluate` / `Engine.Validate`); Integration (`EvaluateProvider` from `EvaluateProviderFactory`: Accounts, Card Management, Security, Transactions services plus HTTP/SAML/JWT/encryption artifacts); Persistence (`ScopeFactory`, `ConfigurationDataContextFactory`, `ConfigurationsContext` over the `sso` schema).

Contract methods: `GetSsoAsync` (production runtime), `ValidateSsoAsync` (parse plus semantic validation, returns an issue list), `GetSsoPlaygroundAsync` (override config; backs Admin Try Now), `GetConfigurationAsync` / `AddOrUpdateConfigurationAsync` / `GetConfigurationHistoryAsync` / `GetTemplateAsync` (admin lifecycle).

`GetSsoAsync` pipeline: fetch config JSON by ProviderName; create `EvaluateProvider`; `Engine.Evaluate` parses JSON, validates semantics, access restrictions, and selection rules, builds the expression graph and order, runs `QueryForExternalData`, evaluates expressions in order, runs `GenerateClientHttpRequest`; `ConvertEvaluateResultToResponse` maps `EvaluateResult` into `GetSsoResponse` (State, RenderResult, DataSets, ValidationResults).

Response states: `Render`, `Input` (missing account/card selection, invalid parameters, speedbump not acknowledged), `Error` (invalid JSON/config, general error), `NoEligibleAccounts`, `NoEligibleCards`, `EmailMissing`, `RestrictedAccess` (sub-user restriction), `MissingUserExtendedProperty`. Dependency failures (timeout/5xx) and expired certificates are captured as issues; payload generation failures map to Error with internal-safe messages.

Admin lifecycle: `AddOrUpdateConfigurationAsync` runs `Engine.Validate` before a transactional save and sets the session username for the trigger-based audit. Creating a config from Admin adds `core.itemsetting` rows (Localizable Resource item type), but real site text rows appear only when the widget is first opened (`SiteTextController`).

Triage: `ValidationError` = invalid config; `DependencyFailure` = external timeout/5xx; `NoEligibleAccounts`/`NoEligibleCards` = filtering rules; `RenderError` = payload generation. Check expired certificates and client credentials, engine logs for expression ids, EvaluateProvider traces, `ConfigurationsContext` for stored JSON; reproduce with Admin Try Now.

Sources: StandardSSO Service: Technical and Workflow Guide (https://confluence.alkami.com/spaces/SDKC/pages/556889924).

## 3. Configuration schema and options

Required top-level: `BankIdentifier` (FI GUID), `ProviderName`, `HttpRequest`. Optional: `Description`, `Methods` (name to expression), `Services[]` (`Key` required plus one of `Http`, `Jwt`, `Saml`), `AccountConfiguration`, `BusinessUserConfiguration`, `UserConfiguration`, `CardConfiguration`, `SelectionConfiguration`, `FdicConfiguration`, `AccountLoop[]`, `EncryptMethods[]`, `HashMethods[]`, `DeriveKeyMethods[]`.

`HttpRequest` (ClientHttpRequest): `DisplayLocation` (`Inline` | `NewWindow`, required), `Uri` (required; relative or absolute, only checked non-empty), `Method` (`Get` | `Post`, default Post), `ContentType` (default `application/json`), `AutoLaunch` (default true), `Body`, `FormFields`, `Headers`, `QueryParameters` (string maps), `AccountLoopQueryParameters`, `AccountLoopFormFields`. Service-level `Http.Request` has the same shape without AutoLaunch/DisplayLocation plus `Thumbprint`; `Http.Response` has `ResponseType` (`Json` | `Xml` | `Text`) and `OnSuccess`.

`EncryptMethods[]`: `Key`, `Algorithm` (`AES` | `TripleDES` | `RSA`), `Encoding` (`Base64` | `Hex`), `IV`, `IVEncoding` (`UTF8` | `Base64` | `Hex`), `IVGenerationMode` (`None` | `Binary` | `ASCII`), `InputEncoding` (`UTF8` | `Base64` | `Unicode`), `KeyDerivationMethod` (`Guess` | `RawUTF8String` | `Base64String` | `RawBytes`), `KeySize`, `KeyType` (`String` | `HexArray`), `Mode` (`CBC` | `ECB` | `OFB` | `CFB` | `CTS`), `PaddingMode` (`None` | `PKCS7` | `Zeros` | `ANSIX923` | `ISO10126`), `RSAPaddingMode` (`Pkcs1` | `OaepSHA1` | `OaepSHA256` | `OaepSHA384` | `OaepSHA512`), `SharedKey`, `Thumbprint`. `HashMethods[]`: `Key`, `Algorithm` (`SHA256` | `SHA384` | `SHA512`), `Encoding` (`Base64` | `Hex`), `SharedKey`. `DeriveKeyMethods[]`: `Key`, `Salt`, `SaltEncoding` (`UTF8` | `Base64` | `Unicode`), `Iterations` (default 1000), `HashEncoding` (`Base64` | `Hex`).

Expression-capable locations (only these are evaluated): `HttpRequest` `Uri`, `Body`, `FormFields`/`Headers`/`QueryParameters` values; `Methods` values; `Services[].Http` `Uri`, `Body`, `FormFields`/`Headers`/`QueryParameters` values, `Thumbprint`; `Services[].Saml.Request` `Subject` and `Attributes` values; `Services[].Jwt.Request` `Payload` and `Headers` values.

Authentication for outbound calls is normally a header or a client certificate thumbprint: `"Headers": { "Authorization": "Basic {{ Base64Encode('UserName:Password') }}" }, "Thumbprint": "..."`.

Behavioral options:

- `AutoLaunch` false shows the description and lets the user launch manually; true (default) opens the SSO immediately.
- `AccountConfiguration`: `CoreNames[]` (account type core names, for example `["AlkamiCheckings", "AlkamiSavings"]` or `["S-98"]`); `AllowedRelationshipTypes[]` (`Unknown`=0, `PrimaryOwner`=1, `JointOwner`=2, `Linked`=3, `Aggregated`=4, `BusinessAccount`=5; numeric or name form; applied to the dropdown and to accounts preselected from MyAccountsV2 or CardManagement); `Permissions[]` (for example `["ViewStatements", "ViewTransactions"]`); `BINFilter[]` (BINs to INCLUDE; filters cards by card number and accounts by credit card number); `RequiredExtendedProperties[]`; `IncludeAccountTypes[]` / `ExcludeAccountTypes[]` with values `AssetAccount`, `AutoLoanAccount`, `BalanceOnlyAccount`, `BusinessAccount`, `CertificateAccount`, `ClosedEndedLoanAccount`, `CoreOnlyScheduledTransfer`, `CreditAccount`, `CreditCardAccount`, `DemandDepositAccount`, `ExternalAccount`, `MoneyMarketAccount`, `MortgageLoanAccount`, `OpenEndedLoanAccount`, `PayoffCalculatorDisplayed`, `SavingsAccount`, `InvestmentAccount`, `IRAAccount`, `HSAAccount`. Validation fails if a type is outside this list or in both lists; exclusion wins when an account matches both.
- `BusinessUserConfiguration`: `RestrictSubUserAccess` true blocks all sub users with "Insufficient Permissions"; `RolePermissions[]` is all-or-nothing (sub user must hold every listed permission); `IgnoreSubUserViewSummaryPermission` (default false) skips the ViewSummary account check for sub users only.
- `UserConfiguration.RequiredExtendedProperties[]`: user must have one of them or gets "Insufficient Permissions".
- `FdicConfiguration.IsSpeedBumpRequired`: with FDIC signage enabled at the FI and RemoteTheme on, shows a non-FDIC speedbump the user must accept; `AutoLaunch` is ignored while it shows. See the "FDIC Compliance Troubleshooting Runbook" (pageId 437944496).
- Platform gates: missing email when `Email` is used redirects to the email-missing page (DEV-137261); admins need "Login as User (Full Access)", View Only is redirected to Insufficient Permissions (DEV-148741); an unaccepted registration disclosure redirects to the disclosure page (DEV-155306).
- No conditional operators exist. Different payloads for mobile vs desktop require separate configurations/widgets with device display settings.

Examples (each also has `BankIdentifier`, `ProviderName`, `Description`):

```json
"HttpRequest": {
  "DisplayLocation": "NewWindow", "Uri": "http://www.alkami.com", "Method": "Get", "ContentType": "application/json",
  "QueryParameters": { "FN": "{{ FirstName }}", "LN": "{{ LastName }}" },
  "Headers": { "H1": "{{ Mask('123456789') }}", "H2": "{{ '123456789' }}" }
}
```

- JSON body: `"Method": "Post", "ContentType": "application/json", "Body": "{\"First\":\"{{ FirstName }}\",\"Last\":\"{{ LastName }}\"}"`. Form post: `"ContentType": "application/x-www-form-urlencoded", "FormFields": { "First": "{{ FirstName }}" }`.
- Methods (computed once, referenced as `@name`, case-insensitive, may reference other methods, no cycles): `"FormFields": { "UserInfo": "{{ @UserInfo }}" }` with `"Methods": { "Salt": "SaltValue", "SaltEncoded": "{{ UrlEncode(@Salt) }}", "UserInfo": "{{ Append(FirstName, @SaltEncoded, LastName) }}" }`.
- HTTP service: `"QueryParameters": { "Token": "{{ Http('http') }}" }` with `"Services": [{ "Key": "http", "Http": { "Request": { "Uri": "http://alk-ncover/feeds/api/Package/Alkami.Api.CUFX", "Method": "Get", "ContentType": "application/json" }, "Response": { "ResponseType": "Json", "OnSuccess": "$.packageId" } } }]`. SOAP: `"ContentType": "text/xml"`, a `SOAPAction` header, the envelope in `Body`, `"ResponseType": "Xml"` with an XPath like `/*[local-name()='Envelope']/*[local-name()='Body']/*[local-name()='SignOnResponse']/*[local-name()='SessionToken']`.
- Hash: `"TaxHash": "{{ Hash('MyHashKey', TaxId) }}"` with `"HashMethods": [{ "Key": "MyHashKey", "SharedKey": "SuperSecret", "Encoding": "Base64", "Algorithm": "SHA256" }]`.
- TripleDES: `"{{ Append(Encrypt('EM', 'happy'), FirstName, LastName) }}"` with `"EncryptMethods": [{ "Key": "EM", "Algorithm": "TripleDES", "SharedKey": "SharedKey", "IV": "12345678", "KeySize": 192, "Mode": "CBC", "PaddingMode": "PKCS7", "KeyDerivationMethod": "RawUTF8String", "IVGenerationMode": "None", "Encoding": "Base64" }]` (`"Encoding": "Hex"` for hex).
- RSA envelope: methods `"RandomKey": "{{ RandomString(32) }}"`, `"RandomIV": "{{ RandomString(8) }}"`, `"EncryptedKey": "{{ Encrypt('RSA', @RandomKey) }}"`, `"EncryptedAccount": "{{ Encrypt('TripleDES', AccountNumber) }}"`, with `EncryptMethods` `{ "Key": "RSA", "Algorithm": "RSA", "Thumbprint": "aa845e7c...", "RSAPaddingMode": "Pkcs1" }` and a TripleDES entry whose `SharedKey`/`IV` are `"{{ @RandomKey }}"` / `"{{ @RandomIV }}"`.
- Derive key: `"Authorization": "{{ DeriveKey('MyDeriveKeyMethod', 'SecretPassword', 16, 32) }}"` with `"DeriveKeyMethods": [{ "Key": "MyDeriveKeyMethod", "Salt": "AlkamiTechnology", "SaltEncoding": "UTF8", "Iterations": "1000", "HashEncoding": "Base64" }]`. Arguments: method Key, secret, key size, optional byte offset (size 16, offset 32 derives 16 bytes after skipping 32).

Encryption requirements: AES needs `Algorithm`, `Encoding`, `InputEncoding`, `IVEncoding`, `IVGenerationMode`, `Key`, `KeyDerivationMethod`, `Mode`, `PaddingMode`, `SharedKey` (default `KeyType` String). RSA needs `Algorithm`, `Encoding`, `InputEncoding`, `Key`, `RSAPaddingMode`, `Thumbprint`; the vendor holds the private key so Alkami cannot decrypt. DeriveKey needs `HashEncoding`, `Key`, `Salt`, `SaltEncoding`.

The overview page attaches `StandardSSO Configuration Widget Template.sql` (and a ROLLBACK script), the DST template that inserts the configuration and widget; the SQL itself is not in the transcript.

Sources: Standard-SSO (https://confluence.alkami.com/spaces/SDKC/pages/556889778); Standard-SSO - Methods (https://confluence.alkami.com/spaces/SDKC/pages/556889923); Standard-SSO - Encryption (https://confluence.alkami.com/spaces/SDKC/pages/556889905); Standard-SSO - How to Troubleshoot StandardSSO (https://confluence.alkami.com/spaces/SDKC/pages/556889908).

## 4. Expression syntax and glossary (summary)

- Operations are wrapped in `{{` and `}}` (no spacing inside the braces themselves); text outside is literal (`Who is {{ FirstName }}?`). Strings use single or double quotes, escaped by doubling (`'Bob said ''Does that work?'''`) or by mixing quote styles. Numbers are strings. Names are case-insensitive and whitespace-tolerant. `@name` references a method.
- User: `FirstName`, `MiddleName`, `LastName`, `DisplayName`, `UserName`, `UserId`, `UserIdentifier`, `MemberIdentifier`, `PersonId`, `TaxId` (9 digits, zero-padded), `BirthDate('fmt')`, `Email`, `PrimaryPhone`, `HomePhone`, `WorkPhone`, `AddressLine1`, `AddressLine2`, `City`, `State`, `PostalCode`, `Country`, `IpAddress`, `SessionId`, `UserExtendedProperty('Name')`, `QueryString('param')`, `ProductCategory`; business master-user variants are prefixed `Master` (`MasterFirstName` ... `MasterTaxId`, `MasterUserExtendedProperty('Name')`).
- Account (require selection): `AccountNumber`, `MaskedAccountNumber`, `AccountDisplayName`, `AccountHolder`, `AccountExtendedProperty('Name')`, `CoreName`, `Relationship`, `MicrNumber`, `MortgageNumber`, `CreditCardNumber`, `CreditLimitAmount`, `NextPaymentAmountDue`, `NextPaymentDate('fmt')`, `PastDueAmount`, `LateChargeAmountDue`. Card (require selection): `CardNumber`, `CardDescription`, `CardType`, `CardHolderDisplayName`, `CardActivationDate('fmt')`, `CardExpirationDate('fmt')`, `CardIsActivated`, `CardIsBlocked`, `CardIsChipped`, `CardIsRegistered`, `CardForeignTransactionsEnabled`, `CardPinUpdateAttemptsLeft`. Transaction (require a Transaction Id parameter): `TransactionAmount`, `TransactionCheckNumber`, `TransactionCreateDate('fmt')`, `TransactionPostingDate('fmt')`, `TransactionImageSide`, `TransactionMicr`, `TransactionRoutingNumber`, `TransactionTraceNumber`, and others listed in the glossary.
- Helpers: `Append`, `Coalesce`, `Format`, `FormatNumber`, `FormatDateTime('fmt'[, 'Timezone Id'])`, `DateTimeOffset`, `UtcDateTime`, `Guid`, `RandomString(n)`, `Left`, `Right`, `Lower`, `Upper`, `Trim`, `TrimStart`, `TrimEnd`, `PadLeft`, `PadRight`, `Mask(input[, '#', keep, total])`, `Replace`, `RegexReplace`, `UrlEncode`, `XmlEscape`, `JsonPath`, `XPath`, `Base64Encode`, `Base64Decode`, `HexEncode`, `HexDecode`, `CombineBase64Strings`, `Hash('Key', input)`, `Encrypt('Key', input)`, `DeriveKey`, `Http('Key')`, `Jwt('Key')`, `Jwt('Key', value)` (JWE only), `Saml('Key')`, `AccountLoop('Key'[, delimiter])`.
- A null input to an operation (for example `{{ Left(AddressLine2, 4) }}`) is a runtime error; wrap with `Coalesce`.

Sources: Standard-SSO - Expression Syntax (https://confluence.alkami.com/spaces/SDKC/pages/556889907); Standard-SSO - Expression Glossary (https://confluence.alkami.com/spaces/SDKC/pages/556889906).

## 5. HTTP services, JWT, and SAML

HTTP services make intermediate requests reused via `{{ Http('Key') }}`. `ResponseType` `Json` interprets `OnSuccess` as JsonPath (Newtonsoft SelectToken), `Xml` as XPath, `Text` returns the body as-is. Any non-2XX status makes the expression error. Client certificates use `Thumbprint` in the service request.

JWT (`Services[].Jwt.Request`): required `Payload`, `SigningAlgorithm` (`HS256`, `HS384`, `HS512`, `RS256`, `RS384`, `RS512`, `PS256`, `PS384`, `PS512`, `ES256`, `ES384`, `ES512`), `TokenExpiryInMinutes`; `CertificateThumbprint` required for RS/PS/ES algorithms; `HMACSecretKey` required for HS algorithms (minimum length HS256 32, HS384 48, HS512 64; the page says "bit"). `Headers` optional; default header has `alg` and `type` = `jwt`. Signing certificates are generated by Alkami (private .pfx). Example: `"SSOToken": "{{Jwt('JwtToken')}}"` with `{ "Key": "JwtToken", "Jwt": { "Request": { "Payload": { "sub": "Test", "test": "{{ Lastname }}" }, "Headers": { "kid": "123" }, "CertificateThumbprint": "908381cb...", "HMACSecretKey": "132456...", "TokenExpiryInMinutes": 60, "SigningAlgorithm": "HS256" } } }`.

JWE: with a JWK, supply `Payload`, `JWK` (standard object such as `{ "kty": "RSA", "kid": "...", "use": "enc", "alg": "RSA_OAEP", "e": "AQAB", "n": "..." }`; `alg` in `RSA1_5`, `RSA_OAEP`, `RSA_OAEP_256`), and `JweEncryptionAlgorithm` (`A128CBC_HS256`, `A192CBC_HS384`, `A256CBC_HS512`, `A128GCM`, `A192GCM`, `A256GCM`). With a certificate, supply `Payload`, `CertificateThumbprint`, `CertKeyType` (same three RSA values), `JweEncryptionAlgorithm`. Field-level encryption reuses the service with a per-field payload: `"ssn": "{{ Jwt('JwtToken', TaxId) }}"`, `"birthday": "{{ Jwt('JwtToken', BirthDate('yyyy-MM-dd')) }}"`.

SAML (`Services[].Saml.Request`): required `Issuer`, `Subject`, `Audiences[]`, `SigningParameters.Thumbprint`, `SigningParameters.SignatureLevel` (`Assertion` | `Response`, default Response), `AttributeFormat` (`Basic` default | `Uri`); optional `Recipient`, `Domain`, `AuthnContext`, `Attributes`, `EncryptionParameters` (`Thumbprint` required; `IncludeCertificateInfo`, `KeyEncryptionAlgorithm`, `SymmetricEncryptionAlgorithm`), `InitialTimestampShift` (default -60), `AssertionNamespacePrefix`, `ProtocolNamespacePrefix`, `DateTimeFormat`, `AccountLoop`. `SigningParameters` also has `SignAssertionsOnly`, `IncludeTransform`, `CanonicalizationAlgorithm`, `DigestMethod`, `SignatureMethod`.

- Alkami only does IdP-initiated flows: no metadata file, redirect URLs, or metadata URLs. If a vendor insists, use the "SAML Metadata Sample" page (pageId 250880392). Ask vendors for a sample SAML metadata file.
- Signing certificate: Alkami-generated private .pfx. Encryption certificate: vendor-generated public .crt/.cer.
- Combinations: signed Response, unencrypted Assertion = `SignatureLevel: Response`, no EncryptionParameters; signed Response, encrypted Assertion = Response plus EncryptionParameters; unsigned Response, signed Assertion = `Assertion`, `SignAssertionsOnly: false`; unsigned Response, encrypted signed Assertion = `Assertion`, `SignAssertionsOnly: true`, EncryptionParameters; signed Response, encrypted signed Assertion = `Response`, `SignAssertionsOnly: true`, EncryptionParameters.
- SHA256RSA certificates (Details tab, "Signature algorithm") require `"SignatureMethod": "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"` and `"DigestMethod": "http://www.w3.org/2001/04/xmlenc#sha256"`; the FAQ example also sets `"IncludeTransform": true`, `"SignAssertionsOnly": true`, `"CanonicalizationAlgorithm": "http://www.w3.org/2001/10/xml-exc-c14n#"`.
- RSA key encryption: the default `http://www.w3.org/2001/04/xmlenc#rsa-1_5` is deprecated by W3C and disallowed by NIST SP 800-131A; Node.js 22+ service providers may fail to decrypt. All new integrations must set `"EncryptionParameters": { "KeyEncryptionAlgorithm": "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p" }`. Legacy providers stay functional and are updated on request. GenericSSO/Consolidated providers share the `Saml20Utility` default and set `Saml20Utility.EncryptionParameters { KeyEncryptionAlgorithm = "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p" }`.

```json
"HttpRequest": { "DisplayLocation": "NewWindow", "Uri": "http://www.alkami.com", "Method": "Post",
  "ContentType": "application/x-www-form-urlencoded", "FormFields": { "SAMLResponse": "{{ Saml('SamlService') }}" } },
"Services": [{ "Key": "SamlService", "Saml": { "Request": {
  "Recipient": "recipient", "Issuer": "issuer", "Subject": "subject", "Audiences": [ "audience1" ],
  "AuthnContext": "authncontext", "Attributes": { "att1": "value1" },
  "SigningParameters": { "Thumbprint": "232b868a...", "SignatureLevel": "Response", "SignAssertionsOnly": false },
  "EncryptionParameters": { "Thumbprint": "232b868a...", "IncludeCertificateInfo": true },
  "AttributeFormat": "Basic", "InitialTimestampShift": -60 } } }]
```

Sources: Standard-SSO - HTTP Services (https://confluence.alkami.com/spaces/SDKC/pages/556889918); Standard-SSO - JWT (https://confluence.alkami.com/spaces/SDKC/pages/556889920); Standard-SSO - SAML (https://confluence.alkami.com/spaces/SDKC/pages/556889921); Standard-SSO (https://confluence.alkami.com/spaces/SDKC/pages/556889778); Standard-SSO - How to Troubleshoot StandardSSO (https://confluence.alkami.com/spaces/SDKC/pages/556889908).

## 6. Certificates: what is needed and how to install

- Signing certificates are generated by Alkami (Security Engineering; example tickets INFOSEC-16114 generation, OPS-47145 installation). The private key (.pfx) is installed at Alkami in the Personal store; the public key goes to the vendor.
- Encryption certificates are generated by the vendor and sent through the FI as a .crt or .cer public key (example OPS-46058); installed in the Trusted People store. External CA certificates go in the Third-Party CA store. Not every SSO needs certificates. Do not accept a bare Base64 blob; require an actual certificate file.
- Install on the lane or pod hosting the FI (locally: "Manage Computer Certificates"). Never share signing or encryption certificates across FIs in production, even for the same SSO.

PFX install: Personal store, right click Certificates, All Tasks, Import; select the file; enter the supplied password and check "Mark this key as exportable"; finish; then right click the certificate, All Tasks, "Manage Private Keys..." and grant `IIS_IUSRS`, `dev.nag`, `dev.radium`, `dev.dbms`, `dev.micro` (the `dev.*` accounts are SDK service accounts: click Advanced, Object Types, tick "Service Accounts"); verify the Certification Path. Missing permissions produce `CryptographicException: Keyset does not exist`. .crt/.cer files are imported into the target store with the wizard. See also "How to Get Certificates for a Provider Implementation" and "Certificates Guide to the Universe" (pageId 95001632); the Jarvis-era SDK certificate page is in 06a.

Sources: Certificates and tickets needed for Standard-SSO: (https://confluence.alkami.com/spaces/SDKC/pages/556889812); Installing Encryption & Signing Certificates for StandardSSO (https://confluence.alkami.com/spaces/SDKC/pages/556889814).

## 7. REST endpoints (StandardSSO Service v1)

Swagger (QA): https://alk-svc-rest-standardsso-v1-neo.alk-qa-use1.k8s.qa.alkami.net/swagger/index.html. DTOs are in `Alkami.MicroServices.StandardSSO.Contracts` (Requests/Responses) unless noted.

- `POST /v1/sso/validate`: `ValidateSsoHttpRequest { ConfigurationData }` returns `ValidateSsoResponse : BaseResponse { Issues: List<Issue> }`.
- `POST /v1/sso/evaluate`: `GetSsoHttpRequest { ProviderName, Parameters }` returns `GetSsoHttpResponse { State, FdicConfiguration, DataSets { Accounts: List<Guid>, Cards, Options }, RenderResult { AutoLaunch, Uri, Headers, ContentType, Body, FormFields, Thumbprint } }`.
- `POST /v1/sso/playground/evaluate`: `GetSsoPlaygroundHttpRequest { ProviderName, Parameters, OverrideConfig }`; auth is the test user's JWT, not admin. Same response as evaluate.
- `GET /v1/sso/playground/users?query={q}` (admin only): `UserLookupResponse { Success, Users: List<UserLookupResult { Id, UserIdentifier, DisplayName, LoginId, CustomerId, BusinessName, business/subuser flags }>, IsValid }`.
- `GET /v1/templates?includeValue&includeDisabled&pageIndex=0&pageSize=100`, `GET /v1/templates/{id}` (404 if missing), `POST /v1/templates/search` (`SearchTemplatesHttpRequest { Filter, Mapping, Sorter, PageIndex, PageSize }`). `Template { Id, Name, Configuration, CreatedDate, LastUpdateDate, LastModifiedBy, ConversionProviderName, ConversionWidgetAssemblyInfo, Disabled }`.
- `GET /v1/configurations?includeValue&pageIndex&pageSize`, `GET /v1/configurations/{id}`, `POST /v1/configurations/search`. `Configuration { Id, ProviderName, Value, CreatedDate, LastModifiedDate, TemplateId }`.
- `POST /v1/configurations`: `CreateConfigurationHttpRequest { Item, WidgetSettings? }` returns 201. `PUT /v1/configurations/{id}`: `UpdateConfigurationHttpRequest { Item, WidgetSettings? }` (in `Alkami.MicroServices.StandardSSO.Service.Host/Models/`); `Item.Id` must equal the path id, else 400.
- `GET /v1/configurations/{configurationId}/history` and `POST /v1/configurations/history-search`: `ConfigurationHistory { Id (long), ConfigurationId, OldValue, NewValue, OldProviderName, NewProviderName, CreatedDate, ChangeNotes }`. A 404 for a deleted configuration with no history is expected (DEV-107065).

Sources: REST Endpoints (https://confluence.alkami.com/spaces/SDKC/pages/556889824); Standard-SSO - How to Troubleshoot StandardSSO (https://confluence.alkami.com/spaces/SDKC/pages/556889908).

## 8. Account Looping

Account Looping builds per-account key/value outputs at runtime for HTTP or SAML requests, or joins them into one string. The loop runs over all eligible accounts, so no account dropdown is shown.

- `AccountLoop[]` entries: `Key` (loop name), `KeyTemplate` (per-item key, normally with `<<index>>`), `ValueTemplate` (expression per account).
- Consumers: `HttpRequest.AccountLoopQueryParameters`, `HttpRequest.AccountLoopFormFields`, `SamlRequest.AccountLoop` (each names a loop Key; results are appended as extra query params, form fields, or unique SAML attributes), and `AccountLoop('loopKey', optionalDelimiter)` anywhere expressions are allowed.
- Runtime: per account, `OptionIdentifier` is set, `ValueTemplate` evaluated, `<<index>>` replaced, results stored in `EvaluationContext.AccountLoopValues` keyed by `AccountLoop.Key`. Key lookup is case-insensitive; null values are skipped. Duplicate keys overwrite earlier output. A consumer naming a missing key is skipped silently; a missing key in the `AccountLoop(...)` expression is a runtime GeneralError.

```json
"AccountLoop": [{ "Key": "SamlAccountLoop", "KeyTemplate": "account_<<index>>", "ValueTemplate": "{{ AccountNumber }}" }],
"AccountConfiguration": { "CoreNames": ["S-98"], "AllowedRelationshipTypes": ["PrimaryOwner"] },
"HttpRequest": { "DisplayLocation": "NewWindow", "AutoLaunch": true, "Uri": "https://auth.pingone.com/.../saml20/sp/acs",
  "Method": "Post", "ContentType": "application/x-www-form-urlencoded",
  "FormFields": { "SAMLResponse": "{{ Saml('SamlService') }}", "RelayState": "REPLACEME" } },
"Services": [{ "Key": "SamlService", "Saml": { "Request": {
  "Subject": "{{ AccountLoop('SamlAccountLoop', ',') }}",
  "Attributes": { "firstName": "{{ Coalesce(FirstName,'.') }}", "transAccountNumbers": "{{ AccountLoop('SamlAccountLoop', ',') }}" },
  "...": "..." } } }]
```

The form-field pattern sets `"AccountLoopFormFields": "SamlAccountLoop"` on `HttpRequest` and appends `account_0`, `account_1`, ... alongside existing fields (the example builds XML fragments with `Append('<Account><ACCOUNT_NAME>', AccountDisplayName, ...)`). The Pinwheel example uses a JSON-fragment `ValueTemplate` and `"accounts": "[{{ AccountLoop('SamlAccountLoop', ',') }}]"`. Recommendations: unique keys, always include `<<index>>` when appending fields, use `AccountConfiguration` filters to keep output deterministic, use a delimiter for list-like targets, and test with a user holding at least two eligible accounts before promoting.

Sources: Account Looping (https://confluence.alkami.com/spaces/SDKC/pages/556889809).

## 9. Account dropdown customization (SelectionConfiguration)

Top-level `SelectionConfiguration` (`PrimaryInfo`, `SecondaryInfo`, `TertiaryInfo`, all expressions) controls what the Iris Account Dropdown shows per account or card. Account defaults: `"{{ AccountDisplayName }}"`, `"{{ MaskedAccountNumber }}"`, `""`. Card defaults: `"{{ MaskedCardNumber }}"`, `"{{ AccountDisplayName }}"`. Non-overridden entries keep defaults; masking follows the FI's Account Display Formatting (pageId 145514039). Example: `"PrimaryInfo": "Account Holder: {{ AccountHolder }}", "SecondaryInfo": "{{ AccountDisplayName }}", "TertiaryInfo": "#{{ AccountNumber }}"`. Since the Iris2 upgrade (DEV-189015) tertiary info renders as a third line on the left. Admin Try Now shows the same dropdown. Example resolution: TCHRV-11835.

Sources: Account Dropdown Customization (https://confluence.alkami.com/spaces/SDKC/pages/556889803).

## 10. Display, navigation, and client redirects

Query parameters on links to a Standard SSO widget:

- `displayMethod=SameWindow` (`_self`; only a spinner shows in the widget; usually best), `displayMethod=NewWindow` (`_blank`), `displayMethod=Inline` (iframe). On native, or with a preselected account, the method is forced to SameWindow except Inline with a preselected account.
- `backUrl=MyAccountsV2` returns the user to the calling widget on back/close (native uses Flutter `webviewActive`/`webviewInactive` events; backUrl is unreliable on iOS/Android, DEV-118319; Chromium drops fast redirects from history but other browsers need it).
- `"target": "browser"` in the config's query parameters opens the link in the iOS system browser instead of the in-app WebView.
- `forceIFrame=true` keeps Inline on native; `hidethirdpartycookielink=true` hides the "open in new window" link for inline SSOs. Both: consult the Carbon team first.
- Link examples: `<a href="/StandardSso/StandardSSOTest?displayMethod=SameWindow&backUrl=MyAccountsV2" target="_blank">` (or `target="_self"` to stay in one tab; or `displayMethod=Inline` for an iframe).
- iPhone/iPad Pay Now: `mobile_pay_now_account_types` entry `{ "CoreName": "T:MORT", "Url": "/Mobile/StandardSso/StandardSSOTest?displayMethod=SameWindow&backUrl=MyAccountsV2", "Target": "_self", "AccountParams": "MortgageNumber" }`.
- Different display for mobile vs desktop (for example Safari popup blocking): create a second provider config and widget with the `StandardSSO Configuration Widget Template.sql` DST, map providers to devices via widget display settings, then in Admin Setup, Navigation Builder, set the `displayMethod` query parameter under "Widget Subroute".

Client redirects: an iframe-hosted SSO can navigate the user within online banking by `window.parent.postMessage(eventName, ssoWidgetUrl)` (data is just the event name; target is the full SSO widget URL). Only the Inline display method is supported (native cannot support new windows). Configuration is entirely in the widget setting `ClientRedirects` (JSON list, no limit; nothing in the SSO JSON). Each `ClientRedirect` requires `EventName` (case-insensitive), `Origin` (host only, no protocol or path, case-insensitive), `WebDestination` (OLB subpath such as `/MyAccountsV2`; `/Mobile` and leading slash are added as needed), `NativeDestination` (`Accounts` = app home, `LastPage` = close SSO and return); no additional properties.

```json
[
  { "EventName": "redirectToDashboard", "Origin": "www.ssourl.com", "WebDestination": "/DashboardV2", "NativeDestination": "LastPage" },
  { "EventName": "redirectToAccounts",  "Origin": "www.ssourl.com", "WebDestination": "/MyAccountsV2", "NativeDestination": "Accounts" }
]
```

Sources: Standard-SSO (https://confluence.alkami.com/spaces/SDKC/pages/556889778); Standard-SSO - Client Redirects (https://confluence.alkami.com/spaces/SDKC/pages/556889885).

## 11. Card Management integration

Card Management offers `GetCards` (used by the Card Management widget; aggregates all providers, stores cards in the DB, populates `CardIdentifier`) and `GetCardNumbers` (only providers supporting CardNumbers, no aggregation, no DB storage, no `CardIdentifier`). Only the SymConnect provider maps cards to accounts via `AccountIdentifier`; it filters by `AccountIdentifiers` when supplied, otherwise by the `SUPPORTEDCARDTYPES` item setting.

`CardConfiguration`: `CardSource` (default GetCards; use GetCards when launching from the Card Management widget, GetCardNumbers when the FI does not use that widget; the schema spells the enum `GetCardsAsync`/`GetCardNumbersAsync` while the overview example uses `"CardSource": "GetCardNumbers"`); `CardDesignator` (default `CardIdentifier`; `CardIdentifier` is unavailable with GetCardNumbers, `ExternalReference` only works with SymConnect); `FilterOnAccounts` (default true; GetCards does not support account filtering, so set false with GetCards).

Prerequisites: `Alkami.MicroServices.CardManagement.Service.Host` 7.0.0+ (required), `Alkami.MicroServices.CardManagementProviders.SymConnect.Host` 6.0.0+ (required), Standard SSO MS 1.6.6+ (required), Standard SSO widget 1.2.0+, `Alkami.Microservices.StandardSSO.ConfigTool` 1.3.0+ for testing, `Alkami.Apps.CardManagement` optional; at least one Card Management provider installed. The source table swaps the package links for the widget and MS rows, and its status table cites `Alkami.MicroServices.StandardSSO.Service.Host/1.5.7-pre00019`, older than the 1.6.6 prerequisite. Required Symitar RepGens for SymConnect: `SYC.GETCARDINFO.ALKAMI.V1` (2021.05.28), `SYC.GETCARDINFO.ALKAMI.PRO` (2021.03.22), `ALKAMI.COMMON.PRO`, `ALKAMI.COMMON.DEF` (bitbucket CORECUSTOM/symitarrepgens, Alkami/CardManagement); outdated custom RepGens need a Carbon dev ticket. `GetCardNumbers` supported core: SymConnect only. Card expressions: `CardActivationDate`, `CardDescription`, `CardExpirationDate`, `CardHolderDisplayName`, `CardNumber`, `CardType`. Sequence diagrams are Lucidchart links; Bounces/Cache, How to Test, and Troubleshooting sections are empty. Tickets: DEV-113999, DEV-111514, DEV-115279.

Sources: Standard-SSO - Card Management Integration (https://confluence.alkami.com/spaces/SDKC/pages/556889881); Standard-SSO (https://confluence.alkami.com/spaces/SDKC/pages/556889778).

## 12. Configuration Tool, deployment, rollout, and issue reporting

Configuration Tool (`Alkami.Microservices.StandardSSO.ConfigTool`, https://packagerepo.orb.alkamitech.com/feeds/choco.dev/Alkami.Microservices.StandardSSO.ConfigTool/versions): visual designer, validation with the Engine, evaluation with real or mocked responses, sample configs, and cURL export (Postman-importable; relative URIs use `RelativeUriHost` from the Input Data tab). Checking syntax with the tool is highly recommended.

```powershell
choco install Alkami.Microservices.StandardSSO.ConfigTool -y
choco upgrade Alkami.Microservices.StandardSSO.ConfigTool -y   # add --version {versionNumber} to pin
```

Test flow: paste the config in "Raw JSON", click Validate ("success"), fill "Input Data", click Execute to get the SSO URL, then open it in a browser or "Copy to Clipboard" and replay in Postman.

Deployment (usually TI/Support): components are independent of major releases and tracked on the "SSO Framework Serverless Environment" Team Release page (pageId 95012802), which has a "Staging Approvals" column because there is one ProGet feed and no QA-to-Staging promotion. Configure an environment by installing the Standard SSO widget and microservice components and running `StandardSSO Configuration Widget Template.sql`, which inserts into `sso.Configuration`, `sso.ConfigurationHistory`, and `Widget` (SLUG plus matching Name).

Rollout prerequisites before testing in any environment: install certificates if applicable (SRE Ops; migrations copy from the APP tier to the MIC tier), whitelist the vendor URL, and add host entries (both Network Engineering; copy APP to MIC for migrations).

- New providers: label the delivery ticket `StandardSSOContract`; developer creates a Downstream Task with fix version `2020.6`, up/down scripts, and the config as a `.txt` or `.json` attachment (example DEV-100629, DST DEV-112282); TI builds a SQL template and Jira template by cloning the DST; PM creates one Staging and one Production ticket; TI executes on client approval. New-provider dev tickets carry label `StandardSSONew`.
- Conversions of existing providers: dev ticket gets label `StandardSSOConversion` and Epic DEV-116387; developer's DST holds the config as a `.txt` attachment (special logic in a separate DST); TI's SQL script (example TI-77926) redirects legacy provider settings, Accounts/Dashboard links (View Transactions, Pay Now, Quick Links), Quick Apply links, CMS marketing links, mobile links, and site text; TI configures the new widget hidden in Staging in the same flavor and userwidget position as the legacy widget, compares behavior, then unhides it with the legacy display settings and hides the legacy widget; Production stays hidden until the FI approves Staging. PSO PM tracks every FI on the Conversion Confluence with a Staging and Production ticket each (test as Retail, Business Master, Business sub-user), updates marketing URLs, and after all FIs convert the PM opens dev/release tickets to delete the legacy code and remove it from the Team Release page. Parameter-change conversions use label `StandardSSOResolvedByConversion`.

Issue reporting: for new rollouts, Support troubleshoots with the troubleshooting guide, then opens a Technical Review ticket for the owning development team (found via the product's Technical Guide) and threads questions in `#sme-carbon` or `#eng-ask-carbon`. For existing or converted SSOs (PSO Category 1), Support engages the assigned PSO Technical Consultant, who opens the Tech Review ticket if needed.

Sources: Standard-SSO - Configuration Tool (https://confluence.alkami.com/spaces/SDKC/pages/556889892); Standard-SSO - Deployment (https://confluence.alkami.com/spaces/SDKC/pages/556889902); How To Rollout a Standard-SSO Implementation For New and Existing SSO Providers to a Client's Production Environment (https://confluence.alkami.com/spaces/SDKC/pages/556889813); Issue Reporting for New and Existing/Standard-SSO Converted SSOs (https://confluence.alkami.com/spaces/SDKC/pages/556889822).

## 13. Troubleshooting

| Log symptom (`Alkami.MicroServices.StandardSSO.Service.Host.log`) | Cause and fix |
| --- | --- |
| `Issue Code: InvalidConfiguration` with `Syntax error ('Unknown expression detected.') occurred in HttpRequest.QueryParameters for the expression: {{ AccountNumbers }}` | Unknown expression (should be `AccountNumber`); fix the expression named in the log |
| `Deserializing JSON failed.` | Invalid JSON; validate with the Configuration Tool |
| `GetConfigurationData: ... StatusCode: NotFound` then `Issue Code: InvalidJson, Message: Configuration was not provided.` | No config for that FI or ProviderName mismatch; make `sso.Configuration.ProviderName` match the `core.Widget` slug |
| `System.Security.Cryptography.CryptographicException: Keyset does not exist` | Signing certificate private key permissions; redo "Manage Private Keys" (section 6) |
| `StatusCode: 403, ReasonPhrase: 'Forbidden'`, `x-amzn-ErrorType: ForbiddenException` | Check system environment variable `ALKAMI.API_GATEWAY_URL` (example `https://xyz.execute-api.us-east-1.amazonaws.com/dev/`) |
| Pop-up blocked | User clicks the open button or allows pop-ups |
| iPhone mobile web iframe reloads, cookies not set (CHLS logs) | Cross-site cookies blocked in iframes; vendor must serve from a subdomain of the FI domain (DNS record) |

FAQ: site text rows are created only when the widget is first viewed, then edited in Admin; SiteText Admin lists only namespaces matching the `core.Widget` Name case-sensitively (Standard SSO namespace = `AssemblyInfo + Name`); no caching in widget or service beyond the SiteTextController javascript endpoint; the SSO button cannot be removed to show only text (use a campaign); auto-launch is only suppressed when an account selection is required; multiple buttons to different sites is a Quick Links use case; sub users need ALL `RolePermissions`; separate business and retail rules by creating two widgets with their own configs in different packages.

Sources: Standard-SSO - How to Troubleshoot StandardSSO (https://confluence.alkami.com/spaces/SDKC/pages/556889908).

## 14. Standard-SSO Admin

Purpose: FI admins view, search, create, edit, test (Try Now), and audit configurations from the FI admin site instead of direct DB edits or the SuiteTI tool. Versions: `Alkami.Admin.Widget.StandardSSO >= 1.0.4`, `Alkami.MicroServices.StandardSSO.Service.Host >= 1.29.0`.

Access: feature flag `EnableStandardSSOAdmin` under `Internal.StandardSSOAdmin` (set via scripts/DST, SuiteTI above DEV, or the `Feature.Flag` table in a DEV1 DB) plus the "StandardSSO Admin" role permission (Setup section; key `StandardSSOAdmin`, DEV-164709). Try Now also requires "Login As User (Full Access)"; View Only leaves the button disabled.

Database (DEV-140577): table `sso.Template` (`Id INT IDENTITY PK`, `Name VARCHAR(128)`, `Configuration VARCHAR(MAX)`, `CreatedDate DateTimeOffset`, `LastUpdateDate`, `LastModifiedBy VARCHAR(128)`, `ConversionProviderName varchar(128)`, `ConversionWidgetAssemblyInfo varchar(128)`, `Disabled BIT DEFAULT 0`) and `ALTER TABLE [sso].[Configuration] ADD [TemplateId] INT NULL CONSTRAINT [FK_sso_Configuration_sso_Template] FOREIGN KEY REFERENCES [sso].[Template](ID)`.

Admin Widget Project scope: view/update, audit history, permissions, raw and smart editors with JSON validation, template loading, expression glossary, widget create/delete/display settings, and a test sandbox; explicitly cannot support whitelisting, promote to prod, scaffold to lower, or FI comparison. Its migration table lists legacy providers converted to templates (for example `Alkami.MicroServices.SSOProviders.Avoka` / `alkami.apps.avokasso`, BALoyaltyRewards, Black Knight, Blend, Centrix, Cubus, CUDL, IMSI, Instant Open, Merchant Capture, MeridianLink, MyCardInfo, PayTrace, QCash, SymApp, TCI, UChoose, PSCU uChoose) plus net-new ones (AlogentSSO, Fiserv SCO, HippoMortgage, MessagePay, TruTreasury, Finastra NetCapture, HSA Lively, ScribeUp) with ticket and commit links.

Landing page: lists configurations with widget name and last modified/created date; sort by name, widget, date; search by name; Edit, History, Create SSO.

Edit: opens read-only; toggle edit mode (upper right); unsaved changes warn on exit. Recommended: edit large JSON in VS Code, Notepad++, or Sublime and paste back; keep a local copy (history cannot be copied). Save errors: "unable to parse configuration" (malformed JSON: commas, brackets, quotes); `Syntax error occurred in <location> for the expression: <expr>` (unknown or misspelled expression); `Invalid expression occurred in <location> for the expression: <expr>` (wrong or missing arguments, for example `Left` with one argument); "An unknown error has occurred" (contact Alkami or the Carbon team).

History: all changes since creation including DST and manual DB changes; "INSERTED" marks creation; "View Changes" shows before/after. Limitations: no true diff, cannot copy from history (DEV-194414), and the recorded user is the microservice service account, not the admin.

SSO Creation: "Create SSO", choose a pre-made or Default template, fill all fields (Provider Name, Widget Name, Description (internal), Display Settings (default all platforms), plus template-specific fields, all required), "Generate Config", then "Save SSO", which creates the configuration, the widget at `/StandardSso/<Provider Name>`, and site text under the widget-name namespace. Nothing is saved before Save SSO.

Try Now: open a configuration, click "Try Now"; search users by name or username (partial match, no wildcards); table shows name, username, type, and for business users sub/master and entity. Yellow toast errors: "User has no accounts eligible for this SSO" / "User has no cards eligible for this SSO", "User is missing email", "User has insufficient permissions to access this SSO", "User is missing a user level extended property required for this SSO". Account/card selection appears only when account or card expressions are used, showing the end-user dropdown. Inline and NewWindow behave normally; SameWindow opens a new window so the admin stays in Admin. Red errors: invalid JSON, null values into operation expressions (use `Coalesce`), or an HTTP service returning status 300 or above. Backed by `POST /v1/sso/playground/evaluate` (test user JWT) and `GET /v1/sso/playground/users` (admin).

Templates: define how a config is presented and editable in Admin. Existing templates: https://gitlab.mgmt.alkami.net/WIDD/alkami.microservices.standardsso/-/tree/develop/Alkami.MicroServices.StandardSSO.Migrations/Migrations/Templates. Build new ones with https://gitlab.mgmt.alkami.net/WIDD/standardssotemplater (clone, run locally in Visual Studio; README; DEV-203747). Prompts: dev ticket number (for the `[MicroServiceMigration]` attribute); provider name (file and template name); legacy conversion yes/no, then legacy provider and widget names which must match `core.Provider` and `core.Widget` or pre-fill fails; form items as `key=Label|Type|DefaultValue` (`key` referenced in the config as `"[FieldName]": $.[key]`; `Type` is `text`, `select` (then label/value pairs), or `bool`; `DefaultValue` optional, may use `$.providerSettings.[SettingName]` or `$.widgetSettings.[SettingName]`, for example `url=SSO Url|text|$.providerSettings.requesturl ? $.providerSettings.requesturl : ''''`); DisplayLocation default (`1` Inline, Enter NewWindow); then paste the raw config. Templates are JSONata (`$` is the input document, `.` a field). Transformations: double quotes become doubled single quotes (`''`) except in embedded JSON; single quotes inside double quotes become backticks; embedded raw JSON such as `Body` must use double-escaped quotes (`\""`).

Checklist (client walkthrough): confirm flag and permissions (and Full Access VAU for Try Now); gather vendor values; prefer a template; create or open; generate/update; review expressions against the glossary and guard nullable values with `Coalesce`; test with Try Now using a succeeding user and one that exercises filters/permissions/missing data; save; review history.

Sources: Standard-SSO Admin (https://confluence.alkami.com/spaces/SDKC/pages/556889825); SSO Admin Template Creation (https://confluence.alkami.com/spaces/SDKC/pages/556889839); SSO Creation (https://confluence.alkami.com/spaces/SDKC/pages/556889855); Try Now (https://confluence.alkami.com/spaces/SDKC/pages/556889862); View/Edit Configurations (https://confluence.alkami.com/spaces/SDKC/pages/556889871); Standard-SSO - Admin Widget Project (https://confluence.alkami.com/spaces/SDKC/pages/556889880); Standard SSO Admin Client Guide (Full Walkthrough) (https://confluence.alkami.com/spaces/SDKC/pages/586125826); REST Endpoints (https://confluence.alkami.com/spaces/SDKC/pages/556889824).
