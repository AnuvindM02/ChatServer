using ChatServer.Application.DTOs;
using ChatServer.Application.Interfaces;
using ChatServer.Domain.Entities;
using ChatServer.Domain.Enums;
using ChatServer.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Infrastructure.Repositories
{
    public class ChatRepository(ApplicationDbContext _context) : IChatRepository
    {

        public async Task<ChatMessageDto> SaveMessageAsync(ChatMessageDto messageDto)
        {
            var isParticipant = await _context.UserConversations
                .AnyAsync(uc => uc.ConversationId == messageDto.ConversationId && uc.UserId == messageDto.SenderId);

            if (!isParticipant)
                throw new HubException("Sender is not a participant of this conversation");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = messageDto.ConversationId,
                SenderId = messageDto.SenderId,
                Content = messageDto.Content,
                CreatedAt = messageDto.CreatedAt,
                Conversation = await _context.Conversations.FindAsync(messageDto.ConversationId)
                    ?? throw new KeyNotFoundException($"Conversation not found: ConversationId - {messageDto.ConversationId}"),
                Sender = await _context.Users.FindAsync(messageDto.SenderId)
                    ?? throw new KeyNotFoundException($"User not found: UserId - {messageDto.SenderId}")
            };

            _context.Messages.Add(message);
            messageDto.SenderMailId = message.Sender.Email;
            return messageDto;
        }

        public async Task<IEnumerable<ChatMessageDto>> GetConversationHistoryAsync(Guid conversationId, int skip = 0, int take = 50)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    SenderId = m.SenderId,
                    SenderMailId = m.Sender.Email,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<Guid> CreateP2PConversationAsync(int user1Id, int user2Id, CancellationToken cancellationToken)
        {
            //If a conversation already exists between the 2 users, return the conversation id

            var existingConversationId = await _context.UserConversations
                                .Where(uc => (uc.UserId == user1Id || uc.UserId == user2Id)
                                && uc.Conversation.Type == ConversationType.P2P)
                                .GroupBy(uc => uc.ConversationId)
                                .Where(g =>
                                    g.Count() == 2 &&
                                    g.Any(x => x.UserId == user1Id) &&
                                    g.Any(x => x.UserId == user2Id))
                                .Select(g => g.Key)
                                .FirstOrDefaultAsync(cancellationToken);

            if(existingConversationId != Guid.Empty)
                return existingConversationId;

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.P2P,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Conversations.Add(conversation);

            var userConversations = new[]
            {
                new UserConversation { UserId = user1Id, Conversation = conversation,
                User = await _context.Users.FindAsync(user1Id)
                        ?? throw new KeyNotFoundException($"User not found: UserId - {user1Id}"),
                ConversationId = conversation.Id},

                new UserConversation { UserId = user2Id, Conversation = conversation,
                    User = await _context.Users.FindAsync(user2Id)
                        ?? throw new KeyNotFoundException($"User not found: UserId - {user2Id}"),
                ConversationId = conversation.Id}
            };

            _context.UserConversations.AddRange(userConversations);
            await _context.SaveChangesAsync(cancellationToken);
            return conversation.Id;
        }

        public async Task<Conversation> CreateGroupConversationAsync(string groupName, IEnumerable<int> userIds)
        {
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.Group,
                GroupName = groupName,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Conversations.Add(conversation);

            var userConversations = userIds.Select((userId, index) => new UserConversation
            {
                UserId = userId,
                Conversation = conversation,
                IsAdmin = index == 0, // You might want to set the first user as admin
                User = _context.Users.Find(userId) ?? throw new KeyNotFoundException($"User not found: UserId - {userId}")
            }).ToList();

            _context.UserConversations.AddRange(userConversations);
            return conversation;
        }

        public async Task AddUserToGroupAsync(Guid conversationId, int userId)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.Type == ConversationType.Group);

            if (conversation == null)
                throw new ArgumentException("Group conversation not found");

            var userConversation = new UserConversation
            {
                UserId = userId,
                ConversationId = conversationId,
                IsAdmin = false,
                Conversation = conversation,
                User = await _context.Users.FindAsync(userId)
                        ?? throw new KeyNotFoundException($"User not found: UserId - {userId}")
            };

            _context.UserConversations.Add(userConversation);
        }

        public async Task RemoveUserFromGroupAsync(Guid conversationId, int userId)
        {
            var userConversation = await _context.UserConversations
                .FirstOrDefaultAsync(uc => uc.ConversationId == conversationId && uc.UserId == userId);

            if (userConversation != null)
            {
                _context.UserConversations.Remove(userConversation);
            }
        }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId)
        {
            return await _context.UserConversations
                .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Conversation)
                .ThenInclude(c => c.UserConversations)
                .ThenInclude(uc => uc.User)
                .Select(uc => uc.Conversation)
                .ToListAsync();
        }

        public async Task<IEnumerable<ContactCardDto>> GetUserContactsAsync(int userId, DateTimeOffset? cursor, int limit, string? search, CancellationToken cancellationToken)
        {
            var query = _context.UserConversations
                .Where(uc => uc.UserId == userId)
                .SelectMany(uc => uc.Conversation.UserConversations)
                .Where(uc => uc.UserId != userId && uc.Conversation.Type == ConversationType.P2P).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var loweredSearch = search.ToLower();

                query = query.Where(u =>
                    EF.Functions.ILike(u.User.FirstName, $"{loweredSearch}%") ||
                    (u.User.Lastname != null && EF.Functions.ILike(u.User.Lastname, $"{loweredSearch}%")) ||
                    EF.Functions.ILike(u.User.Email, $"{loweredSearch}%"));
            }

            if (cursor.HasValue)
            {
                query = query.Where(uc => uc.Conversation.UpdatedAt < cursor.Value);
            }

            var contacts = await query
                .Select(uc => new ContactCardDto
                {
                    UserId = uc.User.AuthUserId,
                    FirstName = uc.User.FirstName,
                    MiddleName = uc.User.Middlename,
                    LastName = uc.User.Lastname,
                    Email = uc.User.Email,
                    ConversationId = uc.ConversationId,
                    CreatedAt = uc.Conversation.UpdatedAt
                })
                .Distinct()
                .OrderByDescending(uc => uc.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return contacts;
        }


        public async Task<IEnumerable<int>> GetParticipantUserIdsAsync(Guid conversationId)
        {
            return await _context.UserConversations
                    .Where(uc => uc.ConversationId == conversationId)
                    .Select(uc => uc.UserId)
                    .ToListAsync();
        }
    }
}
