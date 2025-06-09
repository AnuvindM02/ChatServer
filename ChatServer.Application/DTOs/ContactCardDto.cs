namespace ChatServer.Application.DTOs
{
    public class ContactCardDto
    {
        public required int UserId { get; set; }
        public required string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public required string Email { get; set; }
        public required Guid ConversationId { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
    }
}
