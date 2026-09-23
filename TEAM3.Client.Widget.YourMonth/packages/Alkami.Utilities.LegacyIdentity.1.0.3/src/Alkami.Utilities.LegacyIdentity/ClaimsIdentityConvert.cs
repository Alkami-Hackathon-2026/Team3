using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Alkami.Utilities.LegacyIdentity
{
    internal static class ClaimsIdentityConvert
    {
        internal static ClaimsIdentity ToClaimsIdentity(this JsonClaimsIdentity identity)
        {
            // Create list of claims
            var claims = new List<Claim>();

            foreach (var jsonClaim in identity.Claims)
            {
                // Create new claim
                var claim = new Claim(jsonClaim.Type, jsonClaim.Value);

                if (jsonClaim.Properties?.Any() == true)
                {
                    foreach(var property in jsonClaim.Properties)
                    {
                        // Add the properties
                        claim.Properties.Add(property.Key, property.Value);
                    }
                }

                // Add the claim
                claims.Add(claim);
            }

            // Return the ClaimsIdentity with the correct AuthenticationType
            return new ClaimsIdentity(claims, identity.AuthenticationType, identity.NameClaimType, identity.RoleClaimType)
            {
                Label = identity.Label
            };
        }

        internal static JsonClaimsIdentity ToJsonClaimsIdentity(this ClaimsIdentity identity)
        {
            // Create json claims identity
            var claims = new List<JsonClaim>();

            foreach (var claim in identity.Claims)
            {
                // Create new json claim
                var jsonClaim = new JsonClaim(claim.Type, claim.Value);

                if (claim.Properties.Any())
                {
                    // Create new properties list
                    var properties = new List<JsonClaimProperty>();

                    foreach (var property in claim.Properties)
                    {
                        // Add the json property
                        properties.Add(new JsonClaimProperty(property.Key, property.Value));
                    }

                    // Set the properties on the claim
                    jsonClaim.Properties = properties;
                }

                // Add the json claim to the identity
                claims.Add(jsonClaim);
            }

            // Return the json claims identity
            return new JsonClaimsIdentity(identity.AuthenticationType, identity.NameClaimType, identity.RoleClaimType)
            {
                Claims = claims,
                Label = identity.Label
            };
        }
    }
}
