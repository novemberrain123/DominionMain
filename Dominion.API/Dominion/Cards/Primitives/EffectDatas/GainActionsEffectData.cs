using Dominion.API.Dominion.Serialization;

namespace Dominion.API.Dominion.Cards.Primitives.PrimitiveEffectDatas
{
    public class GainActionsEffectData : EffectData
    {
        public int Amount { get; set; }

        public override string ToDisplayText()
        {
            return $"+{Amount} " + (Amount == 1 ? "Action" : "Actions");
        }
    }
}
