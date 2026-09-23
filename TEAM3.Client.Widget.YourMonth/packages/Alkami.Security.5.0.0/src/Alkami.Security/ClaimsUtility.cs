using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Alkami.Contracts;

namespace Alkami.Security
{
    /// <summary>
    /// The ClaimsUtility contains various methods related to security claims.
    /// </summary>
    public static class ClaimsUtility
    {
        /// <summary>
        /// Determines if the provided <see cref="BaseRequest"/> has an authenticated Alkami principal and the <see cref="BaseRequest"/>
        /// values line up with the values on the <see cref="BaseRequest.ClaimsIdentity"/>.
        /// </summary>
        /// <param name="request">The <see cref="BaseRequest"/> to validate.</param>
        /// <returns>True - the provided <paramref name="request"/> is valid; False - otherwise.</returns>
        public static bool IsAuthenticated(this BaseRequest request)
        {
            var identity = request.ClaimsIdentity;

            return IsAuthenticated(identity) && DoesRequestMatchClaim(request, identity);
        }

        /// <summary>
        /// Determines whether the provided <see cref="BaseRequest"/> was from a masquerading admin.
        /// </summary>
        /// <param name="request">The <see cref="BaseRequest"/> to validate.</param>
        /// <returns>True - the provided <paramref name="request"/> is from a maquerading admin; False - otherwise.</returns>
        public static bool IsMasqueradingAdmin(this BaseRequest request)
        {
            var claimValue = request.GetClaimValue(AlkamiClaimTypes.MasqueradingIdentifier);
            Guid adminIdentifier;

            return (Guid.TryParse(claimValue, out adminIdentifier) && (adminIdentifier != Guid.Empty));
        }

        /// <summary>
        /// Determines whether the provided <see cref="BaseRequest"/> was from an admin.
        /// </summary>
        /// <param name="request">The <see cref="BaseRequest"/> to validate.</param>
        /// <returns>True - the provided <paramref name="request"/> is an admin; False - otherwise.</returns>
        public static bool IsAdmin(this BaseRequest request)
        {
            var permissions = request.GetPermission(AlkamiClaimTypes.AllGrantedPermissions);

            return (permissions != null) && permissions.HasPermission(Permission.BankEntity);
        }

        /// <summary>
        /// Gets the value of the desired <see cref="Claim"/> from the <paramref name="request"/>'s claims.
        /// </summary>
        /// <param name="request">The <see cref="BaseRequest"/> to get the <see cref="Claim"/> from.</param>
        /// <param name="claimType">The type of the claim to get.</param>
        /// <returns>The value of the <see cref="Claim"/>.</returns>
        public static string GetClaimValue(this BaseRequest request, string claimType)
        {
            var claim = GetClaim(request.ClaimsIdentity, claimType);

            return (claim != null) ? claim.Value : null;
        }

        /// <summary>
        /// Gets the requested permissions on the provided <paramref name="request"/>'s claims from the desired <paramref name="claimType"/>.
        /// </summary>
        /// <param name="request">The <see cref="BaseRequest"/> to get the permissions from.</param>
        /// <param name="claimType">The type of the claim to search for.</param>
        /// <returns>The <see cref="Mask{Permission}"/> hydrated from the claim.</returns>
        public static Mask<Permission> GetPermission(this BaseRequest request, string claimType)
        {
            var claim = GetClaim(request.ClaimsIdentity, claimType);

            return (claim != null) ? Mask<Permission>.FromString(claim.Value) : null;
        }

        /// <summary>
        /// Gets the requested permissions on the provided <paramref name="request"/> from the desired <paramref name="claimType"/> with the provided <paramref name="suffix"/>.
        /// </summary>
        /// <param name="request">The <see cref="BaseRequest"/> to get the permissions from.</param>
        /// <param name="claimType">The type of the claim to search for.</param>
        /// <param name="suffix">The suffix to apply to the claimType for claims that are prefixed.</param>
        /// <returns>The <see cref="Mask{Permission}"/> hydrated from the claim.</returns>
        public static Mask<Permission> GetPermission(this BaseRequest request, string claimType, string suffix)
        {
            var completeClaimType = claimType + suffix;

            return GetPermission(request.ClaimsIdentity, completeClaimType);
        }

        /// <summary>
        /// Determines if the provided <see cref="IPrincipal"/> is an authenticated Alkami principal.
        /// </summary>
        /// <param name="principal">The <see cref="IPrincipal"/> to validate.</param>
        /// <returns>True - the provided <paramref name="principal"/> is valid; False - otherwise.</returns>
        public static bool IsAuthenticated(IPrincipal principal)
        {
            return (principal != null) && ClaimsUtility.IsAuthenticated(principal.Identity);
        }

        /// <summary>
        /// Determines if the provided <see cref="IIdentity"/> is an authenticated Alkami identity.
        /// </summary>
        /// <param name="identity">The <see cref="IIdentity"/> to validate.</param>
        /// <returns>True - the provided <paramref name="identity"/> is valid; False - otherwise.</returns>
        public static bool IsAuthenticated(IIdentity identity)
        {
            return (identity != null) && ClaimsUtility.IsAuthenticated(identity as ClaimsIdentity);
        }

