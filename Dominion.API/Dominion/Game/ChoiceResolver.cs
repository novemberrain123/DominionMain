using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Cards.Choices;
using Dominion.API.Dominion.Game;
using Dominion.API.Dominion.Players;
using Dominion.API.Dominion.Serialization.RequestDto;

namespace Dominion.API.Dominion.Game
{
    public sealed class ChoiceResolver
    {
        public void Resolve(
            PendingChoice choice,
            ResolveChoiceRequest request,
            GameEngine engine,
            Player player)
        {
            switch (choice)
            {
                case TrashCardsPendingChoice trashChoice:
                    ResolveTrashCards(
                        trashChoice,
                        request.SelectedCardInstanceIds,
                        engine,
                        player);
                    break;

                case GainCardsPendingChoice gainChoice:
                    ResolveGainCards(
                        gainChoice,
                        request.SelectedDefinitionIds,
                        engine,
                        player);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown pending choice: {choice.GetType().Name}");
            }
        }

        private static void ResolveTrashCards(
            TrashCardsPendingChoice choice,
            IReadOnlyCollection<Guid> selectedCardIds,
            GameEngine engine,
            Player player)
        {
            if (selectedCardIds.Count == 0)
            {
                if (choice.Minimum > 0)
                {
                    throw new InvalidOperationException(
                        "This choice cannot be skipped.");
                }

                return;
            }

            var distinctIds = selectedCardIds
                .Distinct()
                .ToList();

            if (distinctIds.Count != selectedCardIds.Count)
            {
                throw new InvalidOperationException(
                    "The same card cannot be selected more than once.");
            }

            if (distinctIds.Count < choice.Minimum ||
                distinctIds.Count > choice.Maximum)
            {
                throw new InvalidOperationException(
                    $"You must select between {choice.Minimum} " +
                    $"and {choice.Maximum} cards.");
            }

            var eligibleIds = choice.EligibleCardIds.ToHashSet();

            if (distinctIds.Any(id => !eligibleIds.Contains(id)))
            {
                throw new InvalidOperationException(
                    "One or more selected cards are not eligible.");
            }

            var selectedCards = distinctIds
                .Select(id =>
                    player.Hand.SingleOrDefault(card => card.Id == id)
                    ?? throw new InvalidOperationException(
                        "A selected card is no longer in your hand."))
                .ToList();

            foreach (var card in selectedCards)
            {
                player.Hand.Remove(card);
                engine.State.Trash.Add(card);
            }
        }

        private static void ResolveGainCards(
            GainCardsPendingChoice choice,
            IReadOnlyCollection<string> selectedDefinitionIds,
            GameEngine engine,
            Player player)
        {
            if (selectedDefinitionIds.Count == 0)
            {
                if (choice.Minimum > 0)
                {
                    throw new InvalidOperationException(
                        "This choice cannot be skipped.");
                }

                return;
            }

            if (selectedDefinitionIds.Count < choice.Minimum ||
                selectedDefinitionIds.Count > choice.Maximum)
            {
                throw new InvalidOperationException(
                    $"You must select between {choice.Minimum} " +
                    $"and {choice.Maximum} cards.");
            }

            var eligibleDefinitionIds =
                choice.EligibleDefinitionIds.ToHashSet();

            if (selectedDefinitionIds.Any(
                    id => !eligibleDefinitionIds.Contains(id)))
            {
                throw new InvalidOperationException(
                    "One or more selected cards are not eligible.");
            }

            foreach (var definitionId in selectedDefinitionIds)
            {
                CardDefinition cardDefinition = engine.Cards.Get(definitionId);
                engine.GainCard(player, cardDefinition, choice.Destination);
            }
        }
    }
}