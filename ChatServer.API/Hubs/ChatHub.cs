using Microsoft.AspNetCore.SignalR;
using ChatServer.Application.DTOs;
using ChatServer.Application.Interfaces;
using ChatServer.Application.Interfaces.Repositories;

namespace ChatServer.API.Hubs
{
    public class ChatHub: Hub
    {
        private readonly IChatRepository _chatService;
        private readonly IUnitOfWork _unitOfWork;
        public ChatHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _chatService = _unitOfWork.CustomRepository<IChatRepository>();
        }

        public async Task SendPrivateMessage(ChatMessageDto message, CancellationToken cancellationToken)
        {
            var savedMessage = await _chatService.SaveMessageAsync(message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            // You can add user connection tracking here if needed
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // You can add user disconnection handling here if needed
            await base.OnDisconnectedAsync(exception);
        }
    }
}
