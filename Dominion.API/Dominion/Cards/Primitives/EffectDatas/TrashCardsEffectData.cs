using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Serialization;

public class TrashCardsEffectData : EffectData
{
    public int Minimum { get; init; }

    public int Maximum { get; init; }

    public CardType? RequiredType { get; init; }

    //TODO: allow overriding the display text from json
    public override string ToDisplayText()
    {
        if (Minimum == Maximum)
        {
            return $"Trash {Minimum} " +
                (Minimum == 1 ? "Card" : "Cards") +
                (RequiredType.HasValue ? $" of type {RequiredType.Value}" : "");
        }
        else
        {
            return $"Trash between {Minimum} and {Maximum} " +
                (RequiredType.HasValue ? $"Cards of type {RequiredType.Value}" : "Cards");
        }
    }
}