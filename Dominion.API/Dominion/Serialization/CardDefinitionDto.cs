namespace Dominion.API.Dominion.Serialization
{
    public class CardDefinitionDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public int Cost { get; set; }
        public List<string> Types { get; set; }
        public List<string> Effects { get; set; }
    }
}
