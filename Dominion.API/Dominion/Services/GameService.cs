using Dominion.API.Dominion.Game;
using Dominion.API.Dominion.Persistance;
using Dominion.API.Dominion.Serialization;

public class GameService
{
    private readonly GameSessionManager _sessionManager;
    private readonly GameRepository _repository;
    private readonly GameEngineFactory _factory;
    private readonly GameStateSerializer _serializer;

    public GameService(
        GameSessionManager sessionManager,
        GameRepository repository,
        GameEngineFactory factory,
        GameStateSerializer serializer)
    {
        _sessionManager = sessionManager;
        _repository = repository;
        _factory = factory;
        _serializer = serializer;
    }

    public async Task<GameSession> CreateGameAsync(
        string mode,
        CancellationToken cancellationToken = default)
    {
        var modePath = $"Content/Modes/{mode}.json";

        var engine = _factory.Create(modePath);

        var stateJson = _serializer.Serialize(engine.State);

        await _repository.SaveAsync(
            engine.State.GameId,
            mode,
            stateJson,
            cancellationToken);

        return _sessionManager.Get(engine.State.GameId)!;
    }

    public async Task SaveGameAsync(
        GameSession session,
        CancellationToken cancellationToken = default)
    {
        var stateJson = _serializer.Serialize(session.Engine.State);

        await _repository.SaveAsync(
            session.Engine.State.GameId,
            session.Mode,
            stateJson,
            cancellationToken);
    }

    public async Task<GameSession?> GetOrRestoreAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var session = _sessionManager.Get(gameId);

        if (session is not null)
        {
            return session;
        }

        var entity = await _repository.LoadAsync(gameId);

        if (entity is null)
        {
            return null;
        }

        _factory.Restore(
            $"Content/Modes/{entity.Mode}.json",
            entity.StateJson);

        return _sessionManager.Get(gameId);
    }
}