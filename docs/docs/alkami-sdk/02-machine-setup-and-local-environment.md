# SDK Machine Setup and Local Environment

## What this covers

This document consolidates everything in the Alkami SDK Confluence space about standing up and maintaining a local SDK developer machine: hardware and software prerequisites, machine-level permissions, the Merlin (SDK Wizard) installer and the legacy PowerShell/Chocolatey install, upgrade procedures, port reservations, hosts entries, Redis and SQL configuration, certificates, Visual Studio templates and samples, logging in to the local DeveloperDynamic site, stage-match environments and core packages, exposing the SDK site publicly, Shared Access enablement, and a set of common fixes (Log4Net, snippet recompile, DashboardV2 500s). It also summarizes the PowerShell command set shipped in `Alkami.Powershell.SDK`, the database layout, and training resources. The local environment is required to run and view work, but not to write and compile code against Alkami's libraries.

## Hardware and software requirements

### Hardware

Minimum specifications for a developer machine running the Alkami Platform:

- Processor: 2.4-GHz Intel Core i5 or i7 (10th generation or later) with at least 4 physical cores (for example i5-10200H, i5-10500, i7-10700, i7-10750H). Apple M-series chips work but require the Mac M1/M2 procedure below.
- Memory: 32 GB 1600-MHz DDR3.
- Storage: 500 GB SSD with at least 100 GB free. If disk encryption is used, do it in SSD hardware, not CPU-based software encryption.
- Optional hypervisor: VMware Workstation Pro (Windows), VMware Fusion Professional (Mac), or Parallels Desktop (Mac). Must be within the latest two major versions. Others are untested and may not be supportable.

Sample configurations: Dell XPS 15 9520 (i7-12700H, 16 GB DDR5, 1 TB NVMe); MacBook Pro 16-inch 2019 (i9-9980HK, 32 GB, 500 GB+ SSD, Parallels Desktop 17).

Virtual machine guidance: the host must have at least 8 threads and a fast SSD. Suggested VM allocation: 4 processor cores, 16 GB memory, 100 GB storage. The VM runs all three tiers of the Alkami platform, so fewer threads means poor performance. Merlin's hardware checks fail with dynamic memory/CPU allocation in a VM; turn it off and set fixed amounts.

One environment per developer: each developer must have their own machine or VM. Multiple developers cannot attach the Visual Studio debugger to processes on the same machine simultaneously.

### Network access

The SDK machine needs egress to the public Internet. At minimum these endpoints must be reachable:

- https://feeds.alkamitech.com
- https://assets.orb.alkamitech.com
- https://iris.alkamitech.com

Plus Windows Update, Visual Studio update sources, download sites for the software below, and top-level certificate authorities (CRL retrieval). Alkami employees ("Alkamists") must use https://packagerepo.orb.alkamitech.com with AD credentials instead of feeds.alkamitech.com.

For Merlin and the One-Click SDK Manager, also allow-list:

```
atl-gen-alk-sdk-prod-installer-us-east-1.s3.amazonaws.com
atl-gen-alk-sdk-prod-installer-us-east-1.s3.us-east-1.amazonaws.com
atl-gen-alk-sdk-dev-installer-us-east-1.s3.amazonaws.com
atl-gen-alk-sdk-dev-installer-us-east-1.s3.us-east-1.amazonaws.com
https://api.dev.alkami.com/tooling
```

### Software (install all of these BEFORE installing SDK components)

