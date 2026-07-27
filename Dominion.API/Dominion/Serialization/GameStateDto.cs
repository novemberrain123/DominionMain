using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Game.Enums;

namespace Dominion.API.Dominion.Serialization;

public class GameStateDto
{
    public required Guid GameId { get; init; }
    public required int TurnNumber { get; init; }
    public required GamePhase Phase { get; init; }
    public required int CurrentPlayerIndex { get; init; }
    public required Guid? CurrentPlayerId { get; init; }
    public required GameStatus Status { get; init; }

    public required List<PlayerDto> Players { get; init; }
    public required List<SupplyPileDto> Supply { get; init; }

    public required int TrashCount { get; init; }
    public GameResultDto? Result { get; init; }
    public PendingChoiceDto? PendingChoice { get; init; }
}

public class PlayerDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }

    public required int Actions { get; init; }
    public required int Buys { get; init; }
    public required int Coins { get; init; }

    public List<CardDto>? Hand { get; init; }
    public required List<CardDto> InPlay { get; init; }
    public required List<CardDto> DiscardPile { get; init; }
    public List<CardDto>? Deck { get; init; }

    public required int HandCount { get; init; }
    public required int DeckCount { get; init; }
}

public class CardDto
{
    public required Guid InstanceId { get; init; }
    public required string DefinitionId { get; init; }
    public required string Name { get; init; }
    public required int Cost { get; init; }
    public required List<CardType> Types { get; init; }
    public required List<string> Effects { get; init; }
}

public class SupplyPileDto
{
    public required string DefinitionId { get; init; }
    public required string Name { get; init; }
    public required int Cost { get; init; }
    public required List<CardType> Types { get; init; }
    public required int Remaining { get; init; }
    public required List<string> Effects { get; init; }
}

public class GameResultDto
{
    public required List<PlayerResultDto> PlayerResults { get; init; }
}

public class PlayerResultDto
{
    public required Guid PlayerId { get; init; }
    public required int VictoryPoints { get; init; }
    public required int Rank { get; init; }
}

public class PendingChoiceDto
{
    public required string Type { get; init; }

    public required Guid PlayerId { get; init; }

    public required string Prompt { get; init; }

    public required bool CanSkip { get; init; }

    public required List<string> EligibleCardIds { get; init; }
    public required List<string>? EligibleDefinitionIds { get; init; } //for supply pile interactions
}