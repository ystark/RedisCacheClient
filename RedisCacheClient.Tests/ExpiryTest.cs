using System;
using System.Configuration;
using System.Runtime.Caching;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RedisCacheClient.Tests
{
    [TestClass]
    public class ExpiryTest
    {
        private ObjectCache cache;

        [TestInitialize]
        public void Initialize()
        {
            cache = CreateRedisCache();
        }

        [TestCleanup]
        public void Cleanup()
        {
            DisposeRedisCache(cache);
        }

        [TestMethod]
        public void AbsoluteLiving()
        {
            var key = "absolute";
            var expected = "value";

            cache.Add(key, expected, DateTimeOffset.Now.AddSeconds(3));

            Thread.Sleep(2000);

            var actual = cache.Get(key);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AbsoluteExpired()
        {
            var key = "absolute";
            var value = "value";

            cache.Add(key, value, DateTimeOffset.Now.AddSeconds(3));

            Thread.Sleep(4000);

            var actual = cache.Get(key);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void SlidingLiving()
        {
            var key = "sliding";
            var expected = "value";

            cache.Add(key, expected, new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, 3) });

            Thread.Sleep(2000);

            var actual = cache.Get(key);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SlidingLivingExtended()
        {
            var key = "sliding";
            var expected = "value";

            cache.Add(key, expected, new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, 2) });

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(1000);
                cache.Get(key);
            }

            var actual = cache.Get(key);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SlidingExpired()
        {
            var key = "sliding";
            var value = "value";

            cache.Add(key, value, new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, 3) });

            Thread.Sleep(4000);

            var actual = cache.Get(key);

            Assert.IsNull(actual);
        }

        private ObjectCache CreateRedisCache()
        {
#if false
            var cache = MemoryCache.Default;
#else
            var cache = new RedisCache(1, ConfigurationManager.AppSettings["RedisConfiguration"]);
#endif

            foreach (var item in cache)
            {
                cache.Remove(item.Key);
            }

            return cache;
        }

        private void DisposeRedisCache(ObjectCache cache)
        {
            if (cache is RedisCache)
            {
                ((RedisCache)cache).Dispose();
            }
        }
    }
}
