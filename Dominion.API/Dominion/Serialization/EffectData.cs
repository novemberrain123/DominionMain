using Dominion.Dominion.Serialization.EffectDatas;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dominion.Dominion.Serialization
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(DrawCardsEffectData), "drawCards")]
    [JsonDerivedType(typeof(GainActionsEffectData), "gainActions")]
    [JsonDerivedType(typeof(GainCoinsEffectData), "gainCoins")]
    [JsonDerivedType(typeof(GainBuysEffectData), "gainBuys")]
    public abstract class EffectData
    {
    }
}
