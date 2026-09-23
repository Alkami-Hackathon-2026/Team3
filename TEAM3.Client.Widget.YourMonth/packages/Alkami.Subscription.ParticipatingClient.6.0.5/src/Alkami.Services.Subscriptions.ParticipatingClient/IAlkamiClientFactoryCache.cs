using System;
using System.Threading.Tasks;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    /// This will be used to create the Alkami Client Factory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAlkamiClientFactoryCache<T> where T : class
    {

        /// <summary>
        /// Gets or creates the Client factory for the endpoint and timeout sent
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <returns></returns>
        IAlkamiClientFactory<T> GetOrCreateFactory(Uri endpointUri, int timeoutInSeconds);

    }
}
