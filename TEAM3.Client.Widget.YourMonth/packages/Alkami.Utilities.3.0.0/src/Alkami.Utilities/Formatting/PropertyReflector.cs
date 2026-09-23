using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using Common.Logging;

namespace Alkami.Utilities.Formatting
{
    /// <summary>
    /// Responsible for enumerating and caching discovered reflection information for types
    /// </summary>
    public class PropertyReflector
    {
        private static MemoryCache _cache = MemoryCache.Default;
        private static ILog _logger = LogManager.GetLogger<PropertyReflector>();
        private static IEnumerable<PropertyInfo> EMPTY_SET = Enumerable.Empty<PropertyInfo>();

        /// <summary>
        /// This discovers eligible property information from reflection for the given object
        /// </summary>
        public virtual IEnumerable<PropertyInfo> DiscoverEligiblePropertiesFrom(object sourceData)
        {
            if (sourceData == null)
                return EMPTY_SET;

            return DiscoverEligiblePropertiesFrom(sourceData.GetType());
        }

        /// <summary>
        /// This discovered eligible property information from reflection for the given source type
        /// </summary>
        public virtual IEnumerable<PropertyInfo> DiscoverEligiblePropertiesFrom(Type sourceType)
        {
            IEnumerable<PropertyInfo> discoveredProperties = null;
            var cacheKey = CreateCacheKeyFor(sourceType);

            discoveredProperties = _cache.Get(cacheKey) as IEnumerable<PropertyInfo>;

            if (discoveredProperties != null && discoveredProperties.Any())
                return discoveredProperties;

            try
            {
                discoveredProperties = sourceType.GetProperties()
                    .Where(property => property.GetGetMethod() != null)
                    .ToList();

                var cacheItem = new CacheItem(cacheKey, discoveredProperties);
                var cacheItemPolicy = new CacheItemPolicy
                {
                    SlidingExpiration = TimeSpan.FromHours(1)
                };

                _cache.Set(cacheItem, cacheItemPolicy);

                return discoveredProperties;
            }
            catch (Exception ex)
            {
                if (_logger.IsDebugEnabled)
                    _logger.DebugFormat("An unexpected error occurred discovering eligible properties for type '{0}'", ex, sourceType.AssemblyQualifiedName);

                var cacheItem = new CacheItem(cacheKey, EMPTY_SET);
                var cacheItemPolicy = new CacheItemPolicy
                {
                    SlidingExpiration = TimeSpan.FromHours(1)
                };

                return EMPTY_SET;
            }
        }

        private static string CreateCacheKeyFor(Type type)
        {
            return "C:ReflectedPropertiesFor:" + type.AssemblyQualifiedName;
        }
    }
}
