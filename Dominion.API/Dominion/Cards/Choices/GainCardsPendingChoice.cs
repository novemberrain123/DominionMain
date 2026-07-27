using Dominion.API.Dominion.Cards.Choices;
using Dominion.API.Dominion.Game.Enums;

public class GainCardsPendingChoice : PendingChoice
{
    public required int Minimum { get; init; }
    public required int Maximum { get; init; }
    public int? MinimumCardCost{ get; init; }
    public int? MaximumCardCost { get; init; }
    public CardDestination Destination { get; init; }
    public required IReadOnlyList<string> EligibleDefinitionIds { get; init; }
}
