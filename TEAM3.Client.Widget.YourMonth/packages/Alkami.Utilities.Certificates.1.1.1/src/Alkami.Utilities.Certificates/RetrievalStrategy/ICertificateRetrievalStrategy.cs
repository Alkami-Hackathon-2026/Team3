using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Alkami.Utilities.Certificates.RetrievalStrategy
{
    /// <summary>
    /// Defines a strategy for loading certificates
    /// </summary>
    public interface ICertificateRetrievalStrategy
    {
        /// <summary>
        /// Retrieves a <see cref="X509Certificate2"/> from a certificate store.
        /// </summary>
        /// <param name="storeName"></param>
        /// <param name="location"></param>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <returns></returns>
        Task<X509Certificate2?> GetCertificate(StoreName storeName, StoreLocation location, X509FindType findType, string? findValue);

        /// <summary>
        /// Whether the supplied <see cref="X509FindType"/> is supported by this strategy.
        /// </summary>
        bool IsSupportedFindType(X509FindType findType);
    }
}
