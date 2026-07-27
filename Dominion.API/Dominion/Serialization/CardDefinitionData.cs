namespace Dominion.API.Dominion.Serialization
{
    public class CardDefinitionData
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public int Cost { get; set; }
        public int VictoryPoints { get; set; }
        public string Description { get; set; }
        public List<string> Types { get; set; }
        public List<EffectData> Effects { get; set; }
    }
}
