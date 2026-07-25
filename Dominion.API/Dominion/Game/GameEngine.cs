using Dominion.Dominion.Cards;
using Dominion.Dominion.Players;

namespace Dominion.Dominion.Game
{
    public class GameEngine
    {
        public CardRegistry Cards { get; }
        public GameState State { get; }

        private readonly EffectResolver _effectResolver;

        public GameEngine(CardRegistry cards, GameState state, EffectResolver effectResolver)
        {
            Cards = cards;
            State = state;
            _effectResolver = effectResolver;
        }

        //action phase
        public void PlayCard(GameState state, Player player, Card card)
        {
            ValidateCanPlay(state, player, card);

            MoveToInPlay(player, card);

            if (IsAction(card))
                player.Actions--;

            ExecuteEffect(state, player, card);

            state.Events.Add(new GameEvent
            {
                SequenceNumber = state.Events.Count + 1,
                Type = GameEventType.CardPlayed,
                PlayerId = player.Id,
                Card = card.Id
            });
        }

        private void ValidateCanPlay(GameState state, Player player, Card card)
        {
            if (!IsValidatePlay(state, player, card, out var error))
            {
                throw new InvalidOperationException(error);
            }
        }

        private bool IsValidatePlay(GameState state, Player player, Card card, out string? error)
        {
            if (!player.Hand.Contains(card))
            {
                error = "Card not in hand";
                return false;
            }

            if (state.Phase == GamePhase.Action && IsAction(card) && player.Actions <= 0)
            {
                error = "No actions left";
                return false;
            }

            if (state.Phase != GamePhase.Buy && IsTreasure(card))
            {
                error = "Treasures only playable in Buy phase";
                return false;
            }

            if (state.Phase == GamePhase.Buy && !IsTreasure(card))
            {
                error = "Only treasures can be played in Buy phase";
                return false;
            }

            if (!IsAction(card) && !IsTreasure(card))
            {
                error = "This card cannot be played";
                return false;
            }

            error = null;
            return true;
        }

        //for frontend
        public bool CanPlay(GameState state, Player player, Card card)
        {
            return IsValidatePlay(state, player, card, out _);
        }

        private void MoveToInPlay(Player player, Card card)
        {
            player.Hand.Remove(card);
            player.InPlay.Add(card);
        }

        private void ExecuteEffect(GameState state, Player player, Card card)
        {
            foreach (var effect in card.Definition.Effects)
            {
                _effectResolver.Apply(effect, this, player);
            }
        }

        //buy phase
        public void BuyCard(GameState state, Player player, CardDefinition card)
        {
            ValidateCanBuy(state, player, card);
            ResolveBuy(state, player, card);
        }

        private void ValidateCanBuy(GameState state, Player player, CardDefinition card)
        {
            if (!IsValidBuy(state, player, card, out var error))
            {
                throw new InvalidOperationException(error);
            }
        }

        private bool IsValidBuy(GameState state, Player player, CardDefinition card, out string? error)
        {
            if (state.Phase != GamePhase.Buy)
            {
                error = "Not in Buy phase";
                return false;
            }

            if (player.Buys <= 0)
            {
                error = "No buys remaining";
                return false;
            }

            if (player.Coins < card.Cost)
            {
                error = "Not enough coins";
                return false;
            }

            if (!state.SupplyPiles.TryGetValue(card.DisplayName, out var pile))
            {
                error = "Card not in supply";
                return false;
            }

            if (pile.Count <= 0)
            {
                error = "Card pile empty";
                return false;
            }

            error = null;
            return true;
        }

        private void ResolveBuy(GameState state, Player player, CardDefinition card)
        {
            var pile = state.SupplyPiles[card.DisplayName];

            // 1. Remove from supply
            pile.RemoveCard();

            // 2. Create new card instance
            var instance = new Card
            {
                Definition = card
            };

            // 3. Add to discard pile
            player.DiscardPile.Add(instance);

            // 4. Deduct resources
            player.Coins -= card.Cost;
            player.Buys--;

            // 5. Log event
            state.Events.Add(new GameEvent
            {
                SequenceNumber = state.Events.Count + 1,
                Type = GameEventType.CardBought,
                PlayerId = player.Id,
                Card = instance.Id
            });
        }

        //for frontend
        public bool CanBuy(GameState state, Player player, CardDefinition card)
        {
            return IsValidBuy(state, player, card, out _);
        }

        //cleanup phase
        private void Cleanup(Player player)
        {
            player.DiscardPile.AddRange(player.Hand);
            player.DiscardPile.AddRange(player.InPlay);

            player.Hand.Clear();
            player.InPlay.Clear();

            player.Draw(5);
        }

        //start turn
        private void StartTurn(Player player)
        {
            player.Actions = 1;
            player.Buys = 1;
            player.Coins = 0;
        }

        //next player
        private void NextPlayer(GameState state)
        {
            state.CurrentPlayerIndex = (state.CurrentPlayerIndex + 1) % state.Players.Count;
            state.TurnNumber++;
            state.Phase = GamePhase.Action;
        }

        //end turn
        public void EndTurn(GameState state, Player player)
        {
            Cleanup(player);
            NextPlayer(state);
            StartTurn(state.Players[state.CurrentPlayerIndex]);
        }

        //helper
        private bool IsAction(Card card)
        {
            return card.Definition.Types.Contains(CardType.Action);
        }

        private bool IsTreasure(Card card)
        {
            return card.Definition.Types.Contains(CardType.Treasure);
        }

        private bool IsVictory(Card card)
        {
            return card.Definition.Types.Contains(CardType.Victory);
        }
    }
}
