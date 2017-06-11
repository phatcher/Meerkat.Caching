using System;
using Meerkat.Caching.Configuration;
using NUnit.Framework;

namespace Meerkat.Test.Caching.Configuration
{
    [TestFixture]
    public class CacheConfigurationFixture
    {
        [TestCase(null, "00:10")]
        [TestCase("A", "00:20")]
        [TestCase("B", "00:10")]
        public void ConfiguredDuration(string key, string value)
        {
            TimeSpan expected = TimeSpan.Parse(value);
            var candidate = CacheConfiguration.Duration(key);

            Assert.That(candidate, Is.EqualTo(expected), "TimeSpan differs");
        }

        [TestCase(null, "00:10")]
        [TestCase("A", "00:20")]
        [TestCase("B", "00:30")]
        public void DefaultedDuration(string key, string value)
        {
            TimeSpan expected = TimeSpan.Parse(value);
            var candidate = CacheConfiguration.Duration(key, TimeSpan.FromMinutes(30));

            Assert.That(candidate, Is.EqualTo(expected), "TimeSpan differs");
        }
    }
}