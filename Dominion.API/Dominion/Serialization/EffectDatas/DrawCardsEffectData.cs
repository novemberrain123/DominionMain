using Dominion.API.Dominion.Serialization;

namespace Dominion.API.Dominion.Serialization.EffectDatas
{
    public class DrawCardsEffectData : EffectData
    {
        public int Amount { get; set; }

        public override string ToDisplayText()
        {
            return $"+{Amount} " + (Amount == 1 ? "Card" : "Cards");
        }
    }
}
