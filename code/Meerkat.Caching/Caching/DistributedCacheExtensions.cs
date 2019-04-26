#if NETSTANDARD
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;

namespace Meerkat.Caching
{
    public static class DistributedCacheExtensions
    {
        /// <summary>
        /// Sets the value with the given key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            await cache.SetAsync(key, value.ToByteArray(), options, cancellationToken);
        }

        /// <summary>
        /// Gets a value with the given key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            var result = await cache.GetAsync(key, cancellationToken);
            return result?.FromByteArray<T>();
        }

        /// <summary>
        /// Gets a value with the given key if present, otherwise invoke the factory to create the value and store that in the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> GetOrCreateAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> factory, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            var value = await cache.GetAsync<T>(key, cancellationToken);
            if (value != null)
            {
                return value;
            }

            value = await factory();

            // TODO: Do we want to cache default values?
            // TODO: Do we want to wait on the cache write or just presume it succeeds.
            await cache.SetAsync(key, value, options, cancellationToken);

            return value;
        }
    }
}
#endif