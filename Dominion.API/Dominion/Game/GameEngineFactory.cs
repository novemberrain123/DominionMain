using Dominion.Dominion.Cards;
using Dominion.Dominion.Game.Debug;
using Dominion.Dominion.Players;

namespace Dominion.Dominion.Game
{
    public class GameEngineFactory
    {
        private readonly ContentLoader _contentLoader;
        private readonly GameModeLoader _modeLoader;
        private readonly SupplyBuilder _supplyBuilder;
        private readonly EffectResolver _effectResolver;
        private readonly GameEngineProvider _engineProvider;
        private readonly GameSetupService _gameSetupService;

        public GameEngineFactory(
            ContentLoader contentLoader,
            GameModeLoader modeLoader,
            SupplyBuilder supplyBuilder,
            EffectResolver effectResolver,
            GameEngineProvider engineProvider,
            GameSetupService gameSetupService
            )
        {
            _contentLoader = contentLoader;
            _modeLoader = modeLoader;
            _supplyBuilder = supplyBuilder;
            _effectResolver = effectResolver;
            _engineProvider = engineProvider;
            _gameSetupService = gameSetupService;
        }

        public GameEngine Create(string modePath, string cardsPath, List<Player> players)
        {
            var config = _modeLoader.Load(modePath);
            var registry = _contentLoader.LoadCards(cardsPath);

            var supply = _supplyBuilder.Build(config, registry);
            
            var state = new GameState();
            state.Initialize(players, supply);

            var engine = new GameEngine(registry, state, _effectResolver);

            _gameSetupService.InitializePlayers(engine, config);

            _engineProvider.Add(engine); // inject the engine into the provider for debug tools

            return engine;
        }

        //for testing
        public GameEngine Create(string modePath, string cardsPath)
        {
            var defaultPlayers = new List<Player>
                {
                    new Player { Id = Guid.NewGuid(), Name = "P1" },
                    new Player { Id = Guid.NewGuid(), Name = "P2" }
                };

            return Create(modePath, cardsPath, defaultPlayers);
        }
    }
}
