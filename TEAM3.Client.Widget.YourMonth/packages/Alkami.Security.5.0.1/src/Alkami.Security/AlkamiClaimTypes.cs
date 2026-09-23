using System.Diagnostics.CodeAnalysis;

namespace Alkami.Security
{
    /// <summary>
    /// The types of claims that can be applied to an RP-STS security token
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AlkamiClaimTypes
    {
        /// <summary>
        /// Permissions that relate to an account as well as the entity that owns the account. This is a PREFIX
        /// Add the Account ID to the end of this URI
        /// </summary>
        public const string AccountPermission = "http://security.alkamitech.com/2016/09/claims/AccountPermission/";

        /// <summary>
        /// The display name for the admin user masquerading as an FI member.
        /// </summary>
        public const string AdminDisplayName = "http://security.alkamitech.com/2011/07/claims/AdminDisplayName";

        /// <summary>
        /// Permissions for the admin user.
        /// </summary>
        public const string AdministratorPermissions = "http://security.alkamitech.com/2016/07/claims/Adminstrator";

        /// <summary>
        /// This is an exhaustive list of all permissions this user has from all of their claims, entities etc.. this should be used as a global checker
        /// to control main level entry. Additional security should be applied afterwards
        /// </summary>
        public const string AllGrantedPermissions = "http://security.alkamitech.com/2016/09/claims/AllGrantedPermissions";

        /// <summary>
        /// The internal unique identifier of the financial institution to which the user belongs
        /// </summary>
        public const string BankId = "http://security.alkamitech.com/2011/07/claims/BankId";

        /// <summary>
        /// The external unique identifier of the financial institution to which the user belongs
        /// </summary>
        public const string BankIdentifier = "http://security.alkamitech.com/2011/07/claims/BankIdentifier";

        /// <summary>
        /// The name of the financial institution to which the user belongs
        /// </summary>
        public const string BankName = "http://security.alkamitech.com/2011/07/claims/BankName";

        /// <summary>
        /// The time zone information that is used to localize date/time data representations
        /// for a FI's time zone.
        /// </summary>
        public const string BankTimeZoneInfo = "http://security.alkamitech.com/2011/07/claims/BankTimeZoneInfo";

        /// <summary>
        /// Bank URL signature, used to identify the proper financial institution when requesting a security token.
        /// </summary>
        public const string BankUrlSignature = "http://security.alkamitech.com/2011/07/claims/BankUrlSignature";

        /// <summary>
        /// The client application that is requesting the security token, otherwise known as the
        /// application with which the user is interacting, if the user has an interactive context
        /// </summary>
        public const string ClientApp = "http://security.alkamitech.com/2011/07/claims/ClientApp";

        /// <summary>
        /// The EntityId for the current entity, if one exists
        /// </summary>
        public const string EntityId = "http://security.alkamitech.com/2016/08/claims/EntityId";

        /// <summary>
        /// The EntityIdentifier for the current entity, if one exists
        /// </summary>
        public const string EntityIdentifier = "http://security.alkamitech.com/2016/08/claims/EntityIdentifier";

        /// <summary>
        /// Permissions that relate to an entity. This is a PREFIX
        /// Add the Entity ID to the end of this URI
        /// </summary>
        public const string EntityPermissions = "http://security.alkamitech.com/2016/07/claims/EntityPermissions/";

        /// <summary>
        /// Whether or not the user is a business master user
        /// </summary>
        public const string IsBusinessMasterUser = "http://security.alkamitech.com/2016/08/claims/IsBusinessMasterUser";

        /// <summary>
        /// Whether or not the user is a business sub user
        /// </summary>
        public const string IsBusinessSubUser = "http://security.alkamitech.com/2016/08/claims/IsBusinessSubUser";

        /// <summary>
        /// The type of token for which the other associated claims are built.
        /// </summary>
        public const string IssuedTokenTypes = "http://security.alkamitech.com/2011/07/claims/IssuedTokenTypes";

        /// <summary>
        /// Sign in request issuer evidence.
        /// </summary>
        public const string IssuerEvidence = "http://security.alkamitech.com/2011/07/claims/IssuerEvidence";

        /// <summary>
        /// The locality of the user.
        /// </summary>
        public const string LocaleId = "http://security.alkamitech.com/2011/07/claims/LocaleId";

        /// <summary>
        /// The admin's permissions while masquerading as a user.
        /// </summary>
        public const string MasqueradePermissions = "http://security.alkamitech.com/2016/07/claims/MasqueradePermissions";

        /// <summary>
        /// The administrator identifier that is logged in as a bank user.
        /// </summary>
        public const string MasqueradingIdentifier = "http://security.alkamitech.com/2011/07/claims/MasqueradingIdentifier";

        /// <summary>
        /// Claim to identify that the Masquerading User is View Only
        /// </summary>
        public const string MasqueradingUserViewOnly = "http://security.alkamitech.com/2011/07/claims/MasqueradingUserViewOnly";

        /// <summary>
        /// The unique identifier of a user for a specific their primary core.
        /// </summary>
        public const string MemberIdentifier = "http://security.alkamitech.com/2017/04/claims/MemberIdentifier";

        /// <summary>
        /// Claim to determine the session id.
        /// </summary>
        public const string SessionId = "http://security.alkamitech.com/2021/03/claims/SessionId";

        /// <summary>
        /// Claim to determine the theme name to use
        /// </summary>
        public const string ThemeId = "http://security.alkamitech.com/2016/08/claims/ThemeId";

        /// <summary>
        /// Claim to determine the theme name to use
        /// </summary>
        public const string ThemeName = "http://security.alkamitech.com/2016/08/claims/ThemeName";

        /// <summary>
        /// The time zone information that is used to localize date/time data representations
        /// for a user's preferred time zone.
        /// </summary>
        public const string TimeZoneInfo = "http://security.alkamitech.com/2011/07/claims/TimeZoneInfo";

        /// <summary>
        /// The internal unique identifier of a user
        /// </summary>
        public const string UserId = "http://security.alkamitech.com/2011/07/claims/UserId";

        /// <summary>
        /// The well-known (external) unique identifier of a user
        /// </summary>
        public const string UserIdentifier = "http://security.alkamitech.com/2011/07/claims/UserIdentifier";

        /// <summary>
        /// The source internet protocol address of the user making the request, or of the service
        /// that is acting on behalf of the user if the request is not an interactive context
        /// </summary>
        public const string UserIPAddress = "http://security.alkamitech.com/2011/07/claims/UserIPAddress";

        /// <summary>
        /// The unique identifier of a user by a Secure Token Service issuing authority.  One user may have
        /// multiple STSID values from one or more identifying parties, however this value is unique in
        /// the namespace context of a single identifying party.
        /// </summary>
        /// <remarks>Uri Type</remarks>
        public const string UserName = "http://security.alkamitech.com/2011/07/claims/STSID";

        /// <summary>
        ///  Permissions that relate to a user
        /// </summary>
        public const string UserPermissions = "http://security.alkamitech.com/2016/09/claims/UserPermissions";

        /// <summary>
        ///  Unique identifier for a FI within an environment
        /// </summary>
        public const string BankInstanceIdentifier = "http://security.alkamitech.com/2021/03/claims/BankInstanceIdentifier";

        /// <summary>
        /// Claim containing the OAuthScopes tied to this identity.
        /// </summary>
        public const string OAuthScopes = "http://security.alkamitech.com/2021/06/claims/OAuthScopes";
    }
}
