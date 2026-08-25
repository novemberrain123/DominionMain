using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace Dominion.API.Dominion.Game
{
    public class GameSessionManager
    {
        private readonly ConcurrentDictionary<Guid, GameSession> _sessions = new();

        public void Add(GameSession session)
        {
            if (!_sessions.TryAdd(session.GameId, session))
            {
                throw new InvalidOperationException(
                    $"Game {session.GameId} already exists.");
            }
        }

        public GameSession? Get(Guid gameId)
        {
            return _sessions.GetValueOrDefault(gameId);
        }

        public bool Remove(Guid gameId)
        {
            return _sessions.TryRemove(gameId, out _);
        }
    }
}