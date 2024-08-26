using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.System;
using Ape.Volo.IBusiness.Dto.System;
using Ape.Volo.IBusiness.ExportModel.System;
using Ape.Volo.IBusiness.Interface.System;
using Ape.Volo.IBusiness.QueryModel;

namespace Ape.Volo.Business.System;

/// <summary>
/// QuartzNet作业服务
/// </summary>
public class QuartzNetService : BaseServices<QuartzNet>, IQuartzNetService
{
    #region 字段

    private readonly IQuartzNetLogService _quartzNetLogService;

    #endregion

    #region 构造函数

    public QuartzNetService(IQuartzNetLogService quartzNetLogService)
    {
        _quartzNetLogService = quartzNetLogService;
    }

    #endregion

    #region 基础方法

    public async Task<List<QuartzNet>> QueryAllAsync()
    {
        return await SugarRepository.QueryListAsync();
    }


    public async Task<QuartzNet> CreateAsync(CreateUpdateQuartzNetDto createUpdateQuartzNetDto)
    {
        if (await TableWhere(q =>
                q.AssemblyName == createUpdateQuartzNetDto.AssemblyName &&
                q.ClassName == createUpdateQuartzNetDto.ClassName).AnyAsync())
        {
            throw new BadRequestException(
                $"作业执行目录=>{createUpdateQuartzNetDto.AssemblyName + "_" + createUpdateQuartzNetDto.ClassName}=>已存在!");
        }

        var quartzNet = App.Mapper.MapTo<QuartzNet>(createUpdateQuartzNetDto);
        return await SugarRepository.AddReturnEntityAsync(quartzNet);
    }

    public async Task<bool> UpdateAsync(CreateUpdateQuartzNetDto createUpdateQuartzNetDto)
    {
        var oldQuartzNet =
            await TableWhere(x => x.Id == createUpdateQuartzNetDto.Id).FirstAsync();
        if (oldQuartzNet.IsNull())
        {
            throw new BadRequestException("数据不存在！");
        }

        if ((oldQuartzNet.AssemblyName != createUpdateQuartzNetDto.AssemblyName ||
             oldQuartzNet.ClassName != createUpdateQuartzNetDto.ClassName) && await TableWhere(q =>
                q.AssemblyName == createUpdateQuartzNetDto.AssemblyName &&
                q.ClassName == createUpdateQuartzNetDto.ClassName).AnyAsync())
        {
            throw new BadRequestException(
                $"作业执行目录=>{createUpdateQuartzNetDto.AssemblyName + "_" + createUpdateQuartzNetDto.ClassName}=>已存在!");
        }

        var quartzNet = App.Mapper.MapTo<QuartzNet>(createUpdateQuartzNetDto);
        return await UpdateEntityAsync(quartzNet);
    }


    [UseTran]
    public async Task<bool> UpdateJobInfoAsync(QuartzNet quartzNet, QuartzNetLog quartzNetLog)
    {
        await UpdateEntityAsync(quartzNet);
        await _quartzNetLogService.CreateAsync(quartzNetLog);
        return true;
    }

    public async Task<bool> DeleteAsync(List<QuartzNet> quartzNets)
    {
        var ids = quartzNets.Select(x => x.Id).ToList();
        return await LogicDelete<QuartzNet>(x => ids.Contains(x.Id)) > 0;
    }

    public async Task<List<QuartzNetDto>> QueryAsync(QuartzNetQueryCriteria quartzNetQueryCriteria,
        Pagination pagination)
    {
        var whereExpression = GetWhereExpression(quartzNetQueryCriteria);
        var queryOptions = new QueryOptions<QuartzNet>
        {
            Pagination = pagination,
            WhereLambda = whereExpression,
        };
        return App.Mapper.MapTo<List<QuartzNetDto>>(
            await SugarRepository.QueryPageListAsync(queryOptions));
    }

    public async Task<List<ExportBase>> DownloadAsync(QuartzNetQueryCriteria quartzNetQueryCriteria)
    {
        var whereExpression = GetWhereExpression(quartzNetQueryCriteria);
        var quartzNets = await TableWhere(whereExpression).ToListAsync();
        List<ExportBase> quartzExports = new List<ExportBase>();
        quartzExports.AddRange(quartzNets.Select(x => new QuartzNetExport()
        {
            TaskName = x.TaskName,
            TaskGroup = x.TaskGroup,
            Cron = x.Cron,
            AssemblyName = x.AssemblyName,
            ClassName = x.ClassName,
            Description = x.Description,
            Principal = x.Principal,
            AlertEmail = x.AlertEmail,
            PauseAfterFailure = x.PauseAfterFailure,
            RunTimes = x.RunTimes,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            TriggerType = x.TriggerType,
            IntervalSecond = x.IntervalSecond,
            CycleRunTimes = x.CycleRunTimes,
            IsEnable = x.IsEnable ? BoolState.True : BoolState.False,
            RunParams = x.RunParams,
            TriggerStatus = x.TriggerStatus,
            TriggerTypeStr = x.TriggerType.GetDisplayName(),
            CreateTime = x.CreateTime
        }));
        return quartzExports;
    }

    #endregion

    #region 条件表达式

    private static Expression<Func<QuartzNet, bool>> GetWhereExpression(QuartzNetQueryCriteria quartzNetQueryCriteria)
    {
        Expression<Func<QuartzNet, bool>> whereExpression = x => true;
        if (!quartzNetQueryCriteria.TaskName.IsNullOrEmpty())
        {
            whereExpression = whereExpression.AndAlso(x => x.TaskName.Contains(quartzNetQueryCriteria.TaskName));
        }

        if (!quartzNetQueryCriteria.CreateTime.IsNullOrEmpty() && quartzNetQueryCriteria.CreateTime.Count > 1)
        {
            whereExpression = whereExpression.AndAlso(x =>
                x.CreateTime >= quartzNetQueryCriteria.CreateTime[0] &&
                x.CreateTime <= quartzNetQueryCriteria.CreateTime[1]);
        }

        return whereExpression;
    }

    #endregion
}
