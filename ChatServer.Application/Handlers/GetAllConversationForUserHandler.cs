using ChatServer.Application.DTOs;
using ChatServer.Application.Queries;
using ChatServer.Application.Interfaces;
using MediatR;

namespace ChatServer.Application.Handlers
{
    public class GetAllConversationForUserHandler(IChatRepository _chatRepository) : IRequestHandler<GetAllConversationForUserQuery, GetAllContactsDto>
    {
        public async Task<GetAllContactsDto> Handle(GetAllConversationForUserQuery request, CancellationToken cancellationToken)
        {
            var conversations = await _chatRepository.GetUserContactsAsync(request.UserId
                , request.Cursor, request.Limit, request.Search, cancellationToken);

            return new GetAllContactsDto
            {
                NextCursor = conversations != null ? conversations.LastOrDefault()?.CreatedAt : null,
                Users = conversations
            };
        }
    }
}
