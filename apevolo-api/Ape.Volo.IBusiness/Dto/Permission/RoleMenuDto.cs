﻿using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Permission;

namespace Ape.Volo.IBusiness.Dto.Permission;

/// <summary>
/// 角色菜单Dto
/// </summary>
[AutoMapping(typeof(Menu), typeof(RoleMenuDto))]
public class RoleMenuDto
{
    /// <summary>
    /// ID
    /// </summary>
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; set; }
}
