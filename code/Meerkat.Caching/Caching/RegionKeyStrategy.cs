namespace Meerkat.Caching
{
    public class RegionKeyStrategy : IRegionKeyStrategy
    {
        public RegionKeyStrategy()
        {
            Separator = "::";
            Pattern = "{2}{1}{0}";
        }

        public string Separator { get; set; }

        public string Pattern { get; set; }

        public string Key(string key, string regionName)
        {
            return string.IsNullOrEmpty(regionName) ? key : string.Format(Pattern, key, Separator, regionName);
        }
    }
}