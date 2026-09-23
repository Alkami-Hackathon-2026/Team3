#if NETFRAMEWORK
using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Threading;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    internal partial class AlkamiClientFactoryCache
    {
        protected static Lazy<MemoryCache> LazyInstance = new Lazy<MemoryCache>(() => MemoryCache.Default);

    }
    internal partial class AlkamiClientFactoryCache<T> : AlkamiClientFactoryCache
    {
        private readonly MemoryCache _cache;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores;
        private readonly TimeSpan _cacheTimeout;

        //Have this specifically for tests
        internal Func<Uri, int, IAlkamiClientFactory<T>> GenerateFactory = DefaultGenerateFactory;
        internal static Func<Uri, int, IAlkamiClientFactory<T>> DefaultGenerateFactory { get; } = (uri, timeout) => { return new AlkamiClientFactory<T>(uri, timeout); };

        public AlkamiClientFactoryCache() : this(LazyInstance.Value)
        {

        }

        public AlkamiClientFactoryCache(MemoryCache cache)
        {
            _cache = cache;
            _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
            _cacheTimeout = TimeSpan.FromMinutes(GetCacheTimeout());
        }


        public IAlkamiClientFactory<T> GetOrCreateFactory(Uri endpointUri, int timeout)
        {
            var actualKey = GenerateKey(endpointUri.ToString(), timeout);
            if (_cache[actualKey] is IAlkamiClientFactory<T> client)
            {
                return client;
            }

            var locker = _semaphores.GetOrAdd(actualKey, new SemaphoreSlim(1, 1));

            locker.Wait();
            try
            {
                client = _cache[actualKey] as IAlkamiClientFactory<T>;
                if (client != null)
                {
                    return client;
                }

                client = GenerateFactory(endpointUri, timeout);

                var cacheExpirationOptions = new CacheItemPolicy
                {
                    SlidingExpiration = _cacheTimeout,
                    RemovedCallback = (x) =>
                    {
                        var test = x.RemovedReason;
                        (x.CacheItem.Value as IAlkamiClientFactory<T>)?.Dispose();
                    }
                };

                _cache.Set(actualKey, client, cacheExpirationOptions);
                return client;
            }
            finally
            {
                locker.Release();
            }
        }

    }
}
#endif
