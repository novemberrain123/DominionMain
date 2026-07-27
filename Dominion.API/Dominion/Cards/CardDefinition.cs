using Dominion.API.Dominion.Serialization;
using Microsoft.VisualBasic;

namespace Dominion.API.Dominion.Cards
{
    public class CardDefinition
    {
        public required string Id { get; init; }
        public required string DisplayName { get; init; }
        public required int Cost { get; init; }
        public int VictoryPoints { get; init; }
        public HashSet<CardType> Types { get; init; }

        public List<EffectData> Effects { get; init; }
    }
}
