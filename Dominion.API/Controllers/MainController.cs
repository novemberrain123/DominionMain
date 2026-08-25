using Dominion.API.Dominion.Game;
using Dominion.API.Dominion.Serialization;
using Dominion.API.Dominion.Serialization.RequestDto;
using Dominion.API.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading;

namespace Dominion.API.Controllers;

[ApiController]
[Route("games")]
public class MainController : ControllerBase
{
    private const string PlayerTokenHeader = "X-Player-Token";

    private readonly GameEngineFactory _factory;
    private readonly GameSessionManager _sessionManager;
    private readonly GameService _gameService;
    private readonly IHubContext<GameHub> _gameHub;

    public MainController(
        GameEngineFactory factory,
        GameSessionManager sessionManager,
        IHubContext<GameHub> gameHub,
        GameService gameService)
    {
        _factory = factory;
        _sessionManager = sessionManager;
        _gameHub = gameHub;
        _gameService = gameService;
    }

    [HttpPost]
    public async Task<ActionResult<GameStateDto>> Bootstrap(CancellationToken cancellationToken)
    {
        var session = await _gameService.CreateGameAsync(
            "test_mode",
            cancellationToken);

        return Ok(ToDto(session.Engine));
    }

    [HttpGet("{gameId:guid}")]
    public async Task<ActionResult<GameStateDto>> GetGame(Guid gameId, CancellationToken cancellationToken)
    {
        var session = await _gameService.GetOrRestoreAsync(gameId, cancellationToken);

        if (session is null)
        {
            return NotFound($"Game {gameId} was not found.");
        }

        var engine = session.Engine;

        try
        {
            var playerId = ResolveOptionalPlayerId(engine);

            return Ok(ToDto(engine, playerId));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(exception.Message);
        }
    }

    [HttpPost("{gameId:guid}/join")]
    public async Task<ActionResult> JoinGame(
        Guid gameId,
        [FromBody] JoinGameRequest request)
    {
        var session = _sessionManager.Get(gameId);

        if (session is null)
        {
            return NotFound($"Game {gameId} was not found.");
        }

        var engine = session.Engine;

        try
        {
            var player = engine.AddPlayer(request.PlayerName);
            var token = engine.CreatePlayerSession(player.Id);

            await BroadcastGameUpdated(gameId);

            return Ok(new
            {
                GameId = gameId,
                PlayerId = player.Id,
                PlayerToken = token
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("{gameId:guid}/start")]
    public Task<ActionResult<GameStateDto>> StartGame(Guid gameId)
    {
        return ExecutePlayerAction(
            gameId,
            (engine, playerId) =>
                engine.StartGame(playerId));
    }

    [HttpPost("{gameId:guid}/play-card")]
    public Task<ActionResult<GameStateDto>> PlayCard(
        Guid gameId,
        [FromBody] PlayCardRequest request)
    {
        return ExecutePlayerAction(
            gameId,
            (engine, playerId) =>
                engine.PlayCard(
                    playerId,
                    request.CardInstanceId));
    }

    [HttpPost("{gameId:guid}/end-action-phase")]
    public Task<ActionResult<GameStateDto>> EndActionPhase(
        Guid gameId)
    {
        return ExecutePlayerAction(
            gameId,
            (engine, playerId) =>
                engine.EndActionPhase(playerId));
    }

    [HttpPost("{gameId:guid}/buy-card")]
    public Task<ActionResult<GameStateDto>> BuyCard(
        Guid gameId,
        [FromBody] BuyCardRequest request)
    {
        return ExecutePlayerAction(
            gameId,
            (engine, playerId) =>
                engine.BuyCard(
                    playerId,
                    request.DefinitionId));
    }

    [HttpPost("{gameId:guid}/end-turn")]
    public Task<ActionResult<GameStateDto>> EndTurn(Guid gameId)
    {
        return ExecutePlayerAction(
            gameId,
            (engine, playerId) =>
                engine.EndTurn(playerId));
    }

    [HttpPost("{gameId:guid}/play-all-treasures")]
    public Task<ActionResult<GameStateDto>> PlayAllTreasures(Guid gameId)
    {
        return ExecutePlayerAction(
            gameId,
            (engine, playerId) =>
                engine.PlayAllTreasures(playerId));
    }

    [HttpPost("{gameId:guid}/resolve-choice")]
    public Task<ActionResult<GameStateDto>> ResolveChoice(
        Guid gameId,
        [FromBody] ResolveChoiceRequest request)
    {
        return ExecutePlayerAction(
            gameId,
            (engine, playerId) =>
                engine.ResolveChoice(
                    playerId,
                    request));
    }

    private async Task<ActionResult<GameStateDto>> ExecuteGameAction(
        Guid gameId,
        Action<GameEngine> action)
    {
        var session = _sessionManager.Get(gameId);

        if (session is null)
        {
            return NotFound("No game has been initialized.");
        }

        await session.Lock.WaitAsync();

        var engine = session.Engine;

        try
        {
            action(engine);

            await BroadcastGameUpdated(gameId);

            return Ok(GameStateDtoMapper.ToDto(
                engine.State,
                engine.Cards));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
        finally
        {
            session.Lock.Release();
        }
    }

    private async Task<ActionResult<GameStateDto>> ExecutePlayerAction(
        Guid gameId,
        Action<GameEngine, Guid> action,
        CancellationToken cancellationToken = default)
    {
        var session = _sessionManager.Get(gameId);

        if (session is null)
        {
            return NotFound($"Game {gameId} was not found.");
        }

        await session.Lock.WaitAsync(cancellationToken);

        var engine = session.Engine;

        try
        {
            var playerId = ResolvePlayerId(engine);

            action(engine, playerId);

            await _gameService.SaveGameAsync(
                session,
                cancellationToken);

            await BroadcastGameUpdated(gameId);

            return Ok(ToDto(engine, playerId));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
        finally
        {
            session.Lock.Release();
        }
    }

    private Task BroadcastGameUpdated(Guid gameId)
    {
        return _gameHub.Clients
            .Group(GameHub.GetGroupName(gameId))
            .SendAsync("GameUpdated");
    }

    private Guid ResolvePlayerId(GameEngine engine)
    {
        if (!Request.Headers.TryGetValue(
                PlayerTokenHeader,
                out var tokenValues))
        {
            throw new UnauthorizedAccessException(
                "Missing player token.");
        }

        var token = tokenValues.ToString();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Missing player token.");
        }

        return engine.GetPlayerIdFromToken(token);
    }

    private Guid? ResolveOptionalPlayerId(
        GameEngine engine)
    {
        if (!Request.Headers.TryGetValue(
                PlayerTokenHeader,
                out var tokenValues))
        {
            return null;
        }

        var token = tokenValues.ToString();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return engine.GetPlayerIdFromToken(token);
    }

    private static GameStateDto ToDto(
        GameEngine engine,
        Guid? requestingPlayerId = null)
    {
        return GameStateDtoMapper.ToDto(
            engine.State,
            engine.Cards,
            requestingPlayerId);
    }
}