using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;
using OrderService.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using IModel = RabbitMQ.Client.IModel;

namespace OrderService.Services;

public class RabbitMQConsumer : BackgroundService
{
    private string _queueName;
        private readonly string _routingKey = "create.organization"; // Replace with your routing key if needed

        private IConnection _connection;
        private IModel _channel;
        private readonly IConfiguration _configuration;
        private IServiceProvider _serviceProvider;

        public RabbitMQConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            // Establish connection and create a channel
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare a queue (optional for pre-declared queues like amq.topic)
            _queueName = _channel.QueueDeclare().QueueName;

            // Bind to the topic exchange
            _channel.QueueBind(queue: _queueName,
                               exchange: "amq.topic",
                               routingKey: _routingKey);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => StopRabbitMQ());

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Process the message
                await HandleMessageAsync(message);
            };

            _channel.BasicConsume(queue: _queueName,
                                  autoAck: true,
                                  consumer: consumer);

            return Task.CompletedTask;
        }

        private Task HandleMessageAsync(string message)
        {
            // Add your message processing logic here
            Console.WriteLine($"Received message: {message}");
            using (var scope = _serviceProvider.CreateScope())
            {
                var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
                
                // Verwerk RabbitMQ-event
                tenantContext.SetConnectionString(message);
            }

            return Task.CompletedTask;
        }

        private async void StopRabbitMQ()
        {
            _channel?.Close();
            _connection?.Close();
        }

        public override void Dispose()
        {
            StopRabbitMQ();
            base.Dispose();
        }
}