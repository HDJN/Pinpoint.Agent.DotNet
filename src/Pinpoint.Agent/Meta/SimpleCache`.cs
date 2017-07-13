namespace Pinpoint.Agent.Meta
{
    using System.Collections.Concurrent;
    using System.Threading;

    public class SimpleCache<T>
    {
        // zero means not exist.
        private int idGen;
        private ConcurrentDictionary<T, Result> cache;


        public SimpleCache() : this(1024, 1)
        {

        }

        public SimpleCache(int cacheSize) : this(cacheSize, 1)
        {

        }

        public SimpleCache(int cacheSize, int startValue)
        {
            idGen = startValue;
            cache = createCache(cacheSize);
        }

        private ConcurrentDictionary<T, Result> createCache(int maxCacheSize)
        {
            return new ConcurrentDictionary<T, Result>();
        }

        public Result put(T value)
        {
            Result find;
            if (cache.TryGetValue(value, out find))
            {
                return find;
            }

            // Use negative values too to reduce data size
            var newId = Interlocked.Increment(ref idGen);
            var result = new Result(false, newId);
            this.cache.AddOrUpdate(value, (v) => { return result; }, (v, k) => { return result; });
            return new Result(true, newId);
        }
    }
}
