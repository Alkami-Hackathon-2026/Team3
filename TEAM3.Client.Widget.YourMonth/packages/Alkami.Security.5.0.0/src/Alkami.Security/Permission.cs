using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Alkami.Security
{
    [DataContract]
    public enum Permission
    {
        [EnumMember]
        [Category("EntityGroup")]
        [Description("Provides access to the Users and Roles sections of the Business Setup app")]
        [Display(Name = "Manage Users and Roles")]
        ManageUsers = 0,

        [EnumMember]
        [Category("Unknown")]
        [Description("Allows a user to manage shared budgets at the entity level")]
        [Display(Name = "Manage Shared Budgets")]
        ManageSharedBudgets = 1,

        [EnumMember]
        [Category("Unknown")]
        [Description("User can manage shared transaction categories at the entity level.")]
        [Display(Name = "Manage Shared Transaction Categories")]
        ManageSharedTransactionCategories = 2,

        [EnumMember]
        [Category("Account")]
        [Description("View Summary, account balance.")]
        [Display(Name = "View Summary")]
        ViewSummary = 3,

        [EnumMember]
        [Category("Account")]
        [Description("View Transactions, requires view transactions.")]
        [Display(Name = "View Transactions")]
        ViewTransactions = 4,

        [EnumMember]
        [Category("Account")]
        [Description("Viewing of statements from the user account level.")]
        [Display(Name = "View Statements")]
        ViewStatements = 5,

        [EnumMember]
        [Category("Account")]
        [Description("Ability for a user to view draft image at the account level.")]
        [Display(Name = "View Draft Images")]
        ViewDraftImages = 6,

        [EnumMember]
        [Category("Unknown")]
        [Description("User can invite others via guardianship or add users via entity user management, view transactions permission is required.")]
        [Display(Name = "Share")]
        Share = 7,

        [EnumMember]
        [Category("Account")]
        [Description("Ability for a user to transfer funds out of the associated account.")]
        [Display(Name = "Transfer Funds Out From")]
        TransferFundsOutFrom = 8,

        [EnumMember]
        [Category("Account")]
        [Description("Ability to use bill pay on the given account")]
        [Display(Name = "Bill Pay From")]
        BillpayFrom = 9,

        [EnumMember]
        [Category("Account")]
        [Description("Permits user to wire funds out of the given account.")]
        [Display(Name = "Wire Funds Out From")]
        WireFundsOutFrom = 10,

        [EnumMember]
        [Category("Administrator")]
        [Description("Gives the user the ability to manage content through the administrator client.")]
        [Display(Name = "Manage Content")]
        ContentManage = 11,

        [EnumMember]
        [Category("Administrator")]
        [Description("Gives the user the ability to manage profiles through the administrator client.")]
        [Display(Name = "Manage Profile")]
        ProfileManage = 12,

        [EnumMember]
        [Category("Administrator")]
        [Description("Gives the user the ability to login in as a user of the client application.")]
        [Display(Name = "Manage Login As")]
        ManageLoginAs = 13,

        [EnumMember]
        [Category("Entity")]
        [Description("An entity marked with this permission is the administrator of the bank.")]
        [Display(Name = "Bank Entity")]
        BankEntity = 14,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Provides access to the Payees section of the Business Setup app")]
        [Display(Name = "Manage Payees")]
        ManagePayees = 15,

        [EnumMember]
        [Category("Administrator")]
        [Description("Gives a user the ability to manage financial institution holidays.")]
        [Display(Name = "Manage financial institution holidays")]
        ManageHolidays = 16,

        [EnumMember]
        [Category("Administrator")]
        [Description("Gives a user the ability to manage 'master mappings', such as account type classes and transaction types.")]
        [Display(Name = "Manage master mappings")]
        ManageMasterMappings = 17,

        [EnumMember]
        [Category("Administrator")]
        [Description("Administrative ability to manage customer support.")]
        [Display(Name = "Manage customer support")]
        ManageCustomerSupport = 18,

        [EnumMember]
        [Category("Administrator")]
        [Description("Administrative ability to manage bank entity staff")]
        [Display(Name = "Manage staff")]
        ManageStaff = 19,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage brands for the customer experience.")]
        [Display(Name = "Manage customer experience brands")]
        ManageBrands = 20,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage packages for the customer experience.")]
        [Display(Name = "Manage customer experience packages")]
        ManagePackages = 21,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage themes for the customer experience.")]
        [Display(Name = "Manage customer experience themes")]
        ManageThemes = 22,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage widgets for the customer experience.")]
        [Display(Name = "Manage customer experience widgets")]
        ManageWidgets = 23,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage customer rewards.")]
        [Display(Name = "Manage customer experience rewards")]
        ManageRewards = 24,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage customer experience pricing lists.")]
        [Display(Name = "Manage customer experience price lists")]
        ManagePricelists = 25,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to view content reporting.")]
        [Display(Name = "View content reports")]
        ContentReporting = 26,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to view full user records. This excludes the ability to edit user info.")]
        [Display(Name = "View Users - Elevated")]
        ViewUsersElevated = 27,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to edit a customer records, this excludes the ability to manage a customer's accounts or packages.")]
        [Display(Name = "Modify customers")]
        ModifyCustomers = 28,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage a customer's accounts.")]
        [Display(Name = "Manage customer accounts")]
        ManageCustomerAccount = 29,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage a customer's packages.")]
        [Display(Name = "Manage customer packages")]
        ManageCustomerPackages = 30,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to view and respond to messages in the message center.")]
        [Display(Name = "View/Response messages")]
        ViewRespondMessageQueues = 31,

        [EnumMember]
        [Category("Account")]
        [Description("Ability to allow a user to stop a payment from the account")]
        [Display(Name = "Stop Payment")]
        StopPayment = 32,

        [EnumMember]
        [Category("Account")]
        [Description("Allow a user to use savings goals associated to the account")]
        [Display(Name = "Savings Goals")]
        Goals = 33,

        [EnumMember]
        [Category("Entity")]
        [Description("Create ACH templates to debit consumer accounts for goods and services.")]
        [Display(Name = "Collect Funds from Consumers")]
        ACHPPDDebits = 34,

        [EnumMember]
        [Category("Account")]
        [Description("Ability to transfer funds into a given account")]
        [Display(Name = "Transfer Funds Into")]
        TransferFundsInto = 35,

        [EnumMember]
        [Category("Account")]
        [Description("Ability to ACH funds from a given account")]
        [Display(Name = "ACH Funds From")]
        ACHFundsOutFrom = 36,

        [EnumMember]
        [Category("Account")]
        [Description("Ability to ACH funds into a given account")]
        [Display(Name = "ACH Funds Into")]
        ACHFundsInto = 37,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to indicate whether or not a customer is an Employee of the institution.")]
        [Display(Name = "Manage customer employee status.")]
        ManageCustomerEmployeeStatus = 38,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to view / manage accounts for a user that has the Employee flag set to true.")]
        [Display(Name = "Manage employee accounts.")]
        ManageEmployeeAccounts = 39,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to view account balances for customers.")]
        [Display(Name = "View Customer Account Balances")]
        ViewCustomerAccountBalances = 40,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to view customer transactions")]
        [Display(Name = "View Customer Transactions")]
        ViewCustomerTransactions = 41,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage general settings.")]
        [Display(Name = "Manage General Settings")]
        ManageGeneralSettings = 42,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to view operation reports")]
        [Display(Name = "View Operation Reports")]
        ViewOperationReports = 43,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage ACH transactions and batches")]
        [Display(Name = "Manage ACH Transactions")]
        ManageACHTransactions = 44,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to authorize ACH transactions to be included in a batch.")]
        [Display(Name = "Authorize ACH Transactions")]
        AuthorizeACHRequests = 45,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to download wire files")]
        [Display(Name = "Download Wire Files")]
        DownloadWireFiles = 46,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage ACH settings.")]
        [Display(Name = "Manage ACH Settings")]
        ManageACHSettings = 47,

        [EnumMember]
        [Category("Entity")]
        [Description("Create ACH templates to credit businesses for services and distribute or consolidate funds between businesses.")]
        [Display(Name = "Business Payments")]
        ACHCCDCredits = 48,

        [EnumMember]
        [Category("Unknown")]
        [Description("Provides access to the Customers section of the Business Setup app")]
        [Display(Name = "Manage Customers")]
        ManageCustomers = 49,

        [EnumMember]
        [Category("Entity")]
        [Description("Create ACH templates that can be used to credit consumer accounts for payroll direct deposit, bonuses, refunds and more.")]
        [Display(Name = "Payroll")]
        ACHPPDCredits = 50,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Enables users to view statements, notices, tax forms, and annual credit card summary")]
        [Display(Name = "View eDocuments")]
        ViewEDocuments = 51,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Provides the ability to authorize Wire transactions")]
        [Display(Name = "Allowed to Authorize Wires")]
        AllowedToAuthorizeWires = 52,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage general settings")]
        [Display(Name = "Manage Integration Settings")]
        ManageIntegrationSettings = 53,

        [EnumMember]
        [Category("Administrator")]
        [Description("Gives the user the ability to manage content rules through the administrator client.")]
        [Display(Name = "Manage Content Rules")]
        ContentRulesManage = 54,

        [EnumMember]
        [Category("Account")]
        [Description("Gives the ability to view the Amortization Schedule on an Account")]
        [Display(Name = "View Amortization Schedule")]
        AmortizationSchedule = 55,

        [EnumMember]
        [Category("Account")]
        [Description("Gives the ability to use the AutoDraft widget")]
        [Display(Name = "Access AutoDraft Widget")]
        AutoDrafting = 56,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Gives users access to use bill pay.")]
        [Display(Name = "Allowed to Pay Bills")]
        AllowedToPayBills = 57,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage business packages")]
        [Display(Name = "Manage Business Packages")]
        ManageBusinessPackages = 58,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to view business records, this excludes the ability to edit business info.")]
        [Display(Name = "View Businesses")]
        ViewBusinesses = 59,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to modify business records, this excludes the ability to manage business accounts or packages")]
        [Display(Name = "Modify Businesses")]
        ModifyBusinesses = 60,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage business accounts.")]
        [Display(Name = "Manage Business Account")]
        ManageBusinessAccount = 61,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to assign packages to a business")]
        [Display(Name = "Assign Business Packages")]
        AssignBusinessPackages = 62,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage business transaction limits")]
        [Display(Name = "Manage Business Limits")]
        ManageBusinessLimits = 63,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage wire requests and batches")]
        [Display(Name = "Manage Wire Transfer Requests")]
        ManageWireTransferRequests = 64,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage wire settings")]
        [Display(Name = "Manage Wire Settings")]
        ManageWireSettings = 65,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage business ACH Settings")]
        [Display(Name = "Manage BusinessACH Settings")]
        ManageBusinessACHSettings = 66,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to download business ACH files")]
        [Display(Name = "Download ACH Batch Files")]
        DownloadACHBatchFiles = 67,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage and batch template requests")]
        [Display(Name = "Manage Business ACH Template Requests")]
        ManageBusinessACHTemplateRequests = 68,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Provides sub users with access to certain widgets and sections")]
        [Display(Name = "Master User Permission")]
        MasterUserPermission = 69,

        [EnumMember]
        [Category("Administrator")]
        [Description("Gives the user the ability to login as a user of the client application in a read only mode.")]
        [Display(Name = "View only Login As")]
        ViewOnlyLoginAs = 70,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Gives the masquerading user the ability to access SSO Bill Pay systems.")]
        [Display(Name = "Bill Pay SSO access")]
        BillPaySSO = 71,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to create ACH templates")]
        [Display(Name = "Create ACH Template")]
        CreateACHTemplate = 72,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to edit ACH templates")]
        [Display(Name = "Edit ACH Template")]
        EditACHTemplate = 73,

        [Obsolete("Deprecated: Use SubmitACHPaymentsTemplate or SubmitACHCollectionsTemplate instead")]
        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to submit ACH templates")]
        [Display(Name = "Submit ACH Template")]
        SubmitACHTemplate = 74,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to delete ACH templates")]
        [Display(Name = "Delete ACH Template")]
        DeleteACHTemplate = 75,

        [EnumMember]
        [Category("Entity")]
        [Description("Create ACH templates to accept one-time or recurring payments over the telephone from consumers")]
        [Display(Name = "Telephone Collections")]
        ACHTELDebits = 76,

        [EnumMember]
        [Category("Entity")]
        [Description("Create ACH templates to accept one-time or recurring payments from consumers over the Internet.")]
        [Display(Name = "Internet Collections")]
        ACHWEBDebits = 77,

        [EnumMember]
        [Category("Entity")]
        [Description("Create ACH templates to debit business accounts for goods and services.")]
        [Display(Name = "Collect Funds from Businesses")]
        ACHCCDDebits = 78,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to access Remote Deposit Capture")]
        [Display(Name = "Remote Deposit Capture")]
        RemoteDepositCapture = 79,

        [Obsolete("Deprecated: Use AccessRestrictedACHPaymentsTemplates or 'AccessRestrictedACHCollectionsTemplates instead")]
        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to access and modify templates that have been designated for restricted users only")]
        [Display(Name = "Access to Restricted Templates")]
        AccessToRestrictedTemplates = 80,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to add external transfer accounts")]
        [Display(Name = "Add External Transfer Account")]
        AddExternalTransferAccount = 81,

        [Obsolete("Deprecated: Use AllowedToAuthorizeACHPayments or AllowedToAuthorizeACHCollections instead")]
        [EnumMember]
        [Category("EntityGroup")]
        [Description("Provides the ability to authorize ACH transactions")]
        [Display(Name = "Allowed to Authorize ACH")]
        AllowedToAuthorizeACH = 82,

        [Obsolete("Deprecated: Use AllowedToAuthorizeExternalTransfers or AllowedToAuthorizeInternalTransfers instead")]
        [EnumMember]
        [Category("EntityGroup")]
        [Description("Provides the ability to authorize Transfer transactions")]
        [Display(Name = "Allowed to Authorize Transfers")]
        AllowedToAuthorizeTransfers = 83,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allows users to select entry accounts, change statuses, amounts, enter addenda information for entries and delete ACH template entries")]
        [Display(Name = "Edit and Delete ACH Template Entries")]
        EditAndDeleteACHTemplateEntries = 84,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allows users to add new entries to an ACH template")]
        [Display(Name = "Add ACH Template Entries")]
        AddACHTemplateEntries = 85,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allows users to add member accounts for transfers.")]
        [Display(Name = "Add Member To Member Transfer Account")]
        AddMemberToMemberTransferAccount = 86,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Gives users the ability to create Domestic Wires.")]
        [Display(Name = "Allowed to create Domestic Wires")]
        CreateDomesticWires = 87,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Gives users the ability to create International Wires.")]
        [Display(Name = "Allowed to create International Wires")]
        CreateInternationalWires = 88,

        [EnumMember]
        [Category("User")]
        [Description("The user is eligible for RDC")]
        [Display(Name = "RDC Eligible")]
        RDCEligible = 89,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Gives masquerade user the ability to all Generic SSOs")]
        [Display(Name = "Masquerade Generic Single Sign On")]
        MasqueradeGenericSSO = 90,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to import ACH template or create pass-thru template using NACHA or .csv files")]
        [Display(Name = "Import ACH Templates")]
        ImportACHTemplates = 91,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to see the full, unamsked member identifier.")]
        [Display(Name = "View Full Customer Number")]
        ViewFullCustomerNumber = 92,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to see account balance and transaction history of sensitive accounts.")]
        [Display(Name = "View Sensitive Accounts")]
        ViewSensitiveAccounts = 93,

        [EnumMember]
        [Category("User")]
        [Description("RDC Unknown Status")]
        [Display(Name = "RDC Unknown Status")]
        RDCUnknownStatus = 94,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to edit business contact information")]
        [Display(Name = "Edit Business Contact Information")]
        EditBusinessContactInformation = 95,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to modify a user's SSN, PIN and Member ID")]
        [Display(Name = "Modify Users - Elevated")]
        ModifyCustomersElevated = 96,

        [EnumMember]
        [Category("User")]
        [Description("Sets the user as ineligible for bill pay.")]
        [Display(Name = "Deny Bill Pay")]
        DenyBillPay = 97,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Gives users the ability to access Rewards")]
        [Display(Name = "Rewards")]
        Rewards = 98,

        [EnumMember]
        [Category("User")]
        [Description("The user is eligible for RDC and Tier 1")]
        [Display(Name = "RDC Eligible Tier 1")]
        RDCEligibleTier1 = 99,

        [EnumMember]
        [Category("User")]
        [Description("The user is eligible for RDC and Tier 2")]
        [Display(Name = "RDC Eligible Tier 2")]
        RDCEligibleTier2 = 100,

        [EnumMember]
        [Category("User")]
        [Description("The user is eligible for RDC and Tier 3")]
        [Display(Name = "RDC Eligible Tier 4")]
        RDCEligibleTier3 = 101,

        [EnumMember]
        [Category("Account")]
        [Description("Allow Accounts to make a one time payment")]
        [Display(Name = "One-Time Payment")]
        OneTimePayment = 102,

        [EnumMember]
        [Category("Account")]
        [Description("Gives user the ability to use People Pay on the given account")]
        [Display(Name = "PeoplePayFrom")]
        PeoplePayFrom = 103,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to manage payment and collection templates at the EntityGroup level")]
        [Display(Name = "ManageTemplates")]
        ManageTemplates = 104,

        [EnumMember]
        [Category("Administrators")]
        [Description("Ability to authorize Wire transactions to be included in a batch")]
        [Display(Name = "AuthorizeWireRequests")]
        AuthorizeWireRequests = 105,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Access StopPayment")]
        [Display(Name = "MasqueradeStopPayment")]
        MasqueradeStopPayment = 106,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Access ReorderChecks")]
        [Display(Name = "MasqueradeReorderChecks")]
        MasqueradeReorderChecks = 107,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Access CheckWithdrawal")]
        [Display(Name = "MasqueradeCheckWithdrawal")]
        MasqueradeCheckWithdrawal = 108,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Access AutoDraft")]
        [Display(Name = "MasqueradeAutoDraft")]
        MasqueradeAutoDraft = 109,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Access Transfer")]
        [Display(Name = "MasqueradeTransfer")]
        MasqueradeTransfer = 110,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Access Profile Info")]
        [Display(Name = "MasqueradeProfile")]
        MasqueradeProfile = 111,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Access Authenticated Devices")]
        [Display(Name = "MasqueradeAuthenticatedDevices")]
        MasqueradeAuthenticatedDevices = 112,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Access Contact Info")]
        [Display(Name = "MasqueradeContact")]
        MasqueradeContact = 113,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Access Accounts")]
        [Display(Name = "MasqueradeAccounts")]
        MasqueradeAccounts = 114,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Access ACH Accounts")]
        [Display(Name = "MasqueradeACHAccounts")]
        MasqueradeACHAccounts = 115,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Access Saving Goals")]
        [Display(Name = "MasqueradeSavingsGoals")]
        MasqueradeSavingsGoals = 116,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Gives masquerade user the ability add/edit/delete BillPay payees")]
        [Display(Name = "BillPayManagePayee")]
        BillPayManagePayee = 117,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Gives masquerade user the ability to access BillPay Functionality (Payments)")]
        [Display(Name = "MasqueradeBillPay")]
        MasqueradeBillPay = 118,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Gives masquerade user the ability to access Quorum Secure Forms Functionality")]
        [Display(Name = "QuorumSecureForms")]
        QuorumSecureForms = 119,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Gives masquerade user the ability to access Quorum MeridianLink Functionality")]
        [Display(Name = "MeridianLink")]
        MeridianLink = 120,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Gives masquerade user the ability to access Quorum MeridianLink Functionality")]
        [Display(Name = "PSCUAccessPoint")]
        PSCUAccessPoint = 121,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Gives masquerade user the ability to create a new account through our New Account application widget")]
        [Display(Name = "MasqueradeNewAccount")]
        MasqueradeNewAccount = 122,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Gives masquerade user the ability to access P2P Functionality")]
        [Display(Name = "MasqueradeP2P")]
        MasqueradeP2P = 123,

        [EnumMember]
        [Category("Masquerade")]
        [Description("Gives masquerade user the ability to access functionality specific to Business Banking")]
        [Display(Name = "MasqueradeBusinessBanking")]
        MasqueradeBusinessBanking = 124,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage quick apply categories, products, applications")]
        [Display(Name = "ManageApplications")]
        ManageApplications = 125,


        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to do business sweeps")]
        [Display(Name = "Business Sweeps")]
        BusinessSweeps = 126,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to do positive pay")]
        [Display(Name = "Positive Pay")]
        PositivePay = 127,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("Ability to use the Custom user action button on the user profile page.")]
        [Display(Name = "AccessCustomUserAction")]
        AccessCustomUserAction = 128,

        [EnumMember]
        [CategoryAttribute("Account")]
        [DescriptionAttribute("Balance Peek")]
        [Display(Name = "Balance Peek")]
        BalancePeek = 129,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("View/Filter Executive Dashboards, View/Filter Analytics Dashboards, View/Filter Reports, View/Filter Content Target Files")]
        [Display(Name = "View Warehouse Reports")]
        ViewWarehouseReports = 130,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("View/Filter/Edit Executive Dashboards, View/Filter/Edit Analytics Dashboards, View/Filter/Edit Reports, View/Filter/Edit Content Target Files, View/Filter/Edit Dynamic User List(Recurring Content Targeting)")]
        [Display(Name = "Manage Warehouse Reports")]
        [Obsolete("Deprecated - 2019.01 - Use ManageFluxSettings instead")]
        ManageWarehouseReports = 131,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("Ability to view user records except for user's DOB, email, phone and address. this excludes the ability to edit user info.")]
        [Display(Name = "View Users")]
        ViewUsers = 132,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("This permission will be required before Admin users can register retail users.")]
        [Display(Name = "Register New Retail Users")]
        RegisterNewUser = 133,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("This permission will be required before Admin users can enter \"Other\" email and SMS option when selecting a delivery method for temporary password")]
        [Display(Name = "Enter Other For Temporary Passwords")]
        EnterOtherForTemporaryPasswords = 134,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("Ability to update the account number or the routing number field of external accounts for a user's ACH Profile")]
        [Display(Name = "Manage External Account/ABA")]
        ManageExternalAccountABA = 135,

        [EnumMember]
        [CategoryAttribute("User")]
        [DescriptionAttribute("Determines whether or not a user can use OpenId.")]
        [Display(Name = "OpenId")]
        OpenId = 136,

        [EnumMember]
        [CategoryAttribute("User")]
        [DescriptionAttribute("Determines whether or not a user can view their OpenId profile information.")]
        [Display(Name = "View Profile Information")]
        ViewProfileInfo = 137,

        [EnumMember]
        [CategoryAttribute("User")]
        [DescriptionAttribute("Determines whether or not a user can view their OpenId email information.")]
        [Display(Name = "View Email Information")]
        ViewEmailInfo = 138,

        [EnumMember]
        [CategoryAttribute("User")]
        [DescriptionAttribute("Determines whether or not a user can view their OpenId address information.")]
        [Display(Name = "View Address Information")]
        ViewAddressInfo = 139,

        [EnumMember]
        [CategoryAttribute("User")]
        [DescriptionAttribute("Determines whether or not a user can view their OpenId phone information.")]
        [Display(Name = "View Phone Information")]
        ViewPhoneInfo = 140,

        [EnumMember]
        [CategoryAttribute("User")]
        [DescriptionAttribute("Determines whether or not a user can generate the mobile web cookie.")]
        [Display(Name = "Mobile Web")]
        MobileWeb = 141,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("View and retrieve payees withing business banking.")]
        [Display(Name = "View Payees")]
        ViewPayees = 142,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to view and update debit/credit cards associated with a user in the Custom ICCU Business Cards Widget.")]
        [Display(Name = "Manage Cards")]
        ManageCards = 143,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("Ability to view and use Flux analytics reports and tools.")]
        [Display(Name = "View Flux Analytics")]
        ViewFluxAnalytics = 144,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("Ability to manage Flux licenses and setting.")]
        [Display(Name = "Manage Flux Settings")]
        ManageFluxSettings = 145,

        [EnumMember]
        [CategoryAttribute("EntityGroup")]
        [DescriptionAttribute("Ability to reorder checks.")]
        [Display(Name = "Reorder Checks")]
        ReorderChecks = 146,

        [EnumMember]
        [CategoryAttribute("EntityGroup")]
        [DescriptionAttribute("Ability to view restricted bill pay payees")]
        [Display(Name = "View Restricted Payees")]
        ViewRestrictedPayees = 147,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("View Billing Setup.")]
        [Display(Name = "Ability to view the billing setup")]
        ViewBillingSetup = 148,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("Manage Billing Setup.")]
        [Display(Name = "Ability to manage the billing setup")]
        ManageBillingSetup = 149,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("View Price List Configuration")]
        [Display(Name = "Ability to view the price list configuration.")]
        ViewPriceListConfiguration = 150,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("Manage Price List Configuration")]
        [Display(Name = "Ability to manage the price list configuration.")]
        ManagePriceListConfiguration = 151,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("Manage User Billing Preference")]
        [Display(Name = "Ability to manage the user billing preference.")]
        ManageUserBillingPreference = 152,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [DescriptionAttribute("Download Billing File")]
        [Display(Name = "Ability to download the billing file.")]
        DownloadBillingFile = 153,

        [EnumMember]
        [Category("Entity")]
        [Description("Create ACH passthrus to credit consumer accounts for goods and services.")]
        [Display(Name = "Ach Credit Passthru")]
        ACHOtherPassThruCredits = 154,

        [EnumMember]
        [Category("Entity")]
        [Description("Create ACH passthrus to debit consumer accounts for goods and services.")]
        [Display(Name = "Ach Debit Passthru")]
        ACHOtherPassThruDebits = 155,

        [EnumMember]
        [Category("Administrator")]
        [Description("Manage Bulk Operations.")]
        [Display(Name = "Ability to manage bulk enable, disable & delete.")]
        ManageBulkOperations = 156,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allow subusers to pay loans online")]
        [Display(Name = "PayLoans")]
        PayLoans = 157,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allows the business master user to restrict sub users ability to add, edit, and delete a bill pay payee")]
        [Display(Name = "ManageBillPayPayees")]
        ManageBillPayPayees = 158,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage Digital ID")]
        [Display(Name = "ManageDigitalID")]
        ManageDigitalID = 159,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage retail wire transfer request submissions and batches")]
        [Display(Name = "Manage Retail Wire Transfer Requests")]
        ManageRetailWireTransferRequests = 160,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage notifications via admin widget accessible via the admin menu navigation.")]
        [Display(Name = "Manage Notifications")]
        ManageNotifications = 161,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [Description("Ability to manage secure form builder via admin widget accessible via the admin menu navigation.")]
        [Display(Name = "Manage Secure Form Builder")]
        ManageSecureFormBuilder = 162,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [Description("Ability to manage registered applications via admin widget accessible via the admin menu navigation.")]
        [Display(Name = "Manage Registered Applications")]
        ManageRegisteredApplications = 163,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Ability to view and update debit/credit cards associated with a user.")]
        [Display(Name = "Access Card Management Widget")]
        AccessCardManagement = 164,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to view extracts")]
        [Display(Name = "ViewWarehouseExtracts")]
        ViewWarehouseExtracts = 165,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage extracts")]
        [Display(Name = "ViewWarehouseExtracts")]
        ManageWarehouseExtracts = 166,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage navigation")]
        [Display(Name = "ManageNavigation")]
        ManageNavigation = 167,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage Card Management Action and Attribute mapping.")]
        [Display(Name = "Manage Card Configuration")]
        ManageCardConfiguration = 168,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to manage Card Management Card Image mapping.")]
        [Display(Name = "Manage Card Images")]
        ManageCardImages = 169,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allow subusers to access Lockbox")]
        [Display(Name = "Lockbox")]
        Lockbox = 170,

        [EnumMember]
        [Category("Account")]
        [Description("Ability to RDC funds into a given account")]
        [Display(Name = "RDC Funds Into")]
        RDCFundsInto = 171,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("View historical Account Analysis Statements")]
        [Display(Name = "View Account Analysis Statements")]
        ViewAccountAnalysisStatements = 172,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allow sub users to open a loan")]
        [Display(Name = "Allowed to Open Loans")]
        AllowOpenLoans = 173,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allow sub users to open an account")]
        [Display(Name = "Allowed to Open Accounts")]
        AllowOpenAccounts = 174,

        [EnumMember]
        [CategoryAttribute("Administrator")]
        [Description("Ability to view notifications via admin widget accessible via the admin menu navigation.")]
        [Display(Name = "View Notifications")]
        ViewNotifications = 175,

        [EnumMember]
        [Category("Account")]
        [Description("Ability to send funds from Zelle for a given account")]
        [Display(Name = "Zelle Funds From")]
        ZelleFundsFrom = 176,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allowed to Authorize ACH Payments")]
        [Display(Name = "Authorize ACH Payments")]
        AllowedToAuthorizeACHPayments = 177,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allowed to Authorize ACH Collections")]
        [Display(Name = "Authorize ACH Collections")]
        AllowedToAuthorizeACHCollections = 178,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allowed to Authorize External Transfers")]
        [Display(Name = "Authorize External Transfers")]
        AllowedToAuthorizeExternalTransfers = 179,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allowed to Authorize Internal Transfers")]
        [Display(Name = "Authorize Internal Transfers")]
        AllowedToAuthorizeInternalTransfers = 180,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Access to Restricted ACH Payments Templates")]
        [Display(Name = "Access Restricted ACH Payments Templates")]
        AccessRestrictedACHPaymentsTemplates = 181,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Access to Restricted ACH Collections Templates")]
        [Display(Name = "Access Restricted ACH Collections Templates")]
        AccessRestrictedACHCollectionsTemplates = 182,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Submit Internal Transfers")]
        [Display(Name = "Submit Internal Transfers")]
        SubmitInternalTransfers = 183,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Submit External Transfers")]
        [Display(Name = "Submit External Transfers")]
        SubmitExternalTransfers = 184,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Submit ACH Payments Template")]
        [Display(Name = "Submit ACH Payments Template")]
        SubmitACHPaymentsTemplate = 185,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Submit ACH Collections Template")]
        [Display(Name = "Submit ACH Collections Template")]
        SubmitACHCollectionsTemplate = 186,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to view the Business Transaction Limit Dashboard")]
        [Display(Name = "Limit Management - View Only")]
        BusinessTransactionLimitDashboardViewOnly = 187,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to fully access and interact with the Business Transaction Limit Dashboard")]
        [Display(Name = "Limit Management - Full Access")]
        BusinessTransactionLimitDashboardFullAccess = 188,

        [EnumMember]
        [Category("Administrator")]
        [Description("Ability to assign administrators and recommend limits from within the Business Transaction Limit Dashboard")]
        [Display(Name = "Limit Management - Assign Admins and Rec Limits Only")]
        BusinessTransactionLimitDashboardAssignAdminsRecommendLimitsOnly = 189,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("The ability to create transfer templates in the transfer widget")]
        [Display(Name = "Create Transfer Templates")]
        CreateTransferTemplates = 190,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("The ability to modify transfer templates in the transfer widget")]
        [Display(Name = "Modify Transfer Templates")]
        ModifyTransferTemplates = 191,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("The ability to delete transfer templates in the transfer widget")]
        [Display(Name = "Delete Transfer Templates")]
        DeleteTransferTemplates = 192,

        [EnumMember]
        [Category("Administrator")]
        [Description("The ability to configure loan payment options")]
        [Display(Name = "Configure Loan Payment Options")]
        ConfigureLoanPaymentOptions = 193,


        [EnumMember]
        [Category("EntityGroup")]
        [Description("The ability to have CheckFreeSB Full Access SubUser BillPay permissions")]
        [Display(Name = "BillPay CheckFreeSB Full Access SubUser Permissions")]
        BillPayCheckFreeSBSubUserFullAccess = 194,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("The ability to have CheckFreeSB Dual Control SubUser BillPay permissions")]
        [Display(Name = "BillPay CheckFreeSB Dual Control SubUser Permissions")]
        BillPayCheckFreeSBSubUserDualControl = 195,

        [EnumMember]
        [Category("EntityGroup")]
        [Description("Permission for a SubUser to Aggregate Accounts")]
        [Display(Name = "Account Aggregation Sub User Control")]
        AccountAggregationSubUserControl = 196,

        [EnumMember]
        [Category("Administrator")]
        [Description("Permission for an Admin to manage a users eDocuments")]
        [Display(Name = "Manage users eDocs")]
        ManageUserEdocs = 197,

        [EnumMember]
        [Category("Administrator")]
        [Description("Provides FI Admin with access to register Business Users and Business Householding features")]
        [Display(Name = "Register New Business User")]
        RegisterNewBusinessUser = 198,

        /// <summary>
        /// Allows admin user to manually generate one time MFA codes.
        /// </summary>
        [EnumMember]
        [Category("Administrator")]
        [Description("Allows admin user to manually generate one time MFA codes")]
        [Display(Name = "Elevated MFA Management")]
        ElevatedMfaManagement = 199,


        /// <summary>
        /// Gives Business SubUsers the ability to update TDE categories and descriptions
        /// </summary>
        [EnumMember]
        [Category("EntityGroup")]
        [Description("Permission for a SubUser to Update TDE Descriptions and Categories")]
        [Display(Name = "TDE Category and Description Sub User Control")]
        TransactionEnrichmentSubUserControl = 200,

        /// <summary>
        /// Allows an administrator to access and use the StandardSSO Admin Widget
        /// </summary>
        [EnumMember]
        [Category("Administrator")]
        [Description("Allows an administrator to access and use the StandardSSO Admin Widget")]
        [Display(Name = "StandardSSO Admin")]
        StandardSSOAdmin = 201,

        /// <summary>
        /// Allows an administrator to add or remove users from a login group
        /// </summary>
        [EnumMember]
        [Category("Administrator")]
        [Description("Allows an administrator to add or remove users from a login group")]
        [Display(Name = "Manage Login Grouping")]
        ManageLoginGrouping = 202,

        /// <summary>
        /// Allows the controlling of access to RDC via desktop SSO
        /// </summary>
        [EnumMember]
        [Category("EntityGroup")]
        [Description("Allows the controlling of access to RDC via desktop SSO")]
        [Display(Name = "Manage Desktop RDC SSO")]
        ManageDesktopRDCSSO = 203,

        /// <summary>
        /// Ability to RDC into specific accounts via desktop SSO
        /// </summary>
        [EnumMember]
        [Category("Account")]
        [Description("Ability to RDC into specific accounts via desktop SSO")]
        [Display(Name = "Desktop RDC Funds Into SSO")]
        DesktopRDCFundsIntoSSO = 204,

        /// <summary>
        /// Administrative ability to manage acoount configurations
        /// </summary>
        [EnumMember]
        [Category("Administrator")]
        [Description("Administrative ability to manage acoount configurations")]
        [Display(Name = "Manage Account Configuration")]
        ManageAccountConfiguration = 205,

        /// <summary>
        /// Administrative ability to manage compliance settings
        /// </summary>
        [EnumMember]
        [Category("Administrator")]
        [Description("Administrative ability to manage compliance settings")]
        [Display(Name = "Manage Compliance Settings")]
        ManageComplianceSettings = 206
    }
}
