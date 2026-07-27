using System.Text.Json.Serialization;

namespace Dominion.API.Dominion.Game.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter<CardDestination>))]
    public enum CardDestination
    {
        Hand,
        Discard,
        DeckTop,
        DeckBottom,
    }
}