using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace ChatServer.Infrastructure.Identity
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var userIdClaim = connection.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim?.Value;
        }
    }
}
