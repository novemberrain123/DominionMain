using Dominion.API.Dominion.Cards.Primitives.PrimitiveEffectDatas;
using System.Text.Json.Serialization;

namespace Dominion.API.Dominion.Cards.Choices
{
    public abstract class PendingChoice
    {
        public required Guid PlayerId { get; init; }
        public required string Prompt { get; set; }

    }

}
