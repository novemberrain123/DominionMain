namespace Dominion.API.Dominion.Serialization.RequestDto;

public sealed class BuyCardRequest
{
    public required string CardDefId { get; init; }
}