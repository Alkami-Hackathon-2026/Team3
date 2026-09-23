# Submitting SDK Projects and Release Processes

**What this covers.** How a finished Alkami SDK widget or microservice package gets from a developer's machine into an FI's staging and production environments. It consolidates the SDK Submission Standards and Checklist (the exact items reviewers reject on), the current NuGet package version restrictions, the legacy ProGet feed upload plus Jira "SDK Submission" ticket flow, the newer Merlin One Click Submission flow (SDK clients only, as of 2026), OTS (off-the-shelf common project) submission rules, the stage and production deployment windows, what is billable, and the full client-developed and vendor/Gold Partner release processes from code review through Go Live. Where the older pages (2018 to 2023) and the 2025/2026 pages disagree, the newer process is noted as current.

---

## 1. Submission overview and which process applies

Two submission mechanisms exist in the documentation:

1. **Legacy ProGet + Jira process** (pages from 2018 to 2025): build the Chocolatey `.nupkg`, upload it to your private dev feed on https://feeds.alkamitech.com, then open a Jira ticket of issue type **SDK Submission** with the package URL. Automation and the SDK team move it through code review, Fortify scan, stage deployment, stage validation, configuration, and production deployment.
2. **Merlin One Click Submission** (page updated 2026-02-02): "the new standard for submitting SDK widgets and services to Alkami." Available for **SDK Clients only**. Partners and system integrators "should continue using the existing process." Under One Click Submission you no longer upload packages to the feeds repository; "Alkami will not download or review packages from that location."

Regardless of mechanism, the following always apply:

- Review the Code Guidelines and Standards before submitting. Adherence is required for a successful submission.
- Test the package you are submitting using the `choco install` command. "Many issues arise from untested packages being submitted to Alkami. The Visual Studio mechanisms for deploying your code during development is not sufficient to test the integrity of a package."
- QA any work before submitting. Your projects must be tested in your local SDK environment, and you must test the actual code that you are packaging.
- Do not ever manually edit the package or its contents. Other files may be present in the package beyond what the examples show; leave them in place.

