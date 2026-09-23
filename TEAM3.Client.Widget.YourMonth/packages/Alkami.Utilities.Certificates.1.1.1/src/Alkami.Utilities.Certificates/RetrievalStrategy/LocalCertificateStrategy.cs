using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Alkami.Utilities.Certificates.Logging;
using Common.Logging;
#if NET6_0_OR_GREATER
using Microsoft.Extensions.Logging;
#endif

namespace Alkami.Utilities.Certificates.RetrievalStrategy
{
    /// <summary>
    /// Local Certificate Strategy
    /// </summary>
    public class LocalCertificateStrategy : ICertificateRetrievalStrategy
    {
        /// <summary>
        /// Da Logger
        /// </summary>
        private protected readonly ILoggingAdapter Logger;
        private readonly ICertificateStore _certificateStore;

#if NET6_0_OR_GREATER
        /// <summary>
        /// Default Constructor. Accepts the Microsoft logger
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="certificateStore"></param>
        public LocalCertificateStrategy(ILogger<LocalCertificateStrategy> logger, ICertificateStore certificateStore)
        {
            Logger = new MicrosoftLoggingAdapter(logger);
            _certificateStore = certificateStore;
        }
#endif

        private protected LocalCertificateStrategy(ILoggingAdapter logger, ICertificateStore certificateStore)
        {
            Logger = logger;
            _certificateStore = certificateStore;
        }
        
        /// <summary>
        /// Default constructor. Uses the Common.Logging logger.
        /// </summary>
        /// <param name="store"></param>
        public LocalCertificateStrategy(ICertificateStore store)
            :this(new CommonLoggingAdapter(LogManager.GetLogger<LocalCertificateStrategy>()), store)
        {
        }

        /// <inheritdoc cref="ICertificateRetrievalStrategy"/>
        public virtual bool IsSupportedFindType(X509FindType findType)
        {
            switch (findType)
            {
                case X509FindType.FindByThumbprint:
                case X509FindType.FindBySubjectName:
                case X509FindType.FindBySubjectDistinguishedName:
                case X509FindType.FindByIssuerName:
                case X509FindType.FindByIssuerDistinguishedName:
                case X509FindType.FindBySerialNumber:
                case X509FindType.FindByTemplateName:
                case X509FindType.FindByApplicationPolicy:
                case X509FindType.FindByCertificatePolicy:
                case X509FindType.FindByExtension:
                case X509FindType.FindBySubjectKeyIdentifier:
                    return true;
                default:
                    return false;
            }
        }

        /// <inheritdoc cref="ICertificateRetrievalStrategy"/>
        public virtual async Task<X509Certificate2?> GetCertificate(StoreName storeName, StoreLocation location, X509FindType findType,
            string? findValue)
        {
            await Task.Yield();
            var matchingCerts = _certificateStore.GetCertificatesByValue(storeName, location, findType, findValue);

            if (matchingCerts == null || matchingCerts.Count == 0)
            {
                Logger.LogDebug($"No certificate found matching params StoreName {storeName} Location {location} Find Type {findType} Find Value {findValue}");
                return null;
            }

            var nonExpiredCertificates = matchingCerts.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
            if (nonExpiredCertificates.Count == 0)
            {
                Logger.LogWarning($"A certificate was found matching params StoreName {storeName} Location {location} Find Type {findType} Find Value {findValue} but is expired.");
                return null;
            }

            var orderedCerts = nonExpiredCertificates.Cast<X509Certificate2>().OrderByDescending(c => c.NotAfter)
                .ToList();
            var cert = orderedCerts.FirstOrDefault();

            //dispose of any certs we gathered that are not useful
            for (var i = 1; i < orderedCerts.Count; i++)
            {
                orderedCerts[i].Dispose();
            }

            return cert;
        }
    }
}
