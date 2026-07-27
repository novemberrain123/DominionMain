using Dominion.API.Dominion.Cards;

namespace Dominion.API.Dominion.Serialization
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

                Description = data.Description,

                Types = data.Types
                    .Select(Enum.Parse<CardType>)
                    .ToHashSet(),

                Effects = data.Effects
            };
        }
    }
}
