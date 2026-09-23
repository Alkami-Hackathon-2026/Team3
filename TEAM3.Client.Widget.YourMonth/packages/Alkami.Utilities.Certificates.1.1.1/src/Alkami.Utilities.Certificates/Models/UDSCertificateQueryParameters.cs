using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace Alkami.Utilities.Certificates.Models
{
    /// <summary>
    /// Query parameters for retrieving certificates from the sidecar via UDS
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class UDSCertificateQueryParameters
    {
        /// <summary>
        /// The <see cref="StoreName"/> to search certificates on.
        /// </summary>
        public StoreName StoreName { get; set; }
        
        /// <summary>
        /// The <see cref="StoreLocation"/> to search certificates on.
        /// </summary>
        public StoreLocation StoreLocation { get; set; }
        
        /// <summary>
        /// The <see cref="X509FindType"/> that will be used to filter certificates on by <see cref="FindValue"/>
        /// </summary>
        public X509FindType X509FindType { get; set; }
        
        /// <summary>
        /// The find value, this corresponds to the type supplied on <see cref="X509FindType"/>
        /// </summary>
        public string? FindValue { get; set; }
    }
}
