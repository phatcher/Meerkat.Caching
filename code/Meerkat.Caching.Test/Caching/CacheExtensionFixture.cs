using System;

using Meerkat.Caching;

using NUnit.Framework;

namespace Meerkat.Test.Caching
{
#if NETFRAMEWORK
    [TestFixture]
    public class CacheExtensionFixture
    {
        private int count;

        [Test]
        public void CreateViaAddOrGet()
        {
            count = 10;
            var cache = MemoryObjectCacheFactory.Default();

            var candidate = cache.AddOrGetExisting("A", Invoke(), DateTimeOffset.UtcNow.AddSeconds(10));
            Assert.That(candidate, Is.Null, "Cache value differs");
            Assert.That(count, Is.EqualTo(11), "Count differs");
        }

        [Test]
        public void ExistingViaAddOrGet()
        {
            count = 10;
            var cache = MemoryObjectCacheFactory.Default();

            cache["B"] = 5;

            var candidate = cache.AddOrGetExisting("B", Invoke(), DateTimeOffset.UtcNow.AddSeconds(10));
            Assert.That(candidate, Is.EqualTo(5), "Cache value differs");
            Assert.That(count, Is.EqualTo(11), "Count differs");
        }

        [Test]
        public void CreateViaLazyAddOrGet()
        {
            count = 10;
            var cache = MemoryObjectCacheFactory.Default();

            var candidate = cache.AddOrGetExisting("C", Invoke, DateTimeOffset.UtcNow.AddSeconds(10));
            Assert.That(candidate, Is.EqualTo(11), "Cache value differs");
            Assert.That(count, Is.EqualTo(11), "Count differs");
        }

        [Test]
        public void ExistingViaLazyAddOrGet()
        {
            count = 10;
            var cache = MemoryObjectCacheFactory.Default();

            cache["D"] = 5;

            var candidate = cache.AddOrGetExisting("D", Invoke, DateTimeOffset.UtcNow.AddSeconds(10));
            Assert.That(candidate, Is.EqualTo(5), "Cache value differs");
            Assert.That(count, Is.EqualTo(10), "Count differs");
        }

        private int Invoke()
        {
            count++;
            return count;
        }
    }
#endif
}
