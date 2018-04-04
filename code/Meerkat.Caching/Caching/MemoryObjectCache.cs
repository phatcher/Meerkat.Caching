using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace Meerkat.Caching
{
    /// <summary>
    /// Simple <see cref="MemoryCache"/> wrapper for <see cref="ICache"/>
    /// </summary>
    public class MemoryObjectCache : ICache
    {
        private readonly MemoryCache cache;
        private readonly IRegionKeyStrategy keyStrategy;

        /// <summary>
        /// Creates a new instance of the <see cref="MemoryObjectCache"/> class.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="keyStrategy"></param>
        public MemoryObjectCache(MemoryCache cache, IRegionKeyStrategy keyStrategy)
        {
            this.cache = cache;
            this.keyStrategy = keyStrategy;
        }

        public object this[string key]
        {
            get { return cache[key]; }
            set
            {
                if (value == null)
                {
                    cache.Remove(key);
                }
                else
                {
                    cache[key] = value;
                }
            }
        }

        /// <copydoc cref="ICache.CacheMemoryLimit" />
        public long CacheMemoryLimit => cache.CacheMemoryLimit;

        /// <copydoc cref="ICache.Name" />
        public string Name => cache.Name;

        /// <copydoc cref="ICache.AddOrGetExisting" />
        public object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            // Just do a get if we are null as MemoryCache doesn't store nulls.
            if (value == null)
            {                
                return Get(key, regionName);
            }
            var regionKey = keyStrategy.Key(key, regionName);

            return cache.AddOrGetExisting(regionKey, value, absoluteExpiration);
        }

        /// <copydoc cref="ICache.Contains" />
        public bool Contains(string key, string regionName = null)
        {
            var regionKey = keyStrategy.Key(key, regionName);

            return cache.Contains(regionKey);
        }

        /// <copydoc cref="ICache.Get(string, string)" />

        public object Get(string key, string regionName = null)
        {
            var regionKey = keyStrategy.Key(key, regionName);

            return cache.Get(regionKey);
        }

        /// <copydoc cref="ICache.Get" />

        public long GetCount(string regionName = null)
        {
            // NB This is not region aware
            return cache.GetCount();
        }

        /// <copydoc cref="ICache.GetValues" />

        public IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            var regionKeys = keys.Select(key => keyStrategy.Key(key, regionName)).ToList();

            return cache.GetValues(regionKeys);
        }

        /// <copydoc cref="ICache.Remove" />

        public object Remove(string key, string regionName = null)
        {
            var regionKey = keyStrategy.Key(key, regionName);

            return cache.Remove(regionKey);
        }

        /// <copydoc cref="ICache.Set" />

        public void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            if (value == null)
            {
                return;
            }
            var regionKey = keyStrategy.Key(key, regionName);

            cache.Set(regionKey, value, absoluteExpiration);
        }
    }
}