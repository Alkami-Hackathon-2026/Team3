#if NET6_0_OR_GREATER
using System;
using System.Net.Http;

namespace Alkami.Utilities.Certificates.Http
{
    /// <summary>
    /// Wrapper interface for getting HttpClients for Certificates
    /// </summary>
    [Obsolete("Please utilize ICertificateUtility for retrieving certificates.")]
    public interface ICertificateHttpClientFactory
    {
        /// <summary>
        /// Gets the HttpClient for the Certificate Service
        /// </summary>
        /// <returns></returns>
        HttpClient GetHttpClient();
    }
}
#endif