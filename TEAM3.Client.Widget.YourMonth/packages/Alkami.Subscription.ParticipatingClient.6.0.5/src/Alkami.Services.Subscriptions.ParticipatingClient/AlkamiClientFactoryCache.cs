using System;
using Alkami.Utilities.Configuration;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
    internal partial class AlkamiClientFactoryCache
    {

        internal const string MemoryCacheTTLConfigName = "ParticipatingClientMemoryCacheTTL";
        internal const string MemoryCacheTTLEnvironmentVariable = "ALKAMI_PARTICIPATING_CLIENT_MEMORY_CACHE_TTL";

        internal static int GetCacheTimeout()
        {
            var ttl = Manager.GetSetting(MemoryCacheTTLEnvironmentVariable);

            if (string.IsNullOrWhiteSpace(ttl))
            {
                ttl = Manager.GetSetting(MemoryCacheTTLConfigName);
            }

            if (!string.IsNullOrWhiteSpace(ttl) && int.TryParse(ttl, out var parsed))
            {
                return parsed;
            }

            return 15;
        }

    }


    internal sealed partial class AlkamiClientFactoryCache<T> : AlkamiClientFactoryCache, IAlkamiClientFactoryCache<T> where T: class
    {
        private string GenerateKey(string name, int timeout)
        {
            return $"AlkamiClientFactoryCache:{name}:{timeout}:{typeof(T).AssemblyQualifiedName}";
        }

    }
}

