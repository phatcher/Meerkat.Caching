using System;
using System.Configuration;

namespace Meerkat.Caching.Configuration
{
    /// <summary>
    /// Provides configuration settings for caches
    /// </summary>
    public static class CacheConfiguration
    {
        /// <summary>
        /// Gets an expiry duration for a key
        /// </summary>
        /// <param name="key">Cache name to use</param>
        /// <param name="duration">Default value to use if not set</param>
        /// <returns>Assigned cache duration or default value if not set</returns>
        /// <remarks>Looks in AppSettings for Cache.Duration.[key] and Cache.Duration if specific key is not present</remarks>
        public static TimeSpan? Duration(string key, TimeSpan? duration = null)
        {
            // Attempt to get the specific key then the base one.
            bool usedDefault = false;
            var value = DurationSetting(key);
            if (string.IsNullOrEmpty(value))
            {
                usedDefault = true;
                value = DurationSetting(null);
            }

            // Ok, do we have a default value or we can't parse the acquired result
            // Convert into integer or use the default if that fails.
            int m;
            if ((usedDefault && duration != null) || !int.TryParse(value, out m))
            {
                return duration;
            }

            // Now see what units we want
            switch (ConfigurationManager.AppSettings["Cache.Duration.Units"].ToLowerInvariant())
            {
                case "seconds":
                    return TimeSpan.FromMinutes(m);

                default:
                    return TimeSpan.FromMinutes(m);
            }
        }

        private static string DurationSetting(string key)
        {
            const string baseKey = "Cache.Duration";

            key = string.IsNullOrEmpty(key) ? baseKey : baseKey + "." + key;
            return ConfigurationManager.AppSettings[key];
        }
    }
}