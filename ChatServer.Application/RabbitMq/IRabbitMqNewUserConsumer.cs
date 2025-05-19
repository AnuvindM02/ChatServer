namespace ChatServer.Application.RabbitMq
{
    public interface IRabbitMqNewUserConsumer
    {
        void Consume();
        void Dispose();
    }
}