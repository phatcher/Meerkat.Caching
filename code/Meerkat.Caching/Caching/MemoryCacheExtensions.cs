#if NETSTANDARD
using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

namespace Meerkat.Caching
{
    /// <summary>
    /// Extension methods for <see cref="IMemoryCache"/>
    /// </summary>
    public static class MemoryCacheExtensions
    {
        private static readonly ISynchronizer synchronizer = new ConcurrentSynchronizer();

        /// <summary>
        /// Gets a value with the given key if present, otherwise invoke the factory to create the value and store that in the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static T GetOrCreateAtomic<T>(this IMemoryCache cache, string key, Func<ICacheEntry, T> factory)
        {
            // Ok, try outside the lock first
            if (cache.TryGetValue(key, out T value))
            {
                // Found it
                return value;
            }

            var syncKey = cache.GetHashCode() + "." + key;

            // Try again inside the synchronizer
            return synchronizer.Synchronize(syncKey, () => cache.GetOrCreate(key, factory));
        }

        /// <summary>
        /// Gets a value with the given key if present, otherwise invoke the factory to create the value and store that in the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static Task<T> GetOrCreateAtomicAsync<T>(this IMemoryCache cache, string key, Func<ICacheEntry, Task<T>> factory)
        {
            // Ok, try outside the lock first
            if (cache.TryGetValue(key, out T value))
            {
                // Found it
                return Task.FromResult(value);
            }

            var syncKey = cache.GetHashCode() + "." + key;

            // Try again inside the synchronizer
            return synchronizer.Synchronize(syncKey, () => cache.GetOrCreateAsync(key, factory));
        }
    }
}
#endif