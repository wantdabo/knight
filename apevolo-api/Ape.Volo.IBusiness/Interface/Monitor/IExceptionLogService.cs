﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Monitor;
using Ape.Volo.IBusiness.Base;
using Ape.Volo.IBusiness.Dto.Monitor;
using Ape.Volo.IBusiness.QueryModel;

namespace Ape.Volo.IBusiness.Interface.Monitor;

/// <summary>
/// 系统日志接口
/// </summary>
public interface IExceptionLogService : IBaseServices<ExceptionLog>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="exceptionLog"></param>
    /// <returns></returns>
    Task<bool> CreateAsync(ExceptionLog exceptionLog);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="logQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<ExceptionLogDto>> QueryAsync(LogQueryCriteria logQueryCriteria, Pagination pagination);

    #endregion
}
