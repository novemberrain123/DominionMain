using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Cards.Choices;
using Dominion.API.Dominion.Game;
using Dominion.API.Dominion.Game.Enums;
using Dominion.API.Dominion.Players;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dominion.API.Dominion.Serialization
{
    public class GameStateSerializer
    {
        private readonly JsonSerializerOptions _options;

        public GameStateSerializer()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public string Serialize(GameState state)
        {
            var dto = ToDto(state);

            return JsonSerializer.Serialize(dto, _options);
        }

        public GameState Deserialize(
            string json,
            CardRegistry registry)
        {
            var dto = JsonSerializer.Deserialize<GameStateDto>(
                json,
                _options)
                ?? throw new InvalidOperationException(
                    "Failed to deserialize game state.");

            return FromDto(dto, registry);
        }

        private GameStateDto ToDto(GameState state)
        {
            return new GameStateDto
            {
                GameId = state.GameId,
                Players = state.Players
                    .Select(ToPlayerDto)
                    .ToList(),

                CurrentPlayerIndex = state.CurrentPlayerIndex,
                Phase = state.Phase,
                TurnNumber = state.TurnNumber,

                SupplyPiles = state.SupplyPiles
                    .ToDictionary(
                        pair => pair.Key,
                        pair => new SupplyPileDto
                        {
                            CardDefId = pair.Value.CardDefId,
                            Count = pair.Value.Count
                        }),

                Trash = state.Trash
                    .Select(ToCardDto)
                    .ToList(),

                Result = state.Result is null
                    ? null
                    : ToGameResultDto(state.Result),

                Status = state.Status,

                Events = state.Events
                    .Select(ToGameEventDto)
                    .ToList(),

                PendingChoice = state.PendingChoice is null
                    ? null
                    : ToPendingChoiceDto(state.PendingChoice),

                CurrentExecution = state.CurrentExecution is null
                    ? null
                    : ToExecutionContextDto(state.CurrentExecution)
            };
        }

        private GameState FromDto(
            GameStateDto dto,
            CardRegistry registry)
        {
            var state = new GameState(dto.GameId);

            var players = dto.Players
                .Select(p => FromPlayerDto(p, registry))
                .ToList();

            state.Players = players;
            state.CurrentPlayerIndex = dto.CurrentPlayerIndex;
            state.Phase = dto.Phase;
            state.TurnNumber = dto.TurnNumber;

            state.SupplyPiles = dto.SupplyPiles
                .ToDictionary(
                    pair => pair.Key,
                    pair => new SupplyPile(
                        pair.Value.CardDefId,
                        pair.Value.Count));

            state.Trash = dto.Trash
                .Select(card => FromCardDto(card, registry))
                .ToList();

            state.Result = dto.Result is null
                ? null
                : FromGameResultDto(dto.Result);

            state.Status = dto.Status;

            state.Events = dto.Events
                .Select(FromGameEventDto)
                .ToList();

            state.PendingChoice = dto.PendingChoice is null
                ? null
                : FromPendingChoiceDto(dto.PendingChoice);

            state.CurrentExecution = dto.CurrentExecution is null
                ? null
                : FromExecutionContextDto(
                    dto.CurrentExecution,
                    players,
                    registry);

            return state;
        }

        private PlayerDto ToPlayerDto(Player player)
        {
            return new PlayerDto
            {
                Id = player.Id,
                Name = player.Name,
                Actions = player.Actions,
                Buys = player.Buys,
                Coins = player.Coins,

                Deck = player.Deck
                    .Select(ToCardDto)
                    .ToList(),

                Hand = player.Hand
                    .Select(ToCardDto)
                    .ToList(),

                DiscardPile = player.DiscardPile
                    .Select(ToCardDto)
                    .ToList(),

                InPlay = player.InPlay
                    .Select(ToCardDto)
                    .ToList()
            };
        }

        private Player FromPlayerDto(
            PlayerDto dto,
            CardRegistry registry)
        {
            return new Player
            {
                Id = dto.Id,
                Name = dto.Name,
                Actions = dto.Actions,
                Buys = dto.Buys,
                Coins = dto.Coins,

                Deck = dto.Deck
                    .Select(card => FromCardDto(card, registry))
                    .ToList(),

                Hand = dto.Hand
                    .Select(card => FromCardDto(card, registry))
                    .ToList(),

                DiscardPile = dto.DiscardPile
                    .Select(card => FromCardDto(card, registry))
                    .ToList(),

                InPlay = dto.InPlay
                    .Select(card => FromCardDto(card, registry))
                    .ToList()
            };
        }

        private CardDto ToCardDto(Card card)
        {
            return new CardDto
            {
                Id = card.Id,
                DefinitionId = card.Definition.Id
            };
        }

        private Card FromCardDto(
            CardDto dto,
            CardRegistry registry)
        {
            if (!registry.Contains(dto.DefinitionId))
            {
                throw new InvalidOperationException(
                    $"Card definition '{dto.DefinitionId}' " +
                    "does not exist in the current card registry.");
            }

            return new Card(
                dto.Id,
                registry.Get(dto.DefinitionId));
        }

        private GameResultDto ToGameResultDto(GameResult result)
        {
            return new GameResultDto
            {
                Players = result.Players
                    .Select(player => new PlayerResultDto
                    {
                        PlayerId = player.PlayerId,
                        VictoryPoints = player.VictoryPoints,
                        Rank = player.Rank
                    })
                    .ToList()
            };
        }

        private GameResult FromGameResultDto(
            GameResultDto dto)
        {
            return new GameResult
            {
                Players = dto.Players
                    .Select(player => new PlayerResult
                    {
                        PlayerId = player.PlayerId,
                        VictoryPoints = player.VictoryPoints,
                        Rank = player.Rank
                    })
                    .ToList()
            };
        }

        private GameEventDto ToGameEventDto(GameEvent gameEvent)
        {
            return new GameEventDto
            {
                Id = gameEvent.Id,
                Type = gameEvent.Type,
                SequenceNumber = gameEvent.SequenceNumber,
                PlayerId = gameEvent.PlayerId,
                TargetPlayerId = gameEvent.TargetPlayerId,
                Card = gameEvent.Card
            };
        }

        private GameEvent FromGameEventDto(
            GameEventDto dto)
        {
            return new GameEvent
            {
                Id = dto.Id,
                Type = dto.Type,
                SequenceNumber = dto.SequenceNumber,
                PlayerId = dto.PlayerId,
                TargetPlayerId = dto.TargetPlayerId,
                Card = dto.Card
            };
        }

        private PendingChoiceDto ToPendingChoiceDto(
            PendingChoice choice)
        {
            return choice switch
            {
                MineTrashPendingChoice mineTrash =>
                    new PendingChoiceDto
                    {
                        Type = "mineTrash",
                        PlayerId = mineTrash.PlayerId,
                        Prompt = mineTrash.Prompt,
                        Minimum = mineTrash.Minimum,
                        Maximum = mineTrash.Maximum,
                        EligibleCardIds =
                            mineTrash.EligibleCardIds
                                .ToList()
                    },

                TrashCardsPendingChoice trash =>
                    new PendingChoiceDto
                    {
                        Type = "trashCards",
                        PlayerId = trash.PlayerId,
                        Prompt = trash.Prompt,
                        Minimum = trash.Minimum,
                        Maximum = trash.Maximum,
                        EligibleCardIds =
                            trash.EligibleCardIds
                                .ToList()
                    },

                MineGainPendingChoice mineGain =>
                    new PendingChoiceDto
                    {
                        Type = "mineGain",
                        PlayerId = mineGain.PlayerId,
                        Prompt = mineGain.Prompt,
                        Minimum = mineGain.Minimum,
                        Maximum = mineGain.Maximum,
                        MinimumCardCost = mineGain.MinimumCardCost,
                        MaximumCardCost = mineGain.MaximumCardCost,
                        Destination = mineGain.Destination,
                        EligibleDefinitionIds =
                            mineGain.EligibleDefinitionIds
                                .ToList()
                    },

                GainCardsPendingChoice gain =>
                    new PendingChoiceDto
                    {
                        Type = "gainCards",
                        PlayerId = gain.PlayerId,
                        Prompt = gain.Prompt,
                        Minimum = gain.Minimum,
                        Maximum = gain.Maximum,
                        MinimumCardCost = gain.MinimumCardCost,
                        MaximumCardCost = gain.MaximumCardCost,
                        Destination = gain.Destination,
                        EligibleDefinitionIds =
                            gain.EligibleDefinitionIds
                                .ToList()
                    },

                _ => throw new InvalidOperationException(
                    $"Unknown PendingChoice type: " +
                    $"{choice.GetType().Name}")
            };
        }

        private PendingChoice FromPendingChoiceDto(
            PendingChoiceDto dto)
        {
            return dto.Type switch
            {
                "trashCards" =>
                    new TrashCardsPendingChoice
                    {
                        PlayerId = dto.PlayerId,
                        Prompt = dto.Prompt,
                        Minimum = dto.Minimum!.Value,
                        Maximum = dto.Maximum!.Value,
                        EligibleCardIds =
                            dto.EligibleCardIds!
                    },

                "mineTrash" =>
                    new MineTrashPendingChoice
                    {
                        PlayerId = dto.PlayerId,
                        Prompt = dto.Prompt,
                        Minimum = dto.Minimum!.Value,
                        Maximum = dto.Maximum!.Value,
                        EligibleCardIds =
                            dto.EligibleCardIds!
                    },

                "gainCards" =>
                    new GainCardsPendingChoice
                    {
                        PlayerId = dto.PlayerId,
                        Prompt = dto.Prompt,
                        Minimum = dto.Minimum!.Value,
                        Maximum = dto.Maximum!.Value,
                        MinimumCardCost = dto.MinimumCardCost,
                        MaximumCardCost = dto.MaximumCardCost,
                        Destination = dto.Destination!.Value,
                        EligibleDefinitionIds =
                            dto.EligibleDefinitionIds!
                    },

                "mineGain" =>
                    new MineGainPendingChoice
                    {
                        PlayerId = dto.PlayerId,
                        Prompt = dto.Prompt,
                        Minimum = dto.Minimum!.Value,
                        Maximum = dto.Maximum!.Value,
                        MinimumCardCost = dto.MinimumCardCost,
                        MaximumCardCost = dto.MaximumCardCost,
                        Destination = dto.Destination!.Value,
                        EligibleDefinitionIds =
                            dto.EligibleDefinitionIds!
                    },

                _ => throw new InvalidOperationException(
                    $"Unknown PendingChoice type '{dto.Type}'.")
            };
        }

        private EffectExecutionContextDto ToExecutionContextDto(
            EffectExecutionContext context)
        {
            return new EffectExecutionContextDto
            {
                PlayerId = context.Player.Id,
                SourceCard = ToCardDto(context.SourceCard),

                RemainingEffects =
                    context.RemainingEffects.ToList(),

                LastTrashedCard =
                    context.LastTrashedCard is null
                        ? null
                        : ToCardDto(context.LastTrashedCard),

                RememberedCards =
                    context.RememberedCards
                        .Select(ToCardDto)
                        .ToList()
            };
        }

        private EffectExecutionContext FromExecutionContextDto(
            EffectExecutionContextDto dto,
            List<Player> players,
            CardRegistry registry)
        {
            var player = players.Single(
                p => p.Id == dto.PlayerId);

            return new EffectExecutionContext
            {
                Player = player,

                SourceCard = FromCardDto(
                    dto.SourceCard,
                    registry),

                RemainingEffects =
                    new Queue<EffectData>(
                        dto.RemainingEffects),

                LastTrashedCard =
                    dto.LastTrashedCard is null
                        ? null
                        : FromCardDto(
                            dto.LastTrashedCard,
                            registry),

                RememberedCards =
                    dto.RememberedCards
                        .Select(card =>
                            FromCardDto(card, registry))
                        .ToList()
            };
        }

        private class GameStateDto
        {
            public Guid GameId { get; init; }
            public List<PlayerDto> Players { get; init; } = new();
            public int CurrentPlayerIndex { get; init; }
            public GamePhase Phase { get; init; }
            public int TurnNumber { get; init; }
            public Dictionary<string, SupplyPileDto> SupplyPiles { get; init; } = new();
            public List<CardDto> Trash { get; init; } = new();
            public GameResultDto? Result { get; init; }
            public GameStatus Status { get; init; }
            public List<GameEventDto> Events { get; init; } = new();
            public PendingChoiceDto? PendingChoice { get; init; }
            public EffectExecutionContextDto? CurrentExecution { get; init; }
        }

        private class PlayerDto
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = null!;
            public int Actions { get; init; }
            public int Buys { get; init; }
            public int Coins { get; init; }

            public List<CardDto> Deck { get; init; } = new();
            public List<CardDto> Hand { get; init; } = new();
            public List<CardDto> DiscardPile { get; init; } = new();
            public List<CardDto> InPlay { get; init; } = new();
        }

        private class CardDto
        {
            public Guid Id { get; init; }
            public string DefinitionId { get; init; } = null!;
        }

        private class SupplyPileDto
        {
            public string CardDefId { get; init; } = null!;
            public int Count { get; init; }
        }

        private class GameResultDto
        {
            public List<PlayerResultDto> Players { get; init; } = new();
        }

        private class PlayerResultDto
        {
            public Guid PlayerId { get; init; }
            public int VictoryPoints { get; init; }
            public int Rank { get; init; }
        }

        private class GameEventDto
        {
            public Guid Id { get; init; }
            public GameEventType Type { get; init; }
            public long SequenceNumber { get; init; }
            public Guid? PlayerId { get; init; }
            public Guid? TargetPlayerId { get; init; }
            public Guid? Card { get; init; }
        }

        private class PendingChoiceDto
        {
            public string Type { get; init; } = null!;
            public Guid PlayerId { get; init; }
            public string Prompt { get; init; } = null!;

            public int? Minimum { get; init; }
            public int? Maximum { get; init; }

            public int? MinimumCardCost { get; init; }
            public int? MaximumCardCost { get; init; }

            public CardDestination? Destination { get; init; }

            public List<Guid>? EligibleCardIds { get; init; }

            public List<string>? EligibleDefinitionIds { get; init; }
        }

        private class EffectExecutionContextDto
        {
            public Guid PlayerId { get; init; }

            public CardDto SourceCard { get; init; } = null!;

            public List<EffectData> RemainingEffects { get; init; } = new();

            public CardDto? LastTrashedCard { get; init; }

            public List<CardDto> RememberedCards { get; init; } = new();
        }
    }
}



