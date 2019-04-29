#if NETSTANDARD
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;

namespace Meerkat.Caching
{
    /// <summary>
    /// Extension methods for <see cref="IDistributedCache"/>
    /// </summary>
    public static class DistributedCacheExtensions
    {
        private static readonly ISynchronizer synchronizer = new ConcurrentSynchronizer();

        /// <summary>
        /// Sets the value with the given key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static void Set<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
            where T : class
        {
            cache.Set(key, value.ToByteArray(), options);
        }

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
            // NB Wait for it to complete
            await cache.SetAsync(key, value.ToByteArray(), options, cancellationToken);
        }

        /// <summary>
        /// Gets a value with the given key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(this IDistributedCache cache, string key)
            where T : class
        {
            var result = cache.Get(key);
            return result?.FromByteArray<T>();
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
        /// <returns></returns>
        public static T GetOrCreate<T>(this IDistributedCache cache, string key, Func<T> factory, DistributedCacheEntryOptions options)
            where T : class
        {
            var value = cache.Get<T>(key);
            if (value != null)
            {
                return value;
            }

            value = factory();

            // TODO: Do we want to cache default values?
            cache.Set(key, value, options);

            return value;
        }

        /// <summary>
        /// Gets a string value with the given key if present, otherwise invoke the factory to create the value and store that in the cache.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> GetOrCreateAtomicAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> factory, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            // Ok, try outside the lock first
            var value = await cache.GetAsync<T>(key, cancellationToken);
            if (value != null)
            {
                // Found it so return
                return value;
            }

            var syncKey = cache.GetHashCode() + "." + key;

            // Now try again inside the semaphore
            return await synchronizer.Synchronize(syncKey, () => cache.GetOrCreateAsync(key, factory, options, cancellationToken), cancellationToken);
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

            // TODO: Do we want to wait on the cache write or just presume it succeeds.
            await cache.SetAsync(key, value, options, cancellationToken);

            return value;
        }

        /// <summary>
        /// Gets a string value with the given key if present, otherwise invoke the factory to create the value and store that in the cache.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GetOrCreateStringAtomic(this IDistributedCache cache, string key, Func<string> factory, DistributedCacheEntryOptions options)
        {
            // Ok, try outside the lock first
            var value = cache.GetString(key);
            if (value != null)
            {
                // Found it so return
                return value;
            }

            var syncKey = cache.GetHashCode() + "." + key;

            // Now try again inside the semaphore
            return synchronizer.Synchronize(syncKey, () => cache.GetOrCreateString(key, factory, options));
        }
        
        /// <summary>
        /// Gets a value with the given key if present, otherwise invoke the factory to create the value and store that in the cache.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GetOrCreateString(this IDistributedCache cache, string key, Func<string> factory, DistributedCacheEntryOptions options)
        {
            var value = cache.GetString(key);
            if (value != null)
            {
                return value;
            }

            value = factory();

            if (!string.IsNullOrEmpty(value))
            {
                cache.SetString(key, value, options);
            }

            return value;
        }

        /// <summary>
        /// Gets a string value with the given key if present, otherwise invoke the factory to create the value and store that in the cache.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> GetOrCreateStringAtomicAsync(this IDistributedCache cache, string key, Func<Task<string>> factory, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ok, try outside the lock first
            var value = await cache.GetStringAsync(key, cancellationToken);
            if (value != null)
            {
                // Found it so return
                return value;
            }

            var syncKey = cache.GetHashCode() + "." + key;

            // Now try again inside the semaphore
            return await synchronizer.Synchronize(syncKey, () => cache.GetOrCreateStringAsync(key, factory, options, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Gets a string value with the given key if present, otherwise invoke the factory to create the value and store that in the cache.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> GetOrCreateStringAsync(this IDistributedCache cache, string key, Func<Task<string>> factory, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var value = await cache.GetStringAsync(key, cancellationToken);
            if (value != null)
            {
                return value;
            }

            value = await factory();

            if (!string.IsNullOrEmpty(value))
            {
                // TODO: Do we want to wait on the cache write or just presume it succeeds.
                await cache.SetStringAsync(key, value, options, cancellationToken);
            }

            return value;
        }
    }
}
#endif