using Microsoft.AspNetCore.SignalR;

namespace Dominion.API.Hubs;

public sealed class GameHub : Hub
{
    public Task JoinGame(Guid gameId)
    {
        return Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetGroupName(gameId));
    }

    public Task LeaveGame(Guid gameId)
    {
        return Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            GetGroupName(gameId));
    }

    public static string GetGroupName(Guid gameId)
    {
        return $"game:{gameId}";
    }
}