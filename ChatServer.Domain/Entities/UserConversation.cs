namespace ChatServer.Domain.Entities
{
    public class UserConversation
    {
        public required int UserId { get; set; }
        public required User User { get; set; }
        public Guid ConversationId { get; set; }
        public required Conversation Conversation { get; set; }
        public bool IsAdmin { get; set; } = false;
    }
}
