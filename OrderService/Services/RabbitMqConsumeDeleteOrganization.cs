using System.Text;
using OrderService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderService.Services;

public class RabbitMqConsumeDeleteOrganization: BackgroundService
{
        private readonly string _queueName = "organization-delete-queue-order";
        private readonly string _routingKey = "organization.delete";

        private IConnection _connection;
        private IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceProvider;
        private readonly ILogPublisher _logPublisher;

        public RabbitMqConsumeDeleteOrganization(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogPublisher logPublisher)
        {
            _serviceProvider = serviceScopeFactory;
            _configuration = configuration;
            _logPublisher = logPublisher;
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

            // Bind to the topic exchange
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(_queueName, "amq.topic", _routingKey);
            _channel.BasicQos(0, 1, false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
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
            catch (Exception e)
            {
                _logPublisher.SendMessage(new LogMessage
                {
                    ServiceName = "OrderService",
                    LogLevel = "Error",
                    Message = $"Failed to receive message. Error: {e.Message}",
                    Timestamp = DateTime.Now,
                });
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task HandleMessageAsync(string message)
        {
            // Add your message processing logic here
            Console.WriteLine($"Received message: {message}");
            using var scope = _serviceProvider.CreateScope();
            
            var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();
            await migrationService.RemoveSchemaAsync(message);
        }

        private void StopRabbitMQ()
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