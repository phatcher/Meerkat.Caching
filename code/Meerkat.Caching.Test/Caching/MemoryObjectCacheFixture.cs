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
            cache.Set("si", "A", DateTimeOffset.UtcNow.AddMinutes(1));
        }

        [Test]
        public void SetNull()
        {
            cache.Set("si", null, DateTimeOffset.UtcNow.AddMinutes(1));
        }

        [Test]
        public void IndexerSet()
        {
            cache["si"] = "A";

            var candidate = cache["si"];

            Assert.That(candidate, Is.EqualTo("A"));
        }

        [Test]
        public void IndexerSetNull()
        {
            cache["si"] = "A";
            cache["si"] = null;

            var candidate = cache["si"];

            Assert.That(candidate, Is.Null);
        }

        [Test]
        public void AddItem()
        {
            var candidate = cache.AddOrGetExisting("ai", "A", DateTimeOffset.UtcNow.AddMinutes(1));

            Assert.That(candidate, Is.Null);
        }

        [Test]
        public void AddNull()
        {
            cache.AddOrGetExisting("B", null, DateTimeOffset.UtcNow.AddMinutes(1), "customer");

            var candidate = cache.Contains("B", "customer");

            Assert.That(candidate, Is.EqualTo(false));

            var result = cache.Get("B", "customer");

            Assert.That(result, Is.EqualTo(null));
        }

        [Test]
        public void AddExistingItem()
        {
            cache.Set("aei", "A", DateTimeOffset.UtcNow.AddMinutes(1));

            var candidate = cache.AddOrGetExisting("aei", "A", DateTimeOffset.UtcNow.AddMinutes(1));

            Assert.That(candidate, Is.Not.Null);
        }

        [Test]
        public void AddItemRegion()
        {
            var candidate = cache.AddOrGetExisting("air", "A", DateTimeOffset.UtcNow.AddMinutes(1), "customer");

            Assert.That(candidate, Is.Null);
        }

        [Test]
        public void Retrieve()
        {
            cache.AddOrGetExisting("r", "A", DateTimeOffset.UtcNow.AddMinutes(1));

            var candidate = cache.Get("r");

            Assert.That(candidate, Is.EqualTo("A"));
        }

        [Test]
        public void RetrieveRegion()
        {
            cache.AddOrGetExisting("B", "A", DateTimeOffset.UtcNow.AddMinutes(1), "customer");

            var candidate = cache.Get("B", "customer");

            Assert.That(candidate, Is.EqualTo("A"));
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
            cache.AddOrGetExisting("re", "re", DateTimeOffset.UtcNow.AddSeconds(1));

            // Need to wait 20s for the cache to expire
            // See http://stackoverflow.com/questions/1434284/when-does-asp-net-remove-expired-cache-items
            Thread.Sleep(21000);

            var candidate = cache.Contains("re");

            Assert.That(candidate, Is.EqualTo(false));
        }

        [Test]
        public void RegionKeyEquivalance()
        {
            cache.AddOrGetExisting("rke", "A", DateTimeOffset.UtcNow.AddMinutes(1), "customer");

            var candidate = cache.Contains("customer::rke");

            Assert.That(candidate, Is.EqualTo(true));
        }

        [SetUp]
        public void Setup()
        {
            cache = MemoryObjectCacheFactory.Default();
        }
    }
}