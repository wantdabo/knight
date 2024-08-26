﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.IBusiness.Interface.Message.Email;
using Ape.Volo.IBusiness.Interface.Queued;
using Ape.Volo.IBusiness.QueryModel;
using Microsoft.Extensions.Logging;

namespace Ape.Volo.Business.Message.Email;

/// <summary>
/// 电子邮件任务
/// </summary>
public class EmailScheduleTask : IEmailScheduleTask
{
    #region Fields

    private readonly IEmailAccountService _emailAccountService;
    private readonly IEmailSender _emailSender;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly ILogger<EmailScheduleTask> _logger;

    #endregion

    #region Ctor

    public EmailScheduleTask(IEmailAccountService emailAccountService, IEmailSender emailSender,
        IQueuedEmailService queuedEmailService, ILogger<EmailScheduleTask> logger)
    {
        _emailAccountService = emailAccountService;
        _emailSender = emailSender;
        _queuedEmailService = queuedEmailService;
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Executes a task
    /// </summary>
    public virtual async Task ExecuteAsync(long emailId = 0)
    {
        var maxTries = 3;
        QueuedEmailQueryCriteria queuedEmailQueryCriteria = new QueuedEmailQueryCriteria();
        queuedEmailQueryCriteria.MaxTries = maxTries;
        queuedEmailQueryCriteria.IsSend = false;
        if (emailId > 0)
        {
            queuedEmailQueryCriteria.Id = emailId;
        }

        var queuedEmails = await _queuedEmailService.QueryAsync(
            queuedEmailQueryCriteria,
            new Pagination
            {
                PageIndex = 1, PageSize = 100,
                SortFields = new List<string> { "priority asc", "create_time asc" }
            });
        foreach (var queuedEmail in queuedEmails)
        {
            var bcc = string.IsNullOrWhiteSpace(queuedEmail.Bcc)
                ? null
                : queuedEmail.Bcc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var cc = string.IsNullOrWhiteSpace(queuedEmail.Cc)
                ? null
                : queuedEmail.Cc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                await _emailSender.SendEmailAsync(
                    await _emailAccountService.TableWhere(x => x.Id == queuedEmail.EmailAccountId).FirstAsync(),
                    queuedEmail.Subject,
                    queuedEmail.Body,
                    queuedEmail.From,
                    queuedEmail.FromName,
                    queuedEmail.To,
                    queuedEmail.ToName,
                    queuedEmail.ReplyTo,
                    queuedEmail.ReplyToName,
                    bcc,
                    cc);

                queuedEmail.SendTime = DateTime.Now;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Error sending e-mail. {exc.Message}");
            }
            finally
            {
                queuedEmail.SentTries += 1;
                queuedEmail.UpdateBy = "EmailScheduleTask";
                queuedEmail.UpdateTime = DateTime.Now;
                await _queuedEmailService.UpdateTriesAsync(queuedEmail);
            }
        }
    }

    #endregion
}
