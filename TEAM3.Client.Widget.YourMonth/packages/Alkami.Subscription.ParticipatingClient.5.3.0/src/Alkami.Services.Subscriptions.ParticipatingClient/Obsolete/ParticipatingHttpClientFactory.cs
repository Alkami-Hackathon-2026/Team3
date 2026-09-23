using System;
using System.Net.Http;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    /// Based this off of https://github.com/dotnet/aspnetcore/issues/28385#issuecomment-853766480
    /// </summary>
    public sealed class ParticipatingHttpClientFactory : IParticipatingHttpClientFactory
    {
        private static readonly Lazy<IParticipatingHttpClientFactory> lazy = new Lazy<IParticipatingHttpClientFactory>(() => new ParticipatingHttpClientFactory());

        /// <summary>
        /// Get singleton instance of <see cref="ParticipatingHttpClientFactory"/>
        /// </summary>
        public static IParticipatingHttpClientFactory Instance { get { return lazy.Value; } }

        internal readonly TimeSpan DefaultConnectionLifetime = TimeSpan.FromMinutes(15);
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Constructor
        /// </summary>
        internal ParticipatingHttpClientFactory()
        {
#if NET5_0_OR_GREATER
            var httpClientHandler = new SocketsHttpHandler()
            {
                PooledConnectionLifetime = DefaultConnectionLifetime
            };
#else
            var httpClientHandler = new ServicePointHttpMessageHandler(DefaultConnectionLifetime, new HttpClientHandler());
#endif
            _httpClient = new HttpClient(httpClientHandler, disposeHandler: false);
        }

        /// <inheritdoc />
        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }
    }
}
