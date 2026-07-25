namespace Dominion.API.Dominion.Serialization.RequestDto;

public sealed class PlayCardRequest
{
    public required Guid CardInstanceId { get; init; }
}