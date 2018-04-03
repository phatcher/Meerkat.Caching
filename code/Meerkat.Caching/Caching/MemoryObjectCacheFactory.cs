#if NET45
using System.Runtime.Caching;

namespace Meerkat.Caching
{
    /// <summary>
    /// Factory for creating <see cref="MemoryObjectCache"/> with a region support
    /// </summary>
    public static class MemoryObjectCacheFactory
    {
        /// <summary>
        /// Get a reference to the default <see cref="MemoryObjectCache"/> instance.
        /// </summary>
        /// <returns></returns>
        public static MemoryObjectCache Default()
        {
            var strategy = new RegionKeyStrategy();
            var cache = MemoryCache.Default;

            return new MemoryObjectCache(cache, strategy);
        }

        /// <summary>
        /// Create a new instance of the <see cref="MemoryObjectCache"/>.
        /// </summary>
        /// <param name="name">Name to use</param>
        /// <returns></returns>
        public static ICache Create(string name)
        {
            var strategy = new RegionKeyStrategy();
            var cache = new MemoryCache(name);

            return new MemoryObjectCache(cache, strategy);
        }
    }
}
#endif