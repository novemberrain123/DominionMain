using Dominion.API.Dominion.Cards.Primitives.PrimitiveEffectDatas;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dominion.API.Dominion.Serialization
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(DrawCardsEffectData), "drawCards")]
    [JsonDerivedType(typeof(GainActionsEffectData), "gainActions")]
    [JsonDerivedType(typeof(GainCoinsEffectData), "gainCoins")]
    [JsonDerivedType(typeof(GainBuysEffectData), "gainBuys")]
    [JsonDerivedType(typeof(TrashCardsEffectData), "trashCards")]
    [JsonDerivedType(typeof(GainCardsEffectData), "gainCard")]
    public abstract class EffectData
    {
        public abstract string ToDisplayText();
    }
}
