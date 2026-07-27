using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Cards.Choices;
using Dominion.API.Dominion.Game.Enums;
using Dominion.API.Dominion.Players;

namespace Dominion.API.Dominion.Game
{
    public class GameState
    {
        public Guid GameId { get; }
        public List<Player> Players { get; private set; } = new();
        public int CurrentPlayerIndex { get; set; }

        public GamePhase Phase { get; set; }
        public int TurnNumber { get; set; }

        public Dictionary<string, SupplyPile> SupplyPiles { get; set; } = new();

        public List<Card> Trash { get; set; } = new();

        public GameResult? Result { get; set; }

        public GameStatus Status { get; set; } = GameStatus.Lobby;

        public List<GameEvent> Events { get; private set; } = new();

        public PendingChoice? PendingChoice { get; set; }

        public EffectExecutionContext? CurrentExecution { get; set; }

        public GameState()
        {
            GameId = Guid.NewGuid();
        }

        public GameState(Guid gameId)
        {
            GameId = gameId;
        }

        public void Initialize()
        {
            CurrentPlayerIndex = 0;
            Phase = GamePhase.Action;
            TurnNumber = 1;
            Status = GameStatus.Lobby;

            Trash.Clear();
            Events.Clear();
        }

    }
}