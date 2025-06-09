using ChatServer.Domain.Enums;

namespace ChatServer.Domain.Entities
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public ConversationType Type { get; set; }
        public string? GroupName { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public ICollection<UserConversation> UserConversations { get; set; } = [];
        public ICollection<Message> Messages { get; set; } = [];
    }
}
