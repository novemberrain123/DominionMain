using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Game.Enums;
using Dominion.API.Dominion.Serialization;

public class GainCardsEffectData : EffectData
{
    public int Minimum { get; init; } = 1;

    public int Maximum { get; init; } = 1;

    // For invidual cards
    public int? MaximumCardCost { get; init; }

    public int? MinimumCardCost { get; init; }

    public IReadOnlyList<CardType>? AllowedTypes { get; init; }

    public IReadOnlyList<CardType>? ExcludedTypes { get; init; }

    public CardDestination Destination { get; init; }
    public override string ToDisplayText()
    {
        string text = $"Gain {Minimum} to {Maximum} card(s)";
        if (MinimumCardCost.HasValue || MaximumCardCost.HasValue)
        {
            text += $" costing between {MinimumCardCost ?? 0} and {MaximumCardCost ?? int.MaxValue}";
        }
        if (AllowedTypes != null && AllowedTypes.Count > 0)
        {
            text += $" of types: {string.Join(", ", AllowedTypes)}";
        }
        if (ExcludedTypes != null && ExcludedTypes.Count > 0)
        {
            text += $" excluding types: {string.Join(", ", ExcludedTypes)}";
        }
        text += $" to {Destination}.";
        return text;
    }
}