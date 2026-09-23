#if NET6_0_OR_GREATER
using Alkami.Utilities.Json;
using System.Text.Json;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#endif

using System.Security.Claims;
using System.Text;

namespace Alkami.Utilities.LegacyIdentity
{
    /// <summary>
    /// Default implementation of <see cref="IClaimsIdentitySerializer" />
    /// </summary>
    public class ClaimsIdentitySerializer : IClaimsIdentitySerializer
    {
        /// <inheritdoc />
        public ClaimsIdentity ToClaimsIdentity(byte[] compressedJson)
        {
            var serializedJson = ToJson(compressedJson);

            return ToClaimsIdentity(serializedJson);
        }

        /// <inheritdoc />
        public string ToJson(byte[] compressedJson)
        {
            var serializedJson = CompressionUtility.Decompress(compressedJson);

            return Encoding.UTF8.GetString(serializedJson);
        }

        /// <inheritdoc />
        public byte[] ToCompressed(ClaimsIdentity identity)
        {
            var serializedJson = ToJson(identity);

            return ToCompressed(serializedJson);
        }

        /// <inheritdoc />
        public byte[] ToCompressed(string serializedJson)
        {
            var uncompressedBytes = Encoding.UTF8.GetBytes(serializedJson);

            return CompressionUtility.Compress(uncompressedBytes);
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public string ToJson(ClaimsIdentity identity)
        {
            var jsonClaimsIdentity = identity.ToJsonClaimsIdentity();

            return JsonSerializer.Serialize(jsonClaimsIdentity, AlkamiJsonSerializerOptions.Default);
        }

        /// <inheritdoc />
        public ClaimsIdentity ToClaimsIdentity(string serializedJson)
        {
            var jsonClaimsIdentity = JsonSerializer.Deserialize<JsonClaimsIdentity>(serializedJson, AlkamiJsonSerializerOptions.Default);

            return jsonClaimsIdentity.ToClaimsIdentity();
        }
#else
        private static JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <inheritdoc />
        public string ToJson(ClaimsIdentity identity)
        {
            var jsonClaimsIdentity = identity.ToJsonClaimsIdentity();

            return JsonConvert.SerializeObject(jsonClaimsIdentity, Settings);
        }

        /// <inheritdoc />
        public ClaimsIdentity ToClaimsIdentity(string serializedJson)
        {
            var jsonClaimsIdentity = JsonConvert.DeserializeObject<JsonClaimsIdentity>(serializedJson, Settings);

            return jsonClaimsIdentity.ToClaimsIdentity();
        }
#endif
    }
}
