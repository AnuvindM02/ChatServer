using System.Text.Json;
using AutoMapper;
using ChatServer.Application.Commands;
using ChatServer.Application.RabbitMq.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChatServer.Application.RabbitMq
{
    public class RabbitMqNewUserConsumer : IDisposable, IRabbitMqNewUserConsumer
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMqNewUserConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMqNewUserConsumer(IConfiguration configuration, ILogger<RabbitMqNewUserConsumer> logger, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            string hostName = _configuration["RabbitMq:HostName"]!;
            string userName = _configuration["RabbitMq:UserName"]!;
            string password = _configuration["RabbitMq:Password"]!;
            string port = _configuration["RabbitMq:Port"]!;
            int retryCount = 10;

            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = Convert.ToInt32(port)
            };

            for (int i = 1; i <= retryCount; i++)
            {
                try
                {
                    _logger.LogInformation("Attempting to connect to RabbitMQ... Attempt #{Attempt}", i);
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _logger.LogInformation("✅ Connected to RabbitMQ.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "❌ Failed to connect to RabbitMQ. Attempt #{Attempt}", i);
                    if (i == retryCount)
                    {
                        _logger.LogError("🛑 Giving up after {RetryCount} attempts", retryCount);
                        throw;
                    }

                    Thread.Sleep(3000); // Wait 3 seconds before retrying
                }
            }

            _serviceProvider = serviceProvider;
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Consume()
        {
            string routingKey = "auth.create.user";
            string queueName = "chat.auth.create.user.queue";

            string exchangeName = _configuration["RabbitMQ:Auth_Exchange"]!;
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);

            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            _channel.QueueBind(queueName, exchangeName, routingKey);

            EventingBasicConsumer consumer = new (_channel);
            consumer.Received += async (sender, args) =>
            {
                using var scope = _serviceProvider.CreateScope();

                var body = args.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                if (message != null)
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                    var newUserMessage = JsonSerializer.Deserialize<NewUserMessage>(message)!;
                    var newUserRequest = mapper.Map<CreateUserCommand>(newUserMessage);
                    await mediator.Send(newUserRequest);
                }
            };
            _channel.BasicConsume(queueName, autoAck: true, consumer);
        }
    }
}
