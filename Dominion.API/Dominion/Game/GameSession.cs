namespace Dominion.API.Dominion.Game
{
    public class GameSession
    {
        public Guid GameId { get; }
        public string Mode { get; } 
        public GameEngine Engine { get; }
        public SemaphoreSlim Lock { get; }
        public GameSession(Guid gameId, string mode, GameEngine engine)
        {
            GameId = gameId;
            Mode = mode;
            Engine = engine;
            Lock = new SemaphoreSlim(1, 1);
        }
    }
}