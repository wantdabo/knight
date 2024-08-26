using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Message.Email;
using Ape.Volo.IBusiness.Base;

namespace Ape.Volo.IBusiness.Dto.Message.Email;

/// <summary>
/// 邮箱模板Dto
/// </summary>
[AutoMapping(typeof(EmailMessageTemplate), typeof(CreateUpdateEmailMessageTemplateDto))]
public class CreateUpdateEmailMessageTemplateDto : BaseEntityDto<long>
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// 抄送邮箱地址
    /// </summary>
    public string BccEmailAddresses { get; set; }

    /// <summary>
    /// 主题
    /// </summary>
    [Required]
    public string Subject { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Required]
    public string Body { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 邮箱账户标识符
    /// </summary>
    public long EmailAccountId { get; set; }
}
