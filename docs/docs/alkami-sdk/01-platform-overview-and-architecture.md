# Alkami Platform and SDK Overview

What this covers: the Alkami (ORB) platform and how the SDK relates to it, the extension points the SDK exposes (widgets, snippets, modules, microservices, feature compounds), the day-to-day development and deployment pattern, the access and onboarding checklist (feeds, Jira, Confluence), and the SDK Project Proposal process that must precede any coding. It also consolidates several foundational how-to pages: common platform services used from SDK code (Security service, CurrentUser, settings), calling Symitar RepGens through the SymConnect Multiplexer microservice and troubleshooting SymConnect connectivity, business banking and sub users in the local SDK, package assignment rules (Rules microservices), the static-user authentication override for testing failed logins, retrieving a member core identifier from Item settings, and the Getting Started Samples page.

## 1. The ORB platform and the SDK

### What the platform is

The Alkami Platform (also known as the Orb platform, product name ORB) is a collection of web applications and Microsoft Windows Communication Foundation (WCF) services. A Microsoft SQL Server instance serves data through a small set of legacy WCF IIS services and an ever-growing suite of standalone Windows Services referred to as microservices. These services aggregate information from SQL Server databases and third-party vendors and serve it to the client application. The Trusted Roadmap for ORB is published on Midas (https://midas.alkami.com/product/roadmap-trusted/).

### Platform vs SDK

- The Platform is the base run-time product developed and maintained by Alkami. Many of its underlying parts cannot be edited using the SDK.
- The SDK provides the tools to add to the platform in the same way Alkami developers do. The platform is provided so that you can run your work on a local machine.

### Extension points

To extend the platform you create additive components that bolt onto the base platform without altering the underlying environment:

- Widgets: additive web components (client widgets and admin widgets).
- Snippets: widget extensions.
- Modules.
- Microservices: server-side services on the application tier that persist data, retrieve data, or communicate with third-party endpoints. Widgets, snippets and modules communicate with microservices.
- Feature Compound: the combination of services and web applications that defines a user-facing function such as Transfers, Bill Pay, or Card Management. The SDK can be used to create feature compounds.

A developer uses Visual Studio project templates and Alkami-developed NuGet packages to create Widgets, Snippets, Modules and Microservices. Integration Types listed under Getting Started include the Alkami Standard SSO Configuration Tool (Jarvis), building a microservice using the Standard SSO Template, and the SDK Getting Started Samples.

### The development pattern

The common pattern is: write code in Visual Studio using Alkami's templates and NuGet packages, then deploy the assembled code to a running ORB development environment. Typically you must restart the web client to pick up newly added assemblies: the IIS web processes need to be stopped and sometimes the ASP.Net temp cached files must be cleared. After the web client process starts up, the changes are visible in the browser.

### Configurations

Many components require persistent settings and configurations, ranging from descriptive metadata to simple key-value settings. This applies to SDK projects and to many Alkami-developed services. These configurations can change independently of the SDK process, so Business and Development teams must coordinate to track and audit configurations as an SDK project iterates. The Configurations page (Confluence pageId=69993680) has more detail.

### Get access (onboarding checklist)

New developers must go through the onboarding process to access the development package feeds and client service portals. If the answer to any of these is "No", create a Jira support ticket (have a teammate create it if you lack Jira access) and state what is missing:

1. Can you log into https://feeds.alkamitech.com using your company email and see your assigned feeds? The "Add a package to your development feed" page lists which feeds should be assigned to you.
2. Can you log into https://jira.alkami.com/ and see your company's Delivery or Client Services project? Can you select all of the following issue types when creating a new issue: `SDK Submission` and `SDK Support Incident`? (See "SDK Issue Types in Alkami's JIRA Server".)
3. Can you log into https://confluence.alkami.com and see the SDK Documentation space?

### Next steps and support

- Review the Code Guidelines and Standards page (required reading).
- Install the development environment per SDK Machine Setup.
- Use Samples and Tutorials to start coding; read Submit Projects before submitting work.
- SDK support: sdksupport@alkamitech.com.
- Alkami University SDK training courses (log in to https://au.myabsorb.com/#/login first): Introduction to the Alkami SDK; SDK Setup, Support, and Coding Standards; Submitting an SDK Project; SDK and the FI Admin Platform.
- The SDK Documentation space replaces the earlier Dev Labs section on Midas; the SDK Product Guide is on Midas. The space blog announces releases (the space's release pages cover 2018 through 2023.1).
- The "Alkami Service Architecture" page holds only a slide deck (view as PDF if the browser rendering is poor). The "SDK-Accessible Microservices" page (updated 2025-08-04) and "Alkami Client Developer Network" page have no readable content in the export.

Sources: Get Started with the Alkami SDK (https://confluence.alkami.com/spaces/SDKC/pages/55346994); SDK Documentation (https://confluence.alkami.com/spaces/SDKC/pages/33165850); Alkami Service Architecture (https://confluence.alkami.com/spaces/SDKC/pages/48811638); Alkami Client Developer Network (https://confluence.alkami.com/spaces/SDKC/pages/303531334); SDK-Accessible Microservices (https://confluence.alkami.com/spaces/SDKC/pages/470600898).

## 2. SDK Project Proposal (required before coding)

Before writing a line of code you must submit an SDK Project Proposal as a Jira ticket. It is the starting point for every SDK project, no exceptions. The SDK team reviews it from two angles: Product review (does it fit the intended use of the SDK) and Technology review (is it technically feasible; platform constraints, dependencies, integration patterns). Proposals may be routed to other internal Alkami teams. Submitting a proposal does not constitute approval to begin development.

Key rules:

- Incomplete submissions are put on hold until gaps are filled; reviews happen in order of arrival.
- Complex integrations (deep third-party dependencies, non-standard authentication flows, data patterns outside platform design) may require a consultation with Alkami's Professional Services (PSO) team, which is scoped and priced separately.
- Starting development before explicit approval is at your own risk; Alkami support for issues stemming from unapproved work is limited. If you have already started, disclose it in Section 6.
- The SDK team responds within 5 business days: approve for standard SDK development, ask follow-up questions, route for feasibility assessment, or flag for PSO consultation.

### How to submit

Use the Jira `Feature Request` issue type. Address each section below in the description in plain language and attach supporting documents to the ticket. Set these fields exactly:

| Field | Value |
| --- | --- |
| Label | `sdk_project_proposal` |
| Team | `SDK` |
| Component | `SDK` |

The Team field is not available on the Jira create screen: after creating the ticket, edit it and set Team to SDK. Every section must be addressed; if one does not apply, say so explicitly rather than leaving it blank.

### Required sections (2026 version)

1. About Your Project: project name; plain-language description of what it does and what the user sees; the problem it solves; whether it is net-new, an enhancement, or a replacement of an existing integration.
2. Who's Involved: name, email and organization of the primary developer contact; in-house vs third-party partner/SI; whether a contract with a vendor/partner/SI exists and whether it includes a delivery deadline; whether any Alkami team (Support, PSO, Sales) has already been involved.
3. User Experience: intended users (end users, admin portal admins, or both); step-by-step user journey; mockups/wireframes/screenshots if available; whether access is gated by user permissions, account type, or Alkami package, and how.
4. Architecture and Technical Approach: type of build (widget, microservice, SSO integration, admin widget, or combination); proposed architecture and high-level data flow; which Alkami services, microservices, or data sources will be used; whether it connects to anything outside the Alkami platform (vendor API, core banking system, externally hosted service) and, if so, what data moves and whether vendor API docs exist; whether financial transactions (transfers, payments, remittances) are involved; whether credentials, certificates, or configurations must be set up or allowed by Alkami.
5. Data and Security: what user or financial data is accessed, transmitted, or stored; whether sensitive data (PII, account numbers, credentials, financial data) leaves the Alkami platform, and if so where it goes, how it is secured, and retention.
6. Timeline and Milestones: target go-live date; key internal milestones; any contractual delivery deadline (parties and date); current status (not started, planning, in development).
7. Supporting Materials: mockups/wireframes, user journey or flow diagram, architecture diagram, data flow diagram (required if the project connects to anything outside the Alkami platform), vendor API documentation, vendor contract or SOW (redacted is fine).

### Older (2024) Feature Request template

The earlier "Project Proposal" page (updated 2024-01-26) used the same Label/Team/Component settings and asked for: Project Information (business purpose; new vs existing widget/service; which users have access; whether driven by a permission and whether View As User (Read Only) users should have access; whether limited by account type; whether driven by packages; where the widget is accessed; screenshots of current and expected behavior; anticipated launch timeline), Technical Information (what calls are implemented; how the calls are authenticated; whether sensitive data is transferred in body, header, URL; how data is partitioned or validated; hashing or encryption algorithms and key lengths; a data flow diagram for vendor/3rd-party integrations), and Other Information (required certificates, particular configurations, vendor integration details). The 2026 page supersedes it but the questions are still useful prompts.

Sources: SDK Project Proposal (https://confluence.alkami.com/spaces/SDKC/pages/583075954); Project Proposal (https://confluence.alkami.com/spaces/SDKC/pages/282933026).

## 3. Getting Started Samples

The SDK Getting Started Samples page (under Integration Types) offers samples to test services, calls and contracts. Each sample is behind an expandable section on the Confluence page (content not included in the export):

- MyMoney Sample: demonstrates connecting to Alkami microservices and using the Alkami GenericProxy service. The MyMoney client widget is installed locally at `C:\AlkamiSDK\Samples\MyMoney\ClientWidget` and shows reading WidgetSettings and writing UserWidgetSettings.
- My First SSO Provider: what an SSO Provider service is and how to create one with Alkami's Visual Studio template. SSO Provider services define configurable settings and the Single Sign-On implementation; widgets use them to automatically log users into associated applications or domains. Settings are managed in the Admin Portal under Setup > Integration Settings > Providers, so admins can change them without code changes.
- Generic SSO: a standardized contract for communicating with a microservice to accomplish Single Sign-On. Several Alkami widgets call this contract and several Alkami-built microservices respond to it. With the SDK you can build a custom widget that calls one of the Alkami microservices, or build a custom microservice from the Generic SSO template and configure the system to execute SSO using your service. Example widget and microservice projects are linked from the page.

Licensing note: some Alkami components have contractual or licensing requirements. Finding a chocolatey package in the feeds does not mean you are permitted to use it in production; work through contractual considerations with your CSM, and there may be additional cost.

Other sample locations referenced in this topic: `C:\AlkamiSDK\Samples\Widgets\Sample.Client.Widget.ChangeAddress` (CurrentUser contact info), and the SymConnect widget sample (install instructions on Samples and Tutorials, https://confluence.alkami.com/spaces/SDKC/pages/62701693).

Sources: SDK Getting Started Samples (https://confluence.alkami.com/spaces/SDKC/pages/273424412); Common services used in SDK Projects (https://confluence.alkami.com/spaces/SDKC/pages/78662474).

## 4. Common services used in SDK projects

### Get a list of accounts for the current user (Security microservice)

Add these NuGet packages (and their dependencies):

- `Alkami.MicroServices.Security.Client`
- `Alkami.MicroServices.Security.Contracts`

```csharp
// Create a request object to send to the security service
GetUserRequest userRequest = new GetUserRequest();

// "augment" the request. You can manually populate the userid, etc.
// or if you are calling from a microservice then use CopyBaseFrom()
userRequest.CopyBaseFrom(request);

SecurityServiceClient SecurityService = new SecurityServiceClient();

// you must have a filter. If getting the accounts for a specific user
// your filter will just need the user's identifier
userRequest.Filter = new UserFilter() { UserIdentifiers = new List<Guid>() { request.UserIdentifier.Value } };

// you also must have a mapper. To have the accounts returned from the call,
// specify ShouldIncludeUserAccounts = true
userRequest.Mapping = new UserMapper() { ShouldIncludeUserAccounts = true };

// we only expect a single user with its accounts to be returned.
userRequest.MaxResults = 1;

GetUserResponse userResponse = await SecurityService.GetUserAsync(userRequest);

var users = userResponse.Users;
Logger.DebugFormat("{0} user accounts returned in the payload.", userResponse.Users.Count);
List<UserAccount> accounts = users.FirstOrDefault().UserAccounts;

// Filter the accounts based on relationship or other permissions
List<long> accountIds = new List<long>();
foreach (var ua in accounts)
{
    //0 = Unknown
    //1 = Primary Owner
    //2 = Joint Owner
    //3 = Linked - access granted indirectly; some level of permission on this account
    //4 = Aggregated - access granted indirectly from a remote aggregation provider
    //5 = Business account - access granted by virtue of the entity the user belongs to
    if ((ua.Relationship <= 2 || ua.HasMasterRights) && !ua.Deleted && !ua.HideFromEndUser)
    {
        accountIds.Add(ua.AccountId);
    }
}
// For more account details, call the Accounts Microservice with the filtered accountIds
```

Rules: a `Filter` and a `Mapping` are both required on `GetUserRequest`. When calling from a microservice, use `CopyBaseFrom(request)` to carry the base request values (user, bank, correlation) into the downstream request; in a widget controller the equivalent is `this.AugmentRequest(request)`.

### Read settings

- In a Widget: the MyMoney client widget (`C:\AlkamiSDK\Samples\MyMoney\ClientWidget`) shows reading WidgetSettings and writing UserWidgetSettings.
- In a provider service (microservice): follow the "Provider Based Service" pattern in My First Microservice (https://confluence.alkami.com/spaces/SDKC/pages/67617391) to read configurable settings managed in the admin site and stored in the database.

### Current user name and TaxId (widget)

A widget controller that inherits from the Alkami `BaseController` has a `CurrentUser` object:

```csharp
firstName = CurrentUser.FirstName;
lastName = CurrentUser.LastName;
ssn = CurrentUser.TaxId;
```

### Current user address, email, phone (widget)

Contact information is pulled from the core system and cached in the Alkami database tables. One generic method serves the three contact types; the return value is a list because users can have multiple addresses (home, mailing, work) and phone numbers (home, work, mobile) if the core supports it:

```csharp
CurrentUser.GetUserContact<UserContactAddress>()
CurrentUser.GetUserContact<UserContactPhone>()
CurrentUser.GetUserContact<UserContactEmail>()
```

Sample: `C:\AlkamiSDK\Samples\Widgets\Sample.Client.Widget.ChangeAddress`.

Sources: Common services used in SDK Projects (https://confluence.alkami.com/spaces/SDKC/pages/78662474).

## 5. SymConnect: calling RepGens and other Symitar features

### Preferred approach: SymConnect Multiplexer microservice

The current way to reach a SymConnect host is through `Alkami.MicroServices.SymConnectMultiplexer.Service.Host`. Because it is a microservice, it can be called from either a Widget or another Microservice. The legacy Repository approach from a widget (below) is deprecated and will eventually be eliminated.

Required NuGet packages (minimum) from the Alkami feeds:

- `Alkami.Subscriptions.ParticipatingClient`
- `Alkami.App.Multiplexer`
- `Alkami.MicroServices.SymConnectMultiplexer.Client`
- `Alkami.MicroServices.SymConnectMultiplexer.Contracts`
- `Alkami.MicroServices.SymConnectMultiplexer.Utilities` (must be version 2.6.0 or later; beginning September 2020 this Utilities dll must be versioned to avoid problems with different versions installed in the various widgets in an environment)

Widget packaging requirement: the Multiplexer Contracts, Data, Client, and Utilities assemblies must be included in the widget's bin folder. Typically via this .nuspec line:

```xml
<file src="bin\Alkami.MicroServices.SymConnectMultiplexer.*" target="lib" exclude="**\*.config"/>
```

Usings:

```csharp
using Alkami.MicroServices.SymConnectMultiplexer.Contracts;
using Alkami.MicroServices.SymConnectMultiplexer.Contracts.Requests;
using Alkami.MicroServices.SymConnectMultiplexer.Contracts.Responses;
using Alkami.MicroServices.SymConnectMultiplexer.Data;
using Alkami.MicroServices.SymConnectMultiplexer.Data.Constants;
using Alkami.MicroServices.SymConnectMultiplexer.Service.Client;
using Alkami.MicroServices.SymConnectMultiplexer.Utilities;
```

Widget sample (controller calling a RepGen):

```csharp
public class SampleSymConnectController : BaseController
{
    public IUserRepository SecurityUserRepository { get; set; } // for getting the user object

    public static Func<IMultiplexerContract> MultiplexerServiceFactory = () => new MultiplexerClient();

    public CallSymConnectResponse CallCustomRepgenActionWithBody(string repgen, string memberIdentifier, string body)
    {
        var callRepGenRequest = new CallRepGenRequest
        {
            MemberIdentifier = memberIdentifier,
            RepGenName = repgen,
            Body = body
        };
        this.AugmentRequest(callRepGenRequest);
        callRepGenRequest.MaxResults = 1;

        // Either use AsyncHelper to run synchronously, or make this method async and await
        CallSymConnectResponse callSymConnectResponse = AsyncHelper.RunSync(() =>
            MultiplexerServiceFactory().CallRepGenAsync(callRepGenRequest));

        return callSymConnectResponse;
    }

    public ActionResult Index()
    {
        SampleSymConnectModel m = new SampleSymConnectModel();
        try
        {
            var mySpecialRepgen = "SYC.GETACCTINFO.ALKAMI.V6"; // name of your RepGen/PowerOn
            if (string.IsNullOrWhiteSpace(mySpecialRepgen))
                throw new NullReferenceException(nameof(mySpecialRepgen));

            var user = ServiceCheck.IsSuccess(SecurityUserRepository.Get(CurrentUserIdentifier, false));
            if (user == null)
                throw new Exception("No user found");

            // Build the SymConnect message using the AppendField extension method on StringBuilder
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendField("JRGSTATE=START"); // common RepGen parameter; may not be needed

            if (!string.IsNullOrEmpty(user.TaxId))
            {
                // This RepGen expects the TaxId in JRGUSERCHR3
                stringBuilder.AppendField("JRGUSERCHR3", user.TaxId.PadLeft(9, '0'));
            }
            else
            {
                Logger.Debug("User did not have an SSN value to pass to the repgen. Continuing without it.");
            }

            var body = stringBuilder.ToString();
            string memberIdentifier = user.CustomerId; // the RepGen needs the member number

            Logger.Debug($"Calling {mySpecialRepgen} with member identifier {memberIdentifier} and payload {body}.");

            CallSymConnectResponse response = CallCustomRepgenActionWithBody(mySpecialRepgen, memberIdentifier, body);
            Logger.Debug($"Multiplexer response: Status Code: {response.StatusCode}, Status Message: {response.StatusMessage}, Response Lines: {string.Join(Environment.NewLine, response.ResponseLines ?? new List<string>())}");

            string validationMessage = "";
            // Utility.IsResponseSuccess() checks whether the RepGen call succeeded
            if (response.HasError || response.StatusCode != SymConnectStatusCodes.Success || !Utility.IsResponseSuccess(response, true, out validationMessage))
            {
                Logger.Error($"SymConnectSample error status message : {response.StatusMessage}");
                if (string.IsNullOrEmpty(response.StatusMessage) && response.ValidationResults.Count > 0)
                    return View("Error", new ErrorModel(response.ValidationResults[0].Message));
                return View("Error", new ErrorModel(response.StatusMessage + "; " + validationMessage));
            }

            // ResponseLines is a list of strings composed of the print statements from the RepGen
            if (response.ResponseLines == null)
            {
                throw new AlkamiException(null, ErrorCode.ValidationError, SubCode.NoRecordsFound, $"Repgen {mySpecialRepgen} did not return any lines.");
            }

            // Read a specific returned field, e.g. RGUSERCHR2
            string myReturnedFieldValue = response.ResponseLines.GetFieldValue(ReportGeneratorMessageNames.Rguserchr2);

            m.LinesFromRepGen = response.ResponseLines;
            return View("Index", m);
        }
        catch (Exception e)
        {
            Logger.Error("Error [GET] Controller/Index", e);
            return View("Error", new ErrorModel("Error thrown " + e.Message));
        }
    }
}
```

Microservice sample (reading records via `ReadRecordsAsync`):

```csharp
namespace Test.MS.MySymConnect.Service
{
    public partial class ServiceImp : IMySymConnectServiceContract
    {
        public static Func<IMultiplexerContract> MultiplexerServiceFactory = () => new MultiplexerClient();

        public async Task<MySpecialResponse> MySpecialMethogAsync(MySpecialRequest request)
        {
            var response = new MySpecialResponse();

            ReadRecordsRequest req = new ReadRecordsRequest();
            req.CopyBaseFrom(request); // does what AugmentRequest does in a Widget controller

            req.MemberIdentifier = "12345";
            string hierarchicRecordPath = "SHARE#80";
            req.AddRecordFields(hierarchicRecordPath, new List<string> { "ID", "TYPE", "OPENDATE", "BALANCE" });
            req.MaxResults = 1;

            RecordResponse recordResponse = await MultiplexerServiceFactory().ReadRecordsAsync(req);

            if (recordResponse.HasError || recordResponse.StatusCode != SymConnectStatusCodes.Success)
            {
                Logger.Error($"Error status message : {recordResponse.StatusMessage}");
                // add error information to the response
            }
            else
            {
                response.ItemList = recordResponse.ItemList; // may need to translate between object types
            }
            return await Task.FromResult(response);
        }
    }
}
```

Testing note: when testing the microservice with the MicroservicesTester, set the `BankIdentifier` and `BankUrl` to a tenant database that is properly configured to connect to SymConnect. `req.CopyBaseFrom(request)` copies these values into the request sent to the Multiplexer so the SymConnect message can be completed and delivered.

### Legacy approach (deprecated): CallCustomAPIRepository from a widget

Retained for recognition only; the microservice above is the correct long-term solution.

```csharp
var repgen = "MY_REPGEN";
var apiParameters = new CallCustomAPIParameters
{
    FunctionArguments = new Dictionary<string, object>
    {
        {"repgen_name", repgen},
        {"JRGSESSION", "1"},
        {"JRGUSERCHR1", "DO_IT"},
        {"JRGUSERCHR2", "{MemberId}"},
    },
    FunctionName = "CallRepgen",
    Settings = new Dictionary<string, string>()
};

CustomAPIResults message;
try
{
    message = ServiceCheck.IsSuccess(CallCustomAPIRepository.CallCustomAPI(apiParameters));
}
catch (Exception ex)
{
    throw new Exception(string.Format("Error getting results. {0}", ex.Message));
}

if (message != null && message.Results.ContainsKey("Status") && (string)message.Results["Status"] != "Success")
    throw new Exception("Repgen did not return 'Success' status.");

if (message != null && message.Results.ContainsKey("Lines"))
    model.DisplayMethod = (string)message.Results["Lines"];
```

Sources: Call RepGens and access other features from SymConnect (https://confluence.alkami.com/spaces/SDKC/pages/55351960).

## 6. Troubleshooting SDK SymConnect problems

### Microservice vs IIS Multiplexer

SymConnect ports used to be managed by an IIS process. During 2019 Alkami transitioned to the microservice `Alkami.MicroServices.SymConnectMultiplexer.Service.Host`; by the end of 2019 all Alkami stage and prod systems used only the microservice. When the provider setting `MultiplexerMode` is set to `2`, all SymConnect traffic goes through the microservice. All SDK environments connecting to SymConnect should be configured with `MultiplexerMode=2`. As of January 2020 the legacy SymConnection provider is not used: set MultiplexerMode to 2 and only assign ports for the multiplexer.

### Step-by-step guide

1. Change the hosts file entry for the SymConnectMultiplexer to `127.0.0.1` (this is for the IIS Multiplexer process, but check in case legacy code is in use unknowingly).
2. Make sure the correct IP address of the SDK box was used in the `PortDriver.cfg` file in Symitar. The SDK box address may be NATed when connecting to Symitar; Symitar must expect the connection from the correct, possibly translated, IP.
3. Test the TCP connection from the SDK to Symitar with Telnet (example: Symitar at 10.10.10.1, port 13222 assigned to the SDK box):

   ```
   open 10.10.10.1 13222
   ```

   A failure usually shows a message within about 30 seconds; success usually shows nothing. If connected, type `HANDSHAKE` and Enter; you should receive `RSHANDSHAKE`. This does not work if the port is configured to use encryption. Do not continue with later steps until the HANDSHAKE test works.
4. Make sure the latest versions of these packages are installed:

   ```powershell
   choco upgrade Alkami.App.Providers.Multiplexer.Client
   choco upgrade Alkami.App.Providers.Core.SymConnect
   choco upgrade Alkami.MicroServices.SymConnectMultiplexer.Service.Host
   ```

   Some clients have a custom version of the `Alkami.App.Providers.Core.SymConnect` package; search the choco.dev feed for your name (for example `Alkami.App.Providers.Core.SymConnect.AwesomeCU`). These SymConnect-specific packages are not included in `Alkami.MachineSetup.Features`, so after upgrading MachineSetup.Features you must update them too. If you upgrade the SDK and suddenly cannot log in, the core connection may have been lost and these packages need upgrading.
5. Check all provider settings for the SymConnect core provider. Log into the stage admin site and export the provider settings for reference. Usually the SDK connects to the same SYM on the same Symitar system as stage, and all settings match except `HOSTIPADDRESS` and `SOCKETLIST`.
6. Log files containing `multiplexer` in the name help troubleshooting; the requests log contains the actual SymConnect requests and responses.

### Provider configuration notes

- `HOSTIPADDRESS` must be an actual IP address, not a DNS name.
- `SOCKETLIST` syntax (legacy provider): `[{"PortValues":"port1,port2,port3"],"MachineName":"sdkMachineName"}]` where the ports are the integer socket values assigned to this SDK and sdkMachineName is the Windows machine name (syntax reproduced as written on the page).
- If the Multiplexer microservice is used in addition to the legacy provider, separate sockets must be assigned to the microservice with a second entry whose MachineName has the `_MS` suffix: `[{"PortValues":"port1,port2,port3"],"MachineName":"sdkMachineName"},{"PortValues":"port4,port5"],"MachineName":"sdkMachineName_MS"}]`.
- Current (microservice-only) example:

  ```
  [{"MachineName":"sdkmachinename_MS","PortValues":[12100,12101]}]
  ```

- Corresponding entries in the Symitar Episys `portdriver.cfg` file:

  ```
  ; for each socket you need to add a line to map the socket to the correct SYM
  socket12100 -NET=4 -CRYPT=0 -TIMEOUT=0 -SYC 099
  ; also need to add an entry to set the source IP address for each socket
  12100 192.168.0.10 unknown Alkami SDK Box
  ```

### Known issues

- Error in the Multiplexer log after a machine restart: `System.Management.Instrumentation.InstanceNotFoundException: Could not find a compatible service for Alkami.MicroServices.Settings.Contracts.ISettingsServiceContract with minimum version of [4.0.0.0].` Resolution: stop the Settings microservice, stop the Multiplexer microservice, start Settings and wait about 30 seconds, start Multiplexer and wait about 30 seconds. If the first login errors, try again. If it recurs after restarts, set the Multiplexer microservice startup mode to automatic (delayed start).
- If the SSN of a user previously registered in the SDK is changed in the core, that user cannot log in. The sync fails with the message `user is not eligible to login for bank` in `Alkami.Bank.Host.log`. Fix by updating the SSN in the `core.users` table.

Sources: Troubleshooting SDK SymConnect problems (https://confluence.alkami.com/spaces/SDKC/pages/78674510).

## 7. Business banking and sub users in the SDK

Starting point for business banking features. If connected to a test core, some steps may differ; it may be better to set up a user directly from the admin portal in the staging environment.

### Business entities (users)

The SDK's `DeveloperDynamic` database has two business users representing two entities: Rene Beecken and Alina Adamec. Their usernames and business details are visible in the admin portal at https://admin-developer.dev.alkamitech.com/Users/ManageUser. One of these users is used to create sub-users once the Business Admin feature is enabled.

### Enabling the Business Administration widget

The SDK does not include the Business Admin widget by default. Three SQL scripts are provided on the page; each ends with `ROLLBACK TRAN` and a commented `--COMMIT TRAN`, so you must switch to COMMIT after verifying the output. Summary of what each does:

1. Install Widget: inserts or updates the `core.Widget` row with `Name = 'BusinessAdmin'`, `AssemblyInfo = 'Alkami.Client.Widgets.BusinessAdmin'`, display name `Business Admin`, icon `business-admin`, `DisplaySettings = 7`, `WidgetType = 0`, initially `Active = 0`; creates the `dbo.LocalizableResource` named `Widget.BusinessAdmin` plus the `core.Item` (ItemType `Localizable Resource`, SecondaryId 1033) and `core.ItemSetting` rows for `DisplayName` and `Description`; inserts a `core.WidgetNotificationAction` (BusinessOnly = 1); copies `core.WidgetSetting` rows from the old widget named `BusinessAdministration` if it exists.
2. Configure Widget Flavor: for the flavors listed in `@TargetFlavors` (default `'Business Banking Flavor'` from `core.Flavor`), replaces the old `BusinessAdministration` widget with `BusinessAdmin` in `core.FlavorWidget` and `core.UserWidget`, or inserts new rows (`AddedByDefault = 1`, `FavedByDefault = 1`) when neither exists. A misspelled flavor name is silently not configured.
3. Enable Business Admin Widget:

```sql
WHILE @@TRANCOUNT > 0 ROLLBACK
SET XACT_ABORT ON
SET NOCOUNT ON
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

--USE DeveloperDynamic

BEGIN TRAN

DECLARE @WidgetId BIGINT,
@RowCount BIGINT;

SELECT @WidgetId = Id FROM [core].[Widget] WHERE Name = 'BusinessAdmin'

IF @WidgetId IS NOT NULL
BEGIN
UPDATE core.Widget
SET Active = 1
WHERE Id = @WidgetId;

SET @RowCount = @@rowcount
IF (SELECT @RowCount) > 0
PRINT 'Updated ' + CAST(@RowCount as nvarchar(10)) + ' core.Widget records'

PRINT 'Widget BusinessAdmin updated to Active = 1'
END
ELSE
PRINT 'Widget BusinessAdmin not found!';

ROLLBACK TRAN
--COMMIT TRAN

SET TRANSACTION ISOLATION LEVEL READ COMMITTED
WHILE @@TRANCOUNT > 0 ROLLBACK
```

The full Install Widget and Configure Widget Flavor scripts are on the source page (the widget install script is a general pattern for registering any widget: core.Widget, LocalizableResource, core.Item, core.ItemSetting, WidgetNotificationAction, WidgetSetting).

Then install the Chocolatey packages with force so WebClient recycles and the SQL changes are picked up (PowerShell as Admin):

```powershell
choco install Alkami.Apps.BusinessAdmin -yf

choco install Alkami.MicroServices.AchTemplates.Service.Host -yf
(start the service, or flag to automatic start)

choco install Alkami.MS.PaymentCompany.service.host -yf
```

### Logging in and using the Business Admin widget

1. Log into the local environment as a business user, for example the default account `roadrunner.admin`.
2. The widget is at the `/BusinessAdmin` URL resource, opening on the "Authorizations" view (for ACH Authorizations see the Business Banking Payees and ACH Authorizations guide).
3. Roles tab: Add a Role with a name and description, click "Create Role".
4. Click "Add Accounts", select permissions, choose Internal and External accounts, click "Assign Accounts". Accounts appear under "Accounts & Limits"; limits are optional for creating a sub user.
5. Users tab: click "Add a User", fill the form (use your own email) and select the role. The new sub user (Bill Williams in the example) is registered under the business entity and can be switched between roles from this screen.

### Logging in with a sub user

Log out and log back in with the sub user's registered username (example `second.roadrunner`); the password is the standard SDK password `12345`. The sub user completes the registration process on first login.

Sources: Business Banking and Sub Users in the SDK (https://confluence.alkami.com/spaces/SDKC/pages/227909663).

## 8. Package assignment rules and fact providers

The "Package Assignment Rules and Fact Providers" page contains no content of its own; it refers to "Adding additional features to the SDK" (https://confluence.alkami.com/spaces/SDKC/pages/89659272). That page explains that the SDK install is kept to the minimum packages for a viable environment, so package assignment (Rules) is an optional feature. To enable it:

```powershell
choco install Alkami.MicroServices.Rules.Service.Host
choco install Alkami.MicroServices.Rules.DataSheet.Service.Host

## Make sure these services are running.
```

The same page lists other optional features: Content Management System (`Alkami.MicroServices.CMS.Service.Host`, `Alkami.Admin.Cms`, `Alkami.Modules.Cms`) and Client Fragment Manager (`Alkami.MicroServices.ClientFragmentManager.Service.Host`, `Alkami.Admin.ClientFragmentManagement`, `Alkami.Modules.ClientFragmentInjector`), described as a useful replacement for using Html.Raw with dangerous content. No documentation on writing fact providers was present in the exported pages.

Sources: Package Assignment Rules and Fact Providers (https://confluence.alkami.com/spaces/SDKC/pages/266831307); Adding additional features to the SDK (https://confluence.alkami.com/spaces/SDKC/pages/89659272).

## 9. Override static user authentication (test failed logins)

Available since release 2020.03. The development environment has a Static User context that always passes authentication for the test users (the Brady family members). To trigger a failed authentication, answer one of the two challenge questions with a predefined status value instead of the usual "1" and "2" (normally any non-empty value is accepted).

Available overrides (parsed into an Enum; casing must match exactly):

- `Blocked`
- `AccountDisabled`
- `AccountLocked`
- `InvalidCredentials`
- `NotAuthorized`
- `PasswordExpired`
- `PasswordMustChange`
- `ResourceNotProtected`
- `Error`
- `ChallengeUser`
- `UserNotRegistered`

Evaluation rules:

- If the first answer satisfies a fail condition, the second answer is not evaluated.
- If the first does not, the second answer is evaluated.
- If neither does, authentication succeeds.

Example: answering `InvalidCredentials` triggers an InvalidCredentials failure; the user is prompted with the failure and asked to re-enter the answers.

Sources: Override Static User Authentication (https://confluence.alkami.com/spaces/SDKC/pages/87019119).

## 10. Get the current member core identifier from Item settings

Purpose: obtain the `MemberIdentifier` for a member from ItemSettings using the `ConnectorUser` item type. This is only necessary when retrieving the MemberIdentifier from a provider based service. The MemberIdentifier is in the ItemSettings list returned by `GetItemsAsync` on the Settings microservice.

Every microservice call sends a `BaseRequest`; best practice is a dedicated request class inheriting from `BaseRequest`.

GetItemSettingsRequest.cs (Contracts project):

```csharp
using Alkami.Contracts;
using System.Runtime.Serialization;

namespace Training.MS.SettingsTest.Contracts.Requests
{
    [DataContract(IsReference = true)]
    public class GetItemSettingsRequest : BaseRequest
    {
    }
}
```

Service implementation usings and factory delegate:

```csharp
using Alkami.MicroServices.Settings.Contracts;
using Alkami.MicroServices.Settings.Contracts.Filters_And_Mappers;
using Alkami.MicroServices.Settings.Contracts.Requests;
using Alkami.MicroServices.Settings.Data;
using Alkami.MicroServices.Settings.Service.Client;

public static Func<ISettingsServiceContract> _settingsServiceFactory = () => new ServiceClient();
```

Service method:

```csharp
/// <inheritdoc />
public async Task<ItemSettingsResponse> GetItemSettingsAsync(GetItemSettingsRequest request)
{
    var itemSettings = new ItemSettingsResponse();
    var getSettingsRequest = new GetItemRequest
    {
        BankIdentifier = request.BankIdentifier,
        CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString(),
        Filter = new ItemFilter
        {
            ItemType = "ConnectorUser",
            SecondaryIds = new List<long>() { (long)request?.UserId }
        },
        Mapping = new ItemMapper
        {
            ShouldIncludeSettings = true
        }
    };

    var response = await _settingsServiceFactory().GetItemsAsync(getSettingsRequest);

    itemSettings.ItemList = response.Items.FirstOrDefault(item => item.ItemType == "ConnectorUser")?.ItemSettings ?? new List<ItemSetting>();

    return itemSettings;
}
```

Wire the operation through the standard three layers of an SDK microservice:

Contracts interface:

```csharp
[OperationContract]
Task<ItemSettingsResponse> GetItemSettingsAsync(GetItemSettingsRequest request);
```

ServiceClient.cs:

```csharp
/// <inheritdoc />
public Task<ItemSettingsResponse> GetItemSettingsAsync(GetItemSettingsRequest request)
{
    return ProxyCall((operation, inner) => operation.GetItemSettingsAsync(inner), request);
}
```

DistributedService.cs:

```csharp
/// <inheritdoc />
public Task<ItemSettingsResponse> GetItemSettingsAsync(GetItemSettingsRequest request)
{
    return _serviceContract.GetItemSettingsAsync(request);
}
```

Sources: Get the current member core identifier from Item settings (https://confluence.alkami.com/spaces/SDKC/pages/510210542).
