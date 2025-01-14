using System.Text;
using System.Text.Json;
using OrderService.Models;
using RabbitMQ.Client;

namespace OrderService.Services;

public class LogPublisher : ILogPublisher
{
    private readonly IConfiguration _configuration;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    
    public LogPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
        
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"]
        };

        // Establish connection and create a channel
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }
    
    public void SendMessage(LogMessage message)
    {
        var logMessage = JsonSerializer.Serialize(message);

        var body = Encoding.UTF8.GetBytes(logMessage); 
        
        var routingKey = $"logs.{message.LogLevel.ToLower()}";
        _channel.BasicPublish(
            exchange: "amq.topic", // The topic exchange
            routingKey: routingKey, // Routing key to target specific queues
            basicProperties: null, // Message properties (can add headers, etc.)
            body: body);

        Console.WriteLine($"Message sent to {routingKey}: {message}");
    }
    
    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}