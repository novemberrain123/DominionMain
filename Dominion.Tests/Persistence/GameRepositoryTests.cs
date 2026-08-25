using Dominion.API.Dominion.Game;
using Dominion.API.Dominion.Persistance;
using Dominion.API.Dominion.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Tests.Persistence;

public class GameRepositoryTests
{
    private readonly ServiceProvider _services;
    private readonly GameEngineFactory _factory;
    private readonly GameStateSerializer _serializer;
    private readonly GameSessionManager _sessionManager;
    private readonly GameRepository _repository;

    public GameRepositoryTests()
    {

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();

        services.AddDominionServices(configuration);

        _services = services.BuildServiceProvider();

        _factory = _services.GetRequiredService<GameEngineFactory>();
        _serializer = _services.GetRequiredService<GameStateSerializer>();
        _sessionManager = _services.GetRequiredService<GameSessionManager>();
        _repository = _services.GetRequiredService<GameRepository>();
    }

    [Fact]
    public async Task GameRepository_CanSaveAndLoad()
    {
        var gameId = Guid.NewGuid();
        var mode = "test_mode";
        var stateJson = """{"test":"hello"}""";

        await _repository.SaveAsync(
            gameId,
            mode,
            stateJson);

        var entity = await _repository.LoadAsync(gameId);

        Assert.Equal(stateJson, entity?.StateJson);
    }

    private GameEngine CreateTestEngine()
    {
        return _factory.Create(
            "Content/Modes/test_mode.json");
    }

    [Fact]
    public async Task GameRepository_CanPersistGameState()
    {
        var engine = CreateTestEngine();

        try
        {
            var player1 = engine.AddPlayer("Alice");
            engine.AddPlayer("Bob");

            engine.StartGame(player1.Id);

            var json = _serializer.Serialize(engine.State);

            await _repository.SaveAsync(
                engine.State.GameId,
                "test_mode",
                json);

            _sessionManager.Remove(engine.State.GameId);

            var entity =
                await _repository.LoadAsync(
                    engine.State.GameId);

            Assert.NotNull(entity);

            var restoredEngine = _factory.Restore(
                "Content/Modes/test_mode.json",
                entity.StateJson);

            AssertHelper.AssertGameStatesEqual(
                engine.State,
                restoredEngine.State);
        }
        finally
        {
            _sessionManager.Remove(engine.State.GameId);

            // Delete the test game from the DB.
            await _repository.DeleteAsync(
                engine.State.GameId);
        }
    }
}
