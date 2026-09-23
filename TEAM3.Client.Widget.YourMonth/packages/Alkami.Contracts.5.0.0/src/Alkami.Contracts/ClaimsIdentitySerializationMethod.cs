using System;

namespace Alkami.Contracts
{
    /// <summary>
    /// Defines the supported methods for serializing the ClaimsIdentity on the request.
    /// </summary>
    [Flags]
    public enum ClaimsIdentitySerializationMethod
    {
        /// <summary>
        /// The legacy method for serializing the ClaimsIdentity using the DataContract Serializer.
        /// </summary>
        DataContractSerializer = 1 << 0,

        /// <summary>
        /// Converting the ClaimsIdentity to a JSON structure.
        /// </summary>
        Json = 1 << 1,

        /// <summary>
        /// Converting the ClaimsIdentity to a JSON structure, then compressing the resulting output.
        /// </summary>
        CompressedJson = 1 << 2,
    }
}