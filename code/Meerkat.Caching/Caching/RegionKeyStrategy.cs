namespace Meerkat.Caching
{
    /// <summary>
    /// Provides a strategy for faking a region key into a cache
    /// </summary>
    public class RegionKeyStrategy : IRegionKeyStrategy
    {
        /// <summary>
        /// Creates a new instance of the <see cref="RegionKeyStrategy"/> class.
        /// </summary>
        public RegionKeyStrategy()
        {
            Separator = "::";
            Pattern = "{2}{1}{0}";
        }

        /// <summary>
        /// Get or set the separator between the region and key (default ::)
        /// </summary>
        public string Separator { get; set; }

        /// <summary>
        /// Gets or sets the pattern to combine the key and the region (default region::key)
        /// </summary>
        public string Pattern { get; set; }

        /// <copydoc cref="IRegionKeyStrategy.Key" />
        public string Key(string key, string regionName)
        {
            return string.IsNullOrEmpty(regionName) ? key : string.Format(Pattern, key, Separator, regionName);
        }
    }
}