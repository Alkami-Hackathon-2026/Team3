#if NET6_0_OR_GREATER
using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    internal partial class AlkamiClientFactoryCache
    {
        protected static Lazy<IMemoryCache> LazyInstance = new(() => new MemoryCache(new MemoryCacheOptions()));

    }

    internal partial class AlkamiClientFactoryCache<T> : AlkamiClientFactoryCache
    {
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores;
        private readonly TimeSpan _cacheTimeout;
        
        //Have this specifically for testsd
        internal Func<Uri, int, IAlkamiClientFactory<T>> GenerateFactory = DefaultGenerateFactory;
        internal static Func<Uri, int, IAlkamiClientFactory<T>> DefaultGenerateFactory { get; } = (uri, timeout) => { return new AlkamiClientFactory<T>(uri, timeout); };


        public AlkamiClientFactoryCache(): this(LazyInstance.Value)
        {

        }

        public AlkamiClientFactoryCache(IMemoryCache cache)
        {
            _cache = cache;
            _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
            _cacheTimeout = TimeSpan.FromMinutes(GetCacheTimeout());
        }

        public IAlkamiClientFactory<T> GetOrCreateFactory(Uri endpointUri, int timeout)
        {
            var actualKey = GenerateKey(endpointUri.ToString(), timeout);

            if (_cache.TryGetValue(actualKey, out IAlkamiClientFactory<T> client)) {
                return client;
            }

            var locker = _semaphores.GetOrAdd(actualKey, new SemaphoreSlim(1, 1));

            locker.Wait();
            try
            {
                if (_cache.TryGetValue(actualKey, out client))
                {
                    return client;
                }

                client = GenerateFactory(endpointUri, timeout);

                var options = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(_cacheTimeout)
                    .RegisterPostEvictionCallback(PostEvictionCallback)
                    ;

                _cache.Set(actualKey, client, options);
                return client;
            }
            finally
            {
                locker.Release();
            }
        }

        private void PostEvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            var client = value as IAlkamiClientFactory<T>;
            client?.Dispose();
        }
    }
}

#endif
