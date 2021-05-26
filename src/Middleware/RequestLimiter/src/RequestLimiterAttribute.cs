// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.ResourceLimits;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.RequestLimiter
{
    // TODO: Double check ordering
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequestLimitAttribute : Attribute
    {
        public RequestLimitAttribute() { }

        public RequestLimitAttribute(string policy)
        {
            Policy = policy;
        }

        public RequestLimitAttribute(long requestPerSecond)
            : this(new TokenBucketRateLimiter(requestPerSecond, requestPerSecond)) { }

        public RequestLimitAttribute(ResourceLimiter limiter)
            : this(_ => new AggregatedResourceLimiterOfHttpContextWrapper(limiter)) { }

        public RequestLimitAttribute(AggregatedResourceLimiter<HttpContext> limiter)
            : this(_ => limiter) { }

        internal RequestLimitAttribute(Func<IServiceProvider, ResourceLimiter> resolveLimiter)
        {
            ResolveLimiter = services => new AggregatedResourceLimiterOfHttpContextWrapper(resolveLimiter(services));
        }

        internal RequestLimitAttribute(Func<IServiceProvider, AggregatedResourceLimiter<HttpContext>> resolveLimiter)
        {
            ResolveLimiter = resolveLimiter;
        }

        internal string? Policy { get; set; }

        internal Func<IServiceProvider, AggregatedResourceLimiter<HttpContext>>? ResolveLimiter { get; set; }
    }
}
