using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderService.Services;

public class RabbitMqSenderOrganization : BackgroundService
{
    private readonly IModel _channel;
    private readonly string _replyQueue;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingRequests = new();
    
    public RabbitMqSenderOrganization(IConfiguration configuration)
    {
        var _configuration = configuration;
        
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"]
        };

        // Establish connection and create a channel
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
        
        
        
        _replyQueue = _channel.QueueDeclare().QueueName;

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (sender, e) =>
        {
            var correlationId = e.BasicProperties.CorrelationId;
            var response = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"Received message {response}");
            if (_pendingRequests.TryRemove(correlationId, out var tcs))
            {
                tcs.SetResult(response);
            }

            _channel.BasicAck(e.DeliveryTag, false);
        };

        _channel.BasicConsume(_replyQueue, false, consumer);

        _pendingRequests = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        
    }

    
    public Task<string> SendMessage(string message)
    {
        var correlationId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<string>();
        _pendingRequests.TryAdd(correlationId, tcs);
        var body = Encoding.UTF8.GetBytes(message);
        var routingKey = "request-queue";
        var properties = _channel.CreateBasicProperties();
        properties.ReplyTo = _replyQueue;
        properties.CorrelationId = correlationId;
        _channel.BasicPublish(
            exchange: "", // The topic exchange
            routingKey: routingKey, // Routing key to target specific queues
            basicProperties: properties, // Message properties (can add headers, etc.)
            body: body);

        Console.WriteLine($"Respond to: {properties.ReplyTo}");
        Console.WriteLine($"Message sent to {routingKey}: {correlationId}");

        return tcs.Task;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Houd de service draaiende totdat deze wordt gestopt
        await Task.CompletedTask;
    }
}