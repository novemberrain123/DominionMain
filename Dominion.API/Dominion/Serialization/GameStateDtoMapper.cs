using Dominion.API.Dominion.Game;
using Dominion.API.Dominion.Serialization;
using Dominion.Dominion.Cards;
using Dominion.Dominion.Game;
using Dominion.Dominion.Players;
using Dominion.Dominion.Serialization.EffectDatas;

namespace Dominion.Dtos;

public static class GameStateDtoMapper
{
    public static GameStateDto ToDto(
        GameState state,
        CardRegistry cardRegistry)
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
                .Select(pile => ToSupplyPileDto(pile, cardRegistry))
                .ToList(),

            Players = state.Players
                .Select(ToPlayerDto)
                .ToList(),

            TrashCount = state.Trash.Count
        };
    }

    private static PlayerDto ToPlayerDto(Player player)
    {
        return new PlayerDto
        {
            Id = player.Id,
            Name = player.Name,

            Actions = player.Actions,
            Buys = player.Buys,
            Coins = player.Coins,

            Hand = player.Hand
                .Select(ToCardDto)
                .ToList(),

            InPlay = player.InPlay
                .Select(ToCardDto)
                .ToList(),

            DiscardPile = player.DiscardPile
                .Select(ToCardDto)
                .ToList(),

            Deck = player.Deck
                .Select(ToCardDto)
                .ToList(),
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
            Types = card.Definition.Types.ToList()
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