using System;

using Meerkat.Caching;

using NUnit.Framework;

namespace Meerkat.Test.Caching
{
    [TestFixture]
    public class CacheExtensionsFixture
    {
        [Test]
        public void LazyAdd()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo();

            var candidate = cache.AddOrGetExisting("la", () => expected, DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        public void LazyAddAlreadyExists()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo();
            var other = new Foo();

            cache["lae"] = expected;

            var candidate = cache.AddOrGetExisting("lae", () => other, DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        public void LazyAddThrowsOnException()
        {
            var cache = MemoryObjectCacheFactory.Default();

            Assert.Throws<NotSupportedException>(() => cache.AddOrGetExisting("bad", BadFunction, DateTimeOffset.UtcNow.AddSeconds(10)));
            Assert.That(cache["bad"], Is.Null, "Cache entry differs");
        }

        private Foo BadFunction()
        {
            throw new NotSupportedException();
        }

        private class Foo
        {
        }
    }
}