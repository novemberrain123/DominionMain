using Dominion.API.Dominion.Serialization;
using Dominion.API.Dominion.Players;
using Dominion.API.Dominion.Cards.Primitives.PrimitiveEffectDatas;

namespace Dominion.API.Dominion.Game
{
    public class EffectResolver
    {
        public void Apply(EffectData effect, GameEngine engine, Player player)
        {
            switch (effect)
            {
                case DrawCardsEffectData d:
                    player.Draw(d.Amount);
                    break;

                case GainActionsEffectData d:
                    player.Actions += d.Amount;
                    break;

                case GainCoinsEffectData d:
                    player.Coins += d.Amount;
                    break;

                case GainBuysEffectData d:
                    player.Buys += d.Amount;
                    break;

                case TrashCardsEffectData trash:
                    engine.CreatePendingChoice(CreateTrashChoice(trash, player));
                    break;

                case GainCardsEffectData gain:
                    engine.CreatePendingChoice(CreateGainChoice(gain, engine, player));
                    break;

                default:
                    throw new InvalidOperationException($"Unknown effect: {effect.GetType().Name}");
            }
        }

        private static TrashCardsPendingChoice CreateTrashChoice(
            TrashCardsEffectData effect,
            Player player)
        {
            if (effect.Minimum < 0)
            {
                throw new InvalidOperationException(
                    "Minimum trash count cannot be negative.");
            }

            if (effect.Maximum < effect.Minimum)
            {
                throw new InvalidOperationException(
                    "Maximum trash count cannot be less than minimum.");
            }

            var eligibleCardIds = player.Hand
                .Where(card =>
                    effect.RequiredType is null ||
                    card.Definition.Types.Contains(effect.RequiredType.Value))
                .Select(card => card.Id)
                .ToList();

            var maximum = Math.Min(
                effect.Maximum,
                eligibleCardIds.Count);

            var minimum = Math.Min(
                effect.Minimum,
                maximum);

            return new TrashCardsPendingChoice
            {
                PlayerId = player.Id,
                Minimum = minimum,
                Maximum = maximum,
                EligibleCardIds = eligibleCardIds,
                Prompt = effect.RequiredType is not null
                    ? $"Trash {minimum} to {maximum} {effect.RequiredType} cards."
                    : $"Trash {minimum} to {maximum} cards."
            };
        }

        private static GainCardsPendingChoice CreateGainChoice(
            GainCardsEffectData effect,
            GameEngine engine,
            Player player)
        {
            var eligibleDefinitionIds = engine.State.SupplyPiles
                .Where(pair => pair.Value.Count > 0)
                .Select(pair => new
                {
                    DefinitionId = pair.Key,
                    Definition = engine.Cards.Get(pair.Value.CardDefId)
                })
                .Where(item =>
                    effect.MaximumCardCost is null ||
                    item.Definition.Cost <= effect.MaximumCardCost)
                .Where(item =>
                    effect.MinimumCardCost is null ||
                    item.Definition.Cost >= effect.MinimumCardCost)
                .Where(item =>
                    effect.AllowedTypes is null ||
                    effect.AllowedTypes.Any(
                        type => item.Definition.Types.Contains(type)))
                .Where(item =>
                    effect.ExcludedTypes is null ||
                    !effect.ExcludedTypes.Any(
                        type => item.Definition.Types.Contains(type)))
                .Select(item => item.DefinitionId)
                .ToList();

            return new GainCardsPendingChoice
            {
                PlayerId = player.Id,
                Minimum = effect.Minimum,
                Maximum = effect.Maximum,
                EligibleDefinitionIds = eligibleDefinitionIds,
                Destination = effect.Destination,
                Prompt = $"Gain {effect.Minimum} to {effect.Maximum} cards."
            };
        }

    }
}
