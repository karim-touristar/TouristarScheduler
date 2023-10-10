using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TouristarModels.Enums;
using TouristarScheduler.Contracts;
using TouristarScheduler.Models;

namespace TouristarScheduler.Services;

public class PublishingService : IPublishingService
{
    private readonly ILogger<PublishingService> _logger;
    private readonly string _hostName;
    private readonly string _username;
    private readonly string _password;
    private readonly int _port;

    public PublishingService(ILogger<PublishingService> logger, IOptionsMonitor<ConnectionConfig> optionsMonitor)
    {
        _logger = logger;
        _hostName = optionsMonitor.CurrentValue.RabbitConnection;
        _username = optionsMonitor.CurrentValue.RabbitUser;
        _password = optionsMonitor.CurrentValue.RabbitPassword;
        _port = int.Parse(optionsMonitor.CurrentValue.RabbitPort);
    }

    public void AddMessage(string message, Queues queue)
    {
        var channel = GetChannel();
        if (channel == null)
        {
            _logger.LogError("Could not get channel to declare queue.");
            return;
        }

        DeclareQueue(channel, queue);
        PublishMessage(message, channel, queue);
    }

    private IModel? GetChannel()
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            UserName = _username,
            Password = _password,
            Port = _port
        };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        return channel;
    }

    private static void DeclareQueue(IModel channel, Queues queue)
        => channel.QueueDeclare(
            queue: Enum.GetName(typeof(Queues), queue),
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

    private static void PublishMessage(string message, IModel channel, Queues queue)
    {
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: string.Empty,
            routingKey: Enum.GetName(typeof(Queues), queue),
            basicProperties: null,
            body: body);
    }
}