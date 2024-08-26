﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Permission;
using Ape.Volo.IBusiness.Base;
using Ape.Volo.IBusiness.Dto.Permission;
using Ape.Volo.IBusiness.QueryModel;

namespace Ape.Volo.IBusiness.Interface.Permission;

/// <summary>
/// 部门接口
/// </summary>
public interface IDepartmentService : IBaseServices<Department>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateDepartmentDto"></param>
    /// <returns></returns>
    Task<bool> CreateAsync(CreateUpdateDepartmentDto createUpdateDepartmentDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateDepartmentDto"></param>
    /// <returns></returns>
    Task<bool> UpdateAsync(CreateUpdateDepartmentDto createUpdateDepartmentDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(List<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="deptQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<DepartmentDto>> QueryAsync(DeptQueryCriteria deptQueryCriteria, Pagination pagination);

    /// <summary>
    /// 查询全部
    /// </summary>
    /// <returns></returns>
    Task<List<DepartmentDto>> QueryAllAsync();

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="deptQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(DeptQueryCriteria deptQueryCriteria);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 根据父ID获取全部
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<List<DepartmentDto>> QueryByPIdAsync(long id);

    /// <summary>
    /// 根据ID获取一个部门
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<DepartmentSmallDto> QueryByIdAsync(long id);


    /// <summary>
    /// 获取子级所有部门
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<List<DepartmentDto>> QuerySuperiorDeptAsync(long id);

    /// <summary>
    /// 获取所选部门及全部下级部门ID
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="allIds"></param>
    /// <returns></returns>
    Task<List<long>> GetChildIds(List<long> ids, List<long> allIds);

    #endregion
}
