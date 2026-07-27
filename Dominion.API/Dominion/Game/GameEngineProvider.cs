using System.Collections.Concurrent;

namespace Dominion.API.Dominion.Game
{
    public class GameEngineProvider
    {
        private readonly ConcurrentDictionary<Guid, GameEngine> _engines = new();

        public void Add(GameEngine engine)
        {
            if (!_engines.TryAdd(engine.State.GameId, engine))
            {
                throw new InvalidOperationException(
                    $"Game {engine.State.GameId} already exists.");
            }
        }

        public GameEngine? Get(Guid gameId)
        {
            return _engines.GetValueOrDefault(gameId);
        }

        public bool Remove(Guid gameId)
        {
            return _engines.TryRemove(gameId, out _);
        }
    }
}