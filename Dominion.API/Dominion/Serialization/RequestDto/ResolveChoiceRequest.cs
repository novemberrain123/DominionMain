public sealed class ResolveChoiceRequest
{
    public IReadOnlyList<Guid>? SelectedCardInstanceIds { get; init; }
    public IReadOnlyList<string>? SelectedDefinitionIds { get; init; } 
    public bool Skip { get; init; }
}