using Dominion.Dominion.Cards;

namespace Dominion.Dominion.Players
{
    public class Player
    {
        public Guid Id { get; init; }
        public string Name { get; init; }

        public int Actions { get; set; }
        public int Buys { get; set; }
        public int Coins { get; set; }

        public List<Card> Deck { get; set; } = new();
        public List<Card> Hand { get; set; } = new();
        public List<Card> DiscardPile { get; set; } = new();
        public List<Card> InPlay { get; set; } = new();

        public void Draw(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (Deck.Count == 0)
                {
                    ReshuffleDiscardIntoDeck();
                }

                if (Deck.Count == 0)
                {
                    // No cards left anywhere
                    return;
                }

                var card = Deck[^1];
                Deck.RemoveAt(Deck.Count - 1);
                Hand.Add(card);
            }
        }

        public void Discard(Card card)
        {
            Hand.Remove(card);
            DiscardPile.Add(card);
        }

        public void Gain(Card card)
        {
            DiscardPile.Add(card);
        }

        private void ReshuffleDiscardIntoDeck()
        {
            Deck.AddRange(DiscardPile);
            DiscardPile.Clear();

            //shuffle
            Deck = Deck.OrderBy(_ => Guid.NewGuid()).ToList();
        }

    }
}