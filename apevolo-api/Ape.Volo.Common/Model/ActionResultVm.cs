﻿using Microsoft.AspNetCore.Http;

namespace Ape.Volo.Common.Model;

/// <summary>
/// 请求响应结果
/// </summary>
public class ActionResultVm
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int Status { get; set; } = StatusCodes.Status200OK;

    /// <summary>
    /// 错误
    /// </summary>
    public ActionError ActionError { get; set; }

    /// <summary>
    /// 返回消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public string Timestamp { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    public string Path { get; set; }
}
