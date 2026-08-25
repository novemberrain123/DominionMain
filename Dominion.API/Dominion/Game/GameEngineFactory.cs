using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Players;
using Dominion.API.Dominion.Serialization;


namespace Dominion.API.Dominion.Game
{
    public class GameEngineFactory
    {
        private readonly ContentLoader _contentLoader;
        private readonly GameModeLoader _modeLoader;
        private readonly SupplyBuilder _supplyBuilder;
        private readonly EffectResolver _effectResolver;
        private readonly ChoiceResolver _choiceResolver;
        private readonly GameSessionManager _sessionManager;
        private readonly GameSetupService _gameSetupService;
        private readonly GameStateSerializer _serializer;

        public GameEngineFactory(
            ContentLoader contentLoader,
            GameModeLoader modeLoader,
            SupplyBuilder supplyBuilder,
            EffectResolver effectResolver,
            ChoiceResolver choiceResolver,
            GameSessionManager sessionManager,
            GameSetupService gameSetupService,
            GameStateSerializer serializer
            )
        {
            _contentLoader = contentLoader;
            _modeLoader = modeLoader;
            _supplyBuilder = supplyBuilder;
            _effectResolver = effectResolver;
            _choiceResolver = choiceResolver;
            _sessionManager = sessionManager;
            _gameSetupService = gameSetupService;
            _serializer = serializer;
        }

        public GameEngine Create(string modePath)
        {
            var state = new GameState();
            state.Initialize();

            return CreateEngine(modePath, state);
        }

        public GameEngine Restore(string modePath, string stateJson)
        {
            var config = _modeLoader.Load(modePath);

            var registry = _contentLoader.LoadCards(
                $"Content/Cards/{config.CardSetId}.json");

            var state = _serializer.Deserialize(
                stateJson,
                registry);

            return CreateEngine(modePath, state);
        }

        private GameEngine CreateEngine(
            string modePath,
            GameState state)
        {
            var config = _modeLoader.Load(modePath);

            var registry = _contentLoader.LoadCards(
                $"Content/Cards/{config.CardSetId}.json");

            var engine = new GameEngine(
                registry,
                state,
                _effectResolver,
                _choiceResolver,
                _gameSetupService,
                config);

            var session = new GameSession(
                state.GameId,
                config.Name,
                engine);

            _sessionManager.Add(session);

            return engine;
        }

    }
}
