using Dominion.Dominion.Cards;
using Dominion.Dominion.Players;

namespace Dominion.Dominion.Game
{
    public class GameSetupService
    {
        public void InitializePlayers(GameEngine engine, GameConfig config)
        {
            foreach (var player in engine.State.Players)
            {
                CreateStartingDeck(engine, player, config);

                player.Draw(config.StartingHandSize);

                player.Actions = config.StartingActions;
                player.Buys = config.StartingBuys;
                player.Coins = 0;
            }
        }

        public void InitializeSupply(GameEngine engine, GameConfig config)
        {
            var supplyBuilder = new SupplyBuilder();
            engine.State.SupplyPiles = supplyBuilder.Build(config, engine.Cards);
        }

        private void CreateStartingDeck(GameEngine engine, Player player, GameConfig config)
        {
            foreach (var entry in config.StartingDeck)
            {
                for (int i = 0; i < entry.Value; i++)
                {
                    //add to discardpile so that it gets shuffled into the deck when the player draws
                    player.DiscardPile.Add(
                        new Card { Definition = engine.Cards.Get(entry.Key) }
                    );
                }
            }
        }
    }
}
