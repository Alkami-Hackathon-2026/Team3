using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Alkami.Utilities.Certificates.Extensions;
using Alkami.Utilities.Certificates.RetrievalStrategy;

namespace Alkami.Utilities.Certificates
{
    /// <inheritdoc cref="ICertificateUtility"/>
    public class CertificateUtility : ICertificateUtility
    {
        private readonly ICertificateRetrievalStrategy _retrievalStrategy;
        private readonly ConcurrentDictionary<string, X509Certificate2?> _certificateCache;
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Default constructor. Accepts a certificate retrieval strategy 
        /// </summary>
        /// <param name="retrievalStrategy"></param>
        public CertificateUtility(ICertificateRetrievalStrategy retrievalStrategy)
        {
            _retrievalStrategy = retrievalStrategy;
            _certificateCache = new ConcurrentDictionary<string, X509Certificate2?>();
            _semaphore = new SemaphoreSlim(1, 1);
        }

        /// <inheritdoc cref="ICertificateUtility"/>
        public Task<X509Certificate2?> FindBySubjectName(StoreName storeName, string subjectName)
        {
            return FindCertificateInternal(storeName, GetStoreLocationByOsPlatformDefault(), X509FindType.FindBySubjectName, subjectName);
        }

        /// <inheritdoc cref="ICertificateUtility"/>
        public Task<X509Certificate2?> FindByThumbprint(StoreName storeName, string thumbprint)
        {
            return FindCertificateInternal(storeName, GetStoreLocationByOsPlatformDefault(), X509FindType.FindByThumbprint, thumbprint);
        }

        /// <inheritdoc cref="ICertificateUtility"/>
        public Task<X509Certificate2?> FindCertificate(StoreName storeName, X509FindType findType, string findValue)
        {
            return FindCertificateInternal(storeName, GetStoreLocationByOsPlatformDefault(), findType, findValue);
        }

        /// <inheritdoc cref="ICertificateUtility"/>
        public Task<X509Certificate2?> FindBySubjectName(StoreName storeName, StoreLocation storeLocation, string subjectName)
        {
            return FindCertificateInternal(storeName, storeLocation, X509FindType.FindBySubjectName, subjectName);
        }

        /// <inheritdoc cref="ICertificateUtility"/>
        public Task<X509Certificate2?> FindByThumbprint(StoreName storeName, StoreLocation storeLocation, string thumbprint)
        {
            return FindCertificateInternal(storeName, storeLocation, X509FindType.FindByThumbprint, thumbprint);
        }

        /// <inheritdoc cref="ICertificateUtility"/>
        public Task<X509Certificate2?> FindCertificate(StoreName storeName, StoreLocation storeLocation, X509FindType findType, string findValue)
        {
            return FindCertificateInternal(storeName, storeLocation, findType, findValue);
        }

        private async Task<X509Certificate2?> FindCertificateInternal(StoreName storeName, StoreLocation location, X509FindType findType, string findValue)
        {
            if (!_retrievalStrategy.IsSupportedFindType(findType))
            {
                throw new InvalidOperationException($"Unsupported find type {findType} for retrieval strategy {_retrievalStrategy.GetType().Name}");
            }

            if (findType == X509FindType.FindByThumbprint)
            {
                // When copying values out of MMC directly, you can get a non-printing Unicode character.
                // This logic is here to ensure the non-printing character turns to a '?', all extra spaces are removed,
                // and the characters are all upper-cased.
                findValue = findValue.Replace(" ", string.Empty)
                                     .ToUpperInvariant();
            }

            if (string.IsNullOrWhiteSpace(findValue) && findType != X509FindType.FindBySubjectName)
            {
                throw new ArgumentNullException(nameof(findValue));
            }

            var certKey = Tuple.Create(storeName, findType, findValue).ToString();

            if (_certificateCache.TryGetValue(certKey, out var cert) && cert.IsActive())
            {
                return cert;
            }

            await _semaphore.WaitAsync();
            try
            {
                //Check again in case a previous thread added the cert while invocation was locked
                if (_certificateCache.TryGetValue(certKey, out cert) && cert.IsActive())
                {
                    return cert;
                }

                var retrievedCert = await _retrievalStrategy.GetCertificate(storeName, location, findType, findValue);

                if (retrievedCert.IsActive())
                {
                    _certificateCache[certKey] = retrievedCert;
                    return retrievedCert;
                }

                _certificateCache.TryRemove(certKey, out var _);
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static StoreLocation GetStoreLocationByOsPlatformDefault()
        {
#if NETFRAMEWORK
            return StoreLocation.LocalMachine;
#else
            return (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? StoreLocation.LocalMachine : StoreLocation.CurrentUser;
#endif
        }
    }
}
