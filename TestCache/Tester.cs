using SelfRefreshingCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestCache
{
    public class Tester
    {
        private int i = 0;
        public int DelayedGet()
        {
            Thread.Sleep(TimeSpan.FromSeconds(4));
            return 1;
        }

        public int ErroredGet()
        {
            throw new Exception("get failed");
        }

        public int FastGet()
        {
            return 2;
        }

        public int Get()
        {
            switch (i)
            {
                case 1:
                    return DelayedGet();
                case 2:
                    return ErroredGet();
                default:
                    return FastGet();
            }
        }

        public async Task Test()
        {
            var cache = new SelfRefreshingCache<int>(new ConsoleLogger(), 2, 10, Get);
            var item = await cache.GetOrCreate();
            Thread.Sleep(TimeSpan.FromSeconds(5));
            cache.Stop();
            Thread.Sleep(TimeSpan.FromSeconds(5));
            item = await cache.GetOrCreate();
            Thread.Sleep(TimeSpan.FromSeconds(5));
            return;
        }
    }
}
