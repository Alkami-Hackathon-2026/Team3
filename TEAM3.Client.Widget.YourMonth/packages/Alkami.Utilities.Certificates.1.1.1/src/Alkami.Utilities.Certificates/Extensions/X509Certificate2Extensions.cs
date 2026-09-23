using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Alkami.Utilities.Certificates.Extensions
{
    internal static class X509Certificate2Extensions
    {
        internal static bool IsActive(this X509Certificate2? cert)
        {
            if (cert == null)
                return false;

            return DateTime.Now >= cert.NotBefore && DateTime.Now <= cert.NotAfter;
        }
    }
}
