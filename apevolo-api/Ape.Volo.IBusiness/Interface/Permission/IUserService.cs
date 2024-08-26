﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Permission;
using Ape.Volo.IBusiness.Base;
using Ape.Volo.IBusiness.Dto.Permission;
using Ape.Volo.IBusiness.QueryModel;
using Microsoft.AspNetCore.Http;

namespace Ape.Volo.IBusiness.Interface.Permission;

/// <summary>
/// 用户接口
/// </summary>
public interface IUserService : IBaseServices<User>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateUserDto"></param>
    /// <returns></returns>
    Task<bool> CreateAsync(CreateUpdateUserDto createUpdateUserDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateUserDto"></param>
    /// <returns></returns>
    Task<bool> UpdateAsync(CreateUpdateUserDto createUpdateUserDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(HashSet<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="userQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<UserDto>> QueryAsync(UserQueryCriteria userQueryCriteria, Pagination pagination);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="userQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(UserQueryCriteria userQueryCriteria);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 查找用户
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    Task<UserDto> QueryByIdAsync(long userId);

    /// <summary>
    /// 查找用户
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <returns>用户实体</returns>
    Task<UserDto> QueryByNameAsync(string userName);


    /// <summary>
    /// 根据部门ID查找用户
    /// </summary>
    /// <param name="deptIds"></param>
    /// <returns></returns>
    Task<List<UserDto>> QueryByDeptIdsAsync(List<long> deptIds);

    /// <summary>
    /// 修改个人中心信息
    /// </summary>
    /// <param name="updateUserCenterDto"></param>
    /// <returns></returns>
    Task<bool> UpdateCenterAsync(UpdateUserCenterDto updateUserCenterDto);

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="userPassDto"></param>
    /// <returns></returns>
    Task<bool> UpdatePasswordAsync(UpdateUserPassDto userPassDto);


    /// <summary>
    /// 修改邮箱
    /// </summary>
    /// <param name="updateUserEmailDto"></param>
    /// <returns></returns>
    Task<bool> UpdateEmailAsync(UpdateUserEmailDto updateUserEmailDto);

    /// <summary>
    /// 修改头像
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    Task<bool> UpdateAvatarAsync(IFormFile file);

    #endregion
}
