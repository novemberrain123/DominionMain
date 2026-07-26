using Dominion.API.Dominion.Game;
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

        public void PlayAllTreasures(GameState state, Player player)
        {
            var treasures = player.Hand
                .Where(IsTreasure)
                .ToList();

            foreach (var treasure in treasures)
            {
                PlayCard(state, player, treasure);
            }
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
        public void BuyCard(GameState state, Player player, string cardDef)
        {
            var card = Cards.Get(cardDef);
            ValidateCanBuy(state, player, card);
            ResolveBuy(state, player, card);
            if (IsGameOver(state))
            {
                EndGame(state);
            }
        }

        private void EndGame(GameState state)
        {
            state.IsGameOver = true;
            state.Result = CalculateResults(state);

            state.Events.Add(new GameEvent
            {
                SequenceNumber = state.Events.Count + 1,
                Type = GameEventType.GameOver
            });
        }

        private GameResult CalculateResults(GameState state)
        {
            var scoredPlayers = state.Players
                .Select(player => new
                {
                    PlayerId = player.Id,
                    VictoryPoints = CalculateVictoryPoints(player)
                })
                .OrderByDescending(player => player.VictoryPoints)
                .ToList();

            var results = new List<PlayerResult>();
            var currentRank = 0;
            int? previousScore = null;

            for (var index = 0; index < scoredPlayers.Count; index++)
            {
                var player = scoredPlayers[index];

                if (previousScore is null ||
                    player.VictoryPoints != previousScore.Value)
                {
                    currentRank = index + 1;
                }

                results.Add(new PlayerResult
                {
                    PlayerId = player.PlayerId,
                    VictoryPoints = player.VictoryPoints,
                    Rank = currentRank
                });

                previousScore = player.VictoryPoints;
            }

            return new GameResult
            {
                Players = results
            };
        }

        private int CalculateVictoryPoints(Player player)
        {
            var ownedCards = player.Deck
                .Concat(player.Hand)
                .Concat(player.DiscardPile)
                .Concat(player.InPlay);

            return ownedCards.Sum(card =>
            {
                return card.Definition.VictoryPoints;
            });
        }

        private bool IsGameOver(GameState state)
        {
            // hardcode for now, may add a game setting to allow for different end conditions
            if (!state.SupplyPiles.TryGetValue("province", out var provincePile))
            {
                throw new InvalidOperationException(
                    "Province pile is missing.");
            }

            if (provincePile.Count == 0)
            {
                return true;
            }

            var emptyPileCount = state.SupplyPiles.Values
                .Count(pile => pile.Count == 0);

            return emptyPileCount == 3;
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

            if (!state.SupplyPiles.TryGetValue(card.Id, out var pile))
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
            var pile = state.SupplyPiles[card.Id];

            pile.RemoveCard();

            var instance = new Card
            {
                Definition = card
            };

            player.DiscardPile.Add(instance);

            player.Coins -= card.Cost;
            player.Buys--;

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

        //end action phase
        public void EndActionPhase(GameState state)
        {
            if (state.IsGameOver)
            {
                throw new InvalidOperationException(
                    "The game is already over.");
            }

            if (state.Phase != GamePhase.Action)
            {
                throw new InvalidOperationException(
                    "The game is not in the Action phase.");
            }

            state.Phase = GamePhase.Buy;
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
        public void EndTurn(GameState state)
        {
            if (state.IsGameOver)
            {
                throw new InvalidOperationException(
                    "The game is already over.");
            }

            var currentPlayer = state.Players[state.CurrentPlayerIndex];

            Cleanup(currentPlayer);
            NextPlayer(state);

            var nextPlayer = state.Players[state.CurrentPlayerIndex];
            StartTurn(nextPlayer);
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
