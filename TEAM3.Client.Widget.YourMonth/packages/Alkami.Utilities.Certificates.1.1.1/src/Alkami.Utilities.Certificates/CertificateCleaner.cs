using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;

namespace Alkami.Utilities.Certificates
{
    [ExcludeFromCodeCoverage]
    internal class CertificateCleaner : CriticalFinalizerObject
    {
        private X509Certificate2? _certificate;
        private static readonly ConditionalWeakTable<X509Certificate2, CertificateCleaner> _associateLifetimes = new ConditionalWeakTable<X509Certificate2, CertificateCleaner>();
        public static void RegisterForDisposalDuringFinalization(X509Certificate2? cert)
        {
            if (cert == null)
                return;
            
            var cleaner = _associateLifetimes.GetOrCreateValue(cert);
            cleaner._certificate = cert;
        }
        ~CertificateCleaner() => _certificate?.Reset();
    }
}
