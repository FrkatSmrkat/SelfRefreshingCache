using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SelfRefreshingCache.Interfaces
{
    interface ISelfRefreshingCache<TResult>
    {
        public Task<TResult> GetOrCreate();
        public void Stop();
    }
}
