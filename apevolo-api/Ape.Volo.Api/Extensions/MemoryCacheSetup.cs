﻿namespace Ape.Volo.Api.Extensions;

/// <summary>
/// 缓存启动器
/// </summary>
public static class MemoryCacheSetup
{
    // public static void AddMemoryCacheSetup(this IServiceCollection services)
    // {
    //     if (services.IsNull()) throw new ArgumentNullException(nameof(services));
    //
    //     services.AddScoped<ICaching, MemoryCaching>();
    //     services.AddSingleton<IMemoryCache>(factory =>
    //     {
    //         var cache = new MemoryCache(new MemoryCacheOptions());
    //         return cache;
    //     });
    // }
}
