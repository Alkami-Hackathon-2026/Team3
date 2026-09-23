#if NET6_0_OR_GREATER

using Alkami.Utilities.Kubernetes;
using Alkami.Utilities.Rpc.Extensions;
using System;
using System.Net.Http;

namespace Alkami.Utilities.Certificates.Http
{
    /// <summary>
    /// Based this off of https://github.com/dotnet/aspnetcore/issues/28385#issuecomment-853766480
    /// </summary>
    [Obsolete("Please utilize ICertificateUtility for retrieving certificates.")]
    public sealed class CertificateHttpClientFactory : ICertificateHttpClientFactory
    {
        private static readonly Lazy<ICertificateHttpClientFactory> lazy = new Lazy<ICertificateHttpClientFactory>(() => new CertificateHttpClientFactory());

        /// <summary>
        /// Get singleton instance of <see cref="ICertificateHttpClientFactory"/>
        /// </summary>
        public static ICertificateHttpClientFactory Instance => lazy.Value;

        internal readonly TimeSpan DefaultConnectionLifetime = TimeSpan.FromMinutes(15);
        private readonly HttpClient _httpClient;


        /// <summary>
        /// Builds out the service url to connect to a service in kubernetes based on if the current process is running in kubernetes or not
        /// </summary>
        internal static Func<string, int, string> GenerateServiceUrl = (servicePath, majorVersion) => ServiceUrlSettings.GenerateServiceUrl(ContractTypeExtensions.BuildRouteName(servicePath, majorVersion, RouteNameServiceType.Rest));

        /// <summary>
        /// Constructor
        /// </summary>
        internal CertificateHttpClientFactory()
        {
            var endpoint = GenerateServiceUrl("certificate", 1);

            var httpClientHandler = new SocketsHttpHandler()
            {
                PooledConnectionLifetime = DefaultConnectionLifetime
            };

            _httpClient = new HttpClient(httpClientHandler, disposeHandler: false);
            _httpClient.BaseAddress = new Uri(endpoint);
        }

        /// <inheritdoc />
        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }
    }
}

#endif
