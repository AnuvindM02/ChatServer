namespace ChatServer.Domain.Entities
{
    public class User
    {
        public int AuthUserId { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<UserConversation> UserConversations { get; set; } = [];
        public ICollection<Message> Messages { get; set; } = [];
    }
}
