﻿using System;
using Ape.Volo.Common;
using Ape.Volo.Common.ConfigOptions;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper.Serilog;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Ape.Volo.Api.Middleware;

/// <summary>
/// IP限流策略中间件
/// </summary>
public static class IpLimitMiddleware
{
    private static readonly ILogger Logger = SerilogManager.GetLogger(typeof(IpLimitMiddleware));

    public static void UseIpLimitMiddleware(this IApplicationBuilder app)
    {
        if (app.IsNull())
            throw new ArgumentNullException(nameof(app));
        try
        {
            if (App.GetOptions<MiddlewareOptions>().IpLimit.Enabled)
            {
                app.UseIpRateLimiting();
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error occured limiting ip rate.\n{e.Message}");
            throw;
        }
    }
}
