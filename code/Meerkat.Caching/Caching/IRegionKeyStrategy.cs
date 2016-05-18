namespace Meerkat.Caching
{
    /// <summary>
    /// Strategy for determining the key when using cache regions
    /// </summary>
    public interface IRegionKeyStrategy
    {
        /// <summary>
        /// Determine the key when using cache regions
        /// </summary>
        /// <param name="key">Key to use</param>
        /// <param name="regionName">Region to use</param>
        /// <returns>Key to use in a non-region aware cache</returns>
        string Key(string key, string regionName);
    }
}