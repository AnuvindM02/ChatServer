using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatServer.Application.RabbitMq
{
    public class RabbitMqNewUserHostedService: IHostedService
    {
        private readonly IRabbitMqNewUserConsumer _rabbitMqNewUserConsumer;
        public RabbitMqNewUserHostedService(IRabbitMqNewUserConsumer rabbitMqNewUserConsumer)
        {
            _rabbitMqNewUserConsumer = rabbitMqNewUserConsumer;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
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
