namespace ChatServer.Application.DTOs
{
    public class ChatMessageDto
    {
        public Guid? Id { get; set; }
        public Guid ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderMailId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTimeOffset TimeStamp { get; set; }
    }
} 