using Microsoft.AspNetCore.SignalR;
using ChatServer.Application.DTOs;
using ChatServer.Application.Interfaces;
using ChatServer.Application.Interfaces.Repositories;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using ChatServer.Domain.Entities;

namespace ChatServer.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Conversation> _conversationRepository;
        public ChatHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _chatService = _unitOfWork.CustomRepository<IChatRepository>();
            _conversationRepository = _unitOfWork.GetRepository<Conversation>();
        }

        public async Task SendPrivateMessage(ChatMessageDto message)
        {
            var savedMessage = await _chatService.SaveMessageAsync(message);
            var conversation = await _conversationRepository.GetByIdAsync(message.ConversationId)
                ??throw new KeyNotFoundException($"Conversation not found ${message.ConversationId}");
            conversation.UpdatedAt = DateTimeOffset.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            var participantIds = await _chatService.GetParticipantUserIdsAsync(message.ConversationId);
            foreach (var participantId in participantIds.Where(id => id != message.SenderId))
            {
                await Clients.User(participantId.ToString()).SendAsync("ReceiveMessage", Context.UserIdentifier, savedMessage);
            }
        }

        public async Task SendGroupMessage(ChatMessageDto message, CancellationToken cancellationToken)
        {
            var savedMessage = await _chatService.SaveMessageAsync(message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await Clients.Group(message.ConversationId.ToString()).SendAsync("ReceiveGroupMessage", Context.UserIdentifier, savedMessage);
        }

        public async Task JoinConversation(Guid conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        public async Task LeaveConversation(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"✅ User connected: {userId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"❌ User disconnected: {Context.UserIdentifier}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
