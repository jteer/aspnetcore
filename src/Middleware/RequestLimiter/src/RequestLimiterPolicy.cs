// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.ResourceLimits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.RequestLimiter
{
    public class RequestLimiterPolicy
    {
        internal ICollection<Func<IServiceProvider, AggregatedResourceLimiter<HttpContext>>> Limiters { get; } = new List<Func<IServiceProvider, AggregatedResourceLimiter<HttpContext>>>();

        public void AddLimiter(ResourceLimiter limiter) => AddLimiter(new AggregatedResourceLimiterOfHttpContextWrapper(limiter));

        public void AddLimiter(AggregatedResourceLimiter<HttpContext> aggregatedLimiter)
        {
            Limiters.Add(_ => aggregatedLimiter);
        }

        // TODO: non aggregated limiters
        public void AddLimiter<TResourceLimiter>() where TResourceLimiter : AggregatedResourceLimiter<HttpContext>
        {
            Limiters.Add(services => services.GetRequiredService<TResourceLimiter>());
        }
    }
}
