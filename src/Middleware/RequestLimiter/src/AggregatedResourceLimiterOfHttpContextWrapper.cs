using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.ResourceLimits;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.RequestLimiter
{
    // TODO: this name is too long
    internal class AggregatedResourceLimiterOfHttpContextWrapper : AggregatedResourceLimiter<HttpContext> 
    {
        private readonly ResourceLimiter _limiter;

        public AggregatedResourceLimiterOfHttpContextWrapper(ResourceLimiter limiter)
        {
            _limiter = limiter;
        }

        public override ResourceLease Acquire(HttpContext resourceID, long requestedCount)
        {
            return _limiter.Acquire(requestedCount);
        }

        public override long EstimatedCount(HttpContext resourceID)
        {
            return _limiter.EstimatedCount;
        }

        public override ValueTask<ResourceLease> WaitAsync(HttpContext resourceID, long requestedCount, CancellationToken cancellationToken = default)
        {
            return _limiter.WaitAsync(requestedCount, cancellationToken);
        }
    }
}
