using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ape.Volo.Business.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.ConfigOptions;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Common.SnowflakeIdHelper;
using Ape.Volo.Entity.Permission;
using Ape.Volo.IBusiness.Dto.Permission;
using Ape.Volo.IBusiness.ExportModel.Permission;
using Ape.Volo.IBusiness.Interface.Permission;
using Ape.Volo.IBusiness.QueryModel;
using Mapster;
using Microsoft.AspNetCore.Http;

namespace Ape.Volo.Business.Permission;

/// <summary>
/// 用户服务
/// </summary>
public class UserService : BaseServices<User>, IUserService
{
    #region 字段

    private readonly IDepartmentService _departmentService;
    private readonly IRoleService _roleService;

    #endregion

    #region 构造函数

    public UserService(IDepartmentService departmentService, IRoleService roleService)
    {
        _departmentService = departmentService;
        _roleService = roleService;
    }

    #endregion

    #region 基础方法

    [UseTran]
    public async Task<bool> CreateAsync(CreateUpdateUserDto createUpdateUserDto)
    {
        if (await TableWhere(x => x.Username == createUpdateUserDto.Username).AnyAsync())
        {
            throw new BadRequestException($"名称=>{createUpdateUserDto.Username}=>已存在!");
        }

        if (await TableWhere(x => x.Email == createUpdateUserDto.Email).AnyAsync())
        {
            throw new BadRequestException($"邮箱=>{createUpdateUserDto.Email}=>已存在!");
        }

        if (await TableWhere(x => x.Phone == createUpdateUserDto.Phone).AnyAsync())
        {
            throw new BadRequestException($"电话=>{createUpdateUserDto.Phone}=>已存在!");
        }

        var user = App.Mapper.MapTo<User>(createUpdateUserDto);

        //设置用户密码
        user.Password = BCryptHelper.Hash("123456");
        user.DeptId = user.Dept.Id;
        //用户
        await AddEntityAsync(user);

        //角色
        if (user.Roles.Count < 1)
        {
            throw new BadRequestException("角色至少选择一个");
        }

        await SugarClient.Deleteable<UserRole>().Where(x => x.UserId == user.Id).ExecuteCommandAsync();
        var userRoles = new List<UserRole>();
        userRoles.AddRange(user.Roles.Select(x => new UserRole() { UserId = user.Id, RoleId = x.Id }));
        await SugarClient.Insertable(userRoles).ExecuteCommandAsync();

        //岗位
        if (user.Jobs.Count < 1)
        {
            throw new BadRequestException("岗位至少选择一个");
        }


        await SugarClient.Deleteable<UserJob>().Where(x => x.UserId == user.Id).ExecuteCommandAsync();
        var userJobs = new List<UserJob>();
        userJobs.AddRange(user.Jobs.Select(x => new UserJob() { UserId = user.Id, JobId = x.Id }));
        await SugarClient.Insertable(userJobs).ExecuteCommandAsync();

        return true;
    }

