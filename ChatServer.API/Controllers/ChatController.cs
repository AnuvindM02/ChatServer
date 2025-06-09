using Microsoft.AspNetCore.Mvc;
using ChatServer.Application.Interfaces;
using ChatServer.Application.DTOs;
using ChatServer.Domain.Entities;
using MediatR;
using ChatServer.Application.Queries;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatServer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatService;
        private readonly IMediator _mediator;

        public ChatController(IChatRepository chatService, IMediator mediator)
        {
            _chatService = chatService;
            _mediator = mediator;
        }

        [HttpGet("conversations/{userId}")]
        public async Task<ActionResult<IEnumerable<Conversation>>> GetUserConversations(int userId)
        {
            var conversations = await _chatService.GetUserConversationsAsync(userId);
            return Ok(conversations);
        }

        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetConversationHistory(
            Guid conversationId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            var messages = await _chatService.GetConversationHistoryAsync(conversationId, skip, take);
            return Ok(messages);
        }

        [HttpPost("conversations/p2p")]
        public async Task<ActionResult<Guid>> CreateP2PConversation(
            [FromBody] CreateP2PConversationRequest request, CancellationToken cancellationToken)
        {
            var conversationId = await _chatService.CreateP2PConversationAsync(request.User1Id, request.User2Id, cancellationToken);
            return Ok(conversationId);
        }

        [HttpPost("conversations/group")]
        public async Task<ActionResult<Conversation>> CreateGroupConversation(
            [FromBody] CreateGroupConversationRequest request)
        {
            var conversation = await _chatService.CreateGroupConversationAsync(request.GroupName, request.UserIds);
            return Ok(conversation);
        }

        [HttpPost("conversations/{conversationId}/users/{userId}")]
        public async Task<IActionResult> AddUserToGroup(Guid conversationId, int userId)
        {
            await _chatService.AddUserToGroupAsync(conversationId, userId);
            return Ok();
        }

        [HttpDelete("conversations/{conversationId}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromGroup(Guid conversationId, int userId)
        {
            await _chatService.RemoveUserFromGroupAsync(conversationId, userId);
            return Ok();
        }

        [HttpGet("conversations")]
        public async Task<ActionResult<GetAllContactsDto>> GetConversations(DateTimeOffset? Cursor, string? Search, CancellationToken cancellationToken, int Limit = 10)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user ID in token.");
            }

            var response = await _mediator.Send(new GetAllConversationForUserQuery(userId, Cursor, Search, Limit), cancellationToken);
            return Ok(response);
        }
    }

    public class CreateP2PConversationRequest
    {
        public int User1Id { get; set; }
        public int User2Id { get; set; }
    }

    public class CreateGroupConversationRequest
    {
        public string GroupName { get; set; } = string.Empty;
        public IEnumerable<int> UserIds { get; set; } = [];
    }
} 