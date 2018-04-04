using System;
using System.Threading.Tasks;
using Meerkat.Caching;

using NUnit.Framework;

namespace Meerkat.Test.Caching
{
    [TestFixture]
    public class CacheExtensionsFixture
    {
        [Test]
        public void FuncAdd()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };

            var candidate = cache.AddOrGetExisting("la", () => expected, DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        public void LazyAdd()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };

            var candidate = cache.LazyAddOrGetExisting("la", () => expected, DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        [Ignore("Not reliable when run in test batch")]
        public async Task FuncAddAsync()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };

            var candidate = await cache.AddOrGetExistingAsync("la", async () => await FooFactory(expected), DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        [Ignore("Not reliable when run in test batch")]
        public async Task LazyFuncAddAsync()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };

            var candidate = await cache.LazyAddOrGetExistingAsync("la", async () => await FooFactory(expected), DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        public void FuncAddAlreadyExists()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };
            var other = new Foo { Name = "B" };

            cache["lae"] = expected;

            var candidate = cache.AddOrGetExisting("lae", () => other, DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        public void LazyAddAlreadyExists()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };
            var other = new Foo { Name = "B" };

            cache.LazyAddOrGetExisting("lae", () => expected, DateTimeOffset.UtcNow.AddSeconds(10));

            var candidate = cache.LazyAddOrGetExisting("lae", () => other, DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        public async Task FuncAddAlreadyExistsAsync()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };
            var other = new Foo { Name = "B" };

            cache["lae"] = expected;

            var candidate = await cache.AddOrGetExistingAsync("lae", async () => await FooFactory(other), DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        public async Task LazyFuncAddAlreadyExistsAsync()
        {
            var cache = MemoryObjectCacheFactory.Default();

            var expected = new Foo { Name = "A" };
            var other = new Foo { Name = "B" };

            cache["lae"] = expected;

            var candidate = await cache.LazyAddOrGetExistingAsync("lae", async () => await FooFactory(other), DateTimeOffset.UtcNow.AddSeconds(10));

            Assert.That(candidate, Is.SameAs(expected), "Cache values differ");
        }

        [Test]
        public void FuncAddThrowsOnException()
        {
            var cache = MemoryObjectCacheFactory.Default();

            Assert.Throws<NotSupportedException>(() => cache.AddOrGetExisting("bad", BadFunction, DateTimeOffset.UtcNow.AddSeconds(10)));
            Assert.That(cache["bad"], Is.Null, "Cache entry differs");
        }

        [Test]
        public void LazyFuncAddThrowsOnException()
        {
            var cache = MemoryObjectCacheFactory.Default();

            Assert.Throws<NotSupportedException>(() => cache.LazyAddOrGetExisting("bad", BadFunction, DateTimeOffset.UtcNow.AddSeconds(10)));
            Assert.That(cache["bad"], Is.Null, "Cache entry differs");
        }

        [Test]
        public void FuncAddThrowsOnExceptionAsync()
        {
            var cache = MemoryObjectCacheFactory.Default();

            Assert.Throws<NotSupportedException>(async () => await cache.AddOrGetExistingAsync("bad", async () => await FooFactory(BadFunction()), DateTimeOffset.UtcNow.AddSeconds(10)));
            Assert.That(cache["bad"], Is.Null, "Cache entry differs");
        }

        [Test]
        public void LazyFuncAddThrowsOnExceptionAsync()
        {
            var cache = MemoryObjectCacheFactory.Default();

            Assert.Throws<NotSupportedException>(async () => await cache.LazyAddOrGetExistingAsync("bad", async () => await FooFactory(BadFunction()), DateTimeOffset.UtcNow.AddSeconds(10)));
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
}