namespace Dominion.API.Dominion.Serialization;

public sealed class PlayCardRequest
{
    public required Guid CardInstanceId { get; init; }
}