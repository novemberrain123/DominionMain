using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Cards.Choices;
using Dominion.API.Dominion.Game.Enums;
using Dominion.API.Dominion.Players;
using Dominion.API.Dominion.Serialization;
using Microsoft.Win32;
using System.Security.Cryptography;

namespace Dominion.API.Dominion.Game
{
    public class GameEngine
    {
        public CardRegistry Cards { get; }
        public GameState State { get; }
        private readonly Dictionary<string, Guid> _playerSessions = new();
        private readonly EffectResolver _effectResolver;
        private readonly ChoiceResolver _choiceResolver;
        private readonly GameSetupService _gameSetupService;
        private readonly GameConfig _config;

        public GameEngine(
            CardRegistry cards,
            GameState state,
            EffectResolver effectResolver,
            ChoiceResolver choiceResolver,
            GameSetupService gameSetupService,
            GameConfig config
            )
        {
            Cards = cards;
            State = state;
            _effectResolver = effectResolver;
            _choiceResolver = choiceResolver;
            _gameSetupService = gameSetupService;
            _config = config;
        }

        private void EnsureNoPendingChoice()
        {
            if (State.PendingChoice is not null)
            {
                throw new InvalidOperationException(
                    "The pending choice must be resolved first.");
            }
        }

        private void EnsureNoCurrentExecution()
        {
            if (State.CurrentExecution is not null)
            {
                throw new InvalidOperationException(
                    "Another card is still being resolved.");
            }
        }

        private void EnsureCurrentPlayer(Guid playerId)
        {
            if (State.Players.Count == 0)
            {
                throw new InvalidOperationException(
                    "The game has no players.");
            }

            var currentPlayer =
                State.Players[State.CurrentPlayerIndex];

            if (currentPlayer.Id != playerId)
            {
                throw new InvalidOperationException(
                    "It is not your turn.");
            }
        }

        public void StartGame(Guid playerId)
        {
            if (State.Status != GameStatus.Lobby)
            {
                throw new InvalidOperationException(
                    "The game has already started.");
            }

            if (State.Players.Count < 2)
            {
                throw new InvalidOperationException(
                    "At least two players are required.");
            }

            _gameSetupService.InitializeSupply(this, _config);

            _gameSetupService.InitializePlayers(this, _config);

            State.Status = GameStatus.Playing;
        }


        public string CreatePlayerSession(Guid playerId)
        {
            var token = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));

            _playerSessions[token] = playerId;

