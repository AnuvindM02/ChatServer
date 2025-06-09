using ChatServer.Application.DTOs;
using ChatServer.Domain.Entities;

namespace ChatServer.Application.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatMessageDto> SaveMessageAsync(ChatMessageDto message);
        Task<IEnumerable<ChatMessageDto>> GetConversationHistoryAsync(Guid conversationId, int skip = 0, int take = 50);
        Task<Guid> CreateP2PConversationAsync(int user1Id, int user2Id, CancellationToken cancellationToken);
        Task<Conversation> CreateGroupConversationAsync(string groupName, IEnumerable<int> userIds);
        Task AddUserToGroupAsync(Guid conversationId, int userId);
        Task RemoveUserFromGroupAsync(Guid conversationId, int userId);
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId);
        Task<IEnumerable<int>> GetParticipantUserIdsAsync(Guid conversationId);
        Task<IEnumerable<ContactCardDto>> GetUserContactsAsync(int userId, DateTimeOffset? cursor, int limit, string? search, CancellationToken cancellationToken);
    }
} 