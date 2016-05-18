using System;
using System.Threading;
using Meerkat.Caching;

using NUnit.Framework;

namespace Meerkat.Test.Caching
{
    [TestFixture]
    public class MemoryObjectCacheFixture
    {
        private MemoryObjectCache cache;

        [Test]
        public void SetItem()
        {
            cache.Set("A1", "A", DateTimeOffset.UtcNow.AddMinutes(1));
        }

        [Test]
        public void AddItem()
        {
            var candidate = cache.AddOrGetExisting("A2", "A", DateTimeOffset.UtcNow.AddMinutes(1));

            Assert.That(candidate, Is.Null);
        }

        [Test]
        public void AddExistingItem()
        {
            cache.Set("A3", "A", DateTimeOffset.UtcNow.AddMinutes(1));

            var candidate = cache.AddOrGetExisting("A3", "A", DateTimeOffset.UtcNow.AddMinutes(1));

            Assert.That(candidate, Is.Not.Null);
        }

        [Test]
        public void AddItemRegion()
        {
            var candidate = cache.AddOrGetExisting("A", "A", DateTimeOffset.UtcNow.AddMinutes(1), "customer");

            Assert.That(candidate, Is.Null);
        }

        [Test]
        public void Retrieve()
        {
            cache.AddOrGetExisting("B", "A", DateTimeOffset.UtcNow.AddMinutes(1));

            var candidate = cache.Contains("B");

            Assert.That(candidate, Is.EqualTo(true));
        }

        [Test]
        public void RetrieveRegion()
        {
            cache.AddOrGetExisting("B", "A", DateTimeOffset.UtcNow.AddMinutes(1), "customer");

            var candidate = cache.Contains("B", "customer");

            Assert.That(candidate, Is.EqualTo(true));
        }

        [Test]
        public void RetrieveNonExistent()
        {
            var candidate = cache.Contains("C");

            Assert.That(candidate, Is.EqualTo(false));
        }

        [Test]
        public void RetrieveExpired()
        {
            cache.AddOrGetExisting("D", "D", DateTimeOffset.UtcNow.AddSeconds(1));

            // Need to wait 20s for the cache to expire
            // See http://stackoverflow.com/questions/1434284/when-does-asp-net-remove-expired-cache-items
            Thread.Sleep(21000);

            var candidate = cache.Contains("D");

            Assert.That(candidate, Is.EqualTo(false));
        }

        [Test]
        public void RegionKeyEquivalance()
        {
            cache.AddOrGetExisting("E", "A", DateTimeOffset.UtcNow.AddMinutes(1), "customer");

            var candidate = cache.Contains("customer::E");

            Assert.That(candidate, Is.EqualTo(true));
        }

        [SetUp]
        public void Setup()
        {
            cache = MemoryObjectCacheFactory.Default();
        }
    }
}