    [UseTran]
    public async Task<bool> UpdateAsync(CreateUpdateUserDto createUpdateUserDto)
    {
        //取出待更新数据
        var oldUser = await TableWhere(x => x.Id == createUpdateUserDto.Id).Includes(x => x.Roles).FirstAsync();
        if (oldUser.IsNull())
        {
            throw new BadRequestException("数据不存在！");
        }

        if (oldUser.Username != createUpdateUserDto.Username &&
            await TableWhere(x => x.Username == createUpdateUserDto.Username).AnyAsync())
        {
            throw new BadRequestException($"名称=>{createUpdateUserDto.Username}=>已存在!");
        }

        if (oldUser.Email != createUpdateUserDto.Email &&
            await TableWhere(x => x.Email == createUpdateUserDto.Email).AnyAsync())
        {
            throw new BadRequestException($"邮箱=>{createUpdateUserDto.Email}=>已存在!");
        }

        if (oldUser.Phone != createUpdateUserDto.Phone &&
            await TableWhere(x => x.Phone == createUpdateUserDto.Phone).AnyAsync())
        {
            throw new BadRequestException($"电话=>{createUpdateUserDto.Phone}=>已存在!");
        }

        //验证角色等级
        var levels = oldUser.Roles.Select(x => x.Level);
        await _roleService.VerificationUserRoleLevelAsync(levels.Min());
        var user = App.Mapper.MapTo<User>(createUpdateUserDto);
        user.DeptId = user.Dept.Id;
        //更新用户
        await UpdateEntityAsync(user, new List<string> { "password", "salt_key", "avatar_name", "avatar_path" });
        //角色
        if (user.Roles.Count < 1)
        {
            throw new BadRequestException("角色至少选择一个！");
        }

        await SugarClient.Deleteable<UserRole>().Where(x => x.UserId == user.Id).ExecuteCommandAsync();
        var userRoles = new List<UserRole>();
        userRoles.AddRange(user.Roles.Select(x => new UserRole() { UserId = user.Id, RoleId = x.Id }));
        await SugarClient.Insertable(userRoles).ExecuteCommandAsync();

        //岗位
        if (user.Jobs.Count < 1)
        {
            throw new BadRequestException("岗位至少选择一个！");
        }

        await SugarClient.Deleteable<UserJob>().Where(x => x.UserId == user.Id).ExecuteCommandAsync();
        var userJobs = new List<UserJob>();
        userJobs.AddRange(user.Jobs.Select(x => new UserJob() { UserId = user.Id, JobId = x.Id }));
        await SugarClient.Insertable(userJobs).ExecuteCommandAsync();

        //清理缓存
        await ClearUserCache(user.Id);
        return true;
    }

    public async Task<bool> DeleteAsync(HashSet<long> ids)
    {
        //验证角色等级
        await _roleService.VerificationUserRoleLevelAsync(await _roleService.QueryUserRoleLevelAsync(ids));
        if (ids.Contains(App.HttpUser.Id))
        {
            throw new BadRequestException("禁止删除自己");
        }

        var users = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        foreach (var user in users)
        {
            await ClearUserCache(user.Id);
        }

        return await LogicDelete<User>(x => ids.Contains(x.Id)) > 0;
    }

    /// <summary>
    /// 用户列表
    /// </summary>
    /// <param name="userQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<UserDto>> QueryAsync(UserQueryCriteria userQueryCriteria, Pagination pagination)
    {
        var whereExpression = await GetWhereExpression(userQueryCriteria);

        var queryOptions = new QueryOptions<User>
        {
            Pagination = pagination,
            WhereLambda = whereExpression,
            IsIncludes = true
        };
        var users = await SugarRepository.QueryPageListAsync(queryOptions);

        return App.Mapper.MapTo<List<UserDto>>(users);
    }


    public async Task<List<ExportBase>> DownloadAsync(UserQueryCriteria userQueryCriteria)
    {
        var whereExpression = await GetWhereExpression(userQueryCriteria);
        var users = await Table.Includes(x => x.Dept).Includes(x => x.Roles)
            .Includes(x => x.Jobs).WhereIF(whereExpression != null, whereExpression).ToListAsync();
        List<ExportBase> userExports = new List<ExportBase>();
        userExports.AddRange(users.Select(x => new UserExport()
        {
            Id = x.Id,
            Username = x.Username,
            Role = string.Join(",", x.Roles.Select(r => r.Name).ToArray()),
            NickName = x.NickName,
            Phone = x.Phone,
            Email = x.Email,
            Enabled = x.Enabled ? EnabledState.Enabled : EnabledState.Disabled,
            Dept = x.Dept.Name,
            Job = string.Join(",", x.Jobs.Select(j => j.Name).ToArray()),
            Gender = x.Gender,
            CreateTime = x.CreateTime
        }));
        return userExports;
    }

    #endregion

    #region 扩展方法

    //[UseCache(Expiration = 60, KeyPrefix = GlobalConstants.CachePrefix.UserInfoById)]
    public async Task<UserDto> QueryByIdAsync(long userId)
    {
        var user = await TableWhere(x => x.Id == userId, null, null, true).Includes(x => x.Dept).Includes(x => x.Roles)
            .Includes(x => x.Jobs).FirstAsync();

        return App.Mapper.MapTo<UserDto>(user);
    }

