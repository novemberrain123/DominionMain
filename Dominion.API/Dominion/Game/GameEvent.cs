using Dominion.API.Dominion.Cards;

namespace Dominion.API.Dominion.Game
{
    public class GameEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required GameEventType Type { get; init; }
        public required long SequenceNumber { get; init; }
        public Guid? PlayerId { get; init; }
        public Guid? TargetPlayerId { get; init; }
        public Guid? Card { get; init; }
    }
}