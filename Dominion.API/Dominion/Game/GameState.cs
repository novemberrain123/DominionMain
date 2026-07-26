using Dominion.API.Dominion.Game;
using Dominion.Dominion.Cards;
using Dominion.Dominion.Players;

namespace Dominion.Dominion.Game
{
    public class GameState
    {
        public Guid GameId { get; }
        public List<Player> Players { get; private set; } = new();
        public int CurrentPlayerIndex { get; set; }

        public GamePhase Phase { get; set; }
        public int TurnNumber { get; set; }

        public Dictionary<string, SupplyPile> SupplyPiles { get; private set; } = new();

        public List<Card> Trash { get; private set; } = new();

        public GameResult? Result { get; set; }

        public GameStatus Status { get; set; } = GameStatus.Lobby;

        public List<GameEvent> Events { get; private set; } = new();

        public GameState()
        {
            GameId = Guid.NewGuid();
        }

        public GameState(Guid gameId)
        {
            GameId = gameId;
        }

        public void Initialize(List<Player> players, Dictionary<string, SupplyPile> supply)
        {
            Players = players;
            SupplyPiles = supply;

            CurrentPlayerIndex = 0;
            Phase = GamePhase.Action;
            TurnNumber = 1;
            Status = GameStatus.Lobby;

            Trash.Clear();
            Events.Clear();
        }

    }
}