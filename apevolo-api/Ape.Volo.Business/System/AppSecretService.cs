using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.ConfigOptions;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Model;
using Ape.Volo.Common.SnowflakeIdHelper;
using Ape.Volo.Entity.System;
using Ape.Volo.IBusiness.Dto.System;
using Ape.Volo.IBusiness.ExportModel.System;
using Ape.Volo.IBusiness.Interface.System;
using Ape.Volo.IBusiness.QueryModel;

namespace Ape.Volo.Business.System;

/// <summary>
/// 应用秘钥
/// </summary>
public class AppSecretService : BaseServices<AppSecret>, IAppSecretService
{
    #region 构造函数

    public AppSecretService()
    {
    }

    #endregion

    #region 基础方法

    public async Task<bool> CreateAsync(CreateUpdateAppSecretDto createUpdateAppSecretDto)
    {
        if (await TableWhere(r => r.AppName == createUpdateAppSecretDto.AppName).AnyAsync())
        {
            throw new BadRequestException($"应用名称=>{createUpdateAppSecretDto.AppName}=>已存在!");
        }

        var id = IdHelper.GetId();
        createUpdateAppSecretDto.AppId = DateTime.Now.ToString("yyyyMMdd") + id[..8];
        createUpdateAppSecretDto.AppSecretKey =
            (createUpdateAppSecretDto.AppId + id).ToHmacsha256String(App.GetOptions<SettingsOptions>().HmacSecret);
        var appSecret = App.Mapper.MapTo<AppSecret>(createUpdateAppSecretDto);
        return await AddEntityAsync(appSecret);
    }

    public async Task<bool> UpdateAsync(CreateUpdateAppSecretDto createUpdateAppSecretDto)
    {
        //取出待更新数据
        var oldAppSecret = await TableWhere(x => x.Id == createUpdateAppSecretDto.Id).FirstAsync();
        if (oldAppSecret.IsNull())
        {
            throw new BadRequestException("数据不存在！");
        }

        if (oldAppSecret.AppName != createUpdateAppSecretDto.AppName &&
            await TableWhere(x => x.AppName == createUpdateAppSecretDto.AppName).AnyAsync())
        {
            throw new BadRequestException($"应用名称=>{createUpdateAppSecretDto.AppName}=>已存在!");
        }

        var appSecret = App.Mapper.MapTo<AppSecret>(createUpdateAppSecretDto);
        return await UpdateEntityAsync(appSecret);
    }

    public async Task<bool> DeleteAsync(HashSet<long> ids)
    {
        var appSecrets = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (appSecrets.Count <= 0)
            throw new BadRequestException("数据不存在！");
        return await LogicDelete<AppSecret>(x => ids.Contains(x.Id)) > 0;
    }

    public async Task<List<AppSecretDto>> QueryAsync(AppsecretQueryCriteria appsecretQueryCriteria,
        Pagination pagination)
    {
        var whereExpression = GetWhereExpression(appsecretQueryCriteria);
        var queryOptions = new QueryOptions<AppSecret>
        {
            Pagination = pagination,
            WhereLambda = whereExpression,
        };
        return App.Mapper.MapTo<List<AppSecretDto>>(
            await SugarRepository.QueryPageListAsync(queryOptions));
    }

    public async Task<List<ExportBase>> DownloadAsync(AppsecretQueryCriteria appsecretQueryCriteria)
    {
        var whereExpression = GetWhereExpression(appsecretQueryCriteria);
        var appSecrets = await TableWhere(whereExpression).ToListAsync();
        List<ExportBase> appSecretExports = new List<ExportBase>();
        appSecretExports.AddRange(appSecrets.Select(x => new AppSecretExport()
        {
            AppId = x.AppId,
            AppSecretKey = x.AppSecretKey,
            AppName = x.AppName,
            Remark = x.Remark,
            CreateTime = x.CreateTime
        }));
        return appSecretExports;
    }

    #endregion

    #region 条件表达式

    private static Expression<Func<AppSecret, bool>> GetWhereExpression(AppsecretQueryCriteria appsecretQueryCriteria)
    {
        Expression<Func<AppSecret, bool>> whereExpression = r => true;
        if (!appsecretQueryCriteria.KeyWords.IsNullOrEmpty())
        {
            whereExpression = whereExpression.AndAlso(r =>
                r.AppId.Contains(appsecretQueryCriteria.KeyWords) ||
                r.AppName.Contains(appsecretQueryCriteria.KeyWords) ||
                r.Remark.Contains(appsecretQueryCriteria.KeyWords));
        }

        if (!appsecretQueryCriteria.CreateTime.IsNull())
        {
            whereExpression = whereExpression.AndAlso(r =>
                r.CreateTime >= appsecretQueryCriteria.CreateTime[0] &&
                r.CreateTime <= appsecretQueryCriteria.CreateTime[1]);
        }

        return whereExpression;
    }

    #endregion
}
