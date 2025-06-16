using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;


namespace MatchLoveWeb.SignaIR
{
    
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // ưu tiên claim NameIdentifier, hoặc custom "UserId"
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? connection.User?.FindFirst("UserId")?.Value;
        }
    }

}
