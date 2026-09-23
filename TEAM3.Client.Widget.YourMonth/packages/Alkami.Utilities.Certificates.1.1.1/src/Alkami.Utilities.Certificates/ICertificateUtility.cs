using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Alkami.Utilities.Certificates
{
    /// <summary>
    /// Utility class used to get the latest valid certificate
    /// </summary>
    public interface ICertificateUtility
    {

        /// <summary>
        /// Finds a certificate based upon the thumbprint. StoreLocation defaults to CurrentUser for Linux, and LocalMachine for Windows.
        /// </summary>
        /// <param name="storeName"><see cref="StoreName"/> value</param>
        /// <param name="thumbprint">Thumbprint to search for</param>
        /// <returns>Returns a certificate if found. If it is not found, returns null.</returns>
        Task<X509Certificate2?> FindByThumbprint(StoreName storeName, string thumbprint);

        /// <summary>
        /// Finds a certificate based upon the subject name. StoreLocation defaults to CurrentUser for Linux, and LocalMachine for Windows.
        /// </summary>
        /// <param name="storeName"><see cref="StoreName"/> value</param>
        /// <param name="subjectName">Subject to search for</param>
        /// <returns>Returns a certificate if found. If it is not found, returns null.</returns>
        Task<X509Certificate2?> FindBySubjectName(StoreName storeName, string subjectName);

        /// <summary>
        /// Asynchronously finds a certificate based on the provided parameters. StoreLocation defaults to CurrentUser for Linux, and LocalMachine for Windows.
        /// </summary>
        /// <param name="storeName">The name of the certificate store.</param>
        /// <param name="findType">The type of search to perform.</param>
        /// <param name="findValue">The value to search for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the found certificate, or null if no certificate was found.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the provided find type is not supported by the retrieval strategy.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the find value is null or whitespace, and the find type is not 'FindBySubjectName'.</exception>
        Task<X509Certificate2?> FindCertificate(StoreName storeName, X509FindType findType, string findValue);


        /// <summary>
        /// Finds a certificate based upon the thumbprint
        /// </summary>
        /// <param name="storeName"><see cref="StoreName"/> value</param>
        /// <param name="storeLocation"><see cref="StoreLocation"/> value</param>
        /// <param name="thumbprint">Thumbprint to search for</param>
        /// <returns>Returns a certificate if found. If it is not found, returns null.</returns>
        Task<X509Certificate2?> FindByThumbprint(StoreName storeName, StoreLocation storeLocation, string thumbprint);

        /// <summary>
        /// Finds a certificate based upon the subject name
        /// </summary>
        /// <param name="storeName"><see cref="StoreName"/> value</param>
        /// <param name="storeLocation"><see cref="StoreLocation"/> value</param>
        /// <param name="subjectName">Subject to search for</param>
        /// <returns>Returns a certificate if found. If it is not found, returns null.</returns>
        Task<X509Certificate2?> FindBySubjectName(StoreName storeName, StoreLocation storeLocation, string subjectName);

        /// <summary>
        /// Asynchronously finds a certificate based on the provided parameters.
        /// </summary>
        /// <param name="storeName">The name of the certificate store.</param>
        /// <param name="storeLocation">The location of the certificate store.</param>
        /// <param name="findType">The type of search to perform.</param>
        /// <param name="findValue">The value to search for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the found certificate, or null if no certificate was found.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the provided find type is not supported by the retrieval strategy.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the find value is null or whitespace, and the find type is not 'FindBySubjectName'.</exception>
        Task<X509Certificate2?> FindCertificate(StoreName storeName, StoreLocation storeLocation, X509FindType findType, string findValue);
    }
}
