using System;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.Entity.Base;

/// <summary>
/// 实体基类
/// </summary>
[SugarIndex("index_{table}_CreateBy", nameof(CreateBy), OrderByType.Asc)]
[SugarIndex("index_{table}_IsDeleted", nameof(IsDeleted), OrderByType.Asc)]
public class BaseEntity : RootKey<long>, ICreateByEntity, ISoftDeletedEntity
{
    /// <summary>
    /// 创建者名称
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string CreateBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新者名称
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string UpdateBy { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDeleted { get; set; }
}