    /// <summary>
    /// 查询用户
    /// </summary>
    /// <param name="userName">邮箱 or 用户名</param>
    /// <returns></returns>
    public async Task<UserDto> QueryByNameAsync(string userName)
    {
        User user;
        if (userName.IsEmail())
        {
            user = await TableWhere(s => s.Email == userName, null, null, true).FirstAsync();
        }
        else
        {
            user = await TableWhere(s => s.Username == userName, null, null, true).FirstAsync();
        }

        return App.Mapper.MapTo<UserDto>(user);
    }

    /// <summary>
    /// 根据部门ID查找用户
    /// </summary>
    /// <param name="deptIds"></param>
    /// <returns></returns>
    public async Task<List<UserDto>> QueryByDeptIdsAsync(List<long> deptIds)
    {
        return App.Mapper.MapTo<List<UserDto>>(
            await SugarRepository.QueryListAsync(u => deptIds.Contains(u.DeptId)));
    }

    /// <summary>
    /// 更新用户公共信息
    /// </summary>
    /// <param name="updateUserCenterDto"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    public async Task<bool> UpdateCenterAsync(UpdateUserCenterDto updateUserCenterDto)
    {
        var user = await TableWhere(x => x.Id == App.HttpUser.Id).FirstAsync();
        if (user.IsNull())
            throw new BadRequestException("数据不存在！");
        if (!updateUserCenterDto.Phone.IsPhone())
            throw new BadRequestException("电话格式错误");

        var checkUser = await SugarRepository.QueryFirstAsync(x =>
            x.Phone == updateUserCenterDto.Phone && x.Id != App.HttpUser.Id);
        if (checkUser.IsNotNull())
            throw new BadRequestException($"电话=>{checkUser.Phone}=>已存在!");

        user.NickName = updateUserCenterDto.NickName;
        user.Gender = updateUserCenterDto.Gender;
        user.Phone = updateUserCenterDto.Phone;
        return await UpdateEntityAsync(user);
    }