Sources: Submit Projects (https://confluence.alkami.com/spaces/SDKC/pages/51351617); One Click Submission (https://confluence.alkami.com/spaces/SDKC/pages/538188377); Submitting an SDK Project - Complete Walkthrough (https://confluence.alkami.com/spaces/SDKC/pages/315294405)

---

## 2. SDK Submission Standards and Checklist

Page last updated 2023-11-03. The same checklist is reproduced on the vendor page "SDK Code Review and Security Scan of Vendor Submission" (updated 2025-10-21) with one wording difference noted below.

### 2.1 Submission Requirements (failing these will likely result in re-submission)

**Packages**

- Proper naming convention. Example: if your organization is "US Best Financial Institution", a Widget would be named like `USBFI.Client.Widget.LoanHealth` and a microservice like `USBFI.MS.LoanHealthService`.
- Content directory for a widget package must follow the Alkami standard. The proper path for a content folder is `content\Areas\App\`.
- Disallowed files: `Web.config` files. Within a Widget package the only DLL files should be: the assembly for your widget, assemblies needed to communicate with your custom microservice, only `Alkami*.dll` files that have a version number within the filename, and any other assemblies that Alkami has specifically told you to include.
- Do not modify the Alkami `ChocolateyInstall.ps1` and `ChocolateyUninstall.ps1` files provided in the `Alkami.SDK.Templates`. (The 2025 vendor page words this as "provided by the nuget package `Alkami.Installer.WidgetConfiguration`".)

**Configuration**

- `NewRelic.AppName` value in the `app.config` for a microservice must be configured properly: it should match the microservice name and must not be `REPLACEME`.
- `AlkamiManifest.xml` should be filled out in its entirety.

**Versioning/Packages**

- The Chocolatey package version of an SDK submission and the C# `AssemblyInfo.cs` must have matching version numbers.
- At minimum, the patch value of the semantic version must be incremented for each submission.
- `Alkami.Common`, `Alkami.Client` and other Alkami reference assemblies cannot be more than two minor versions behind the submission deployment target.
- For approved JavaScript libraries, review the `\lib` folder and the `\javascripts` folder under `C:\Orb\WebClient`. If Alkami includes the library on the page, do not load that library again. All third-party JavaScript packages not in the Alkami Platform must be included, disclosed, or available in the `package.json`.

**Code**

- No PII leak in URL.
- If an object implements `IDisposable`, a `using` statement with that object is required.
- Public Internet API calls through Widget code are NOT permitted.
- Any service request must be wrapped in `try / catch`.
- Any service request must handle validation of error results.
- Exceptions must be logged (not implementing logging for exceptions is a rejection reason).
- Using `console.log` in your custom JavaScript is not permitted.

**Security/Compliance**

- Fortify submission failure.
- Insecure cryptographic algorithms.
- Use of unapproved third-party libraries; license issues with third-party libraries.
- Overriding the Setting TLS Security management value.
- Any crypto keys, API passwords, or other secret information must be removed from the submitted source code and any SQL scripts contained in the submitted package. These values need to be read from configuration settings and not contained in your source code.

**JIRA Submission Ticket**

- The SDK Component Type must be correct.

### 2.2 Submission Suggestions (will NOT cause rejection, but may cause poor performance/stability)

**Code**

- Mixing async/sync behavior.
- For dependency injection, LightInject IOC should be the container used.
- Hard-coded configuration data.
- Using `Html.SiteText` inside the `Html.Raw` helper is strongly discouraged.
- Using `ViewBag`, as this is shared across the entire web platform and can be in an unpredictable state.

**Testing responsibility**

- Your projects must be tested in your local SDK environment prior to submitting the project to Alkami for processing.
- Be sure you test the actual code that you are packaging to upload to the feeds server.

The vendor pages add these frequent Static Code Analysis rejection causes: avoid opening connections and not closing them (wrap all connections in a `using` statement); ensure your database schema does not conflict with the Alkami schema; do not allow any cross-site scripting; avoid SQL injection issues.

### 2.3 Understanding code review comments

Reviewers prefix comments with key phrases:

| Phrase | Meaning |
|---|---|
| Needs work | Describes a reason the submission is being declined. You must change your project to address it before submitting again. |
| Suggestion | Not sufficient to decline the submission, but make the change the next time you rework the project. |
| Tip | A general comment, possibly unrelated to your code. No action required. |
| Question | The reviewer is curious about something. Will not prevent processing, but reply by commenting on the submission Jira ticket. |

Examples from the page:

- Needs Work: your package is missing the widget's dll file.
- Suggestion: you should use the `AugmentRequest` extension method that is contained in WebToolkit rather than maintaining your own method.
- Tip: do a web search for "razor syntax" for examples that will help in building your views.
- Question: are you going to need us to open up a firewall port and configure routing to reach your web service?

Only "Needs Work" comments require an update to the project and a new submission.

Related references on the page: https://semver.org/, https://chocolatey.org/, https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/using-objects.

Sources: SDK Submission Standards and Checklist (https://confluence.alkami.com/spaces/SDKC/pages/62696705); SDK Code Review and Security Scan of Vendor Submission (https://confluence.alkami.com/spaces/SDKC/pages/172112670); Submitting an SDK Project - Complete Walkthrough (https://confluence.alkami.com/spaces/SDKC/pages/315294405)

---

## 3. NuGet package version requirements (what versions are current)

Page last updated 2025-01-16. Versions differ between widgets and microservices.

**Framework note:** some Alkami NuGet packages contain indirect references to .NET Framework 4.8. If you are using Alkami Release 2020.1 or later, build your projects against Framework 4.8 to avoid build errors.

### 3.1 Widget projects (required as of release 2022.6, if you use the package)

- `SymConnectMuliplexer.Utilities` v2.13 requires the addition of `Alkami.Microservices.Accounts.Contracts` to your project. Do NOT include the `Alkami.MicroServices.Accounts.WebApi.Host` package in your project.
- `Newtonsoft.Json`: if used, must be version `8.0.3`.
- `NHibernate`: must be version `3.0.0.4000`.
- `NetMQ`: remain on version `3.3.3.4` or earlier (v4.x is not supported yet).
- `Alkami.MicroServices.Accounts.Contracts` and `Alkami.MicroServices.Accounts.Service.Client`: do NOT use version `2.26.0` (earlier or later is fine).
- `Alkami.MicroServices.SSOProviders`: do not use version `1.5.0`. Use `1.4.0` or something later than 1.5.0 once released.

**If you use `Alkami.MicroServices.SymConnectMultiplexer`:**

- Always use the same version of `Alkami.MicroServices.SymConnectMultiplexer.Client`, `Alkami.MicroServices.SymConnectMultiplexer.Contracts`, and `Alkami.MicroServices.SymConnectMultiplexer.Utilities` (if available).
- The version 2.15 set of these packages works with no special considerations.
- `Alkami.MicroServices.SymConnectMultiplexer.Utilities` v2.9 through v2.14 requires `Alkami.MicroServices.Accounts.Data.2.27`. If you use those, add this package to your project: https://feeds.alkamitech.com/feeds/nuget.dev/Alkami.MicroServices.Accounts.Contracts/2.27.0 and add a `file` line to your `.nuspec` to get `Alkami.MicroServices.Accounts.Data.2.27.DLL` into your Chocolatey package.
- If you are using `Alkami.MS.AccountsOrchestration.Contracts.2.6.dll` or `Alkami.MS.AccountsOrchestration.Data.Validations.2.6.dll`, you must use version 2.30 of `Alkami.MicroServices.Accounts.Data` and include `Alkami.MicroServices.Accounts.Data.2.30.dll` within the `lib` folder in your widget package.

### 3.2 Microservice projects

- `Alkami.Subscriptions.ParticipatingClient`: use `3.7.2` or later (do not use 3.7.0 or 3.7.1).
- `NetMQ`: remain on version `3.3.3.4` or earlier (v4.x is not supported yet).
- `Alkami.MicroServices.Accounts.Contracts` and `Alkami.MicroServices.Accounts.Service.Client`: do NOT use version `2.26.0`.
- `Alkami.Test`: use version `4.2.0` (do not try to use anything newer right now).
- `Moq`: use version `4.16.1` (do not try to use anything newer right now).

Other than the above, it is recommended to update to the most recent versions of any packages you use.

### 3.3 Approved package sources

It is critical that you do not pull packages from any sources not approved by Alkami. Use only these feeds (see Hardware and Software Requirements, page 55347103, for setup):

```
https://feeds.alkamitech.com/nuget/nuget.dev
https://feeds.alkamitech.com/nuget/ThirdParty
```

Do NOT pull any packages from nuget.org or from Microsoft Visual Studio Offline Packages.

### 3.4 Why the DLL rules matter (assembly inclusion logic)

- Include all assemblies your widget uses, EXCEPT anything in `c:\orb\shared` that does not include a version in the file name. Assemblies (Alkami or third-party) that DO have a version in the file name may be included even if they are in `c:\orb\shared`; they are the exact same file and never conflict. Unversioned file names may differ from the `c:\orb\shared` copy and conflict.
- Failure modes: forgetting `Alkami.MicroServices.SymConnectMultiplexer.Service.Client.2.14.dll` works only while another widget ships that exact version, and breaks when that widget moves to 2.15; forgetting `Alkami.MicroServices.Settings.Data.5.0.dll` works only while it is in `c:\orb\shared`, and breaks when a new Alkami release ships 5.1.
- Binding redirects could solve some issues but create others; not recommended.
- `Newtonsoft.Json.dll`: unversioned file name, exists in `c:\orb\shared`, and the WebClient `web.config` binding redirect points to 8.0.0.0 (actually 8.0.3). You must NOT include the DLL in your package, and you must compile against 8.0.3. Upgrading the NuGet package to 12 or 13 will not work: the binding redirect as written will not help, and a small contract change prevents interchangeability between 8.0.3 and later versions.

Sources: Nuget package requirements (what versions are current) (https://confluence.alkami.com/spaces/SDKC/pages/82794502)

---

## 4. Naming conventions and package structure (pre-submission validation)

From the Complete Walkthrough (updated 2025-05-01). Before submitting, confirm compliance with the Submission Standards and Checklist, the NuGet version limits, and the naming conventions in Widget Coding Guidelines (page 53743045) and Microservice Coding Guidelines (page 53743063). Obtain your unique identifier per Name Your SDK Projects (page 55353106). It is recommended to keep the namespace generated from the template in place. Submissions that do not follow the naming convention are subject to fail code review and will need to be resubmitted.

**Widget naming examples**

| | FI example | Vendor example |
|---|---|---|
| Organization | US Best Financial Institution | Digital Cents Technology, Inc. |
| Product name | Loan Health | Regional Campaigns |
| Widget name / Display name | `USBFILoanHealth` | DCTI Regional Campaigns |
| Visual Studio project name | `USBFI.Client.Widget.LoanHealth` | `DCTI.Client.Widget.RegionalCampaigns` OR `<FI Identifier>.Client.Widget.RegionalCampaigns` |

**Microservice naming examples** (the display name is important so personnel can differentiate services running on a server)

| | FI example | Vendor example |
|---|---|---|
| Microservice name | Regional Campaigns | Regional Campaigns |
| Display name | USBFI Regional Campaigns | DCTI Regional Campaigns |
| Visual Studio project name | `USBFI.MS.RegionalCampaigns` | `DCTI.MS.RegionalCampaigns` OR `<FI Identifier>.MS.RegionalCampaigns` |

**Providers:** Alkami is no longer taking new submissions for providers. Existing providers require no changes for future submissions.

**Package structure:** the walkthrough page shows example package trees for Widgets and Microservices as images (not visible in the extract). The vendor page describes the standard widget package layout from the `.nuspec`: `content` (Area folder), `lib` (assemblies), `src` (source of application), `tools` (Chocolatey install/uninstall scripts).

**Nuspec metadata (vendor page):** the Alkami Visual Studio Project Template generates a generic `.nuspec`. Update the metadata with your package/company information: unique ID in the format `YourFI.Widget/Microservice.YourName`, semantic version, `Authors`, `Owners`, `projectUrl`, and `iconUrl`. Packages are immutable: 1.0.2 cannot be used again; the next would be 1.0.3. Breaking changes increment the 1st octet, less impacting changes the 2nd, bug fixes the 3rd.

Sources: Submitting an SDK Project - Complete Walkthrough (https://confluence.alkami.com/spaces/SDKC/pages/315294405); Vendor Submits Widget or Service (https://confluence.alkami.com/spaces/SDKC/pages/171911954)

---

## 5. Legacy process step one: add a package to your development feed (ProGet)

Page last updated 2018-11-19. Note: under One Click Submission (section 7), SDK clients no longer upload to feeds; this section applies to partners, system integrators, and anyone still on the legacy flow.

- Alkami uses Inedo's ProGet Server to host packages. SDK developers pull NuGet packages from Alkami's public feed and push Chocolatey packages to their private Alkami feed.
- Feeds are at https://feeds.alkamitech.com and are configured by the Alkami SDK team. Contact sdksupport@alkamitech.com for onboarding.
- SDK developers can only add/publish packages to their assigned dev feed and have read-only rights to all other feeds. Example FI feeds: `fi1.choco.dev`, `fi1.choco.stage`, `fi1.choco.prod` (vendor pages use `myfi.choco.dev`, `myfi.choco.stage`, `myfi.choco.prod`).

Steps:

1. Click on your Chocolatey dev feed (for example `fi1.choco.dev`).
2. Click the **Add Package** button at the top right.
3. In the dialog choose **Upload From Disk** (almost all cases).
4. Select the package from a folder on your computer and click **Upload**.
5. View and download the package from your private dev feed to confirm it is there. The page shows Project URL, Last Update date, Package Source, Release Notes, and Tags.
6. Copy the main URL of the package's page. This URL is pasted into the Jira submission ticket as the Package URL. Example: `https://feeds.alkamitech.com/feeds/fi1.choco.dev/FIOne.Client.Widgets.MemberServices/1.0.0`

Sources: Add a package to your development feed (https://confluence.alkami.com/spaces/SDKC/pages/51351561); Vendor Submits Widget or Service (https://confluence.alkami.com/spaces/SDKC/pages/171911954)

---

## 6. Legacy process step two: submit your SDK project in Jira

Page last updated 2023-01-24.

**Before creating the ticket:** if you are submitting a new version of an existing project, ensure there is not an open submission ticket for a previous version. If there is, cancel that submission before creating the new ticket. If you cannot cancel it, leave a comment on the old submission asking for cancellation and tag `sdksupport` for visibility. Failure to do this could result in a bad version being deployed if it is in the correct status prior to cancellation. (The automation also cancels previous versions automatically; see section 8.)

Make sure you have a proper ProGet package link (section 5).

Steps:

1. Navigate to https://jira.alkami.com/ and click **Create**.
2. **Project:** your Client Services or Delivery project. Ask your account manager or the SDK team if you are unsure of the project name. (The vendor page example uses Project "ORB Financial".)
3. **Issue Type:** `SDK Submission`.
4. **Summary:** must contain at least the name of the project being submitted, the version, and/or any settings or configurations that need to be added or updated.
5. **Description:** as detailed as needed; any special instructions go here.
6. **SDK Component Type:** the type of project. Values: `Widget`, `Provider`, `Microservice`, `Repository`, `Snippet`. This must be correct (checklist item).
7. **FI Contact** (name) and **FI Contact Info** (email). This can be someone other than the submitter; the FI chooses the contact.
8. **Package URL:** the URL of the package in your dev feed, pointing at the specific version. Example: `https://feeds.alkamitech.com/feeds/fi1.choco.dev/FIOne.Client.Widgets.MemberServices/1.0.0`
9. **Linked Issues:** any issues related to or dependent on this submission.
10. Click **Create**. Wait for updates on the ticket.

The vendor page (2025-10-21) lists additional fields on the same ticket: Contact phone number; any port waivers needed (for example, for a microservice, any external endpoint or services that the microservice will call); the **New Port waiver needed?** checkbox; and the **New submission?** checkbox. Vendors must also list the client(s) expected to receive the code on each submission ticket.

Sources: Submit your SDK projects in Jira (https://confluence.alkami.com/spaces/SDKC/pages/51351566); Vendor Submission Available to Staging (https://confluence.alkami.com/spaces/SDKC/pages/172112706); Vendor - Developed Solution Release Process for SDK (https://confluence.alkami.com/spaces/SDKC/pages/172110574)

---

## 7. One Click Submission (Merlin)

Page last updated 2026-02-02. "One Click Submission is the new standard for submitting SDK widgets and services to Alkami." Available for **SDK Clients only**; partners and system integrators continue using the legacy process. You submit, validate, and deploy packages directly through Merlin. You no longer upload packages to your feeds repository, and Alkami will not download or review packages from that location.

### 7.1 Getting started

**Install Merlin.** See SDK Machine Setup (page 40403249).

**Network requirements.** If your network has strict firewall or proxy policies, whitelist:

```
atl-gen-alk-sdk-prod-installer-us-east-1.s3.amazonaws.com
atl-gen-alk-sdk-prod-installer-us-east-1.s3.us-east-1.amazonaws.com
atl-gen-alk-sdk-dev-installer-us-east-1.s3.amazonaws.com
atl-gen-alk-sdk-dev-installer-us-east-1.s3.us-east-1.amazonaws.com
https://api.dev.alkami.com/tooling
```

**Log in.** Launch Merlin and log in with the **feeds credentials** provided during onboarding (from your original onboarding email), not your Alkami portal credentials. Contact SDK Support if you need new credentials.

**Configure Jira integration** (first-time users). The integration automatically creates and updates Jira tickets as submissions move through the pipeline.

1. Click the **Submissions** tab.
2. Click **Navigate** to generate your Jira API key.
3. Paste your key and click **Start Exploring**.

### 7.2 Submitting a package

1. **Upload:** drag and drop your `.nupkg` file (widget or microservice) into Merlin and click Next.
2. **Local Validation:** Merlin checks package structure against SDK requirements (file layout, manifest format, required metadata). Fix any flagged issues in your project and re-upload. Click Next when everything passes.
3. **Server Validation:** select the target environment. Merlin validates that the bank identifier is valid for the selected environment, the version number is unique (has not been submitted before), and all dependencies required by your package exist in the target environment. Validation errors most commonly mean a missing dependency in the target environment; check your dependency list against what is installed, or contact SDK support.
4. **Submit:** review name, version, and target environment, then click Submit. The package is queued for automated security scanning and manual code review.

### 7.3 The submission pipeline

Track progress from the Submissions tab on the Developer Dashboard.

| Stage | What happens | Typical duration |
|---|---|---|
| SUBMITTED | Package received and queued. | Immediate |
| SECURITY SCAN | Automated static analysis checks for vulnerabilities and policy violations. | ~15 minutes |
| CODE REVIEW | An Alkami reviewer manually inspects your code for quality and compliance. | 0 to 2 business days |
| DEPLOY READY | Passed all checks; ready for deployment. | n/a |

**CHANGES REQUESTED** means a reviewer flagged issues. Open the associated Jira ticket, review the comments, fix your code, and resubmit an updated package through Merlin.

### 7.4 Developer Dashboard

- **Submissions tab** filters: All, My Submissions, Pending Review, Deploy Ready.
- **Deployments tab** filters: All, My Requests, Scheduled, In Progress, Completed. Columns: Package (name and type), Version, Status, Environment, Scheduled (deploy time), Created (when requested), By (who requested it).

### 7.5 Deploying a package

**Staging before Production:** Production environments are locked until you successfully deploy to at least one Staging environment.

1. In the Submissions tab, find your package (must be DEPLOY READY) and click the rocket icon to open the deployment modal.
2. Select target environments. Each is labeled Staging or Production and shows one of: Available for deployment; Deployment scheduled (already queued); Locked (Production waiting for a Staging deployment to complete). You can select multiple Staging environments at once; selected environments show an orange border.
3. Click **Deploy**. A confirmation dialog shows how many environments you are deploying to. **Hold to Confirm:** press and hold the button for 2 seconds. This action cannot be undone.
4. Track in the Deployments tab. Stages: SCHEDULED (queued), IN PROGRESS (being applied), COMPLETE (finished). Once a Staging deployment shows COMPLETE, Production environments automatically unlock.

The page has a Troubleshooting section with headings only (Submissions failing unexpectedly; Server validation errors; Security scan failures; Can't connect to Alkami services); the body text was not present in the extract.

Sources: One Click Submission (https://confluence.alkami.com/spaces/SDKC/pages/538188377)

---

## 8. OTS Submission Guidelines

Page last updated 2026-03-05. Guide for submitting code projects through One-Click Submission.

- For all **FI named projects** (the page names Bellco, SECU and FourLeaf), use the standard One-Click Submission process (section 7).
- For **OTS named common projects** (one package shared across multiple FIs), extra work is required before submission. After building and testing locally, the `AlkamiManifest.xml` files in the `.nupkg` (both the root and the `src` folder copies) must be updated with the BankIdentifier of the first FI, using 7Zip to edit inside the archive:

```xml
<bankIdentifiers>
<bankIdentifier name="Training">639264ff-5ce5-4a9b-b194-682cb1be16a1</bankIdentifier>
</bankIdentifiers>
```

- Save the manifest, close the editor, and make sure the file is updated in the archive.
- Submit the package through One-Click Submission for that FI.
- Repeat the process for each other FI. This adds front-end work but ensures all packages are added to the package groups for consistent deployments that persist over any lane shift or pod move.

Contrast with the older vendor guidance (section 11.1): for a multi-FI component under the legacy process, vendors were told to remove the entire `<bankIdentifiers>` element from the manifest; the 2026 OTS guidance instead sets the identifier per FI and submits once per FI.

Sources: OTS Submission Guidelines (https://confluence.alkami.com/spaces/SDKC/pages/538533805)

---

## 9. Complete submission walkthrough: what happens after you submit (legacy Jira flow)

Page last updated 2025-05-01.

**Step One:** upload the packaged project to your dev feed on ProGet (section 5). **Step Two:** create the SDK Submission ticket (section 6). After that, SDK team automation moves the package through several status transitions.

**Previous versions are canceled.** A key automated step cancels submissions for any previous versions of the same package. Staging can only have one version of a specific package deployed, so a new version makes previous versions obsolete, even if deployed to stage and currently being tested.

### 9.1 Validating the package

1. **SDK automation service validation.** If the package fails any validation rule, the service comments on the ticket with the issues and cancels the submission. Fix the issues and resubmit; do not forget to bump the version.
2. **Static scan analysis by Fortify On Demand.** Examines the code for potential security exploits and incorrect coding practices that cause instability. It does not check functional requirements or whether the submission works. Issues are detailed in the ticket and a document with scan results (vulnerabilities plus suggested mitigation) is attached. After fixing, resubmit and the process repeats. On passing, the submission proceeds to SDK team code review.

### 9.2 SDK team code review

At least two SDK team members review against the Submission Standards and Checklist (section 2). Two approvals are required to move forward. Comments use the Needs work / Suggestion / Tip / Question phrases; only "Needs Work" requires an update and a new submission.

### 9.3 Status flow after code review

- **Waiting for Stage Deployment:** set when the package passes code review. A ticket is created for the Release Management (RM) team to deploy to stage. SDK automation goes on hold; RM automation takes over. (The vendor page notes the ticket shows PACKAGE SUBMITTED initially and WAITING FOR STAGE DEPLOYMENT during code review; the package and source are pushed to Bitbucket and a pull request generates the code review.)
- **Waiting for Stage Validation:** set by RM automation after stage deployment completes. SDK automation waits for stage testing. This is the point at which a submission becomes billable (section 10).
- **Stage Validation Passed / Stage Validation Failed:** you transition the ticket based on your testing. Passed moves it to Awaiting Configuration. Marking **Stage Validation Failed cancels the submission**. When passing, leave a comment stating the submission passed testing, say whether you want it promoted to production, a pre-prod environment, or both, and tag `@sdksupport`.
- **Awaiting Configuration:** SDK automation leaves two comments: one internal to the PSO team (a production configuration change may be associated), and one to the reporter with questions about configuration differences between stage and production. Answer these if there are configuration changes. If there are none, or you plan to make them through the admin site, leave a comment saying so. Tag `@sdksupport` in that comment. Production deployment will not be scheduled until an SDK team member transitions the ticket to Waiting for Prod Deployment, and the team will not do that without confirmation of configuration. If you do not want deployment before a certain date, say so here.
- **Waiting for Prod Deployment:** SDK automation is complete; RM automation handles deployment. The submission is scheduled for the next production time slot for the Pod assigned to your FI.
- **Package Deployment Finished:** verify the submission works in production.

### 9.4 Staging testing

- Staging is a production-like environment. It is your responsibility to adequately test there.
- Vendor page steps: navigate to https://developer.dev.alkamitech.com/Dashboard; in the ORB Financial Dashboard click the More (ellipsis) option and select **Widget Options**; under the Widgets tab locate your widget and click **Add**; validate the widget; make changes in Visual Studio as needed.
- If it is not working as expected, depending on severity either open a support ticket or create a new SDK Submission ticket with a fixed version. Creating a new SDK Submission ticket restarts the process from the start.
- Allow up to three weeks for completion of code testing in staging (vendor page). A build cannot go directly to production; it must be installed in Stage first and signed off.

### 9.5 Production testing

When the ticket is in Package Deployment Finished, confirm the submission works. If not, open a support ticket or create a new SDK Submission ticket with a fix (which restarts the process).

### 9.6 Ongoing responsibility for testing and updates

- The FI is responsible for ongoing testing and maintenance updates to submitted code.
- **For each Alkami release update, the FI is responsible for re-testing their SDK-developed widgets and services:** test in the updated SDK environment, then in staging, then in production.
- If changes are needed (a newly discovered anomaly or platform changes by Alkami), follow the normal submission process. The FI will not be charged for a submission caused by an Alkami breaking change.

Sources: Submitting an SDK Project - Complete Walkthrough (https://confluence.alkami.com/spaces/SDKC/pages/315294405); Vendor Submission Available to Staging (https://confluence.alkami.com/spaces/SDKC/pages/172112706); SDK Code Review and Security Scan of Vendor Submission (https://confluence.alkami.com/spaces/SDKC/pages/172112670)

---

## 10. Submission deployment schedule

Page last updated 2026-03-24.

| Environment | Days | Start time |
|---|---|---|
| Staging | Monday to Thursday | 7:00pm CT |
| Production | Sunday, Tuesday and Thursday | 10:30pm CT |

**Cutoff times**

| Environment | Cutoff | Condition |
|---|---|---|
| Staging | 4:00pm CT | Submissions have passed validation and SDK team code reviews by 4pm the same day and are queued by the submitter via One-Click Deploy functionality. A submission that misses the deadline goes into the following 7:00pm CT slot if available. |
| Production | 2:00pm CT the day of deployment | The ticket must have been transitioned (Awaiting Configuration to Waiting for Prod Deployment) by 2pm. If missed, it deploys in the next available slot. |

**Getting ready for prod deployment**

- RM automation only picks up a submission after approvals are given and the SDK team has transitioned the ticket from Awaiting Configuration to Waiting for Prod Deployment.
- SDK Submission tickets are not monitored. The SDK team relies on being tagged in a comment. When clicking Stage Validation Passed, you must leave a comment that the submission passed testing and tag `sdksupport`. You must also address the configuration questions in a comment after the transition, again tagging `sdksupport`. Without a comment confirming whether configuration is required, the ticket will not be transitioned and the submission will not move forward.

**Factors that shift production deployments**

- Code freeze dates: nothing deploys during a freeze without an escalation. The backlog after a freeze may need to be spread over two or more windows (rarely, this also happens outside a freeze).
- Pod consolidation: if a pod already has a deployment scheduled for a specific window, other deployments for that pod shift to that window. Example: on Monday, Pod 1 has a deployment scheduled for Thursday; your approved Pod 1 submission expected Tuesday is scheduled for Thursday instead.

Sources: Submission Deployment Schedule (https://confluence.alkami.com/spaces/SDKC/pages/334073019)

---

## 11. Billable items

Page last updated 2021-06-30.

### 11.1 SDK submissions

Phases: (1) the client delivers the SDK development package; (2) the client uploads it to the dev feed and creates a Jira "SDK Submission" ticket; (3) Alkami reviews for coding and security issues (steps 1 to 3 repeat until ready for staging); (4) Alkami schedules staging deployment and sets status "Waiting for Stage Validation"; **if the submission reaches this point, it is billable**; (5) after the client tests and approves in stage, Alkami deploys to production.

Alkami provides basic support for code correction suggestions and security vulnerability analysis. Deeper consultation and programming help may be billed.

For update (re-)submissions:

- **Scenario A:** Alkami requests a change and the FI submits exactly that modification. No billable items for the re-submission.
- **Scenario B:** the FI makes changes of their own design (whether or not Alkami also requested changes). Billable items for the additional submission.

An update forced by an Alkami breaking change is not charged (walkthrough page).

### 11.2 SDK support tickets

Jira tickets of type **SDK Support Incident** are the normal mechanism for help using the SDK. Simple questions are not billable. Alkami determines:

1. Time spent by Alkami is minimal: not billable.
2. The incident was caused by Alkami: not billable.
3. The support becomes more of a consulting engagement: billable.
4. Significant Alkami resources are required. All issues requiring 4 hours or more from Alkami fall into this category: billable.

Review the Master Service Record to see how many service tickets are allocated before charges accrue. Beyond that number, further support tickets can still be opened.

**Stage Match request:** a special Support Incident type. Each FI receives one Stage Match every 3 years without charge; more frequent Stage Matches are billable.

Sources: Identify SDK Billable Items (https://confluence.alkami.com/spaces/SDKC/pages/141544224); Submitting an SDK Project - Complete Walkthrough (https://confluence.alkami.com/spaces/SDKC/pages/315294405)

---

## 12. Client-developed solution release process

The page "Client - Developed Solution Release Process for SDK" (updated 2022-12-28) states only: "The following reflects the standard process of widget or service submission by FI clients directly to the Alkami SDK Team." Its workflow content is a diagram/attachment not present in the extract. The process it refers to is the one documented in sections 5, 6, 9 and 10 (upload to dev feed, SDK Submission ticket, validation, Fortify, code review, stage deployment, stage validation, configuration, production deployment), or section 7 for SDK clients using One Click Submission.

Sources: Client - Developed Solution Release Process for SDK (https://confluence.alkami.com/spaces/SDKC/pages/132708444)

---

## 13. Vendor-developed solution release process

Covers third-party vendors hired by clients and Gold Standard Partners (the term "Vendor" means both). Applies when an FI or Gold Partner submits a Feature Request for a vendor solution to be developed and/or deployed. **On each submission ticket, list the client(s) expected to receive the code.**

Phases and their child pages:

- **Development Phase - FI:** Vendor Submits Widget or Service; SDK Code Review and Security Scan of Vendor Submission; Vendor Submission Available to Staging; Request Vendor Preproduction Rollout.
- **Deployment Phase Procedures:** Vendor Deployment Scheduled for Production; Vendor Deployment Production Pilot; Vendor Deployment Production Live.

### 13.1 Development phase: vendor submits widget or service (updated 2025-10-21)

**Vendor steps**

1. **Create your Chocolatey package from the Alkami SDK download.** The SDK download contains a NuGet Chocolatey package. Develop within your Chocolatey package and maintain its configuration. After testing, upload to the private repo assigned to your FI or company on the Alkami NuGet feed. Only your authorization permits upload to your assigned feed.
2. **Install the local build into choco.dev and test:**
   - In Visual Studio, open your project and run the `install_widget.sql` script from the Tools menu in the widget template from the SDK download.
   - Make all needed changes to the widget (see My First Client Widget).
   - In ProGet, in the Dev feed (`myfi.choco.dev`), click Add Package, then Upload from Disk, then Choose File; select the `.nupkg` generated by the local build and click Upload.
   - Open the Dev feed and confirm the file reflects the current update. Copy the Project URL.
3. **Create your choco package from the nuspec file** (see section 4 for metadata rules). Once development and testing are complete, click Add Package to upload to your private choco feed, then copy the URL pointing to the specific version. Share this URL with your CSM for the Jira delivery request ticket.
4. **Submit the SDK package.** Creating the SDK Submission ticket starts an automated review and deployment process. After staging testing, the package is promoted in Jira to Production.

**FI steps: submit a Jira Delivery Request ticket for installation**

Alkami Gold Partners have one of two arrangements: (a) an existing agreement with Alkami to deploy content and services in the Alkami environment, resold by Alkami; or (b) an existing agreement with an FI on the platform, requiring legal authorization and access to upload packages to that FI's environment. Once items are submitted, the SDK Product Owner (PO) creates the Release Management (RM) ticket to route the solution to FI staging. After staging verification and prod deployment approval from the FI, the SDK PO works with RM to deploy to production.

Gold Partners resold by Alkami open a Delivery Request ticket as follows (options in Project and Issue Type may show or hide other fields):

1. Identify whether one or multiple FIs will use the component. For a multi-FI component coded with your own namespace and identifier, open `src/Manifest` in the Alkami SDK and remove (or comment out) the entire `<bankIdentifiers>` element:
   ```xml
   <bankIdentifiers>
   <bankIdentifier name="">00000000-0000-0000-0000-000000000000</bankIdentifier>
   </bankIdentifiers>
   ```
   For a widget for a specific FI coded with the FI's namespace, ensure the FI's `BankIdentifier` node is filled in with the correct FI name and identifier. (See section 8 for the newer 2026 OTS approach.)
2. In Jira click Create. **Project:** `Delivery`.
3. **Issue Type:** typically `Feature Request` for vendor-submitted widgets. Types: `Bug` (report defects; only used prior to the transition to support), `Change` (request new features), `Feature Request` (request additional functionality), `Question` (post questions to Alkami personnel).
4. **Summary:** detailed, concise. Do not enter sensitive information; it appears in email.
5. **Description:** detailed description (see Required Information below).
6. **Priority** (per contract guidelines; a wrong level may delay processing): `Critical (SEV1)` failure of an essential function (site outage, major security issue); `High (SEV2)` bugs preventing usage of a feature by most users (for example Bill Pay not working); `Medium (SEV3)` the above impacting one to a few users (use for all UAT/Pilot issues); `Low` change requests, bugs not noticeable to most users; `Lowest` do not use.
7. **Attachments:** screenshots (including URLs), logs, other helpful files.
8. **Components:** the appropriate platform, if applicable.
9. Click Create.

**Required information for the ticket:** severity and business impact; expected functionality; build version affected; steps to duplicate (bulleted); user information for those impacted (username, FI ID); screenshots (error messages, URLs); environment (Staging or Production); device types (desktop/mobile/tablet, OS, browser with version, device model); third-party applications, vendors or services involved; detailed description. For new features or enhancements also include business requirements, use cases, and requested delivery date.

**Monitor the ticket.** The party identified to receive Jira updates is notified on each status transition and on failures. Feed promotion path: push to `myfi.choco.dev`; after successful code review and Fortify scan the package is promoted to `myfi.choco.stage`; after the FI/vendor completes stage testing and approves, it is promoted to `myfi.choco.prod`.

### 13.2 Development phase: SDK code review and security scan (updated 2025-10-21)

Alkami completes the initial code review and sends the code to Fortify On Demand for static code analysis. The review checks for security exploits and instability-causing practices (such as not closing streams), not functional correctness. Issues are detailed in the ticket; the submitter fixes and resubmits, repeating as needed. Fortify completes its review first, then two SDK team members review the Fortify findings and the code. Once accepted, the package is promoted to the private staging feed and scheduled for staging deployment; the SDK developer clicks **Deployed to Stage** and the package appears in `myfi.choco.stage`. Allow up to three weeks for stage testing. A build cannot go directly to production.

The checklist applied is the one in section 2.

### 13.3 Development phase: vendor submission available to staging (updated 2025-10-21)

Staging testing steps are in section 9.4. After **Stage Validation Passed**, the ticket moves to Awaiting Configuration; work with the SDK team to confirm deployment and rollout plans and any date restrictions; the SDK team then transitions to Waiting for Production Deployment and the submission is scheduled for the next production slot for your FI's pod.

If changes are needed after staging (developer steps): in ProGet open https://feeds.alkamitech.com/feeds, compare the Dev (`myfi.choco.dev`) and Staging feed package numbering, open the staging content in the repo, apply source changes, roll the semantic version, click Build in Visual Studio (message: `Successfully created package <path>\<widget name>.<semantic number>.nupkg`), upload the new build to the Dev feed, copy the Project URL, and open a new SDK Submission ticket (Project: ORB Financial in the example; Issue Type: SDK Submission; Summary: current name and version number; Description; Contact and phone; Package URL; port waivers; New Port waiver needed? and New submission? checkboxes).

### 13.4 Development phase: request vendor preproduction rollout (updated 2022-01-11)

SDK users may schedule deployments to preproduction environments (post staging, before production).

- **FI steps:** open the Jira Feature Request ticket submitted when the vendor submitted the package; when prompted, submit approval for the vendor-submitted deployment.
- **Vendor steps:** add a note to an SDK team member requesting preproduction install after staging deployment, specifying whether the deployment should be copied exactly as it is in Stage into Prod; set the ticket status to Awaiting Configuration; work with SDK and PSO on items shared in the ticket.
- **SDK team:** creates the RM ticket for preproduction install.
- **PSO:** creates the TI configuration ahead of install.

### 13.5 Deployment phase: scheduled for production (updated 2021-12-20)

- Once the SDK Submission team receives approval, the ticket transitions to **Waiting for Prod Deployment** and the submission is scheduled for the next production slot for your FI's Pod.
- When deemed functionally acceptable and secure, Alkami installs the submission into production and runs your configuration scripts to activate the code. This is the final stage; the code is then available to end users.

**Widget identification (required in the ticket).** Either the Widget Name or the Widget Assembly MUST be provided:

| Identifier | Example |
|---|---|
| Widget Name | `MeridianLinkSsoV2` |
| Widget Assembly | `Alkami.Client.Widgets.MeridianLinkSsoV2` |
| Widget DisplayName | Applications |
| Widget Description | View your spending trends, categories, and more. |

**Widget package info:**

| Setting | Example values |
|---|---|
| Display Value | Desktop, Mobile, Table, or a combination (Desktop only, Mobile only, etc.) |
| Added by default | True or False |
| Favorited by default | True or False |
| Package(s) | Default Retail, Default Business, Retail Test, etc. |

### 13.6 Deployment phase: production pilot (updated 2021-12-20)

The SDK team helps configure microservices and widgets in Staging; for Production, the **PSO** group handles the last steps. This applies to **new** widgets/services only; upgrades of existing items do not need PSO. Engage PSO by directing a comment to `@PSO` in the ticket (a visual cue, not a Jira group reference).

- **Configuration:** PSO copies the settings validated in Staging to Production, coordinated with a **pod bounce**. All new widgets require a pod bounce before they become available. If settings differ between Staging and Production, list the differences in the comment; otherwise the exact Staging settings are copied.
- **Pilot testing:** for new widgets, pilot in an isolated package before going live to full membership. Include the name of your test/pilot package with the configuration request; after the pod bounce you can add users to the pilot package. If you have no pilot package, request one (for example, "Please clone our Default Retail package to create a new SDK Test Package").

### 13.7 Deployment phase: production live (updated 2021-12-20)

- Full Go Live is a scripted change. The team works on a weekly sprint schedule and needs **7 to 10 days advance notice** to create and schedule the script.
- The script adds the widget to the "More" section of users' profiles (no user action needed) and can optionally make it a Favorite, without disturbing existing user widget preferences.
- The Go Live script runs in the morning of the chosen day, from a queue, typically completed by 9am Central (priority events may delay).
- No additional bounce is required for Go Live if a Pilot test was done.
- Config, pilot and go-live requests are not necessarily linear and may be made at the same time if timing allows for the initial pod bounce, sufficient pilot testing, and script build time. The page references a Go Live request template that is not in the extract.

Sources: Vendor - Developed Solution Release Process for SDK (https://confluence.alkami.com/spaces/SDKC/pages/172110574); Vendor Submissions - Development Phase - FI (https://confluence.alkami.com/spaces/SDKC/pages/172114015); Vendor Submissions - Deployment Phase Procedures (https://confluence.alkami.com/spaces/SDKC/pages/172114045); Vendor Submits Widget or Service (https://confluence.alkami.com/spaces/SDKC/pages/171911954); SDK Code Review and Security Scan of Vendor Submission (https://confluence.alkami.com/spaces/SDKC/pages/172112670); Vendor Submission Available to Staging (https://confluence.alkami.com/spaces/SDKC/pages/172112706); Request Vendor Preproduction Rollout (https://confluence.alkami.com/spaces/SDKC/pages/172112760); Vendor Deployment Scheduled for Production (https://confluence.alkami.com/spaces/SDKC/pages/172112764); Vendor Deployment Production Pilot (https://confluence.alkami.com/spaces/SDKC/pages/172112768); Vendor Deployment Production Live (https://confluence.alkami.com/spaces/SDKC/pages/172112771)

---

## 14. Gold Partner solution deployment

The page "Gold Partner Solution Deployment" (updated 2021-10-04) contains only headings ("Workflow" and "Jira Ticket Example"); its diagram and example ticket are attachments not present in the extract. The substantive Gold Partner content lives in the vendor pages: Gold Partners are either resold by Alkami (anchor `#GoldPartnerSolutionDeployment-resold`) or contracted directly with an FI (anchor `#GoldPartnerSolutionDeployment-notResold`), and they follow the vendor release process in section 13, opening a Delivery project Feature Request ticket and listing the receiving client(s) on each submission. Gold Partners and system integrators are excluded from One Click Submission and continue to use the ProGet feed and Jira process.

Sources: Gold Partner Solution Deployment (https://confluence.alkami.com/spaces/SDKC/pages/156929428); Vendor Submits Widget or Service (https://confluence.alkami.com/spaces/SDKC/pages/171911954); One Click Submission (https://confluence.alkami.com/spaces/SDKC/pages/538188377)
