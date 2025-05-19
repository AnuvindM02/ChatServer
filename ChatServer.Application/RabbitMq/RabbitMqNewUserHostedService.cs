using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.RabbitMq
{
    public class RabbitMqNewUserHostedService: IHostedService
    {
        private readonly IRabbitMqNewUserConsumer _rabbitMqNewUserConsumer;
        private readonly ILogger<RabbitMqNewUserHostedService> _logger;
        public RabbitMqNewUserHostedService(IRabbitMqNewUserConsumer rabbitMqNewUserConsumer, ILogger<RabbitMqNewUserHostedService> logger)
        {
            _rabbitMqNewUserConsumer = rabbitMqNewUserConsumer;
            _logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("hosted service started!!!!!!!");
            _rabbitMqNewUserConsumer.Consume();
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _rabbitMqNewUserConsumer.Dispose();
            return Task.CompletedTask;
        }
    }
}