            return token;
        }

        public Guid GetPlayerIdFromToken(string token)
        {
            if (!_playerSessions.TryGetValue(token, out var playerId))
            {
                throw new InvalidOperationException(
                    "Invalid player token.");
            }

            return playerId;
        }

        public Player AddPlayer(string playerName)
        {
            if (State.Status != GameStatus.Lobby)
            {
                throw new InvalidOperationException(
                    "Players can only join while the game is in the lobby.");
            }

            if (string.IsNullOrWhiteSpace(playerName))
            {
                throw new ArgumentException(
                    "Player name is required.",
                    nameof(playerName));
            }

            if (State.Players.Count == _config.MaxPlayers)
            {
                throw new InvalidOperationException(
                    "The game is full.");
            }

            var player = new Player
            {
                Id = Guid.NewGuid(),
                Name = playerName.Trim()
            };

            State.Players.Add(player);

            return player;
        }

        public void CreatePendingChoice(PendingChoice choice)
        {
            if (State.PendingChoice is not null)
            {
                throw new InvalidOperationException(
                    "A pending choice already exists.");
            }

            State.PendingChoice = choice;
        }

        public void PlayAllTreasures(Guid playerId)
        {
            var player = State.Players
                .SingleOrDefault(p => p.Id == playerId)
                ?? throw new InvalidOperationException(
                    "Player does not exist.");

            var treasures = player.Hand
                .Where(IsTreasure)
                .ToList();

            foreach (var treasure in treasures)
            {
                PlayCard(playerId, treasure.Id);
            }
        }

        // Action and treasure play
        public void PlayCard(Guid playerId, Guid cardInstanceId)
        {
            EnsureCurrentPlayer(playerId);
            EnsureNoPendingChoice();
            EnsureNoCurrentExecution();

            if (State.CurrentExecution is not null)
            {
                throw new InvalidOperationException(
                    "Another card is still being resolved.");
            }

            var player = State.Players
                .SingleOrDefault(p => p.Id == playerId)
                ?? throw new InvalidOperationException(
                    "Player does not exist.");

            var card = player.Hand
                .SingleOrDefault(c => c.Id == cardInstanceId)
                ?? throw new InvalidOperationException(
                    "The selected card is not in the player's hand.");

            ValidateCanPlay(player, card);

            MoveToInPlay(player, card);

            if (IsAction(card))
            {
                player.Actions--;
            }

            ExecuteEffects(player, card);

            State.Events.Add(new GameEvent
            {
                SequenceNumber = State.Events.Count + 1,
                Type = GameEventType.CardPlayed,
                PlayerId = player.Id,
                Card = card.Id
            });
        }

        private void ValidateCanPlay(Player player, Card card)
        {
            if (!IsValidPlay(player, card, out var error))
            {
                throw new InvalidOperationException(error);
            }
        }

        private bool IsValidPlay(
            Player player,
            Card card,
            out string? error)
        {
            if (State.Status == GameStatus.Finished)
            {
                error = "The game is already over.";
                return false;
            }

            if (!player.Hand.Contains(card))
            {
                error = "Card not in hand";
                return false;
            }

            if (State.Phase == GamePhase.Action &&
                IsAction(card) &&
                player.Actions <= 0)
            {
                error = "No actions left";
                return false;
            }

            if (State.Phase != GamePhase.Buy &&
                IsTreasure(card))
            {
                error = "Treasures only playable in Buy phase";
                return false;
            }

            if (State.Phase == GamePhase.Buy &&
                !IsTreasure(card))
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

        // Used when building frontend DTOs
        public bool CanPlay(Player player, Card card)
        {
            return IsValidPlay(player, card, out _);
        }

        private static void MoveToInPlay(
            Player player,
            Card card)
        {
            player.Hand.Remove(card);
            player.InPlay.Add(card);
        }

        private void ExecuteEffects(
            Player player,
            Card sourceCard)
        {
            EnsureNoCurrentExecution();

            State.CurrentExecution = new EffectExecutionContext
            {
                Player = player,
                SourceCard = sourceCard,
                RemainingEffects = new Queue<EffectData>(
                    sourceCard.Definition.Effects)
            };

            ContinueResolvingEffects();
        }

        private void ContinueResolvingEffects()
        {
            var execution = State.CurrentExecution;

            if (execution is null)
            {
                return;
            }

            while (execution.RemainingEffects.Count > 0)
            {
                if (State.PendingChoice is not null)
                {
                    return;
                }

                var effect = execution.RemainingEffects.Dequeue();

                _effectResolver.Apply(
                    effect,
                    this,
                    execution.Player);

                if (State.PendingChoice is not null)
                {
                    return;
                }
            }

            State.CurrentExecution = null;
        }

        public void ResolveChoice(
            Guid playerId,
            ResolveChoiceRequest request)
        {
            var choice = State.PendingChoice
                ?? throw new InvalidOperationException(
                    "There is no pending choice.");

            if (choice.PlayerId != playerId)
            {
                throw new InvalidOperationException(
                    "This choice belongs to another player.");
            }

            var player = State.Players
                .SingleOrDefault(p => p.Id == playerId)
                ?? throw new InvalidOperationException(
                    "Player does not exist.");

            _choiceResolver.Resolve(
                choice,
                request,
                this,
                player);

            State.PendingChoice = null;

            ContinueResolvingEffects();
        }

        // Buy phase
        public void BuyCard(Guid playerId, string cardDefinitionId)
        {
            EnsureCurrentPlayer(playerId);
            EnsureNoPendingChoice();
            EnsureNoCurrentExecution();

            var player = State.Players
                .SingleOrDefault(p => p.Id == playerId)
                ?? throw new InvalidOperationException(
                    "Player does not exist.");

            var cardDefinition = Cards.Get(cardDefinitionId);

            ValidateCanBuy(player, cardDefinition);
            ResolveBuy(player, cardDefinition);

            if (IsGameOver())
            {
                EndGame();
            }
        }

        public void GainCard(
            Player player,
            CardDefinition card,
            CardDestination destination)
        {
            var pile = State.SupplyPiles[card.Id];

            pile.RemoveCard();

            var instance = new Card(card);

            switch (destination)
            {
                case CardDestination.Discard:
                    player.DiscardPile.Add(instance);
                    break;

                case CardDestination.Hand:
                    player.Hand.Add(instance);
                    break;

                case CardDestination.DeckTop:
                    player.Deck.Insert(0, instance);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown gain destination '{destination}'.");
            }
        }

        private void ValidateCanBuy(
            Player player,
            CardDefinition card)
        {
            if (!IsValidBuy(player, card, out var error))
            {
                throw new InvalidOperationException(error);
            }
        }

        private bool IsValidBuy(
            Player player,
            CardDefinition card,
            out string? error)
        {
            if (State.Status == GameStatus.Finished)
            {
                error = "The game is already over.";
                return false;
            }

            if (State.Phase != GamePhase.Buy)
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

            if (!State.SupplyPiles.TryGetValue(
                    card.Id,
                    out var pile))
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

        private void ResolveBuy(
            Player player,
            CardDefinition card)
        {
            var pile = State.SupplyPiles[card.Id];

            pile.RemoveCard();

            var instance = new Card(card);

            player.DiscardPile.Add(instance);
            player.Coins -= card.Cost;
            player.Buys--;

            State.Events.Add(new GameEvent
            {
                SequenceNumber = State.Events.Count + 1,
                Type = GameEventType.CardBought,
                PlayerId = player.Id,
                Card = instance.Id
            });
        }

        // Used when building frontend DTOs
        public bool CanBuy(
            Player player,
            CardDefinition card)
        {
            return IsValidBuy(player, card, out _);
        }

        private bool IsGameOver()
        {
            // Hardcoded for now. This could eventually be a game setting.
            if (!State.SupplyPiles.TryGetValue(
                    "province",
                    out var provincePile))
            {
                throw new InvalidOperationException(
                    "Province pile is missing.");
            }

            if (provincePile.Count == 0)
            {
                return true;
            }

            var emptyPileCount = State.SupplyPiles.Values
                .Count(pile => pile.Count == 0);

            return emptyPileCount >= 3;
        }

        private void EndGame()
        {
            State.Status = GameStatus.Finished;
            State.Result = CalculateResults();

            State.Events.Add(new GameEvent
            {
                SequenceNumber = State.Events.Count + 1,
                Type = GameEventType.GameOver
            });
        }

        private GameResult CalculateResults()
        {
            var scoredPlayers = State.Players
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

            for (var index = 0;
                 index < scoredPlayers.Count;
                 index++)
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

        private static int CalculateVictoryPoints(Player player)
        {
            var ownedCards = player.Deck
                .Concat(player.Hand)
                .Concat(player.DiscardPile)
                .Concat(player.InPlay);

            return ownedCards.Sum(
                card => card.Definition.VictoryPoints);
        }

        // Phase transition
        public void EndActionPhase(Guid playerId)
        {
            EnsureCurrentPlayer(playerId);

            if (State.PendingChoice is not null)
            {
                throw new InvalidOperationException(
                    "The pending choice must be resolved first.");
            }

            if (State.CurrentExecution is not null)
            {
                throw new InvalidOperationException(
                    "Another card is still being resolved.");
            }

            if (State.Phase != GamePhase.Action)
            {
                throw new InvalidOperationException(
                    "The game is not in the Action phase.");
            }

            State.Phase = GamePhase.Buy;
        }

        // Turn transition
        public void EndTurn(Guid playerId)
        {
            EnsureCurrentPlayer(playerId);

            if (State.PendingChoice is not null)
            {
                throw new InvalidOperationException(
                    "The pending choice must be resolved first.");
            }

            if (State.CurrentExecution is not null)
            {
                throw new InvalidOperationException(
                    "Another card is still being resolved.");
            }

            var currentPlayer =
                State.Players[State.CurrentPlayerIndex];

            Cleanup(currentPlayer);
            MoveToNextPlayer();

            var nextPlayer =
                State.Players[State.CurrentPlayerIndex];

            StartTurn(nextPlayer);
        }

        private static void Cleanup(Player player)
        {
            player.DiscardPile.AddRange(player.Hand);
            player.DiscardPile.AddRange(player.InPlay);

            player.Hand.Clear();
            player.InPlay.Clear();

            player.Draw(5);
        }

        private static void StartTurn(Player player)
        {
            player.Actions = 1;
            player.Buys = 1;
            player.Coins = 0;
        }

        private void MoveToNextPlayer()
        {
            State.CurrentPlayerIndex =
                (State.CurrentPlayerIndex + 1) %
                State.Players.Count;

            State.TurnNumber++;
            State.Phase = GamePhase.Action;
        }

        private static bool IsAction(Card card)
        {
            return card.Definition.Types.Contains(
                CardType.Action);
        }

        private static bool IsTreasure(Card card)
        {
            return card.Definition.Types.Contains(
                CardType.Treasure);
        }
    }
}