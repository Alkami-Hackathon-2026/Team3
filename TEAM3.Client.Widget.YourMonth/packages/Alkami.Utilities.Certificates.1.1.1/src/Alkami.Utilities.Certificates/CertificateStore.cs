using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace Alkami.Utilities.Certificates
{
    /// <summary>
    /// A certificate store for retrieving local certs.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CertificateStore : ICertificateStore
    {
        /// <summary>
        /// Attempts to get certificates by a provided string value
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public X509Certificate2Collection? GetCertificatesByValue(StoreName storeName, StoreLocation storeLocation,
            X509FindType findType, string? findValue)
        {
            if (string.IsNullOrWhiteSpace(findValue))
                throw new ArgumentNullException(nameof(findValue));
            
            var certs = new X509Certificate2Collection();

            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            certs.AddRange(store.Certificates.Find(findType, findValue, false));

            store.Close();

            return certs;
        }
    }
}
