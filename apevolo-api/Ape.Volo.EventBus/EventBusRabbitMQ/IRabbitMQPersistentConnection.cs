﻿using RabbitMQ.Client;

namespace Ape.Volo.EventBus.EventBusRabbitMQ;

/// <summary>
/// RabbitMQ 持久连接
/// </summary>
public interface IRabbitMQPersistentConnection
{
    /// <summary>
    /// 已连接
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 尝试连接
    /// </summary>
    /// <returns></returns>
    bool TryConnect();

    IModel CreateModel();
}