    public async Task<bool> UpdatePasswordAsync(UpdateUserPassDto userPassDto)
    {
        var rsaHelper = new RsaHelper(App.GetOptions<RsaOptions>());
        string oldPassword = rsaHelper.Decrypt(userPassDto.OldPassword);
        string newPassword = rsaHelper.Decrypt(userPassDto.NewPassword);
        string confirmPassword = rsaHelper.Decrypt(userPassDto.ConfirmPassword);

        if (oldPassword == newPassword)
            throw new BadRequestException("新密码不能与旧密码相同");

        if (!newPassword.Equals(confirmPassword))
        {
            throw new BadRequestException("两次输入不匹配");
        }

        var curUser = await TableWhere(x => x.Id == App.HttpUser.Id).FirstAsync();
        if (curUser.IsNull())
            throw new BadRequestException("数据不存在！");
        if (!BCryptHelper.Verify(oldPassword, curUser.Password))
        {
            throw new BadRequestException("旧密码错误");
        }

        //设置用户密码
        curUser.Password = BCryptHelper.Hash(newPassword);
        curUser.PasswordReSetTime = DateTime.Now;
        var isTrue = await UpdateEntityAsync(curUser);
        if (isTrue)
        {
            //清理缓存
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserInfoById +
                                        curUser.Id.ToString().ToMd5String16());

            //退出当前用户
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.OnlineKey +
                                        App.HttpUser.JwtToken.ToMd5String16());
        }

        return true;
    }

    /// <summary>
    /// 修改邮箱
    /// </summary>
    /// <param name="updateUserEmailDto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateEmailAsync(UpdateUserEmailDto updateUserEmailDto)
    {
        var curUser = await TableWhere(x => x.Id == App.HttpUser.Id).FirstAsync();
        if (curUser.IsNull())
            throw new BadRequestException("数据不存在！");
        var rsaHelper = new RsaHelper(App.GetOptions<RsaOptions>());
        string password = rsaHelper.Decrypt(updateUserEmailDto.Password);
        if (!BCryptHelper.Verify(password, curUser.Password))
        {
            throw new BadRequestException("密码错误");
        }

        var code = await App.Cache.GetAsync<string>(
            GlobalConstants.CachePrefix.EmailCaptcha + updateUserEmailDto.Email.ToMd5String16());
        if (code.IsNullOrEmpty() || !code.Equals(updateUserEmailDto.Code))
        {
            throw new BadRequestException("验证码错误");
        }

        curUser.Email = updateUserEmailDto.Email;
        return await UpdateEntityAsync(curUser);
    }

    public async Task<bool> UpdateAvatarAsync(IFormFile file)
    {
        var curUser = await TableWhere(x => x.Id == App.HttpUser.Id).FirstAsync();
        if (curUser.IsNull())
            throw new BadRequestException("数据不存在！");

        var prefix = App.WebHostEnvironment.WebRootPath;
        string avatarName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + IdHelper.GetId() +
                            file.FileName.Substring(Math.Max(file.FileName.LastIndexOf('.'), 0));
        string avatarPath = Path.Combine(prefix, "uploads", "file", "avatar");

        if (!Directory.Exists(avatarPath))
        {
            Directory.CreateDirectory(avatarPath);
        }

        avatarPath = Path.Combine(avatarPath, avatarName);
        await using (var fs = new FileStream(avatarPath, FileMode.CreateNew))
        {
            await file.CopyToAsync(fs);
            fs.Flush();
        }

        string relativePath = Path.GetRelativePath(prefix, avatarPath);
        relativePath = "/" + relativePath.Replace("\\", "/");
        curUser.AvatarPath = relativePath;
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserInfoById +
                                    curUser.Id.ToString().ToMd5String16());
        return await UpdateEntityAsync(curUser);
    }

    #endregion

    #region 用户缓存

    private async Task ClearUserCache(long userId)
    {
        //清理缓存
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserInfoById +
                                    userId.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(
            GlobalConstants.CachePrefix.UserPermissionUrls + userId.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(
            GlobalConstants.CachePrefix.UserPermissionRoles + userId.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserMenuById +
                                    userId.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserDataScopeById +
                                    userId.ToString().ToMd5String16());
    }

    #endregion

    #region 条件表达式

    private async Task<Expression<Func<User, bool>>> GetWhereExpression(UserQueryCriteria userQueryCriteria)
    {
        Expression<Func<User, bool>> whereExpression = u => true;
        if (userQueryCriteria.Id > 0)
        {
            whereExpression = whereExpression.AndAlso(u => u.Id == userQueryCriteria.Id);
        }

        if (!userQueryCriteria.Enabled.IsNullOrEmpty())
        {
            whereExpression = whereExpression.AndAlso(u => u.Enabled == userQueryCriteria.Enabled);
        }

        if (userQueryCriteria.DeptId > 0)
        {
            var depts = await _departmentService.QueryByPIdAsync(userQueryCriteria.DeptId);
            userQueryCriteria.DeptIds = new List<long> { userQueryCriteria.DeptId };
            userQueryCriteria.DeptIds.AddRange(depts.Select(d => d.Id));
            whereExpression = whereExpression.AndAlso(u => userQueryCriteria.DeptIds.Contains(u.DeptId));
        }

        if (!userQueryCriteria.KeyWords.IsNullOrEmpty())
        {
            whereExpression = whereExpression.AndAlso(u =>
                u.Username.Contains(userQueryCriteria.KeyWords) ||
                u.NickName.Contains(userQueryCriteria.KeyWords) || u.Email.Contains(userQueryCriteria.KeyWords));
        }

        if (!userQueryCriteria.CreateTime.IsNullOrEmpty())
        {
            whereExpression = whereExpression.AndAlso(u =>
                u.CreateTime >= userQueryCriteria.CreateTime[0] && u.CreateTime <= userQueryCriteria.CreateTime[1]);
        }

        return whereExpression;
    }

    #endregion
}

public static class SourceClassExtensions
{
    public static UserDto AdaptToUserDto(this User self)
    {
        return self.Adapt<UserDto>();
    }
}