- Windows 11, Server 2019, or Server 2022 with the latest Microsoft updates. The local user must have Administrator privileges and must be able to run PowerShell as admin. On Server 2019 you must install the .NET Framework 4.8 developer pack (https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-developer-pack-offline-installer).
- SQL Server 2019/2022, Developer or Enterprise Edition. SQL 2022 is highly recommended; anything earlier than SQL 2017 will not work. Requirements: local instance, default instance name, Database Engine only, Windows Authentication (or both Windows and SQL auth), and the SDK user must be a SQL sysadmin (easiest if that user installs SQL). Express and Standard editions cannot be used because Alkami indexing commands use `online=on`, supported only in Developer and Enterprise. Do not install a trial version. Microsoft ODBC Driver versions below 17 are not compatible; uninstall them.
- Visual Studio 2022 (required; Professional recommended, Community works). Workloads: ASP.NET and web development, .NET desktop development. .NET Framework 4.8 is required; .NET 6 and 7 are not needed. Add .NET 8 or 9 (may need https://dotnet.microsoft.com/download/visual-studio-sdks). Update VS to the latest version. Launch VS once as administrator and confirm it is not asking for a license or color scheme, otherwise the Alkami templates will not install.
- Microsoft .NET 8.0 SDK is required (https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.404-windows-x64-installer).
- SQL Server Management Studio.
- 7-Zip (required to install some packages).
- Notepad++ (Alkami developers expect it for log/config troubleshooting).
- Chocolatey, version not greater than 1.4.0. Chocolatey v2.0.0 is known to cause installation errors.

```
# Set and environment variable to install a specific version of Chocolatey. The current version of Chocolatey, v2.0.0, is known to cause installation errors.
$env:chocolateyVersion = '1.4.0'

Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

# If you get an error on the above command (especially on Windows Server) you might need to run this first
Set-ExecutionPolicy Bypass
# then
Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
```

- NuGet command line:

```
choco install nuget.commandline

# Restart or refresh the powershell window
```

### Configure Chocolatey and NuGet feeds

Client (FI) configuration. Passwords with special characters must be wrapped in double-then-single quotes for choco.

```
# Add Alkami's NuGet sources
nuget sources add -name "Alkami Dev" -source https://feeds.alkamitech.com/nuget/nuget.dev -user emailAddress -password secret
nuget sources add -name "Alkami Third-Party" -source https://feeds.alkamitech.com/nuget/ThirdParty -user emailAddress -password secret
nuget sources add -name "Alkami Third-Party Core" -source https://feeds.alkamitech.com/nuget/ThirdParty-Core -user emailAddress -password secret

# Then add our Chocolatey Dev feed as a choco source, passwords with special characters will need to be surrounded in "'mypassword'" double and single quotes
choco source add -n=AlkamiChocoDev -s "'https://feeds.alkamitech.com/nuget/choco.dev'" -u="'emailAddress'" -p="'password'"
```

Updating credentials after a feeds password change:

```
nuget sources update -name "Alkami Dev" -username emailAddress -password secret
nuget sources update -name "Alkami Third-Party" -username emailAddress -password secret
nuget sources update -name "Alkami Third-Party Core" -username emailAddress -password secret

# To update credentials for the chocolatey source, you need to remove and re-add the source as follows.
choco source remove -n=AlkamiChocoDev
choco source add -n=AlkamiChocoDev -s "'https://feeds.alkamitech.com/nuget/choco.dev'" -u="'emailAddress'" -p="'password'"
```

Alkamist (internal) configuration uses packagerepo without stored credentials:

```
nuget sources add -name "Alkami Dev" -source https://packagerepo.orb.alkamitech.com/nuget/nuget.dev
nuget sources add -name "Alkami Third-Party" -source https://packagerepo.orb.alkamitech.com/nuget/ThirdParty
choco source add -n=AlkamiChocoDev -s "'https://packagerepo.orb.alkamitech.com/nuget/choco.dev'"
```

### Reserve TCP ports for the Subscriptions service

Run as Administrator, ideally right after a reboot. If the SDK is already installed, run `stop-IISandservices` first and `start-IISandservices` afterward.

```
# Note: Run below 2 commands to reserve specific ports.
netsh int ipv4 Add excludedportrange protocol=tcp startport=12345 numberofports=2
netsh int ipv4 Add excludedportrange protocol=tcp startport=50000 numberofports=30

# Note: Run below 2 commands to enable listening on IPv4 and IPv6 respectively
netsh http add iplisten 0.0.0.0
netsh http add iplisten ::
```

If either `netsh` add command says "The process cannot access the file because it is being used by another process", either the reservation already exists or another process holds the ports. Verify with `netsh int ip show excludedportrange protocol=tcp`; the expected result shows 12345-12346 and 50000-50029. Use `netstat -o` to find the offending PID.

Sources: Hardware and Software Requirements (https://confluence.alkami.com/spaces/SDKC/pages/55347103); SDK Permission Requirements (https://confluence.alkami.com/spaces/SDKC/pages/572886218); Merlin Troubleshooting Steps (https://confluence.alkami.com/spaces/SDKC/pages/349145256); Platform Machine Setup on Mac M1 and M2 Machines (https://confluence.alkami.com/spaces/SDKC/pages/275112330); Troubleshooting Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/55347098).

## Permission requirements (for Security, IT, and Network teams)

The SDK is installed via the SDK Wizard (Merlin), which installs and configures SQL Server and SSMS, PostgreSQL, Alkami Nexus client libraries and configuration (Nexus is Alkami's tenant data connections service; the SDK uses it to resolve tenant and environment connection information rather than relying on locally cloned databases), Visual Studio, Chocolatey, 7-Zip, and the SDK itself (services, packages, templates, the DeveloperDynamic sandbox database).

Merlin requires true local administrator access, both to install Merlin itself and for Merlin to install, modify, and remove files, folders, Windows services, environment variables, and registry entries. The common failure mode ("the I have admin trap") is a corporate image where local admin is restricted by Group Policy, MDM, or endpoint protection, so the account looks like admin but cannot perform the needed operations.

Required for the installing account on each developer machine:

- True local administrator rights (not GPO-restricted "admin")
- Ability to install third-party applications
- Ability to create, start, stop, and configure Windows services
- Ability to modify the Windows registry and environment variables
- Ability to create, modify, and delete files and folders system-wide
- Outbound network access to the hosts listed under Network access above

Endpoint protection (CrowdStrike, SentinelOne, Microsoft Defender for Endpoint, etc.) will likely need exclusions for the Merlin install path and the components it installs; quarantined installer artifacts produce the same symptom as a permissions failure. The scope is broad by design and Alkami states it will not shrink meaningfully under review; Alkami can join a working session with Security/IT/Network teams on request.

Sources: SDK Permission Requirements (https://confluence.alkami.com/spaces/SDKC/pages/572886218).

## Installing with Merlin (SDK Wizard)

As of March 2025 Alkami supports installs and upgrades via Merlin, which is faster and needs less interaction than the legacy method (which still works). Downloads exist for Windows x64 and for a Windows VM on Mac (Parallels, arm64); the download links are on the SDK Machine Setup page.

- Run as Administrator and log in with your Feeds credentials.
- Fresh install: follow the in-app prompts.
- Upgrade (from SDK 2024.x): gear icon (top-right), scroll down, click Upgrade once the current SDK version is detected.
- Uninstall: gear icon, scroll down, click Uninstall. Uninstalling removes any SDK stage match. Afterward return to the main menu to start a fresh install.

Changelogs for each ORB update: https://confluence.alkami.com/spaces/SDKC/pages/442832619.

### Merlin troubleshooting

- Log file: `%APPDATA%\Merlin\log\combine.log`.
- Upgrading: the Merlin Troubleshooting page (March 2024) says Merlin does not support SDK updates (use Uninstall SDK in Settings, reboot Merlin, fresh install). The newer SDK Machine Setup page (May 2025) says upgrades from 2024.x are supported via the gear icon. Prefer the newer guidance; fall back to uninstall/reinstall if the upgrade fails.
- Cannot download or install Merlin: company policy, firewall, or antivirus. Fails to start: unsupported OS.
- Hardware check failures: verify specs; in a VM turn off dynamic memory/CPU allocation.
- Network check failures: firewall blocking https://feeds.alkamitech.com/feeds. Proxy servers can block Merlin; contact the SDK team for a workaround.
- Software check: Merlin checks Visual Studio 2022, SQL Server 2022, and Chocolatey 1.4.0, installing missing ones and verifying versions of existing ones. If checks keep failing, uninstall the software completely and let Merlin install it.
- Jira API key: being redirected back to the Jira setup screen means the key is incorrect.
- Uninstall SDK (Settings page) removes all traces of the SDK (Chocolatey packages, app pools, Orb folder) but not VS, SQL, or other software. If installation fails, uninstall first and retry.
- Windows Server 2022 Datacenter Azure Edition: installers may exit without a UAC prompt when logged in as the built-in Administrator (SID ending in -500, often renamed in cloud images). Find it with `Get-LocalUser | ? {$_.SID -like "*-500"}` and use a different admin account.
- `EBUSY: resource busy or locked` in combine.log: antivirus is locking downloaded files. Temporarily disable AV, Uninstall SDK, reboot Merlin, fresh install.
- Microsoft ODBC Driver below 17 is incompatible; uninstall it, reboot Merlin, and let Merlin install the correct driver.

Sources: SDK Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/40403249); Merlin Troubleshooting Steps (https://confluence.alkami.com/spaces/SDKC/pages/349145256).

## Legacy platform machine setup (PowerShell and Chocolatey)

### Prerequisites specific to the install

- Complete the Hardware and Software Requirements first.
- Clear or complete pending Windows Updates and confirm no pending restart. Alkami enables Windows features (IIS) during install and pending updates block this. Pause Windows Update during the SDK install.
- Update Intel graphics drivers. The Intel Graphics control panel is known to claim ports needed by Alkami services if drivers are outdated. Use Intel Driver & Support Assistant (https://www.intel.com/content/www/us/en/support/detect.html) or disable that Windows service.
- Microsoft .NET 8.0 SDK is required (sdk-8.0.404). The Mac page also references .NET 6.0 (sdk-6.0.406) for navigation and its services, and .NET 8.0 (sdk-8.0.400) for the 2024.3 release.
- When upgrading from 2025.1 or earlier: run this against AlkamiMaster so newer services can run their migrations and register properly.

```
use AlkamiMaster
update dbo.Tenant set ConnectionString = ConnectionString + 'TrustServerCertificate=true;' where not (ConnectionString like '%TrustServerCertificate=true;%')
```

- When upgrading to 2025.4: `Alkami.Powershell.SDK` must be at least version 1.5.7 or both upgrade and install will fail.
- When upgrading to 2025.1: the release contains `Alkami.Migrations.Nexus`, which requires a Postgres database and environment variables. Two install options:
  - Docker: install Docker Desktop, then run `docker run --name alk-local-postgres -e POSTGRES_PASSWORD=postgres -d -p 5432:5432 ghcr.io/dbsystel/postgresql-partman:16-4` (Postgres 16.4 with pg_partman). A different password or port must be set as an environment variable. Connect at localhost:5432 (PgAdmin works for testing).
  - Windows native: install Postgres 16, set a password for the `postgres` user (save it for the environment variable), leave Stack Builder checked and use it to install Npgsql. PgAdmin is installed with Postgres.
  - The page does not name the specific environment variables; it says "need to setup few environment variables" and that a non-default password/port must be set as one.

### First-time install commands (PowerShell as Administrator)

```
## Update PowerShell's execution policy
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Bypass -Force;Set-ExecutionPolicy Bypass -Scope Process -Force;
choco install Alkami.PowerShell.Configuration Alkami.PowerShell.Database Alkami.PowerShell.Choco Alkami.Powershell.IIS Alkami.Powershell.SDK Alkami.Installer.Services -y
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Install-Nexus
Install-SDKRelease -Latest
## Note: you might see some error messages related to AppPools.
## As long as the installation continues, you don't need to worry about those errors.
choco install Alkami.SDK.Templates -y
choco install Alkami.SDK.Samples -y
winrm quickconfig
## If you have troubles getting the Alkami services to start, try a reboot before panicking and doing further troubleshooting.
```

If the system asks for a reboot, reboot and run the commands again.

### Upgrade the SDK runtime (every two to three months)

```
choco upgrade Alkami.Powershell.SDK -y

Import-Module -Name Alkami.Powershell.SDK -Force

# If upgrading from 2025.1 or later you can skip the Nexus install
Install-Nexus

Install-SDKRelease -Latest

choco upgrade Alkami.SDK.Templates -y

choco upgrade Alkami.SDK.Samples -y

## A few people have experienced problems where certain services do not start (or stay started) after a reboot. The following lines should take care of that if you experience the same problem
& sc.exe config Alkami.MS.Householding.Service.Host start= delayed-auto | Out-Default
& sc.exe config Alkami.MicroServices.MessageCenter.Service.Host start= delayed-auto | Out-Default
& sc.exe config Alkami.MicroServices.EventManagement.Service.Host start= delayed-auto | Out-Default
& sc.exe config Alkami.MicroServices.Transactions.Service.Host start= delayed-auto | Out-Default
```

Symitar users only (do NOT run otherwise): after each SDK update, stop the Alkami SymConnect multiplexer microservice and run `choco upgrade Alkami.SDK.Features.Core.SymConnect -y`.

### After installation: disable the SQL broker

Run `Stop-IISandservices`, execute this in SSMS, then run `restart-sdkservices`. Databases created via TSQL default to broker enabled, which causes unneeded server work.

```
ALTER DATABASE AlkamiMaster SET SINGLE_USER
go
alter database alkamimaster set DISABLE_BROKER
go
ALTER DATABASE AlkamiMaster SET MULTI_USER
go
```

### Wildcard certificate update

The `*.dev.alkamitech.com` certificate is updated from time to time, sometimes before expiration; the old one is revoked so IIS cannot use it. The certificate that expired November 2, 2024 requires this if you did not upgrade to 2024.4 before then (installs package version 1.2.11):

```
choco upgrade Alkami.DeveloperKit.Certificates.Wildcard -y
```

### Reset the development environment

Warning: this removes projects the SDK setup may not add back. First save `Get-SDKSupport` output outside `C:\Orblogs`, then run `Stop-IISAndServices` as admin.

Soft reset:

- Run `remove-sdkservices`.
- Delete every Alkami folder in `C:\ProgramData\chocolatey\lib` and all packages in `lib-bad` or `lib-bkp` if present.
- Delete `C:\Orb`.
- Optional: delete the AlkamiMaster and DeveloperDynamic databases. If you do, the environment variable `Alkami_SDK_Initialization_DatabaseStandup` must also be deleted:

```
# as admin
Remove-EnvironmentVariable -Key 'Alkami_SDK_Initialization_DatabaseStandup' -StoreName Machine
Remove-EnvironmentVariable -Key 'Alkami_SDK_Initialization_DatabaseStandup' -StoreName Process
```

If a service is Disabled and Marked for Deletion, end its process from Task Manager > Details, or reboot.

Additional references: https://chocolatey.org/docs/create-packages and https://docs.microsoft.com/en-us/nuget/reference/nuspec.

Sources: Alkami Platform Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/48800506); Update *.dev.alkamitech.com certificate (https://confluence.alkami.com/spaces/SDKC/pages/69997657); SDK Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/40403249).

## Mac M1/M2 (ARM64) setup

Intel Macs follow the standard Windows steps. For M1/M2, the SDK runs in Windows 11 on Parallels (Alkamists without a license submit an INTIT ticket). Network: Alkamists need packagerepo.orb.alkamitech.com; FI clients need feeds.alkamitech.com (credentials supplied during onboarding).

1. Install Windows: open Parallels, select Get Windows 11 from Microsoft, Continue, Install Windows, accept the license, then "Click to continue".
2. Install base software by either method:
   - EZ Method: download `SDKPreInstall.zip` (attached on the Confluence page), move it to `C:\`, Extract All, open PowerShell as Administrator in that folder, and execute the script.
   - Manual Method: download the MS SQL Server install script from https://github.com/jimm98y/MSSQLEXPRESS-M1-Install, copy the folder to `C:\`, go to `C:\MSSQLEXPRESS-M1-Install-main\src`, and double-click the `install2022Developer` batch file (click Allow for PowerShell; if it seems idle press Enter). It downloads, installs SQL, then installs SSMS. Then install Visual Studio 2022 (Community or Professional) with ASP.NET and web development and .NET desktop development, start VS and select a theme (required for template installers), and install 7-Zip ARM64 from https://www.7-zip.org/a/7z2300-arm64.exe.
3. Install the package manager: Chocolatey 1.4.0 and `nuget.commandline` as above, then configure feeds (Alkamist or FI client variant as above, using `-u <username> -p <password>` for nuget and the double/single quoted form for choco).
4. Reserve ports (note the extra first line):

```
Set-NetConnectionProfile -InterfaceAlias Ethernet -NetworkCategory "Private"
netsh int ipv4 Add excludedportrange protocol=tcp startport=12345 numberofports=2
netsh int ipv4 Add excludedportrange protocol=tcp startport=50000 numberofports=30
netsh http add iplisten 0.0.0.0
netsh http add iplisten ::
```

5. Ensure the .NET 6.0 SDK (6.0.406) and, for 2024.3 and later, the .NET 8.0 SDK are installed, then run:

```
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Bypass -Force;Set-ExecutionPolicy Bypass -Scope Process -Force;
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
choco install Alkami.PowerShell.Configuration Alkami.PowerShell.Database Alkami.PowerShell.Choco Alkami.Powershell.IIS Alkami.Powershell.SDK -y
Install-SDKRelease -Latest
```

6. Templates and samples:

```
choco install Alkami.SDK.Templates -y
choco install Alkami.SDK.Samples -y
winrm quickconfig
```

Note: this page (November 2024) predates the Nexus/Postgres requirement introduced in 2025.1 and omits `Alkami.Installer.Services` and `Install-Nexus`; apply the 2025.x prerequisites from the platform setup section when installing a current release. Merlin also ships an arm64 build for Parallels VMs.

Sources: Platform Machine Setup on Mac M1 and M2 Machines (https://confluence.alkami.com/spaces/SDKC/pages/275112330); Hardware and Software Requirements (https://confluence.alkami.com/spaces/SDKC/pages/55347103).

## Troubleshooting the local platform

Anytime the SDK misbehaves, work this checklist in order. If it does not resolve, open an SDK Support Incident and attach the log created by:

```
## Make sure that you are updated to the latest version
choco upgrade Alkami.Powershell.SDK -y

Get-SDKSupport
```

1. Services stuck in Starting while `Alkami.Services.Subscriptions.Host` is not running. The Subscriptions service must run so all other services can route messages. Start it in Services.msc; if it fails, run `Stop-SDKServices` (kills all Alkami processes) then `Restart-SDKServices`. Alternatives: `start-servicesonly`, or reinstall with `choco install Alkami.Services.Subscriptions.Host -f` (or `choco upgrade Alkami.Services.Subscriptions.Host -y` when it did not install completely). If problems persist, the port reservations above are probably missing.
2. Cannot reach the Login page: port 808 must be free for NET TCP or IIS will not run the site. Symptom in `C:\Orblogs\Alkami.Client.WebClient.log`: `FATAL ... Unable to retrieve BankId. Returning null` with `EndpointNotFoundException: There was no endpoint listening at net.tcp://stsconfiguration/STSConfiguration/STSConfiguration.svc`, even though app pools run and http://localhost/STSConfiguration/ works.

```
Get-Process -Id (Get-NetTCPConnection -LocalPort 808).OwningProcess
# alternative (also matches 8080; check with ":8080" to exclude those)
netstat -oan | findstr ":808" | % { Get-Process -ID $_.Substring($_.Length-7).Trim() }
```

Anything other than `SMSvcHost` (also acceptable: `tomcat8.exe.x64`, `w3wp`) should be killed with `taskkill /f /pid <pid>`, then `restart-service @("WAS") -Force` (also restart Net TCP Port Sharing Service and Net TCP Listener Adapter).

3. IIS application pools: every Alkami app pool must have exactly one application, except WebClient and WebClientAdmin, which may hold more when working with the ORBFX or AFX API projects. Reassign or remove any foreign application (Application Pools > View Applications).
4. Login page asks only for a username: confirm the Alkami Authentication Workflow Service is running and set to Automatic. It is now required (older SDKs did not need it).
5. 500 error on DashboardV2 after an update. Run against DeveloperDynamic (or the stage-match DB):

```
update core.FlavorWidget set NativeDisplaySetting = 0 where NativeDisplaySetting is null
```

6. Login-critical packages; reinstalling these fixes most login issues after a fresh install or upgrade:

```
choco upgrade Alkami.Apps.Authentication -y; choco upgrade Alkami.Apps.DashboardV2 -y; choco upgrade Alkami.Apps.MyAccountsV2 -y;
```

7. View error on the dashboard after login: `choco upgrade Alkami.MicroServices.UserInterface.Service.Host -y;`. Also check in Admin UI Settings that the Accounts module URL is `MyAccountsV2/Module`, not an Accounts view inside the Dashboard module.
8. Minimum package set (verify with `choco list -lo`, install any missing):

```
Alkami.Apps.Authentication
Alkami.Apps.DashboardV2
Alkami.Apps.MyAccountsV2
Alkami.Apps.Settings
Alkami.MicroServices.Accounts.Service.Host
Alkami.MicroServices.Broker.Host
Alkami.MicroServices.Contacts.Service.Host
Alkami.MicroServices.Settings.Service.Host
Alkami.MicroServices.SiteText.Service.Host
Alkami.MicroServices.UserInterface.Service.Host
Alkami.Microservices.DeveloperKit
Alkami.Microservices.MyAccounts
Alkami.Modules.LegacyAuthAdmin
Alkami.Modules.LegacyAuthClient
Alkami.Services.Subscriptions.Host
```

9. Subscription service cannot start because of Redis: confirm Redis is installed and running, confirm the environment variable `ALKAMI_REDIS_CONNECTION_STRING` is set, and confirm the hosts file contains:

```
127.0.0.1 AuditService
127.0.0.1 BankService
127.0.0.1 ContentService
127.0.0.1 CoreService
127.0.0.1 ExceptionService
127.0.0.1 IP-STS
127.0.0.1 MessageCenterService
127.0.0.1 NagConfigurationService
127.0.0.1 NotificationService
127.0.0.1 RP-STS
127.0.0.1 Scheduler
127.0.0.1 SecurityManagementService
127.0.0.1 STSConfiguration
127.0.0.1 redis-18620.redis.corp.alkamitech.com
127.0.0.1 developer.dev.alkamitech.com
127.0.0.1 admin-developer.dev.alkamitech.com
```

10. Redis/database connection string errors from services: each `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Config\machine.config` must contain this section exactly:

```
<connectionStrings>
<add name="AlkamiMaster" connectionString="data source=localhost;Integrated Security=SSPI; Database=AlkamiMaster" providerName="System.Data.SqlClient" />
<add name="RedisSetting" connectionString="redis-18620.redis.corp.alkamitech.com:18620,keepAlive=60,password=gamehendge2020" />
</connectionStrings>
```

11. Cannot edit a local microservice's Log On user/password in service Properties: the service is managed. Run `sc.exe managedaccount "My.Service.Name" False`.
12. "You need to have Visual Studio installed first..." when installing templates: confirm VS 2022 is installed and operational.

Sources: Alkami Platform Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/48800506); Troubleshooting Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/55347098).

## Visual Studio templates and sample projects

### Install and upgrade templates

Templates ship as a single VSIX inside the Chocolatey package `Alkami.SDK.Templates` (https://feeds.alkamitech.com/feeds/choco.dev/Alkami.SDK.Templates). Close all Visual Studio instances first.

```
choco install Alkami.SDK.Templates -y
```

Upgrade with `choco upgrade Alkami.SDK.Templates -y`. After upgrading from older per-template extensions: in Visual Studio open Extensions, keep the extension named "Alkami SDK Templates", and remove any other Alkami extensions. In the New Project dialog, filter by platform (Windows), language (C#), and project tag (Alkami); other tags include service, database, and events. The upgrade page (2023) mentions VS 2019; current requirements are VS 2022.

Templates not appearing in VS:

- Visual Studio must support .NET Framework 4.8 or the VSIX will not install.
- Exit VS and run `choco install Alkami.SDK.Templates -f`.
- Otherwise double-click the vsix in `C:\ProgramData\chocolatey\lib\Alkami.SDK.Templates\tools` to see the install log.
- Obscure case: missing `c:\Program Files(x86)\Microsoft Visual Studio\Installer\vswhere.exe` (seen with VS 2017 Community). Rerunning the VS installer restores it.

### Available templates

| Template | Purpose |
| --- | --- |
| Admin Widget | Admin widget for the Alkami admin web platform |
| Card Management Service | Projects and files for a new Card Management provider service |
| Client Widget | Client widget for the Alkami web client application |
| Embedded Snippet | Embedded snippet to extend an existing client widget |
| Event Processor | Framework and references for an Event Management Processor |
| Facts Provider | Framework and references for an Alkami facts provider |
| Logical Service | Logical microservice plus client and tests |
| Provider Service | Distributed provider-based microservice plus client and tests |
| Quick Apply Service | Projects for a new Alkami QuickApply Provider microservice |
| Slim Logical Service | Minimal service plus client |
| Single Sign On Service | Generic SSO microservice |

### Sample projects

`choco install Alkami.SDK.Samples -y` installs samples to `C:\AlkamiSDK\Samples`. Before using `Sample.Client.Widget.ChangeAddress`, run in an admin PowerShell:

```
choco install Alkami.MS.UserContacts.Service.Host -y
```

Sources: Visual Studio Templates (https://confluence.alkami.com/spaces/SDKC/pages/38503450); Upgrade the Alkami SDK Templates (https://confluence.alkami.com/spaces/SDKC/pages/78674342); Troubleshooting Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/55347098); Use SDK Sample Projects (https://confluence.alkami.com/spaces/SDKC/pages/135992745); Log Into Developer Dynamic On Your Local SDK (https://confluence.alkami.com/spaces/SDKC/pages/227925800).

## Validating the install and logging in to DeveloperDynamic

Software validation:

- Launch Visual Studio as administrator, create a new project from the Alkami Client Widget template, select Framework 4.8 (install it if missing), and confirm it builds. A successful build also proves the Alkami package feeds are configured. VS must show no license or permission errors.
- In SSMS confirm at least the `AlkamiMaster` and `DeveloperDynamic` databases exist.
- Confirm `C:\AlkamiSDK\Samples` exists and is populated (otherwise `choco install Alkami.SDK.Samples`).

Client platform: browse to https://developer.dev.alkamitech.com/ (startup can take a few minutes). Log in as username `mike.brady`, password `12345`. Click Send Code (simulated; nothing is sent) and enter authentication code `560142`. The Dashboard opens with account balances.

Admin platform: browse to https://admin-developer.dev.alkamitech.com/. Log in as `admin.developer.dev`; any answer works for challenge questions and any password. Confirm these pages work: Setup Menu, Integration Settings, Providers, User Interface Settings, Widget Settings, Packages, Support, Members.

Sources: Log Into Developer Dynamic On Your Local SDK (https://confluence.alkami.com/spaces/SDKC/pages/227925800); Preparing for the SDK Virtual Training (https://confluence.alkami.com/spaces/SDKC/pages/235932152).

## Stage-match environments and tenants

### What a stage match is and how to get one

A Stage Match environment is a copy of the SDK's DeveloperDynamic database overlaid with your FI's settings from the Alkami staging environment, plus a new site URL mapped to that database. With the correct core provider packages it lets you test SDK components against your test core. The database is typically named `AlkamiYourFiName_Dev`.

To request one, create a Jira ticket in your FI's Delivery project with Issue Type "SDK Support Incident" (include FI contact info). Alkami then opens a linked script-generation ticket; script generation can take up to two weeks. An SDK team member tests the script locally (a few days), attaches a zipped script package and a list of core-specific packages to install, and schedules a session to help install it. Merlin's Uninstall removes any stage match.

### Applying a tenant (Master Script process, 2020 page)

A tenant is a named environment tied to a URL with its own configuration. The Alkami Tenant Master Script is a large SQL script of self-contained sections (provider, widget, banksetting, billpay, themes, etc.) staged into temp tables and merged into the tenant database. It ships with `ROLLBACK` at the end; run it once in rollback mode, review the output tables, then rerun with commit. One output tab is a set of `REPLACEME` update statements for secure values (core and third-party credentials, which Alkami never supplies; arrange whitelisting for those services in advance). The SDK machine must be able to telnet to the test-core host.

Process (script numbers refer to the package attached to the ticket):

1. Alkami prepares a one-release conversion package (GenerateSDKTenant PowerShell tool, secrets scrubbed).
2. Clone DeveloperDynamic with script `1 - Backup-Restore`.
3. Run `2 - insert-tenant` (inserts the core.Tenant row, removes old users such as Brady).
4. Apply `3 - master script`.
5. Apply any Alkami-supplied registration field SQL, then `4 - users and registration`.
6. Run PowerShell `5 - iis site add` to create the IIS site, bindings, and hosts entries.
7. Apply environment-specific changes (test-core hosts entries, core provider DB updates). Skipping this almost always fails.
8. Run `iisreset` and restart the Alkami Radium Scheduler Service.
9. Test at the URL Alkami provided: register a user, then log in. No Entrust, email, or SMS out of the box, so "Questions" is the preferred login method. Radium must be running for account sync (synchronous at login) and user/transaction sync (asynchronous after the dashboard loads); if values are missing, log out and in again.

A centralized SQL server is possible (a VM in the same workgroup, not domain, with the same local account names so BankService, CoreService, etc. can connect); otherwise each SDK VM keeps a private copy that must be backed up before each SDK upgrade. Unsupported in this configuration: BillPay, Nag alerts, Email/SMTP, SMS. For support attach `C:\OrbLogs\Alkami.Client.WebClient.log`, `C:\OrbLogs\Alkami.App.Bank.Host.log`, `C:\OrbLogs\Alkami.App.Core.Host.log`, and for sync issues `C:\OrbLogs\Alkami.App.Radium.WindowsService.log`. The page's helpdesk URL (https://helpedesk.alkamitech.com, sdksupport@alkamitech.com) is older; the Jira SDK Support Incident path is current.

### Stage-match core connection packages (2026)

Versions below are examples; on the latest SDK release omit `--version`. Otherwise read `version.txt` in `C:\Orb\WebClient` and match Major.Minor (for example 2025.3). Releases earlier than 2025.3 must upgrade to use these consolidated packages.

Required for every core on 2025.3 or earlier (included by default from 2025.4); without it registration fields do not show for member registration:

```
choco upgrade alkami.app.providers.core.service.client --version 5.5.220 -y
```

Primary cores:

- Symitar (Episys): `choco upgrade Alkami.SDK.Core.Symitar --version 2025.3 -y`. SymXchange configuration is required: stop the Multiplexer service, edit `appsettings.json` in `C:\ProgramData\chocolatey\lib\Alkami.MicroServices.SymConnectMultiplexer.Service.Host\lib`, replace the `PortConfiguration` section with the block below (BankIdentifier comes from the stage-match tenant record in `dbo.tenant` in AlkamiMaster), then restart `Alkami.Microservices.Symconnect.Multiplexer.Service.Host`. See also "Troubleshooting SDK SymConnect problems" (https://confluence.alkami.com/spaces/SDKC/pages/78674510).

```
"PortConfiguration": [
{
"BankIdentifier": "Your Bank Identifier here",
"PortValues": [0]
}
]
```

- CoreAPI: `choco upgrade Alkami.SDK.Core.CoreAPI --version 2025.3 -y`
- OSI: `choco upgrade Alkami.SDK.Core.OSI --version 2025.3 -y`
- CUFX API: `choco upgrade Alkami.SDK.Core.CUFX --version 2025.3 -y`, then install the stage-match scripts and fill in redacted provider settings.
- FiServ DNA: no special packages.
- CoA: `choco upgrade Alkami.SDK.Core.CoA --version 2025.3 -y`
- Corelation: `choco upgrade Alkami.SDK.Core.Corelation --version 2025.3 -y` (possible missing setting `E_STMT_OPTION`).
- Summit Spectrum (Pathways middleware): `choco upgrade Alkami.SDK.Core.SummitSpectrum --version 2025.3 -y` (check for client-specific package versions).
- Salesforce: `choco upgrade Alkami.SDK.Core.Salesforce --version 2025.3 -y`
- XP2: `choco upgrade Alkami.SDK.Core.XP2 --version 2025.3 -y` (client-specific versions possible).
- Horizon: `choco upgrade Alkami.SDK.Core.Horizon --version 2025.3 -y` (client-specific versions possible).

Secondary cores: `Alkami.SDK.Core.Base2000`, `Alkami.SDK.Core.RTA` (mortgages), `Alkami.SDK.Core.PSCU` (cards), `Alkami.SDK.Core.TMG` (credit cards), `Alkami.SDK.Core.CCM` (cards), `Alkami.SDK.Core.DMI` (listed under a second "RTA (for mortgages)" heading, likely a page typo), all with `--version 2025.3 -y` via `choco upgrade` or `choco install`.

Sources: What is a Stage Match environment and why do I need or want one? (https://confluence.alkami.com/spaces/SDKC/pages/306061358); Creating/Updating a New ORB Tenant for SDK Clients (Stage Match DB) (https://confluence.alkami.com/spaces/SDKC/pages/51347805); Stage-match core connections for SDK (https://confluence.alkami.com/spaces/SDKC/pages/510210821); Alkami Platform Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/48800506).

## Access your SDK from the public Internet

Requirements: a working SDK site, a static internal IP on the SDK machine, control of the Internet firewall, a public static IP from your ISP, and the ability to edit the hosts file on the browsing machine. Example: SDK machine at 192.168.1.2, public IP 12.1.2.3. Create a static mapping in the firewall from the public IP to the SDK machine and allow inbound port 443, then add a hosts entry on the remote computer pointing `developer.dev.alkamitech.com` to the public IP. Hairpinning (going out and back in through the same firewall) usually does not work, so the browsing computer must be across the public Internet.

Sources: Access your SDK from the public Internet (https://confluence.alkami.com/spaces/SDKC/pages/268641495).

## Enabling Shared Access

Shared Access is a set of UI elements in the Shared Access tab of the Settings widget plus ORB back-end logic (for example in M2M Transfers). An Inviter shares accounts with an Invitee (account holder or not) with per-account permissions: Transfer To, Transfer From, View Balance, Pay Bills. Available in dev, QA, staging, and production.

Prerequisites: Settings widget (`Alkami.Apps.Settings`) installed, configured, and added to the desired flavors via the admin UI; plus `choco install Alkami.Services.SharedAccess` and `choco install Alkami.Services.Tenant`.

Database steps (each script ends with `ROLLBACK` and a commented `--COMMIT`; run clean in rollback first, then uncomment commit):

1. Merge a `SharedAccess` row (IsEnabled = 1) into `core.feature`.
2. (Guest users only) Merge a flavor named `DefaultGuestUserFlavor` into `core.flavor` for your bank (replace `FILL_IN_BANK_ID_HERE` with the `core.bank` name; the script copies IdleLogoutURL, UserLogoutURL, and TimeZoneInfo from core.Bank and uses LocaleID 1033).
3. (Guest users only) Merge `core.FlavorWidget` rows adding Dashboard (`Alkami.Client.Widgets.Dashboard.V2`, IsDefault), Accounts (`Alkami.Client.Widgets.MyAccounts.V2`), Transfers (`Alkami.Client.Widgets.TransferV2`), Bill Pay (`Alkami.Client.Widgets.BillPayV2`), and Settings (`Alkami.Client.Widgets.Settings`) to that flavor, and set `CanBeRemoved = 0` on the Dashboard widget. Comment out lines for widgets you do not have installed or the script fails.

The full merge scripts are on the source page.

Settings (core.ItemSetting; editable in DB or admin provider settings):

| Setting | Scope | Type / Default | Notes |
| --- | --- | --- | --- |
| IsSharingLinkedAccountsEnabled | Settings widget | boolean / false | Allow linked accounts (relationship code 3) with full permissions to be shared |
| DisableDesktopRegistration | Bank | boolean / false | Must be false; registration must be on for Shared Access |
| InvitationExpirationTime | Bank | hours / 24 | Required |
| InvitationAcceptanceMaxAttempts | Bank | integer / 3 | Required; attempts before invitation is blocked |
| SharedAccessPermissionGroups | Bank | JSON | Required; groups of CorrelationId, PermissionDisplayName, PermissionNames (ViewSummary, TransferFundsInto, TransferFundsOutFrom, BillpayFrom; optional ViewTransactions, ViewStatements) |
| SharedAccessPreventAllAdminActions | Bank | boolean / false | Required; blocks full-access admins from managing shares |
| SharedAccessExpirationGracePeriod | Bank | hours / 24 | Friendly error window after expiry |
| SharedAccessEnableGuestUsers | Identity Provider (Entrust) | boolean / false | Guest users get an ORB login but no core user |
| DisableSharedAccessLoginForExistingUsers | Identity Provider (Entrust) | boolean / false | |

Default SharedAccessPermissionGroups value:

```
[ { "CorrelationId":1, "PermissionDisplayName":"View account", "PermissionNames":[ "ViewSummary" ] }, { "CorrelationId":2, "PermissionDisplayName":"Transfer into", "PermissionNames":[ "TransferFundsInto" ] }, { "CorrelationId":3, "PermissionDisplayName":"Transfer from", "PermissionNames":[ "ViewSummary", "TransferFundsOutFrom" ] }, { "CorrelationId":4, "PermissionDisplayName":"Pay bills", "PermissionNames":[ "ViewSummary", "BillpayFrom" ] }]
```

Logs: `Alkami.Client.WebClient.log` (Web) and `Alkami.App.Bank.Host.log` (App) in `C:\OrbLogs`. Permission display names come from `PermissionDisplayName`, not site text. Disable the feature by setting `IsEnabled = 0` on the core.Feature row. Only core-synced, non-external accounts with at least ViewSummary can be shared, and not accounts received via Shared Access.

Sources: Enabling Shared Access (https://confluence.alkami.com/spaces/SDKC/pages/284697044).

## Common fixes

### Log4Net fix

```
choco install Alkami.Log4Net.Fix --source https://feeds.alkamitech.com/nuget/choco.dev/
```

This installs `C:\AlkamiSDK\Log4NetFix\fix-solution.exe`. Identify affected projects and run the fix (two YouTube walkthroughs are embedded on the page: https://www.youtube.com/embed/iPzygjOJjEA and https://www.youtube.com/embed/Xz0c0UYnaxM). Test by adding these calls to a method you can execute:

```
Logger.TraceFormat("A trace message");
Logger.DebugFormat("A debug message");
Logger.Info("An info message");
Logger.ErrorFormat("A error message");
Logger.FatalFormat("A fatal message");
```

Verify the trace line starts with `|*| TRACE |*|` and is not registering as DEBUG. If it does not appear, set the log4net config level to Trace.

### Snippet recompile fix

Problem: every Index load recompiles the snippet and writes a new DLL to temporary internet files. Fix: return a static string as a `ContentResult` from the Index action in `EmbeddedSnippetBehavior.cs`. Solution 1: type the (escaped) HTML directly into the `Content` parameter (fine for small snippets; escaping becomes painful, e.g. `"Check out my 'escaped' \"Snippet\" <b> bold Move! </b>"`). Solution 2: add `SnippetContent.txt` to the project as an Embedded Resource (Save All afterward) and load it. Replace `{AssembleName}` with the assembly name from project properties.

```
private ActionResult Index(Controller controller)
{
var assembly = Assembly.GetExecutingAssembly();
var resourceName = "{AssembleName}.SnippetContent.txt";

string result;
using (Stream stream = assembly.GetManifestResourceStream(resourceName))
using (StreamReader reader = new StreamReader(stream))
{
result = reader.ReadToEnd();
}

return new ContentResult() { Content = result, ContentType = "text/html", ContentEncoding = System.Text.Encoding.UTF8 };
}
```

### FI rebrand for SDK components

When an FI rebrands, URLs, widget names, and links are too deeply integrated for a script to change. Responsibilities: the FI recreates each widget from the SDK template as a new project and moves the code over; renaming the namespace as a shortcut is strongly advised against; testing is joint; the Alkami SDK team and Release Management handle the release pipeline (uninstall old versions, install new ones on the hosted platform). The same applies to microservices paired with those widgets.

Sources: Log4Net Fix (https://confluence.alkami.com/spaces/SDKC/pages/136001878); Snippet recompile fix (https://confluence.alkami.com/spaces/SDKC/pages/132709997); FI Rebrand for SDK Components (https://confluence.alkami.com/spaces/SDKC/pages/428577216).

## Common PowerShell commands (Alkami.Powershell.SDK)

Update the module with `choco upgrade Alkami.Powershell.SDK -y` (then `Import-Module -Name Alkami.Powershell.SDK -Force`).

| Command | Purpose |
| --- | --- |
| `Get-SDKSupport` | Collects system info, all SDK/Alkami packages, and Windows Experience Index into a file (path printed). Attach to support tickets. |
| `Install-SDKRelease` | Installs or updates an SDK release. `-Latest` switch; `-Version <string>`; `-Features <string>` (tab-completed, e.g. API). |
| `New-SDKMachineSetup` | Automates the Hardware and Software Requirements setup. Switches `-Alkamist`, `-Daily` (daily feed); mandatory `-Username` and `-Password` for the package feed. |
| `Remove-SDK` | Removes SDK artifacts. Switches: `-sites` (DeveloperDynamic WebAdmin and ClientFacing sites), `-appPools`, `-devDb` (DeveloperDynamic), `-MainDb` (AlkamiMaster), `-Hard` (all of the above plus port exclusions and nuget/choco sources; requires full reinstall). |
| `Remove-SDKServices` | Removes Windows services whose name contains "Alkami" (prompts). |
| `Restart-SDKServices` | Restarts all Alkami services including Redis. |
| `Restart-SDKWebClients` | Restarts WebClient and WebAdmin IIS processes; `-ClearTemp` removes Temp ASP.NET files. |
| `Set-SDKServiceRecovery` | `-ServiceName` (mandatory; `'Name1','Name2'` or `'All'`), `-Action` (`'TakeNoAction'` default or `'recovery'`). |
| `Set-SDKServiceStartupType` | `-ServiceName` (mandatory; `@('Name1','Name2')` or `'All'`), `-StartupType` (`'Automatic'`, `'Disabled'`, `'InvalidValue'`, `'Manual'`; default Manual). For delayed auto start use `sc.exe config Alkami.MS.Householding.Service.Host start= delayed-auto`. |
| `Start-SDKServices` | Starts IIS and Alkami services; `-maxParallel <int>` default 10. |
| `Stop-SDKServices` | Stops all running Alkami SDK services. |
| `Update-DeveloperModules` | Updates local developer modules consistently. |

Also used throughout the setup pages: `Stop-IISAndServices`, `Start-IISAndServices`, `start-servicesonly`, `Install-Nexus`, `Remove-EnvironmentVariable`.

Sources: Common Powershell Commands (https://confluence.alkami.com/spaces/SDKC/pages/236361303); Alkami Platform Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/48800506).

## Alkami database basics

SDK projects must never read or write database tables directly; the tables matter for understanding the system and for adjusting test data in SSMS.

| Database | Role | Notes |
| --- | --- | --- |
| AlkamiMaster | Master | System-wide functions (scheduling). The key table is `core.Tenant`. |
| DeveloperDynamic | Tenant | All configuration and data for the fictitious example FI, with dummy users, accounts, and balances. |
| AlkamiYourFiName_Dev (optional) | Stage-Match Tenant | Copy of DeveloperDynamic overlaid with the FI's staging configuration, connecting to a test core. |

`core.Tenant` maps incoming requests to tenant databases via `BankUrlSignatures` (default `developer.dev.alkamitech.com`), `DataSource` (`localhost`), `Catalog` (`DeveloperDynamic`), and a `ConnectionString` column that must be correct for the site to start (see the `TrustServerCertificate=true;` upgrade step). A stage-match tenant adds another row. Note the 2025 setup page refers to this table as `dbo.Tenant` in its update statement.

Tenant tables of interest: `core.Bank`, `core.Users`, `core.Item`, `core.ItemSetting`, `core.Widget`, `core.WidgetSetting`, `core.Provider`, `core.ProviderType`, `core.Account`, `core.AccountType`, `core.UserAccount`, `core.Flavor`, `core.FlavorWidget`, `core.UserWidget`, `ui.Modules`, `ui.Dashboards`, `ui.LayoutDefinitions`, `audit.UserAction`. The source page (2019) leaves the descriptions blank.

Sources: Alkami Database Basics (https://confluence.alkami.com/spaces/SDKC/pages/65998677); Alkami Platform Machine Setup (https://confluence.alkami.com/spaces/SDKC/pages/48800506).

## Training resources

- SDK training moved (June 2024) from multi-day virtual sessions to self-serve courses in Alkami University. Enroll at https://au.myabsorb.com/#/signup with Key Name `SDK Training`, or request access through your CSM or Partner BA; login is at https://au.myabsorb.com/. Non-developers may attend. Each participant should have a machine or VM that runs the SDK.
- Video playlist: https://www.youtube.com/playlist?list=PLqxV4mpD9OhWmaKi9hBaMQ9AGi4TSGFjq. Legacy downloadable videos (Generic SSO Provider and Widget, Convert SSO Provider To MS) apply only to pre-2018 legacy providers. Client Conference 2019 Developer Track: https://confluence.alkami.com/spaces/SDKC/pages/58461845.
- Legacy virtual-training prep (2023): verify credentials for https://jira.alkami.com/, https://confluence.alkami.com/display/SDKC, and https://feeds.alkamitech.com/, and validate the environment per the login section above.

Sources: SDK Training (https://confluence.alkami.com/spaces/SDKC/pages/62687771); SDK Training Enrollment (https://confluence.alkami.com/spaces/SDKC/pages/366022666); Training Videos (https://confluence.alkami.com/spaces/SDKC/pages/62687796); Preparing for the SDK Virtual Training (https://confluence.alkami.com/spaces/SDKC/pages/235932152); Additional Training Materials (https://confluence.alkami.com/spaces/SDKC/pages/87004955).
