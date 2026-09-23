using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;

namespace Alkami.Utilities.Rpc
{
    /// <summary>
    /// A Surrogate Converter for <see cref="ClaimsIdentity"/>
    /// </summary>
    public class ClaimsIdentitySurrogateConverter : ISurrogateConverter
    {
        /// <inheritdoc />
        public Type SurrogateType => typeof(ClaimsIdentitySurrogate);

        /// <inheritdoc />
        public Type ContractType => typeof(ClaimsIdentity);

        /// <inheritdoc />
        public object ConvertToSurrogate(object obj)
        {
            if (obj == null)
                return null;

            var claimsIdentity = obj as ClaimsIdentity;

            return (ClaimsIdentitySurrogate)claimsIdentity;
        }

        /// <inheritdoc />
        public object ConvertFromSurrogate(object obj)
        {
            if (obj == null)
                return null;

            var surrogate = obj as ClaimsIdentitySurrogate;
            return (ClaimsIdentity)surrogate;
        }
    }

    internal class ClaimSurrogate : ISerializationSurrogate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var item = (Claim)obj;
            info.AddValue("m_issuer", item.Issuer);
            info.AddValue("m_originalIssuer", item.OriginalIssuer);
            info.AddValue("m_type", item.Type);
            info.AddValue("m_value", item.Value);
            info.AddValue("m_valueType", item.ValueType);
            if (item.Properties == null || item.Properties.Count == 0)
            {
                info.AddValue("m_properties", null, typeof(Dictionary<string, string>));
            }
            else
            {
                info.AddValue("m_properties", item.Properties);
            }
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var issuer = (string)info.GetValue("m_issuer", typeof(string));
            var originalIssuer = (string)info.GetValue("m_originalIssuer", typeof(string));
            var type = (string)info.GetValue("m_type", typeof(string));
            var value = (string)info.GetValue("m_value", typeof(string));
            var valueType = (string)info.GetValue("m_valueType", typeof(string));
            var properties = (Dictionary<string, string>)info.GetValue("m_properties", typeof(Dictionary<string, string>));
            var newClaim = new Claim(type, value, valueType, issuer, originalIssuer);

            if (properties != null)
            {
                foreach (var p in properties)
                {
                    newClaim.Properties.Add(p.Key, p.Value);
                }
            }
            return newClaim;
        }
    }

    internal class DictionarySurrogate : ISerializationSurrogate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var actualObj = (Dictionary<string, string>)obj;

            actualObj.GetObjectData(info, context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="info"></param>
        /// <param name="context"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var actualObj = (Dictionary<string, string>)obj;

            var enumerator = info.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Name == "KeyValuePairs" && enumerator.Current.Value != null)
                {
                    var values = (KeyValuePair<string, string>[])enumerator.Current.Value;
                    foreach (var value in values)
                    {
                        actualObj.Add(value.Key, value.Value);
                    }
                    break;
                }
            }

            return obj;
        }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/System.Security.Claims", Name = "ClaimsIdentity")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ClaimsIdentitySurrogate

    {
        [DataMember(Name = "m_actor", Order = 0)]
        public ClaimsIdentity Actor { get; private set; }

        [DataMember(Name = "m_authenticationType", Order = 1)]
        public string AuthenticationType { get; set; }

        [DataMember(Name = "m_bootstrapContext", Order = 2)]
        public object BootstrapContext { get; set; }

        [DataMember(Name = "m_Label", Order = 3)]
        public string Label { get; set; }

        public string Name { get; set; }

        public bool IsAuthenticated { get; set; }

        public IEnumerable<Claim> Claims { get; private set; }

        [DataMember(Name = "m_serializedClaims", Order = 4)]
        public string ClaimsSerializable { get; set; }

        [DataMember(Name = "m_serializedNameType", Order = 5)]
        public string NameClaimType { get; set; }

        [DataMember(Name = "m_serializedRoleType", Order = 6)]
        public string RoleClaimType { get; set; }

        [DataMember(Name = "m_version", Order = 7)]
        public string Version { get; set; } = "1.0";

        public static implicit operator ClaimsIdentitySurrogate(ClaimsIdentity claimIdentity)
        {
            if (claimIdentity == null)
                return null;

            var surrogateSelector = new SurrogateSelector();
            surrogateSelector.AddSurrogate(typeof(Claim), new StreamingContext(StreamingContextStates.All), new ClaimSurrogate());
            surrogateSelector.AddSurrogate(typeof(Dictionary<string, string>), new StreamingContext(StreamingContextStates.All), new DictionarySurrogate());
            var formatter = new BinaryFormatter
            {
                SurrogateSelector = surrogateSelector
            };

            var claims = new List<Claim>();
            foreach (var c in claimIdentity.Claims)
            {
                claims.Add(c.Clone(null));
            }

            using (MemoryStream serializationStream = new MemoryStream())
            {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                formatter.Serialize(serializationStream, claims);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                return new ClaimsIdentitySurrogate()
                {
                    AuthenticationType = claimIdentity.AuthenticationType,
                    Name = claimIdentity.Name,
                    Label = claimIdentity.Label,
                    IsAuthenticated = claimIdentity.IsAuthenticated,
                    ClaimsSerializable = Convert.ToBase64String(serializationStream.GetBuffer(), 0, (int)serializationStream.Length),
                    BootstrapContext = claimIdentity.BootstrapContext,
                    Actor = claimIdentity.Actor,
                    RoleClaimType = claimIdentity.RoleClaimType,
                    NameClaimType = claimIdentity.NameClaimType
                };
            }
        }

        public static implicit operator ClaimsIdentity(ClaimsIdentitySurrogate surrogate)
        {
            if (surrogate == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(surrogate.ClaimsSerializable))
            {
                var formatter = new BinaryFormatter();

                var surrogateSelector = new SurrogateSelector();
                surrogateSelector.AddSurrogate(typeof(Claim), new StreamingContext(StreamingContextStates.All), new ClaimSurrogate());
                surrogateSelector.AddSurrogate(typeof(Dictionary<string, string>), new StreamingContext(StreamingContextStates.All), new DictionarySurrogate());
                formatter.SurrogateSelector = surrogateSelector;

                var bytes = Convert.FromBase64String(surrogate.ClaimsSerializable);
                using (MemoryStream serializationStream = new MemoryStream(bytes))
                {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                    surrogate.Claims = (List<Claim>)formatter.Deserialize(serializationStream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                }
            }

            ClaimsIdentity result;
            if (surrogate.Claims?.Any() == true)
            {
                result = new ClaimsIdentity(surrogate.Claims, surrogate.AuthenticationType, surrogate.NameClaimType, surrogate.RoleClaimType);
            }
            else
            {
                result = new ClaimsIdentity(surrogate.AuthenticationType, surrogate.NameClaimType, surrogate.RoleClaimType);
            }
            result.Label = surrogate.Label;
            result.Actor = surrogate.Actor;
            result.BootstrapContext = surrogate.BootstrapContext;
            return result;
        }
    }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
