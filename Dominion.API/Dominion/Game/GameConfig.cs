namespace Dominion.API.Dominion.Game
{
    public class GameConfig
    {
        public required string Name { get; init; }
        public required string DisplayName { get; init; }
        public required string Description { get; init; }
        public required string CardSetId { get; init; }
        public required int StartingHandSize { get; init; }
        public required int StartingActions { get; init; }
        public required int StartingBuys { get; init; }
        public required int MaxPlayers { get; init; }
        public required Dictionary<string, int> StartingDeck { get; init; }
        public required Dictionary<string, int> Supply { get; init; }
    }
}
