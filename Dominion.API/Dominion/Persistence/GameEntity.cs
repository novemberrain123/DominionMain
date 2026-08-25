namespace Dominion.API.Dominion.Persistance
{
    public class GameEntity
    {
        public Guid Id { get; set; }
        public string Mode { get; set; } = null!;
        public string StateJson { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
