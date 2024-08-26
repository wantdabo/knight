using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using Ape.Volo.IBusiness.Dto.Monitor;
using Ape.Volo.IBusiness.Interface.Monitor;
using Ape.Volo.IBusiness.QueryModel;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Monitor;

/// <summary>
/// 审计管理
/// </summary>
[Area("审计管理")]
[Route("/api/auditing", Order = 13)]
public class AuditingController : BaseApiController
{
    #region 字段

    private readonly IAuditLogService _auditInfoService;

    #endregion

    #region 构造函数

    public AuditingController(IAuditLogService auditInfoService)
    {
        _auditInfoService = auditInfoService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 审计列表
    /// </summary>
    /// <param name="logQueryCriteria">查询对象</param>
    /// <param name="pagination">分页对象</param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("查询")]
    [NotAudit]
    public async Task<ActionResult<object>> Query(LogQueryCriteria logQueryCriteria,
        Pagination pagination)
    {
        var auditInfos = await _auditInfoService.QueryAsync(logQueryCriteria, pagination);

        return JsonContent(new ActionResultVm<AuditLogDto>
        {
            Content = auditInfos,
            TotalElements = pagination.TotalElements
        });
    }


    /// <summary>
    /// 当前用户行为
    /// </summary>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("current")]
    [Description("用户行为")]
    [NotAudit]
    public async Task<ActionResult<object>> FindListByCurrent(Pagination pagination)
    {
        var auditInfos = await _auditInfoService.QueryByCurrentAsync(pagination);

        return JsonContent(new ActionResultVm<AuditLogDto>
        {
            Content = auditInfos,
            TotalElements = pagination.TotalElements
        });
    }

    #endregion
}
