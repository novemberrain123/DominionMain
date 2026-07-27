using Dominion.API.Dominion.Serialization;
using Dominion.API.Dominion.Serialization.RequestDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Dominion.API.Dominion.Game;
using Dominion.API.Hubs;

namespace Dominion.API.Controllers;

[ApiController]
[Route("games")]
public class MainController : ControllerBase
{
    private const string PlayerTokenHeader = "X-Player-Token";

    private readonly GameEngineFactory _factory;
    private readonly GameEngineProvider _provider;
    private readonly IHubContext<GameHub> _gameHub;

    public MainController(
        GameEngineFactory factory,
        GameEngineProvider provider,
        IHubContext<GameHub> gameHub)
    {
        _factory = factory;
        _provider = provider;
        _gameHub = gameHub;

    }

    [HttpPost]
    public ActionResult<GameStateDto> Bootstrap()
    {
        var engine = _factory.Create(
            "Content/Modes/test_mode.json",
            "Content/Cards/test.json");

        return Ok(ToDto(engine));
    }

    [HttpGet("{gameId:guid}")]
    public ActionResult<GameStateDto> GetGame(Guid gameId)
    {
        var engine = _provider.Get(gameId);

        if (engine is null)
        {
            return NotFound($"Game {gameId} was not found.");
        }

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
        var engine = _provider.Get(gameId);

        if (engine is null)
        {
            return NotFound($"Game {gameId} was not found.");
        }

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
        var engine = _provider.Get(gameId);

        if (engine is null)
        {
            return NotFound("No game has been initialized.");
        }

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
    }

    private async Task<ActionResult<GameStateDto>> ExecutePlayerAction(
        Guid gameId,
        Action<GameEngine, Guid> action)
    {
        var engine = _provider.Get(gameId);

        if (engine is null)
        {
            return NotFound($"Game {gameId} was not found.");
        }

        try
        {
            var playerId = ResolvePlayerId(engine);

            action(engine, playerId);

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