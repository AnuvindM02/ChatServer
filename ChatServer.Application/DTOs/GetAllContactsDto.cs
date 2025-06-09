namespace ChatServer.Application.DTOs
{
    public class GetAllContactsDto
    {
        public IEnumerable<ContactCardDto>? Users { get; set; }
        public DateTimeOffset? NextCursor { get; set; }
    }
}