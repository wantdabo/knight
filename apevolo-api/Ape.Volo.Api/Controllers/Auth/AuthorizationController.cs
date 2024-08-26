using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Api.Authentication.Jwt;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Caches;
using Ape.Volo.Common.ConfigOptions;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.IBusiness.Dto.Permission;
using Ape.Volo.IBusiness.Interface.Permission;
using Ape.Volo.IBusiness.Interface.Queued;
using Ape.Volo.IBusiness.Interface.System;
using Ape.Volo.IBusiness.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ape.Volo.Api.Controllers.Auth;

/// <summary>
/// 授权管理
/// </summary>
[Area("授权管理")]
[Route("/auth")]
public class AuthorizationController : BaseApiController
{
    #region 字段

    private readonly IUserService _userService;
    private readonly IPermissionService _permissionService;
    private readonly IOnlineUserService _onlineUserService;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly ITokenService _tokenService;
    private readonly ITokenBlacklistService _tokenBlacklistService;

    #endregion

    #region 构造函数

    public AuthorizationController(IUserService userService, IPermissionService permissionService,
        IOnlineUserService onlineUserService, IQueuedEmailService queuedEmailService,
        ITokenService tokenService, ITokenBlacklistService tokenBlacklistService)
    {
        _userService = userService;
        _permissionService = permissionService;
        _onlineUserService = onlineUserService;
        _queuedEmailService = queuedEmailService;
        _tokenService = tokenService;
        _tokenBlacklistService = tokenBlacklistService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="authUser"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("login")]
    [Description("用户登录")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Login([FromBody] LoginAuthUser authUser)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var userDto = await _userService.QueryByNameAsync(authUser.Username);
        if (userDto == null) return Error("用户不存在");
        var password = new RsaHelper(App.GetOptions<RsaOptions>()).Decrypt(authUser.Password);
        if (!BCryptHelper.Verify(password, userDto.Password))
            return Error("密码错误");

        if (!userDto.Enabled) return Error("用户未激活");

        var netUser = await _userService.QueryByIdAsync(userDto.Id);
        if (netUser != null)
        {
            return await LoginResult(netUser, "login");
        }

        return Error();
    }


    /// <summary>
    /// 刷新Token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("refreshToken")]
    [Description("刷新Token")]
    [AllowAnonymous]
    [NotAudit]
    public async Task<ActionResult<object>> RefreshToken(string token = "")
    {
        if (token.IsNullOrEmpty())
        {
            return Error("token已丢失，请重新登录！");
        }

        var tokenMd5 = token.ToMd5String16();
        var tokenBlacklist = await _tokenBlacklistService.TableWhere(x => x.AccessToken == tokenMd5, null, null, true)
            .FirstAsync();
        if (tokenBlacklist.IsNull())
        {
            var jwtSecurityToken = await _tokenService.ReadJwtToken(token);
            if (jwtSecurityToken != null)
            {
                var userId = Convert.ToInt64(jwtSecurityToken.Claims
                    .FirstOrDefault(s => s.Type == AuthConstants.JwtClaimTypes.Jti)?.Value);
                var loginTime = Convert.ToInt64(jwtSecurityToken.Claims
                    .FirstOrDefault(s => s.Type == AuthConstants.JwtClaimTypes.Iat)?.Value).TicksToDateTime();
                var nowTime = DateTime.Now.ToLocalTime();
                var refreshTime = loginTime.AddHours(App.GetOptions<JwtAuthOptions>().RefreshTokenExpires);
                // 允许token刷新时间内
                if (nowTime <= refreshTime)
                {
                    var netUser = await _userService.QueryByIdAsync(userId);
                    if (netUser.IsNotNull())
                    {
                        if (netUser.UpdateTime == null || netUser.UpdateTime < loginTime)
                        {
                            return await LoginResult(netUser, "refresh");
                        }
                    }
                }
            }
        }

        return Error("token验证失败，请重新登录！");
    }


    [HttpGet]
    [Route("info")]
    [Description("个人信息")]
    [NotAudit]
    public async Task<ActionResult<object>> GetInfo()
    {
        var netUser = await _userService.QueryByIdAsync(App.HttpUser.Id);
        var permissionRoles = await _permissionService.GetPermissionRolesAsync(netUser.Id);
        permissionRoles.AddRange(netUser.Roles.Select(r => r.Permission));
        var jwtUserVo = await _onlineUserService.CreateJwtUserAsync(netUser, permissionRoles);
        return jwtUserVo.ToJson();
    }

    /// <summary>
    /// 获取验证码，申请变更邮箱
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Description("获取邮箱验证码")]
    [Route("code/reset/email")]
    public async Task<ActionResult<object>> ResetEmail(string email)
    {
        if (!email.IsEmail()) throw new BadRequestException("请输入正确的邮箱！");

        var isTrue = await _queuedEmailService.ResetEmail(email, "EmailVerificationCode");
        return isTrue ? Success() : Error();
    }


    /// <summary>
    /// 系统用户登出
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    [Route("logout")]
    [Description("用户登出")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Logout()
    {
        //清理缓存
        if (!App.HttpUser.IsNotNull()) return Success();
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.OnlineKey +
                                    App.HttpUser.JwtToken.ToMd5String16());
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserInfoById +
                                    App.HttpUser.Id.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserMenuById +
                                    App.HttpUser.Id.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserPermissionRoles +
                                    App.HttpUser.Id.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserPermissionUrls +
                                    App.HttpUser.Id.ToString().ToMd5String16());
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserDataScopeById +
                                    App.HttpUser.Id.ToString().ToMd5String16());


        return Success();
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 登录或刷新token相应结果
    /// </summary>
    /// <param name="userDto"></param>
    /// <param name="type">login:登录,refresh:刷新token</param>
    /// <returns></returns>
    private async Task<string> LoginResult(UserDto userDto, string type)
    {
        var permissionRoles = new List<string>();
        bool refresh = true;
        if (type.Equals("login"))
        {
            refresh = false;
            permissionRoles = await _permissionService.GetPermissionRolesAsync(userDto.Id);
            permissionRoles.AddRange(userDto.Roles.Select(r => r.Permission));
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var jwtUserVo = await _onlineUserService.CreateJwtUserAsync(userDto, permissionRoles);
        var loginUserInfo = await _onlineUserService.SaveLoginUserAsync(jwtUserVo, remoteIp);
        var token = await _tokenService.IssueTokenAsync(loginUserInfo, refresh);
        loginUserInfo.AccessToken = refresh ? token.RefreshToken : token.AccessToken;
        var onlineKey = loginUserInfo.AccessToken.ToMd5String16();
        await App.Cache.SetAsync(
            GlobalConstants.CachePrefix.OnlineKey + onlineKey,
            loginUserInfo, TimeSpan.FromHours(2), CacheExpireType.Absolute);

        switch (type)
        {
            case "login":
                var dic = new Dictionary<string, object>
                    { { "user", jwtUserVo }, { "token", token } };
                return dic.ToJson();
            case "refresh":
                return token.ToJson();
            default:
                return "";
        }
    }

    #endregion
}
