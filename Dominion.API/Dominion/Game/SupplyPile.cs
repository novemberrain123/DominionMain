using Dominion.Dominion.Cards;

namespace Dominion.Dominion.Game
{
    public class SupplyPile
    {
        public string CardDefId { get;  set; }
        public int Count { get; set; }

        public SupplyPile(string cardDefId, int count)
        {
            CardDefId = cardDefId;
            Count = count;
        }

        public bool IsEmpty()
        {
            return Count <= 0;
        }

        public void RemoveCard()
        {
            if (Count > 0)
                Count--;
        }

    }
}