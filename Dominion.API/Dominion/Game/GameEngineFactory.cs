using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Players;

namespace Dominion.API.Dominion.Game
{
    public class GameEngineFactory
    {
        private readonly ContentLoader _contentLoader;
        private readonly GameModeLoader _modeLoader;
        private readonly SupplyBuilder _supplyBuilder;
        private readonly EffectResolver _effectResolver;
        private readonly ChoiceResolver _choiceResolver;
        private readonly GameEngineProvider _engineProvider;
        private readonly GameSetupService _gameSetupService;

        public GameEngineFactory(
            ContentLoader contentLoader,
            GameModeLoader modeLoader,
            SupplyBuilder supplyBuilder,
            EffectResolver effectResolver,
            ChoiceResolver choiceResolver,
            GameEngineProvider engineProvider,
            GameSetupService gameSetupService
            )
        {
            _contentLoader = contentLoader;
            _modeLoader = modeLoader;
            _supplyBuilder = supplyBuilder;
            _effectResolver = effectResolver;
            _choiceResolver = choiceResolver;
            _engineProvider = engineProvider;
            _gameSetupService = gameSetupService;
        }

        public GameEngine Create(string modePath, string cardsPath)
        {
            var config = _modeLoader.Load(modePath);
            var registry = _contentLoader.LoadCards(cardsPath);

            var state = new GameState();
            state.Initialize();

            var engine = new GameEngine(registry, state, _effectResolver, _choiceResolver, _gameSetupService, config);

            _engineProvider.Add(engine); // inject the engine into the provider for debug tools

            return engine;
        }

        //for testing
        //public GameEngine Create(string modePath, string cardsPath)
        //{
        //    var defaultPlayers = new List<Player>
        //        {
        //            new Player { Id = Guid.NewGuid(), Name = "P1" },
        //            new Player { Id = Guid.NewGuid(), Name = "P2" }
        //        };

        //    return Create(modePath, cardsPath, defaultPlayers);
        //}
    }
}
