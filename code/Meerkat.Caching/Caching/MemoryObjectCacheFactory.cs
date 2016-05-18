using System.Runtime.Caching;

namespace Meerkat.Caching
{
    public static class MemoryObjectCacheFactory
    {
        public static MemoryObjectCache Default()
        {
            var strategy = new RegionKeyStrategy();
            var cache = MemoryCache.Default;

            return new MemoryObjectCache(cache, strategy);
        }

        public static ICache Create(string name)
        {
            var strategy = new RegionKeyStrategy();
            var cache = new MemoryCache(name);

            return new MemoryObjectCache(cache, strategy);
        }
    }
}