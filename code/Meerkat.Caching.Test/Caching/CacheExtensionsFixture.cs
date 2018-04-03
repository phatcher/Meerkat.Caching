using System;
using System.Threading.Tasks;
using Meerkat.Caching;

using NUnit.Framework;

namespace Meerkat.Test.Caching
{
#if NET45
    [TestFixture]
    public class CacheExtensionsFixture
    {
        [Test]
        public void LazyAdd()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };

            var candidate = cache.AddOrGetExisting("la", () => expected, DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        [Ignore("Not reliable when run in test batch")]
        public async Task LazyAddAsync()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };

            var candidate = await cache.AddOrGetExistingAsync("la", async () => await FooFactory(expected), DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        public void LazyAddAlreadyExists()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };
            var other = new Foo { Name = "B" };

            cache["lae"] = expected;

            var candidate = cache.AddOrGetExisting("lae", () => other, DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        public async Task LazyAddAlreadyExistsAsync()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };
            var other = new Foo { Name = "B" };

            cache["lae"] = expected;

            var candidate = await cache.AddOrGetExistingAsync("lae", async () => await FooFactory(other), DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        public void LazyAddThrowsOnException()
        {
            var cache = MemoryObjectCacheFactory.Default();

            Assert.Throws<NotSupportedException>(() => cache.AddOrGetExisting("bad", BadFunction, DateTimeOffset.UtcNow.AddSeconds(10)));
            Assert.That(cache["bad"], Is.Null, "Cache entry differs");
        }

        [Test]
        public void LazyAddThrowsOnExceptionAsync()
        {
            var cache = MemoryObjectCacheFactory.Default();

            Assert.ThrowsAsync<NotSupportedException>(async () => await cache.AddOrGetExistingAsync("bad", async () => await FooFactory(BadFunction()), DateTimeOffset.UtcNow.AddSeconds(10)));
            Assert.That(cache["bad"], Is.Null, "Cache entry differs");
        }

        private Foo BadFunction()
        {
            throw new NotSupportedException();
        }

        private Task<Foo> FooFactory(Foo entity)
        {
            return Task.FromResult(entity);
        }

        private class Foo
        {
            public string Name { get; set; }
        }
    }
#endif
}