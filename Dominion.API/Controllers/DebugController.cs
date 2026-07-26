using Dominion.API.Dominion.Serialization;
using Dominion.API.Dominion.Serialization.RequestDto;
using Dominion.Dominion.Game;
using Dominion.Dominion.Game.Debug;
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

        //[HttpGet("cards")]
        //public IActionResult GetCards()
        //{
        //    var engine = _provider.Engine!;

        //    return Ok(engine.Cards.GetAllDtos());
        //}

        //[HttpGet("supply")]
        //public IActionResult GetSupply()
        //{
        //    var engine = _provider.Engine!;

        //    return Ok(engine.State.SupplyPiles.ToDictionary(
        //        x => x.Key,
        //        x => new
        //        {
        //            x.Value.CardDefId,
        //            x.Value.Count
        //        }
        //    ));
        //}


        //[HttpGet("state")]
        //public IActionResult GetState()
        //{
        //    var engine = _provider.Engine!;

        //    return Ok(new
        //    {
        //        turn = engine.State.TurnNumber,
        //        phase = engine.State.Phase,
        //        currentPlayerIndex = engine.State.CurrentPlayerIndex,

        //        players = engine.State.Players.Select(p => new
        //        {
        //            p.Id,
        //            p.Name,
        //            p.Actions,
        //            p.Buys,
        //            p.Coins,
        //            deckSize = p.Deck.Count,
        //            handSize = p.Hand.Count,
        //            discardSize = p.DiscardPile.Count
        //        }),

        //        trashSize = engine.State.Trash.Count,
        //        eventCount = engine.State.Events.Count,
        //        isGameOver = engine.State.IsGameOver
        //    });
        //}

        //[HttpGet("players")]
        //public IActionResult GetPlayers()
        //{
        //    var engine = _provider.Engine!;

        //    return Ok(
        //        engine.State.Players.Select(player => new
        //        {
        //            player.Id,
        //            player.Name,

        //            player.Actions,
        //            player.Buys,
        //            player.Coins,

        //            Hand = player.Hand.Select(c => new
        //            {
        //                c.Definition.Id,
        //                Types = c.Definition.Types.Select(t => t.ToString()),
        //                c.Definition.Cost
        //            }),

        //            DeckCount = player.Deck.Count,

        //            Deck = player.Deck.Select(c => c.Definition.Id),

        //            DiscardPile = player.DiscardPile.Select(c => c.Definition.Id),

        //            InPlay = player.InPlay.Select(c => c.Definition.Id)
        //        })
        //    );
        //}


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

            var currentPlayer = state.Players[state.CurrentPlayerIndex];

            var card = currentPlayer.Hand
                .FirstOrDefault(card => card.Id == request.CardInstanceId);

            if (card is null)
            {
                return BadRequest("The selected card is not in the current player's hand.");
            }

            try
            {
                engine.PlayCard(currentPlayer, card);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
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
            var currentPlayer = state.Players[state.CurrentPlayerIndex];

            try
            {
                engine.BuyCard(currentPlayer, request.DefinitionId);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
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
            var currentPlayer = state.Players[state.CurrentPlayerIndex];

            try
            {
                engine.PlayAllTreasures(currentPlayer);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }

            return Ok(GameStateDtoMapper.ToDto(
                state,
                engine.Cards));
        }

    }
}
