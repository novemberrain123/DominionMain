using Dominion.Dominion.Cards;

namespace Dominion.Dominion.Serialization
{
    public class CardDefinitionFactory
    {
        public CardDefinition Create(CardDefinitionData data)
        {
            return new CardDefinition
            {
                Id = data.Id,

                DisplayName = data.DisplayName,

                Cost = data.Cost,

                VictoryPoints = data.VictoryPoints,

                Types = data.Types
                    .Select(Enum.Parse<CardType>)
                    .ToHashSet(),

                Effects = data.Effects
            };
        }
    }
}
