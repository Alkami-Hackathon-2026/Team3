using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Alkami.Utilities.Certificates;
using Alkami.Utilities.Certificates.RetrievalStrategy;
#if NET6_0_OR_GREATER
using Alkami.Utilities.Kubernetes;
#endif

namespace Alkami.Utilities.Cryptography
{
    /// <summary>
    /// Utility class used to find certificates in the computer's cert store.
    /// </summary>
    public static class CertificateUtility
    {
        internal const string DefaultCertName = "Alkami Mutual Client";
        internal const string CertificateServiceEnabledEnvVar = "ALKAMI_CERTIFICATE_SERVICE_ENABLED";

        internal static Lazy<ICertificateUtility> InternalCertificateUtility =
            new Lazy<ICertificateUtility>(() =>
            {
                var certificateRetrievalStrat = CertificateStrategyFactory();
                return new Certificates.CertificateUtility(certificateRetrievalStrat);
            });

        //Exposed for testing
        internal static ICertificateRetrievalStrategy CertificateStrategyFactory()
        {
            var certificateStore = new CertificateStore();
            ICertificateRetrievalStrategy certificateRetrievalStrat = null;
#if NET6_0_OR_GREATER
            //Checking whether we're in kubernetes and that we have the cert service url set, this is to accomodate backwards compatibility
            var eligibleForK8Strategy = ServiceUrlSettings.IsRunningInKubernetes() &&
                                        bool.TryParse(Environment.GetEnvironmentVariable(CertificateServiceEnabledEnvVar), out var enabled) && enabled;

            certificateRetrievalStrat = eligibleForK8Strategy
                ? new KubernetesCertificateStrategy()
                : new LocalCertificateStrategy(certificateStore);
#else
            certificateRetrievalStrat = new LocalCertificateStrategy(certificateStore);
#endif
            return certificateRetrievalStrat;
        }


        /// <summary>
        /// Finds a certificate by the specified criteria.  If the certificate cannot be found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">Store name to search.</param>
        /// <param name="location">Store location to search.</param>
        /// <param name="findType">Type of criteria by which to search.</param>
        /// <param name="findValue">Criteria to which to search.</param>
        /// <returns>An instance of the certificate, otherwise throws <see cref="InvalidOperationException"/>.</returns>
        /// <exception cref="InvalidOperationException">If a non-expired cert cannot be found, an <see cref="InvalidOperationException"/> is thrown.</exception>
        public static X509Certificate2 GetCertificate(
            StoreName name,
            StoreLocation location,
            X509FindType findType,
            object findValue)
        {
            return GetCertificate(name, location, findType, findValue as string);
        }

        /// <summary>
        /// Finds a certificate by the specified criteria.  If the certificate cannot be found, a null is returned.
        /// </summary>
        /// <param name="name">Store name to search.</param>
        /// <param name="location">Store location to search.</param>
        /// <param name="findType">Type of criteria by which to search.</param>
        /// <param name="findValue">Criteria to which to search.</param>
        /// <returns>An instance of the certificate, otherwise null.</returns>
        public static X509Certificate2 GetCertificateIfPresent(
            StoreName name,
            StoreLocation location,
            X509FindType findType,
            object findValue)
        {
            return GetCertificateIfPresent(name, location, findType, findValue as string);
        }

        /// <summary>
        /// Finds a certificate by the specified criteria.  If the certificate cannot be found, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="name">Store name to search.</param>
        /// <param name="location">Store location to search.</param>
        /// <param name="findType">Type of criteria by which to search.</param>
        /// <param name="findValue">Criteria to which to search.</param>
        /// <returns>An instance of the certificate, otherwise throws <see cref="InvalidOperationException"/>.</returns>
        /// <exception cref="InvalidOperationException">If a non-expired cert cannot be found, an <see cref="InvalidOperationException"/> is thrown.</exception>
        public static X509Certificate2 GetCertificate(
            StoreName name,
            StoreLocation location,
            X509FindType findType,
            string findValue)
        {
            var cert= GetCertificateIfPresent(name, location, findType, findValue);
            if (cert == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.NoCertificateFound, location, name, findType, findValue));
            }

            return cert;
        }

        /// <summary>
        /// Finds a certificate by the specified criteria.  If the certificate cannot be found, a null is returned.
        /// </summary>
        /// <param name="name">Store name to search.</param>
        /// <param name="location">Store location to search.</param>
        /// <param name="findType">Type of criteria by which to search.</param>
        /// <param name="findValue">Criteria to which to search.</param>
        /// <returns>An instance of the certificate, otherwise null.</returns>
        public static X509Certificate2 GetCertificateIfPresent(
            StoreName name,
            StoreLocation location,
            X509FindType findType,
            string findValue)
        {
            ValidateArguments(findType, findValue);

            var standardFindValue = StandardizeFindValue(findType, findValue);

            return AsyncTaskHelper.RunSync(() => InternalCertificateUtility.Value.FindCertificate(name, location, findType, standardFindValue));
        }

        private static void ValidateArguments(
            X509FindType findType,
            object findValue)
        {
            if (findValue == null && findType != X509FindType.FindBySubjectName)
            {
                throw new ArgumentNullException("findValue");
            }
        }

        private static string StandardizeFindValue
            (X509FindType findType,
            string findValue)
        {
            if (findType == X509FindType.FindBySubjectName)
            {
                // Default the subject to the most commonly requested system cert and in which the caller may not know the identity of.
                return string.IsNullOrWhiteSpace(findValue) ? DefaultCertName : findValue;
            }
            else if (findType == X509FindType.FindByThumbprint)
            {
                // When copying values out of MMC directly, you can get a non-printing Unicode character.
                // This logic is here to ensure the non-printing character turns to a '?', all extra spaces are removed,
                // and the characters are all upper-cased.
                return Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(Convert.ToString(findValue)))
                    .Replace(" ", string.Empty)
                    .ToUpperInvariant();
            }

            return findValue;
        }
    }
}
