using System.Security.Cryptography.X509Certificates;

namespace Alkami.Utilities.Certificates
{
    /// <summary>
    /// Represents a store used to keep certificates safe and sound.
    /// </summary>
    public interface ICertificateStore
    {
        /// <summary>
        /// Attempts to retrieve a certificate from the store with the provided values.
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        /// <param name="findType"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        X509Certificate2Collection? GetCertificatesByValue(StoreName storeName, StoreLocation storeLocation, X509FindType findType, string? identifier);
    }
}
