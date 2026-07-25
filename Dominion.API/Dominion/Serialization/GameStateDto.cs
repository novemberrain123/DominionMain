using Dominion.Dominion.Cards;
using Dominion.Dominion.Game;

namespace Dominion.API.Dominion.Serialization;

public sealed class GameStateDto
{
    public required int TurnNumber { get; init; }
    public required GamePhase Phase { get; init; }
    public required int CurrentPlayerIndex { get; init; }
    public required Guid CurrentPlayerId { get; init; }
    public required bool IsGameOver { get; init; }

    public required List<PlayerDto> Players { get; init; }
    public required List<SupplyPileDto> Supply { get; init; }

    public required int TrashCount { get; init; }
}

public sealed class PlayerDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }

    public required int Actions { get; init; }
    public required int Buys { get; init; }
    public required int Coins { get; init; }

    public required List<CardDto> Hand { get; init; }
    public required List<CardDto> InPlay { get; init; }
    public required List<CardDto> DiscardPile { get; init; }
    public required List<CardDto> Deck { get; init; }
}

public sealed class CardDto
{
    public required Guid InstanceId { get; init; }
    public required string DefinitionId { get; init; }
    public required string Name { get; init; }
    public required int Cost { get; init; }
    public required List<CardType> Types { get; init; }
}

public sealed class SupplyPileDto
{
    public required string DefinitionId { get; init; }
    public required string Name { get; init; }
    public required int Cost { get; init; }
    public required List<CardType> Types { get; init; }
    public required int Remaining { get; init; }
}