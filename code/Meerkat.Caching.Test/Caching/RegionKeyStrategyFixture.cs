using Meerkat.Caching;

using NUnit.Framework;

namespace Meerkat.Test.Caching
{
    [TestFixture]
    public class RegionKeyStrategyFixture
    {
        [Test]
        public void NoRegion()
        {
            var strategy = new RegionKeyStrategy();

            var source = "foo";
            string region = null;
            var expected = source;

            var candidate = strategy.Key(source, region);

            Assert.That(expected, Is.EqualTo(candidate));
        }

        [Test]
        public void RegionKey()
        {
            var strategy = new RegionKeyStrategy();

            var source = "foo";
            var region = "customer";
            var expected = "customer::foo";

            var candidate = strategy.Key(source, region);

            Assert.That(expected, Is.EqualTo(candidate));
        }

        [Test]
        public void KeyRegion()
        {
            var strategy = new RegionKeyStrategy();
            strategy.Pattern = "{0}{1}{2}";

            var source = "foo";
            var region = "customer";
            var expected = "foo::customer";

            var candidate = strategy.Key(source, region);

            Assert.That(expected, Is.EqualTo(candidate));
        }
    }
}
