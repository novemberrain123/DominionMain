using Dominion.API.Dominion.Serialization;
using Dominion.API.Dominion.Serialization.RequestDto;
using Dominion.Dominion.Cards;
using Dominion.Dominion.Game;
using Dominion.Dominion.Game.Debug;
using Dominion.Dominion.Players;
using Dominion.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Dominion.Controllers
{
    [ApiController]
    [Route("debug/games")]
    public class DebugController : ControllerBase
    {
        private readonly GameEngineFactory _factory;
        private readonly GameEngineProvider _provider;

        public DebugController(GameEngineFactory factory, GameEngineProvider provider)
        {
            _provider = provider;
            _factory = factory;
        }

        private Guid ResolvePlayerId(GameEngine engine)
        {
            if (!Request.Headers.TryGetValue(
                    "X-Player-Token",
                    out var tokenValues))
            {
                throw new InvalidOperationException(
                    "Missing player token.");
            }

            var token = tokenValues.ToString();

            return engine.GetPlayerIdFromToken(token);
        }


        [HttpPost]
        public IActionResult Bootstrap()
        {
            var engine = _factory.Create(
                "Content/Modes/base_mode.json",
                "Content/Cards/test.json"
            );

            var state = engine.State;

            return Ok(GameStateDtoMapper.ToDto(
                        state,
                        engine.Cards));
        }

        [HttpGet("{gameId:guid}")]
        public ActionResult<GameStateDto> GetGame(Guid gameId)
        {
            var engine = _provider.Get(gameId);

            if (engine is null)
            {
                return NotFound($"Game {gameId} was not found.");
            }

            return Ok(GameStateDtoMapper.ToDto(
                engine.State,
                engine.Cards));
        }

        [HttpPost("{gameId:guid}/join")]
        public ActionResult JoinGame(Guid gameId, [FromBody] JoinGameRequest request)
        {
            var engine = _provider.Get(gameId);
            Player player;

            if (engine is null)
            {
                return NotFound($"Game {gameId} was not found.");
            }

            try
            {
                player = engine.AddPlayer(request.PlayerName);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }

            var token = engine.CreatePlayerSession(player.Id);

            return Ok(new
            {
                GameId = gameId,
                PlayerId = player.Id,
                PlayerToken = token
            });
        }

        [HttpPost("{gameId:guid}/start")]
        public ActionResult<GameStateDto> StartGame(Guid gameId)
        {
            var engine = _provider.Get(gameId);

            if (engine is null)
            {
                return NotFound("Game not found.");
            }

            try
            {
                engine.StartGame();
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }

            return Ok(GameStateDtoMapper.ToDto(
                engine.State,
                engine.Cards));
        }

        [HttpPost("{gameId:guid}/play-card")]
        public ActionResult<GameStateDto> PlayCard(Guid gameId, [FromBody] PlayCardRequest request)
        {
            var engine = _provider.Get(gameId);

            if (engine is null)
            {
                return NotFound("No game has been initialized.");
            }

            var state = engine.State;

            if (state.Status == GameStatus.Finished)
            {
                return BadRequest("The game is already over.");
            }

            try
            {
                var playerId = ResolvePlayerId(engine);

                engine.PlayCard(playerId, request.CardInstanceId);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }

            var dto = GameStateDtoMapper.ToDto(
                state,
                engine.Cards);

            return Ok(dto);
        }

        [HttpPost("{gameId:guid}/end-action-phase")]
        public ActionResult<GameStateDto> EndActionPhase(Guid gameId)
        {
            var engine = _provider.Get(gameId);

            if (engine is null)
            {
                return NotFound("No game has been initialized.");
            }

            var state = engine.State;

            try
            {
                engine.EndActionPhase();
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }

            return Ok(GameStateDtoMapper.ToDto(
                state,
                engine.Cards));
        }

        [HttpPost("{gameId:guid}/buy-card")]
        public ActionResult<GameStateDto> BuyCard(Guid gameId, [FromBody] BuyCardRequest request)
        {
            var engine = _provider.Get(gameId);

            if (engine is null)
            {
                return NotFound("No game has been initialized.");
            }

            var state = engine.State;

            try
            {
                var playerId = ResolvePlayerId(engine);

                engine.BuyCard(playerId, request.DefinitionId);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }

            return Ok(GameStateDtoMapper.ToDto(
                state,
                engine.Cards));
        }

        [HttpPost("{gameId:guid}/end-turn")]
        public ActionResult<GameStateDto> EndTurn(Guid gameId)
        {
            var engine = _provider.Get(gameId);

            if (engine is null)
            {
                return NotFound("No game has been initialized.");
            }

            var state = engine.State;

            try
            {
                engine.EndTurn();
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }

            return Ok(GameStateDtoMapper.ToDto(
                state,
                engine.Cards));
        }

        [HttpPost("{gameId:guid}/play-all-treasures")]
        public ActionResult<GameStateDto> PlayAllTreasures(Guid gameId)
        {
            var engine = _provider.Get(gameId);

            if (engine is null)
            {
                return NotFound($"Game {gameId} was not found.");
            }

            var state = engine.State;

            try
            {
                var playerId = ResolvePlayerId(engine);

                engine.PlayAllTreasures(playerId);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }

            return Ok(GameStateDtoMapper.ToDto(
                state,
                engine.Cards));
        }

    }
}
