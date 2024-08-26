﻿using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.IBusiness.RequestModel;

/// <summary>
/// 登录用户
/// </summary>
public class LoginAuthUser
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    public string Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    public string Password { get; set; }
}
