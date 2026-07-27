using Dominion.API.Dominion.Serialization;
using Dominion.API.Dominion.Game;
using System.Text.Json;

namespace Dominion.API.Dominion.Cards
{
    public class CardRegistry
    {
        private readonly Dictionary<string, CardDefinition> _cards = new();
        public int Count => _cards.Count;

        public void Register(CardDefinition card)
            => _cards[card.Id] = card;

        public CardDefinition Get(string id)
            => _cards[id];

        public bool Contains(string id)
            => _cards.ContainsKey(id);
        public IEnumerable<CardDefinitionDto> GetAllDtos()
        {
            return _cards.Values.Select(ToDto);
        }

        private static CardDefinitionDto ToDto(CardDefinition card)
        {
            return new CardDefinitionDto
            {
                Id = card.Id,
                DisplayName = card.DisplayName,
                Cost = card.Cost,
                Types = card.Types.Select(t => t.ToString()).ToList(),
                Effects = card.Effects.Select(e => e.ToDisplayText()).ToList()
            };
        }

    }
}
