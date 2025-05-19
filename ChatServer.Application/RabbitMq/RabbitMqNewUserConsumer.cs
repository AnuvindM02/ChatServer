using System.Text.Json;
using ChatServer.Application.RabbitMq.Models;
using Microsoft.Extensions.Configuration;
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


        public RabbitMqNewUserConsumer(IConfiguration configuration, ILogger<RabbitMqNewUserConsumer> logger)
        {
            _logger = logger;
            _configuration = configuration;
            string hostName = _configuration["RabbitMq:HostName"]!;
            string userName = _configuration["RabbitMq:UserName"]!;
            string password = _configuration["RabbitMq:Password"]!;
            string port = _configuration["RabbitMq:Port"]!;

            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = Convert.ToInt32(port)
            };
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }

        public void Consume()
        {
            string routingKey = "auth.create.user";
            string queueName = "chat.auth.create.user.queue";

            string exchangeName = _configuration["RabbitMQ:Auth_Exchange"]!;
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);

            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            _channel.QueueBind(queueName, exchangeName, routingKey);

            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, args) =>
            {
                _logger.LogWarning("________Running hosted consumer______________________");
                var body = args.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                if (message != null)
                {
                    var newUserMessage = JsonSerializer.Deserialize<NewUserMessage>(message)!;
                    _logger.LogInformation($"Chat Service Received Message: {newUserMessage.UserId}");
                }
                _logger.LogWarning("Consumer attached to queue: " + queueName);
            };
            _channel.BasicConsume(queueName, autoAck: true, consumer);
        }
    }
}
