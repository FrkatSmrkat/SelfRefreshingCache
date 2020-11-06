using Microsoft.Extensions.Logging;
using SelfRefreshingCache.Interfaces;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SelfRefreshingCache
{
    public class SelfRefreshingCache<TResult> : ISelfRefreshingCache<TResult>
    {
        private ILogger logger;
        private int refreshPeriodSeconds;
        private int validityOfResultSeconds;
        private Func<TResult> createFunction;

        private TResult cachedItem;

        private bool cacheInitialized = false;
        private bool backgroundWorkerRunning = false;
        private DateTime lastFetch = DateTime.MinValue;

        private object lockObject = new object();
        private Task cacheRefreshTask = null;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public SelfRefreshingCache(ILogger logger,
                   int refreshPeriodSeconds,
                   int validityOfResultSeconds,
                   Func<TResult> createFunction)
        {
            this.logger = logger;
            this.refreshPeriodSeconds = refreshPeriodSeconds;
            this.validityOfResultSeconds = validityOfResultSeconds;
            this.createFunction = createFunction;
            cachedItem = default;
        }

        private async Task RefreshCache(CancellationToken cancellationToken) 
        {
            try
            {
                var result = await Task.Run(() =>
                {
                    return createFunction.Invoke();
                },
                cancellationToken);

                lock (lockObject)
                {
                    cachedItem = result;
                    cacheInitialized = true;
                    lastFetch = DateTime.Now;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "refresh failed");
            }
        }

        private async Task BackgroundWorker(CancellationToken cancellationToken)
        {
            logger.LogInformation($"worker started  - {DateTime.Now.TimeOfDay}");
            await Task.Delay(TimeSpan.FromSeconds(refreshPeriodSeconds), cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                lock (lockObject)
                {
                    cacheRefreshTask = RefreshCache(cancellationToken);
                }
                await cacheRefreshTask;
                logger.LogInformation($"cache refreshed by worker  - {DateTime.Now.TimeOfDay}");
                await Task.Delay(TimeSpan.FromSeconds(refreshPeriodSeconds), cancellationToken);
            }
        }

        public async Task<TResult> GetOrCreate()
        {
            lock (lockObject)
            {
                if (!backgroundWorkerRunning)
                {
                    Task.Run(() => BackgroundWorker(cancellationTokenSource.Token));
                    backgroundWorkerRunning = true;
                }

                bool cacheItemValid = DateTime.Now - lastFetch <= TimeSpan.FromSeconds(validityOfResultSeconds);
                if (cacheInitialized && cacheItemValid)
                {
                    return cachedItem;
                }
                else
                {
                    cachedItem = default;
                    cacheRefreshTask = RefreshCache(cancellationTokenSource.Token);
                }
            }

            await cacheRefreshTask;

            return cachedItem;
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            backgroundWorkerRunning = false;
            logger.LogInformation($"stoped - {DateTime.Now.TimeOfDay}");
        }
    }
}
