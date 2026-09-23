using System.Net.Http;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    /// Wrapper interface for getting HttpClient's for ParticipatingClient
    /// </summary>
    public interface IParticipatingHttpClientFactory
    {
        /// <summary>
        /// Gets the HttpClient for ParticipatingClient
        /// </summary>
        /// <returns></returns>
        HttpClient GetHttpClient();
    }
}
