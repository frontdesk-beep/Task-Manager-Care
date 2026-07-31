using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Task_Manager_Care.Helpers
{
    public class CustomUserIdProvider : IUserIdProvider
    {
            public string? GetUserId(HubConnectionContext connection)
            {
                return connection.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;
            }
    }
}
