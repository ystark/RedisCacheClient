using System;
using System.Collections;
using System.Configuration;
using System.Runtime.Caching;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RedisCacheClient.Tests
{
    [TestClass]
    public class BasicTest
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
        public void Add()
        {
            var key = "foo";
            var expected = "bar";

            cache.Add(key, expected, ObjectCache.InfiniteAbsoluteExpiration);

            var actual = cache.Get(key);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AddOrGetExisting()
        {
            var key = "foo";
            var expected = "bar";

            cache.AddOrGetExisting(key, expected, ObjectCache.InfiniteAbsoluteExpiration);

            var actual = cache.Get(key);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SetAndAddOrGetExisting()
        {
            var key = "foo";
            var expected = "bar";
            var newValue = "baz";

            cache.Set(key, expected, ObjectCache.InfiniteAbsoluteExpiration);

            var actual = cache.AddOrGetExisting(key, newValue, ObjectCache.InfiniteAbsoluteExpiration);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Contains()
        {
            var key = "foo";
            var value = "bar";

            cache.Set(key, value, ObjectCache.InfiniteAbsoluteExpiration);

            var actual = cache.Contains(key);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void SetAndGet()
        {
            var key = "foo";
            var expected = "bar";

            cache.Set(key, expected, ObjectCache.InfiniteAbsoluteExpiration);

            var actual = cache.Get(key);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetValues()
        {
            var keys = new[] { "foo", "bar", "baz" };
            var expected = new[] { "bar", "baz", "foo" };

            for (int i = 0; i < keys.Length; i++)
            {
                cache.Set(keys[i], expected[i], ObjectCache.InfiniteAbsoluteExpiration);
            }
            
            var actual = cache.GetValues(keys);

            CollectionAssert.AreEqual(expected, (ICollection)actual.Values);
        }

        [TestMethod]
        public void SlidingGetValues()
        {
            var keys = new[] { "foo", "bar", "baz" };
            var expected = new[] { "bar", "baz", "foo" };

            for (int i = 0; i < keys.Length; i++)
            {
                cache.Set(keys[i], expected[i], new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, 1) });
            }

            var actual = cache.GetValues(keys);

            CollectionAssert.AreEqual(expected, (ICollection)actual.Values);
        }

        [TestMethod]
        public void Remove()
        {
            var key = "foo";
            var value = "bar";

            cache.Set(key, value, ObjectCache.InfiniteAbsoluteExpiration);

            var removed = cache.Remove(key);

            Assert.AreEqual(value, removed);

            var actual = cache.Get(key);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void SlidingRemove()
        {
            var key = "foo";
            var value = "bar";

            cache.Set(key, value, new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, 1) });

            var removed = cache.Remove(key);

            Assert.AreEqual(value, removed);

            var actual = cache.Get(key);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void RemoveAndReturn()
        {
            var key = "foo";
            var excepted = "bar";

            cache.Set(key, excepted, ObjectCache.InfiniteAbsoluteExpiration);

            var actual = cache.Remove(key);

            Assert.AreEqual(excepted, actual);
        }

        [TestMethod]
        public void Indexer()
        {
            var key = "foo";
            var expected = "bar";

            cache[key] = expected;

            var actual = cache[key];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Empty()
        {
            var key = "foobarbaz";

            var actual = cache.Get(key);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void GetCount()
        {
            var expected = 3;

            for (int i = 0; i < expected; i++)
            {
                cache.Set("key" + i, "value" + i, ObjectCache.InfiniteAbsoluteExpiration);
            }

            var actual = cache.GetCount();

            Assert.AreEqual(expected, actual);
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
