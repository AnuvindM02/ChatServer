using ChatServer.Application.DTOs;
using MediatR;

namespace ChatServer.Application.Queries;
    public sealed record GetAllConversationForUserQuery(int UserId, DateTimeOffset? Cursor, string? Search, int Limit = 10) :IRequest<GetAllContactsDto>;
