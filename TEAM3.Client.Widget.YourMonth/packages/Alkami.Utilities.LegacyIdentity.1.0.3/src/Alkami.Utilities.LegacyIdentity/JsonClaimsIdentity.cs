using System.Collections.Generic;

namespace Alkami.Utilities.LegacyIdentity
{
    /// <summary>
    /// Json Claims Identity
    /// </summary>
    /// <remarks>
    /// This object is not intended to be referenced in production code.
    /// It is a POCO only to aid in serialization of the claims identity.
    /// </remarks>
    internal class JsonClaimsIdentity
    {
        public JsonClaimsIdentity() { }

        public JsonClaimsIdentity(string authenticationType, string nameClaimType, string roleClaimType)
        {
            AuthenticationType = authenticationType;
            NameClaimType = nameClaimType;
            RoleClaimType = roleClaimType;
        }

        public string AuthenticationType { get; set; }
        public string NameClaimType { get; set; }
        public string RoleClaimType { get; set; }
        public string Label { get; set; }

        public List<JsonClaim> Claims { get; set; }
    }

    /// <summary>
    /// Json Claim
    /// </summary>
    internal class JsonClaim
    {
        public JsonClaim() { }

        public JsonClaim(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; }

        public string Value { get; set; }

        public List<JsonClaimProperty> Properties { get; set; }
    }

    /// <summary>
    /// Json Claim Property
    /// </summary>
    internal class JsonClaimProperty
    {
        public JsonClaimProperty() { }

        public JsonClaimProperty(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
