using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Cards.Choices;
using Dominion.API.Dominion.Game;
using Dominion.API.Dominion.Game.Enums;
using Dominion.API.Dominion.Players;
using Dominion.API.Dominion.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace Dominion.Tests;

public class GameStateSerializerTests
{
    private readonly ServiceProvider _services;
    private readonly GameEngineFactory _factory;
    private readonly GameStateSerializer _serializer;
    private readonly GameSessionManager _sessionManager;

    public GameStateSerializerTests()
    {
        var services = new ServiceCollection();

        services.AddDominionServices();

        _services = services.BuildServiceProvider();

        _factory = _services.GetRequiredService<GameEngineFactory>();
        _serializer = _services.GetRequiredService<GameStateSerializer>();
        _sessionManager = _services.GetRequiredService<GameSessionManager>();
    }

    private GameEngine CreateTestEngine()
    {
        return _factory.Create(
            "Content/Modes/test_mode.json");
    }

    private GameEngine RestoreRoundTrip(GameEngine engine)
    {
        var json = _serializer.Serialize(engine.State);

        _sessionManager.Remove(engine.State.GameId);

        return _factory.Restore(
            "Content/Modes/test_mode.json",
            json);
    }

    [Fact]
    public void StartedGame_CanRoundTrip()
    {
        var engine = CreateTestEngine();

        try
        {
            var player1 = engine.AddPlayer("Alice");
            engine.AddPlayer("Bob");

            engine.StartGame(player1.Id);

            var restoredEngine = RestoreRoundTrip(engine);

            AssertHelper.AssertGameStatesEqual(
                engine.State,
                restoredEngine.State);
        }
        finally
        {
            _sessionManager.Remove(engine.State.GameId);
        }
    }

    [Fact]
    public void GameAfterActionPhase_CanRoundTrip()
    {
        var engine = CreateTestEngine();

        try
        {
            var player1 = engine.AddPlayer("Alice");
            engine.AddPlayer("Bob");

            engine.StartGame(player1.Id);

            engine.EndActionPhase(player1.Id);

            var restoredEngine = RestoreRoundTrip(engine);

            AssertHelper.AssertGameStatesEqual(
                engine.State,
                restoredEngine.State);
        }
        finally
        {
            _sessionManager.Remove(engine.State.GameId);
        }
    }

    [Fact]
    public void GameAfterBuyingCard_CanRoundTrip()
    {
        var engine = CreateTestEngine();

        try
        {
            var player1 = engine.AddPlayer("Alice");
            engine.AddPlayer("Bob");

            engine.StartGame(player1.Id);

            engine.EndActionPhase(player1.Id);

            player1.Coins = 3;

            engine.BuyCard(
                player1.Id,
                "silver");

            var restoredEngine = RestoreRoundTrip(engine);

            AssertHelper.AssertGameStatesEqual(
                engine.State,
                restoredEngine.State);
        }
        finally
        {
            _sessionManager.Remove(engine.State.GameId);
        }
    }

    [Fact]
    public void GameAfterEndingTurn_CanRoundTrip()
    {
        var engine = CreateTestEngine();

        try
        {
            var player1 = engine.AddPlayer("Alice");
            engine.AddPlayer("Bob");

            engine.StartGame(player1.Id);

            var initialTurnNumber = engine.State.TurnNumber;

            engine.EndActionPhase(player1.Id);

            engine.EndTurn(player1.Id);

            var restoredEngine = RestoreRoundTrip(engine);

            AssertHelper.AssertGameStatesEqual(
                engine.State,
                restoredEngine.State);

            Assert.Equal(
                initialTurnNumber + 1,
                restoredEngine.State.TurnNumber);
        }
        finally
        {
            _sessionManager.Remove(engine.State.GameId);
        }
    }

    [Fact]
    public void RestoredGame_CanContinuePlaying()
    {
        var engine = CreateTestEngine();

        try
        {
            var player1 = engine.AddPlayer("Alice");
            engine.AddPlayer("Bob");

            engine.StartGame(player1.Id);

            engine.EndActionPhase(player1.Id);

            player1.Coins = 3;

            engine.BuyCard(
                player1.Id,
                "silver");

            var restoredEngine = RestoreRoundTrip(engine);

            AssertHelper.AssertGameStatesEqual(
                engine.State,
                restoredEngine.State);

            // Continue using the restored engine.
            restoredEngine.EndTurn(player1.Id);

            Assert.Equal(
                engine.State.TurnNumber + 1,
                restoredEngine.State.TurnNumber);

            Assert.Equal(
                GamePhase.Action,
                restoredEngine.State.Phase);
        }
        finally
        {
            _sessionManager.Remove(engine.State.GameId);
        }
    }

    [Fact]
    public void GameAfterPlayingActionCard_CanRoundTrip()
    {
        var engine = CreateTestEngine();

        try
        {
            var player1 = engine.AddPlayer("Alice");
            engine.AddPlayer("Bob");

            engine.StartGame(player1.Id);

            var actionCard = player1.Hand
                .FirstOrDefault(card =>
                    card.Definition.Types.Contains(CardType.Action));

            Assert.NotNull(actionCard);

            engine.PlayCard(
                player1.Id,
                actionCard.Id);

            var restoredEngine = RestoreRoundTrip(engine);

            AssertHelper.AssertGameStatesEqual(
                engine.State,
                restoredEngine.State);
        }
        finally
        {
            _sessionManager.Remove(engine.State.GameId);
        }
    }

    [Fact]
    public void GameWithPendingChoice_CanRoundTrip()
    {
        var engine = CreateTestEngine();

        try
        {
            var player1 = engine.AddPlayer("Alice");
            engine.AddPlayer("Bob");

            engine.StartGame(player1.Id);

            var choiceCard = player1.Hand
                .FirstOrDefault(card =>
                    card.Definition.Id == "chapel");

            Assert.NotNull(choiceCard);

            engine.PlayCard(
                player1.Id,
                choiceCard.Id);

            Assert.NotNull(engine.State.PendingChoice);

            var restoredEngine = RestoreRoundTrip(engine);

            AssertHelper.AssertGameStatesEqual(
                engine.State,
                restoredEngine.State);

            Assert.NotNull(
                restoredEngine.State.PendingChoice);

            Assert.Equal(
                engine.State.PendingChoice.GetType(),
                restoredEngine.State.PendingChoice.GetType());
        }
        finally
        {
            _sessionManager.Remove(engine.State.GameId);
        }
    }

    [Fact]
    public void GameAfterMultipleTurns_CanRoundTrip()
    {
        var engine = CreateTestEngine();

        try
        {
            var player1 = engine.AddPlayer("Alice");
            var player2 = engine.AddPlayer("Bob");

            engine.StartGame(player1.Id);

            // Complete player 1's turn.
            engine.EndActionPhase(player1.Id);
            engine.EndTurn(player1.Id);

            // Complete player 2's turn.
            engine.EndActionPhase(player2.Id);
            engine.EndTurn(player2.Id);

            var restoredEngine = RestoreRoundTrip(engine);

            AssertHelper.AssertGameStatesEqual(
                engine.State,
                restoredEngine.State);
        }
        finally
        {
            _sessionManager.Remove(engine.State.GameId);
        }
    }





}