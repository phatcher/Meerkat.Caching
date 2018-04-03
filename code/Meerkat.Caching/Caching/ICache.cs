using System;
using System.Collections.Generic;

namespace Meerkat.Caching
{
    /// <summary>
    /// Abstraction over an object cache.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Gets or sets a value in the cache by using the default indexer property for an instance of the <see cref="ICache"/> class.
        /// </summary>
        /// <param name="key">A unique identifier for the cache value to get or set.</param>
        /// <returns>The value in the cache instance for the specified key, if the entry exists; otherwise, null.</returns>
        object this[string key] { get; set; }

        /// <summary>
        /// Gets the amount of memory on the computer, in bytes, that can be used by the cache.
        /// </summary>
        /// <returns>The amount of memory in bytes.</returns>
        long CacheMemoryLimit { get; }

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        /// <returns>The name of the cache.</returns>
        string Name { get; }

        /// <summary>
        /// Adds a cache entry into the cache using the specified key and a value and an absolute expiration value.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to add.</param>
        /// <param name="value">The data for the cache entry.</param>
        /// <param name="absoluteExpiration">The fixed date and time at which the cache entry will expire.</param>
        /// <param name="regionName">A named region in the cache to which a cache entry can be added.</param>
        /// <returns>If a cache entry with the same key exists, the existing cache entry; otherwise, null</returns>
        object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null);

        /// <summary>
        /// Determines whether a cache entry exists in the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to search for.</param>
        /// <param name="regionName">A named region in the cache to which a cache entry was added</param>
        /// <returns>true if the cache contains a cache entry whose key matches key; otherwise, false.</returns>
        bool Contains(string key, string regionName = null);

        /// <summary>
        /// Returns an entry from the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to get.</param>
        /// <param name="regionName">A named region in the cache to which a cache entry was added.</param>
        /// <returns>A reference to the cache entry that is identified by key, if the entry exists; otherwise null</returns>
        object Get(string key, string regionName = null);

        /// <summary>
        /// Returns the total number of cache entries in the cache.
        /// </summary>
        /// <param name="regionName">A named region in the cache to which a cache entry was added.</param>
        /// <returns>The number of entries in the cache.</returns>
        long GetCount(string regionName = null);

        /// <summary>
        /// Returns a set of cache entries that correspond to the specified keys.
        /// </summary>
        /// <param name="keys">A set of unique identifiers for the cache entries to return.</param>
        /// <param name="regionName">A named region in the cache to which a cache entry was added. Do not pass a value</param>
        /// <returns></returns>
        IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null);

        /// <summary>
        /// Removes a cache entry from the cache.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry to remove.</param>
        /// <param name="regionName">A named region in the cache to which a cache entry was added</param>
        /// <returns>If the entry is found in the cache, the removed cache entry; otherwise, null.</returns>
        object Remove(string key, string regionName = null);

        /// <summary>
        /// Inserts a cache entry into the cache by using a key and a value and specifies
        /// time-based expiration details.
        /// </summary>
        /// <param name="key"> A unique identifier for the cache entry to insert.</param>
        /// <param name="value">The data for the cache entry.</param>
        /// <param name="absoluteExpiration">The fixed date and time at which the cache entry will expire.</param>
        /// <param name="regionName">A named region in the cache to which a cache entry can be added</param>
        void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null);
    }
}
