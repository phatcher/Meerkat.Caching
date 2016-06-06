using System;

namespace Meerkat.Caching
{
    public static class CacheExtensions
    {
        /// <summary>
        /// Lazy strongly-typed version of AddOrGetExisting which only invokes the function if the value is not present.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="creator"></param>
        /// <param name="absoluteExpiration"></param>
        /// <param name="regionName"></param>
        /// <returns></returns>
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
    }
}
