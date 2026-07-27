using Dominion.API.Dominion.Cards.Choices;
using Dominion.API.Dominion.Game.Enums;

public class GainCardsPendingChoice : PendingChoice
{
    public required int Minimum { get; init; }
    public required int Maximum { get; init; }
    public int? MinimumCost{ get; init; }
    public int? MaximumCost { get; init; }
    public CardDestination Destination { get; init; }
    public required IReadOnlyList<string> EligibleDefinitionIds { get; init; }
}
