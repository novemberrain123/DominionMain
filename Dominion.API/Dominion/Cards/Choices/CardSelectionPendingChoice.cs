namespace Dominion.API.Dominion.Cards.Choices
{
    public abstract class CardSelectionPendingChoice : PendingChoice
    {
        public required int Minimum { get; init; }

        public required int Maximum { get; init; }

        public required IReadOnlyList<Guid> EligibleCardIds { get; init; }
    }
}
