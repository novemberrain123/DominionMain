using Dominion.Dominion.Game;
using Dominion.Dominion.Game.Debug;
using Microsoft.AspNetCore.Mvc;

namespace Dominion.Controllers
{
    [ApiController]
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        private readonly GameEngineFactory _factory;
        private readonly GameEngineProvider _provider;

        public DebugController(GameEngineFactory factory, GameEngineProvider provider)
        {
            _provider = provider;
            _factory = factory;
        }


        [HttpGet("bootstrap")]
        public IActionResult Bootstrap()
        {
            var engine = _factory.Create(
                "Content/Modes/base_mode.json",
                "Content/Cards/test.json"
            );

            return Ok(new
            {
                cardCount = engine.Cards.Count,
                card = engine.Cards.GetAllDtos(),
                supplyCount = engine.State.SupplyPiles.Count,
                supply = engine.State.SupplyPiles.ToDictionary(x => x.Key, x => x.Value.Count)
            });
        }

        [HttpGet("cards")]
        public IActionResult GetCards()
        {
            var engine = _provider.Engine!;

            return Ok(engine.Cards.GetAllDtos());
        }

        [HttpGet("supply")]
        public IActionResult GetSupply()
        {
            var engine = _provider.Engine!;

            return Ok(engine.State.SupplyPiles.ToDictionary(
                x => x.Key,
                x => new
                {
                    x.Value.CardDefId,
                    x.Value.Count
                }
            ));
        }


        [HttpGet("state")]
        public IActionResult GetState()
        {
            var engine = _provider.Engine!;

            return Ok(new
            {
                turn = engine.State.TurnNumber,
                phase = engine.State.Phase,
                currentPlayerIndex = engine.State.CurrentPlayerIndex,

                players = engine.State.Players.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Actions,
                    p.Buys,
                    p.Coins,
                    deckSize = p.Deck.Count,
                    handSize = p.Hand.Count,
                    discardSize = p.DiscardPile.Count
                }),

                trashSize = engine.State.Trash.Count,
                eventCount = engine.State.Events.Count,
                isGameOver = engine.State.IsGameOver
            });
        }

        [HttpGet("players")]
        public IActionResult GetPlayers()
        {
            var engine = _provider.Engine!;

            return Ok(
                engine.State.Players.Select(player => new
                {
                    player.Id,
                    player.Name,

                    player.Actions,
                    player.Buys,
                    player.Coins,

                    Hand = player.Hand.Select(c => new
                    {
                        c.Definition.Id,
                        Types = c.Definition.Types.Select(t => t.ToString()),
                        c.Definition.Cost
                    }),

                    DeckCount = player.Deck.Count,

                    Deck = player.Deck.Select(c => c.Definition.Id),

                    DiscardPile = player.DiscardPile.Select(c => c.Definition.Id),

                    InPlay = player.InPlay.Select(c => c.Definition.Id)
                })
            );
        }

    }
}
