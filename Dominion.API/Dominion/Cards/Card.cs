namespace Dominion.API.Dominion.Cards
{
    public class Card
    {
        public Guid Id { get; } = Guid.NewGuid();
        public required CardDefinition Definition { get; init; }
    }
}