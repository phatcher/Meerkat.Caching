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

        public MemoryObjectCache(MemoryCache cache, IRegionKeyStrategy keyStrategy)
        {
            this.cache = cache;
            this.keyStrategy = keyStrategy;
        }

        public object this[string key]
        {
            get { return cache[key]; }
            set { cache[key] = value; }
        }

        public long CacheMemoryLimit
        {
            get { return cache.CacheMemoryLimit; }
        }

        public string Name
        {
            get { return cache.Name; }
        }

        public object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            var regionKey = keyStrategy.Key(key, regionName);

            return cache.AddOrGetExisting(regionKey, value, absoluteExpiration);
        }

        public bool Contains(string key, string regionName = null)
        {
            var regionKey = keyStrategy.Key(key, regionName);

            return cache.Contains(regionKey);
        }

        public object Get(string key, string regionName = null)
        {
            var regionKey = keyStrategy.Key(key, regionName);

            return cache.Get(regionKey);
        }

        public long GetCount(string regionName = null)
        {
            // NB This is not region aware
            return cache.GetCount();
        }

        public IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            var regionKeys = keys.Select(key => keyStrategy.Key(key, regionName)).ToList();

            return cache.GetValues(regionKeys);
        }

        public object Remove(string key, string regionName = null)
        {
            var regionKey = keyStrategy.Key(key, regionName);

            return cache.Remove(regionKey);
        }

        public void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            var regionKey = keyStrategy.Key(key, regionName);

            cache.Set(regionKey, value, absoluteExpiration);
        }
    }
}