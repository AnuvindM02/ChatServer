namespace ChatServer.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; set; }
        public required Guid ConversationId { get; set; }
        public required Conversation Conversation { get; set; }
        public required int SenderId { get; set; }
        public required User Sender { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
