using System;
using System.Threading.Tasks;

namespace Meerkat.Caching
{
    /// <summary>
    /// Extension methods for <see cref="ICache"/>
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        /// Lazy strongly-typed version of AddOrGetExisting which only invokes the function if the value is not present, 
        /// and returns either the cache value or the newly created value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="creator"></param>
        /// <param name="absoluteExpiration"></param>
        /// <param name="regionName"></param>
        /// <returns>Returns either the value that exists or the value returns from the creator function</returns>
        public static T AddOrGetExisting<T>(this ICache cache, string key, Func<T> creator, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            T value;
            if (cache.Contains(key, regionName))
            {
                value = (T)cache.Get(key, regionName);
            }
            else
            {
                value = creator();
                cache.Set(key, value, absoluteExpiration, regionName);
            }
            return value;
        }

        /// <summary>
        /// Asynchronous lazy strongly-typed version of AddOrGetExisting which only invokes the function if the value is not present, 
        /// and returns either the cache value or the newly created value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="creator"></param>
        /// <param name="absoluteExpiration"></param>
        /// <param name="regionName"></param>
        /// <returns>Returns either the value that exists or the value returns from the creator function</returns>
        public static async Task<T> AddOrGetExistingAsync<T>(this ICache cache, string key, Func<Task<T>> creator, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            T value;
            if (cache.Contains(key, regionName))
            {
                value = (T)cache.Get(key, regionName);
            }
            else
            {
                value = await creator().ConfigureAwait(false);
                cache.Set(key, value, absoluteExpiration, regionName);
            }
            return value;
        }
    }
}
