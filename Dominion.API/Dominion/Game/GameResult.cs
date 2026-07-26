using Microsoft.AspNetCore.Mvc;

namespace Dominion.API.Dominion.Game
{
    public class GameResult
    {
        public required IReadOnlyList<PlayerResult> Players { get; init; }
    }

    public class PlayerResult
    {
        public required Guid PlayerId { get; init; }
        public required int VictoryPoints { get; init; }
        public required int Rank { get; init; }
    }
}


