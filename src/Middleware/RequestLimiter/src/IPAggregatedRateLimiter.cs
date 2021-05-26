using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.ResourceLimits;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.RequestLimiter
{
    // TODO: update implementation with WaitAsync and use MemoryCache instead of ConcurrentDictionary
    public class IPAggregatedRateLimiter : AggregatedResourceLimiter<HttpContext>
    {
        private long _resourceCount;
        private readonly long _maxResourceCount;
        private readonly long _newResourcePerSecond;

        private Timer _renewTimer;
        // TODO: This is racy
        private ConcurrentDictionary<IPAddress, long> _cache = new ConcurrentDictionary<IPAddress, long>();

        public IPAggregatedRateLimiter(long resourceCount, long newResourcePerSecond)
        {
            _resourceCount = resourceCount;
            _maxResourceCount = resourceCount;
            _newResourcePerSecond = newResourcePerSecond;

            // Start timer (5s for demo)
            _renewTimer = new Timer(Replenish, this, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public override long EstimatedCount(HttpContext resourceId)
        {
            if (resourceId.Connection.RemoteIpAddress == null)
            {
                // Unknown IP?
                return 0;
            }

            return _cache.TryGetValue(resourceId.Connection.RemoteIpAddress, out var count) ? count : 0;
        }

        public override ResourceLease Acquire(HttpContext resourceId, long requestedCount)
        {
            if (requestedCount > _maxResourceCount)
            {
                return ResourceLease.FailedAcquisition;
            }

            if (resourceId.Connection.RemoteIpAddress == null)
            {
                return ResourceLease.SuccessfulAcquisition;
            }

            var key = resourceId.Connection.RemoteIpAddress;

            if (!_cache.TryGetValue(key, out var count))
            {
                if (_cache.TryAdd(key, requestedCount))
                {
                    return ResourceLease.SuccessfulAcquisition;
                }
            }

            while (true)
            {
                var newCount = count + requestedCount;
                if (_cache.TryUpdate(key, count + requestedCount, count))
                {
                    if (newCount > _maxResourceCount)
                    {
                        return ResourceLease.FailedAcquisition;
                    }

                    return ResourceLease.SuccessfulAcquisition;
                }
                if (!_cache.TryGetValue(key, out count))
                {
                    if (_cache.TryAdd(key, requestedCount))
                    {
                        return ResourceLease.SuccessfulAcquisition;
                    }
                }
            }
        }

        public override ValueTask<ResourceLease> WaitAsync(HttpContext resourceId, long requestedCount, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private static void Replenish(object? state)
        {
            // Return if Replenish already running to avoid concurrency.
            var limiter = state as IPAggregatedRateLimiter;

            if (limiter == null)
            {
                return;
            }

            var cache = limiter._cache;

            foreach (var entry in cache)
            {
                if (entry.Value < limiter._newResourcePerSecond)
                {
                    if (cache.TryRemove(entry))
                    {
                        continue;
                    }
                }

                while (true)
                {
                    if (!cache.TryGetValue(entry.Key, out var newCount))
                    {
                        break;
                    }
                    if (cache.TryUpdate(entry.Key, Math.Max(0, newCount - limiter._newResourcePerSecond), newCount))
                    {
                        break;
                    }
                }
            }
        }
    }
}
