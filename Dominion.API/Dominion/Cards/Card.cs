namespace Dominion.API.Dominion.Cards
{
    public class Card
    {
        public Guid Id { get; }
        public CardDefinition Definition { get; init; }
        public Card(CardDefinition definition)
        {
            Id = Guid.NewGuid();
            Definition = definition;
        }
        public Card(Guid id, CardDefinition definition)
        {
            Id = id;
            Definition = definition;
        }
    }
}