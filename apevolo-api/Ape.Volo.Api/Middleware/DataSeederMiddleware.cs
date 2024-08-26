using System;
using System.Threading;
using Ape.Volo.Common;
using Ape.Volo.Common.ConfigOptions;
using Ape.Volo.Common.Helper.Serilog;
using Ape.Volo.Entity.Seed;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Ape.Volo.Api.Middleware;

public static class DataSeederMiddleware
{
    private static readonly ILogger Logger = SerilogManager.GetLogger(typeof(DataSeederMiddleware));

    public static void UseDataSeederMiddleware(this IApplicationBuilder app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        try
        {
            var settingsOptions = App.GetOptions<SettingsOptions>();
            if (settingsOptions.IsInitTable)
            {
                var dataContext = app.ApplicationServices.GetRequiredService<DataContext>();
                DataSeeder.InitMasterDataAsync(dataContext, settingsOptions.IsInitData,
                    settingsOptions.IsQuickDebug).Wait();
                Thread.Sleep(500); //保证顺序输出
                DataSeeder.InitLogData(dataContext);
            }
        }
        catch (Exception e)
        {
            Logger.Error($"创建数据库初始化数据时错误.\n{e.Message}");
            throw;
        }
    }
}
