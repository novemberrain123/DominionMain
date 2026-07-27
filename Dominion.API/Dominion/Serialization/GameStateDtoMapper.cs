using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Game;
using Dominion.API.Dominion.Players;
using Dominion.API.Dominion.Serialization.EffectDatas;

namespace Dominion.API.Dominion.Serialization;

public static class GameStateDtoMapper
{
    public static GameStateDto ToDto(
        GameState state,
        CardRegistry cardRegistry,
        Guid? requestingPlayerId = null,
        bool revealAllPrivateInformation = false)
    {
        var currentPlayer =
            state.Players.Count > 0 &&
            state.CurrentPlayerIndex >= 0 &&
            state.CurrentPlayerIndex < state.Players.Count
                ? state.Players[state.CurrentPlayerIndex]
                : null;

        return new GameStateDto
        {
            GameId = state.GameId,
            TurnNumber = state.TurnNumber,
            Phase = state.Phase,
            CurrentPlayerIndex = state.CurrentPlayerIndex,
            CurrentPlayerId = currentPlayer?.Id,
            Status = state.Status,

            Result = state.Result is null
                ? null
                : ToGameResultDto(state.Result),

            Supply = state.SupplyPiles.Values
                .Select(pile =>
                    ToSupplyPileDto(pile, cardRegistry))
                .ToList(),

            //discard pile visible to all, hand visible only to the player themselves, deck visible only to admin spectator
            //return null for hand and deck if not visible to the requesting player, return [] if either are empty
            Players = state.Players
                .Select(player => ToPlayerDto(
                    player,
                    revealHand:
                        revealAllPrivateInformation ||
                        player.Id == requestingPlayerId,
                    revealDeck:
                        revealAllPrivateInformation))
                .ToList(),

            TrashCount = state.Trash.Count
        };
    }

    private static PlayerDto ToPlayerDto(
        Player player,
        bool revealHand,
        bool revealDeck)
    {
        return new PlayerDto
        {
            Id = player.Id,
            Name = player.Name,

            Hand = revealHand
                ? player.Hand.Select(ToCardDto).ToList()
                : null,

            HandCount = player.Hand.Count,

            DiscardPile = player.DiscardPile
                .Select(ToCardDto)
                .ToList(),

            Deck = revealDeck
                ? player.Deck.Select(ToCardDto).ToList()
                : null,

            DeckCount = player.Deck.Count,

            InPlay = player.InPlay
                .Select(ToCardDto)
                .ToList(),

            Actions = player.Actions,
            Buys = player.Buys,
            Coins = player.Coins
        };
    }
    private static CardDto ToCardDto(Card card)
    {
        return new CardDto
        {
            InstanceId = card.Id,
            DefinitionId = card.Definition.Id,
            Name = card.Definition.DisplayName,
            Cost = card.Definition.Cost,
            Types = card.Definition.Types.ToList(),
            Effects = card.Definition.Effects.Select(e => e.ToDisplayText()).ToList()
        };
    }

    private static SupplyPileDto ToSupplyPileDto(
        SupplyPile pile,
        CardRegistry cardRegistry)
    {
        CardDefinition definition =
            cardRegistry.Get(pile.CardDefId);

        return new SupplyPileDto
        {
            DefinitionId = definition.Id,
            Name = definition.DisplayName,
            Cost = definition.Cost,
            Types = definition.Types.ToList(),
            Effects = definition.Effects.Select(e => e.ToDisplayText()).ToList(),
            Remaining = pile.Count
        };
    }

    private static GameResultDto ToGameResultDto(GameResult result)
    {
        return new GameResultDto
        {
            PlayerResults = result.Players
                .Select(player => new PlayerResultDto
                {
                    PlayerId = player.PlayerId,
                    VictoryPoints = player.VictoryPoints,
                    Rank = player.Rank
                })
                .ToList()
        };


    }


}