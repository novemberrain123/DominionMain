using Dominion.Dominion.Cards;

namespace Dominion.Dominion.Game
{
    public class SupplyBuilder
    {
        public Dictionary<string, SupplyPile> Build(GameConfig config, CardRegistry registry)
        {
            var supply = new Dictionary<string, SupplyPile>();

            foreach (var entry in config.Supply)
            {
                if (!registry.Contains(entry.Key))
                    throw new Exception($"Unknown card: {entry.Key}");

                supply[entry.Key] = new SupplyPile(entry.Key, entry.Value);
            }

            return supply;
        }
    }
}
