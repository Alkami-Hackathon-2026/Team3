using System;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    /// <summary>
    /// Defines the interface that can be used to create Clients to Alkami endpoints
    /// </summary>
    /// <typeparam name="TChannel">The type of channel that the client factory creates.</typeparam>
    public interface IAlkamiClientFactory<TChannel> : IDisposable where TChannel : class
    {

        /// <summary>
        /// Gets a client from the object pool
        /// </summary>
        /// <returns></returns>
        AlkamiCachedClient<TChannel> GetClient();

        /// <summary>
        /// Release the client back to the object pool
        /// </summary>
        /// <param name="client"></param>
        void Release(AlkamiCachedClient<TChannel> client);
    }
}
