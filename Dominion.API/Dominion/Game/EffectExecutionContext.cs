using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Players;
using Dominion.API.Dominion.Serialization;

public class EffectExecutionContext
{
    public required Player Player { get; init; }
    public required Card SourceCard { get; init; }
    public required Queue<EffectData> RemainingEffects { get; init; }
    public Card? LastTrashedCard { get; set; }
    public List<Card> RememberedCards { get; set; } = [];
}