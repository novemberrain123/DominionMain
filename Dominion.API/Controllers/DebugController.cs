using Dominion.API.Dominion.Serialization;
using Dominion.API.Dominion.Serialization.RequestDto;
using Dominion.Dominion.Game;
using Dominion.Dominion.Game.Debug;
using Dominion.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Dominion.Controllers;

[ApiController]
[Route("debug/games")]
public class DebugController : ControllerBase
{
    private const string PlayerTokenHeader = "X-Player-Token";

    private readonly GameEngineFactory _factory;
    private readonly GameEngineProvider _provider;

    public DebugController(
        GameEngineFactory factory,
        GameEngineProvider provider)
    {
        _factory = factory;
        _provider = provider;
    }

    [HttpPost]
    public ActionResult<GameStateDto> Bootstrap()
    {
        var engine = _factory.Create(
            "Content/Modes/base_mode.json",
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
    public ActionResult JoinGame(
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
    public ActionResult<GameStateDto> StartGame(Guid gameId)
    {
        return ExecutePlayerAction(
            gameId,
            (engine, playerId) =>
                engine.StartGame(playerId));
    }

    [HttpPost("{gameId:guid}/play-card")]
    public ActionResult<GameStateDto> PlayCard(
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
    public ActionResult<GameStateDto> EndActionPhase(
        Guid gameId)
    {
        return ExecutePlayerAction(
            gameId,
            (engine, playerId) =>
                engine.EndActionPhase(playerId));
    }

    [HttpPost("{gameId:guid}/buy-card")]
    public ActionResult<GameStateDto> BuyCard(
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
    public ActionResult<GameStateDto> EndTurn(Guid gameId)
    {
        return ExecutePlayerAction(
            gameId,
            (engine, playerId) =>
                engine.EndTurn(playerId));
    }

    [HttpPost("{gameId:guid}/play-all-treasures")]
    public ActionResult<GameStateDto> PlayAllTreasures(
        Guid gameId)
    {
        return ExecutePlayerAction(
            gameId,
            (engine, playerId) =>
                engine.PlayAllTreasures(playerId));
    }

    private ActionResult<GameStateDto> ExecuteGameAction(
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

            return Ok(GameStateDtoMapper.ToDto(
                engine.State,
                engine.Cards));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    private ActionResult<GameStateDto> ExecutePlayerAction(
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