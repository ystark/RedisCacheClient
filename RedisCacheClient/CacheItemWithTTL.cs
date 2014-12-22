using System;
using System.Runtime.Serialization;

namespace RedisCacheClient
{
    [Serializable]
    internal class CacheItemWithTTL
    {
        public object value;
        public TimeSpan TTL;
    }
}
