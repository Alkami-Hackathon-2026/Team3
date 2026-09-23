using System.Security.Claims;

namespace Alkami.Utilities.LegacyIdentity
{
    /// <summary>
    /// Data contract for ClaimsIdentity serialization
    /// </summary>
    public interface IClaimsIdentitySerializer
    {
        /// <summary>
        /// Convert from compressed Json to a <see cref="ClaimsIdentity" />
        /// </summary>
        /// <param name="compressedJson">Compressed json previously created by this serializer</param>
        /// <returns></returns>
        ClaimsIdentity ToClaimsIdentity(byte[] compressedJson);

        /// <summary>
        /// Convert from serialized json to a <see cref="ClaimsIdentity" />
        /// </summary>
        /// <param name="serializedJson">Serialized json previously created by this serializer</param>
        /// <returns></returns>
        ClaimsIdentity ToClaimsIdentity(string serializedJson);

        /// <summary>
        /// Convert from <see cref="ClaimsIdentity" /> to serialized json
        /// </summary>
        /// <param name="identity">Identity to serialize</param>
        /// <returns></returns>
        string ToJson(ClaimsIdentity identity);

        /// <summary>
        /// Convert from compressed json to serialized json
        /// </summary>
        /// <param name="compressedJson">Compressed json previously created by this serializer</param>
        /// <returns></returns>
        string ToJson(byte[] compressedJson);

        /// <summary>
        /// Convert from <see cref="ClaimsIdentity" /> to compressed json
        /// </summary>
        /// <param name="identity">Identity to serialize</param>
        /// <returns></returns>
        byte[] ToCompressed(ClaimsIdentity identity);

        /// <summary>
        /// Convert from serialized json to compressed json
        /// </summary>
        /// <param name="serializedJson">Serialized json previously created by this serializer</param>
        /// <returns></returns>
        byte[] ToCompressed(string serializedJson);
    }
}