        /// <summary>
        /// Determines if the provided <see cref="IIdentity"/> is an authenticated Alkami identity.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> to validate.</param>
        /// <returns>True - the provided <paramref name="identity"/> is valid; False - otherwise.</returns>
        public static bool IsAuthenticated(ClaimsIdentity identity)
        {
            if ((identity == null) || !identity.IsAuthenticated)
                return false;

            var requiredClaimTypes = new[]
            {
                AlkamiClaimTypes.AllGrantedPermissions,
                AlkamiClaimTypes.BankId,
                AlkamiClaimTypes.BankIdentifier,
                AlkamiClaimTypes.BankName,
                AlkamiClaimTypes.ClientApp,
				// TODO: Add these back in, so that the legacy shim populates these properly.
				//AlkamiClaimTypes.IssuedTokenTypes,
				//AlkamiClaimTypes.IssuerEvidence,
				AlkamiClaimTypes.LocaleId,
				// TODO: Add SessionId (once Auth MS is updated) and SessionId ensured to always be present.
				//AlkamiClaimTypes.SessionId,
				AlkamiClaimTypes.TimeZoneInfo,
                AlkamiClaimTypes.UserId,
                AlkamiClaimTypes.UserIdentifier,
                AlkamiClaimTypes.UserIPAddress,
            };
            var currentClaimTypes = identity.Claims
                .Select(c => c.Type)
                .ToArray();

            // Make sure all the required claim types are present.
            if (requiredClaimTypes.Except(currentClaimTypes, StringComparer.InvariantCultureIgnoreCase).Any())
                return false;

            // TODO: Validate the issuer
            return true;
        }

        /// <summary>
        /// Gets the value of the desired <see cref="Claim"/>.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> to get the <see cref="Claim"/> from.</param>
        /// <param name="claimType">The type of the claim to get.</param>
        /// <returns>The value of the <see cref="Claim"/>.</returns>
        public static string GetClaimValue(ClaimsIdentity identity, string claimType)
        {
            var claim = GetClaim(identity, claimType);

            return (claim != null) ? claim.Value : null;
        }

        /// <summary>
        /// Gets the requested permissions on the provided <paramref name="identity"/> from the desired <paramref name="claimType"/>.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> to validate.</param>
        /// <param name="claimType">The type of the claim to search for.</param>
        /// <returns>The <see cref="Mask{Permission}"/> hydrated from the claim.</returns>
        public static Mask<Permission> GetPermission(ClaimsIdentity identity, string claimType)
        {
            var claim = GetClaim(identity, claimType);

            return (claim != null) ? Mask<Permission>.FromString(claim.Value) : null;
        }

        /// <summary>
        /// Gets the requested permissions on the provided <paramref name="identity"/> from the desired <paramref name="claimType"/> with the provided <paramref name="suffix"/>.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> to validate.</param>
        /// <param name="claimType">The type of the claim to search for.</param>
        /// <param name="suffix">The suffix to apply to the claimType for claims that are prefixed.</param>
        /// <returns>The <see cref="Mask{Permission}"/> hydrated from the claim.</returns>
        public static Mask<Permission> GetPermission(ClaimsIdentity identity, string claimType, string suffix)
        {
            var completeClaimType = claimType + suffix;

            return GetPermission(identity, completeClaimType);
        }

        /// <summary>
        /// Gets the requested <paramref name="claimType"/> from the <paramref name="identity"/>.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> to validate.</param>
        /// <param name="claimType">The type of the claim to search for.</param>
        /// <returns>The <see cref="Claim"/> with the specified <paramref name="claimType"/>.</returns>
        private static Claim GetClaim(ClaimsIdentity identity, string claimType)
        {
            return (identity != null)
                ? identity.Claims.FirstOrDefault(claim => (string.Compare(claim.Type, claimType, StringComparison.InvariantCultureIgnoreCase) == 0))
                : null;
        }

        /// <summary>
        /// Determines whether the provided <see cref="BaseRequest"/> matches the claims provided on the <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="request">The <see cref="BaseRequest"/> to validate.</param>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> to validate.</param>
        /// <returns>True - the provided <paramref name="request"/> is valid; False - otherwise.</returns>
        private static bool DoesRequestMatchClaim(BaseRequest request, ClaimsIdentity identity)
        {
            if (request.BankIdentifier.HasValue)
            {
                var claimBankIdentifierValue = GetClaimValue(identity, AlkamiClaimTypes.BankIdentifier);
                Guid claimBankIdentifier;

                Guid.TryParse(claimBankIdentifierValue, out claimBankIdentifier);

                if (request.BankIdentifier.Value != claimBankIdentifier)
                    return false;
            }

            if (request.UserIdentifier.HasValue)
            {
                var claimUserIdentifierValue = GetClaimValue(identity, AlkamiClaimTypes.UserIdentifier);
                Guid claimUserIdentifier;

                Guid.TryParse(claimUserIdentifierValue, out claimUserIdentifier);

                if (request.UserIdentifier.Value != claimUserIdentifier)
                    return false;
            }

            if (request.UserId.HasValue)
            {
                var claimUserIdValue = GetClaimValue(identity, AlkamiClaimTypes.UserId);
                long claimUserId;

                long.TryParse(claimUserIdValue, out claimUserId);

                if (request.UserId.Value != claimUserId)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Deserialize sut into ClaimsIdentity object.
        /// </summary>
        public static ClaimsIdentity ToClaimsIdentity(this byte[] serializedUserToken)
        {
            using (var ms = new MemoryStream(serializedUserToken))
            {
                return new ClaimsIdentity(new BinaryReader(ms));
            }
        }

        /// <summary>
        /// Serialize ClaimsIdentity object into sut.
        /// </summary>
        public static byte[] Serialize(this ClaimsIdentity claimsIdentity)
        {
            using (var mem = new MemoryStream())
            {
                claimsIdentity.WriteTo(new BinaryWriter(mem));
                mem.Position = 0;
                return mem.ToArray();
            }
        }
    }
}
