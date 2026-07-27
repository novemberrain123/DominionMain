using Dominion.API.Config;
using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Serialization;
using System.Text.Json;

namespace Dominion.API.Dominion.Game
{
    public class ContentLoader
    {
        private readonly CardDefinitionFactory _factory;

        public ContentLoader(CardDefinitionFactory factory)
        {
            _factory = factory;
        }

        public CardRegistry LoadCards(string path)
        {
            var json = File.ReadAllText(path);

            var dto = JsonSerializer.Deserialize<List<CardDefinitionData>>(json, JsonConfig.Options);

            var registry = new CardRegistry();

            foreach (var cardData in dto)
            {
                var card = _factory.Create(cardData);
                registry.Register(card);
            }

            return registry;
        }
    }
}
