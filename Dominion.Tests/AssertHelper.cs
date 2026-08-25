using Dominion.API.Dominion.Cards;
using Dominion.API.Dominion.Cards.Choices;
using Dominion.API.Dominion.Game;
using Dominion.API.Dominion.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion.Tests
{
    public class AssertHelper
    {
        public static void AssertGameStatesEqual(
        GameState expected,
        GameState actual)
        {
            // Basic game state
            Assert.Equal(expected.GameId, actual.GameId);
            Assert.Equal(expected.CurrentPlayerIndex, actual.CurrentPlayerIndex);
            Assert.Equal(expected.Phase, actual.Phase);
            Assert.Equal(expected.TurnNumber, actual.TurnNumber);
            Assert.Equal(expected.Status, actual.Status);

            // Players
            Assert.Equal(
                expected.Players.Count,
                actual.Players.Count);

            for (var i = 0; i < expected.Players.Count; i++)
            {
                AssertPlayersEqual(
                    expected.Players[i],
                    actual.Players[i]);
            }

            // Supply
            AssertSupplyPilesEqual(
                expected.SupplyPiles,
                actual.SupplyPiles);

            // Game Result

            if (expected.Result is null)
            {
                Assert.Null(actual.Result);
            }
            else
            {
                Assert.NotNull(actual.Result);

                AssertGameResultsEqual(
                    expected.Result,
                    actual.Result);
            }

            Assert.Equal(
                expected.Events.Count,
                actual.Events.Count);

            for (var i = 0; i < expected.Events.Count; i++)
            {
                AssertGameEventsEqual(
                    expected.Events[i],
                    actual.Events[i]);
            }


            // Pending choice
            if (expected.PendingChoice is null)
            {
                Assert.Null(actual.PendingChoice);
            }
            else
            {
                Assert.NotNull(actual.PendingChoice);

                Assert.Equal(
                    expected.PendingChoice.GetType(),
                    actual.PendingChoice.GetType());

                AssertPendingChoicesEqual(
                    expected.PendingChoice,
                    actual.PendingChoice);
            }

        }

        public static void AssertPlayersEqual(
        Player expected,
        Player actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);

            Assert.Equal(expected.Actions, actual.Actions);
            Assert.Equal(expected.Buys, actual.Buys);
            Assert.Equal(expected.Coins, actual.Coins);

            AssertCardsEqual(expected.Deck, actual.Deck);
            AssertCardsEqual(expected.Hand, actual.Hand);
            AssertCardsEqual(expected.DiscardPile, actual.DiscardPile);
            AssertCardsEqual(expected.InPlay, actual.InPlay);
        }

        public static void AssertCardsEqual(
        IReadOnlyList<Card> expected,
        IReadOnlyList<Card> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(
                    expected[i].Id,
                    actual[i].Id);

                Assert.Equal(
                    expected[i].Definition.Id,
                    actual[i].Definition.Id);

                Assert.Equal(
                    expected[i].Definition.GetType(),
                    actual[i].Definition.GetType());
            }
        }

        public static void AssertSupplyPilesEqual(
        Dictionary<string, SupplyPile> expected,
        Dictionary<string, SupplyPile> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (var (cardDefId, expectedPile) in expected)
            {
                Assert.True(
                    actual.ContainsKey(cardDefId),
                    $"Supply pile '{cardDefId}' was not found in the restored state.");

                var actualPile = actual[cardDefId];

                Assert.Equal(
                    expectedPile.CardDefId,
                    actualPile.CardDefId);

                Assert.Equal(
                    expectedPile.Count,
                    actualPile.Count);
            }
        }

        public static void AssertGameResultsEqual(
        GameResult expected,
        GameResult actual)
        {
            Assert.Equal(
                expected.Players.Count,
                actual.Players.Count);

            for (var i = 0; i < expected.Players.Count; i++)
            {
                var expectedPlayer = expected.Players[i];
                var actualPlayer = actual.Players[i];

                Assert.Equal(
                    expectedPlayer.PlayerId,
                    actualPlayer.PlayerId);

                Assert.Equal(
                    expectedPlayer.VictoryPoints,
                    actualPlayer.VictoryPoints);

                Assert.Equal(
                    expectedPlayer.Rank,
                    actualPlayer.Rank);
            }
        }
        public static void AssertGameEventsEqual(
        GameEvent expected,
        GameEvent actual)
        {
            Assert.Equal(
                expected.Id,
                actual.Id);

            Assert.Equal(
                expected.Type,
                actual.Type);

            Assert.Equal(
                expected.SequenceNumber,
                actual.SequenceNumber);

            Assert.Equal(
                expected.PlayerId,
                actual.PlayerId);

            Assert.Equal(
                expected.TargetPlayerId,
                actual.TargetPlayerId);

            Assert.Equal(
                expected.Card,
                actual.Card);
        }

        public static void AssertPendingChoicesEqual(
        PendingChoice expected,
        PendingChoice actual)
        {
            Assert.Equal(
                expected.GetType(),
                actual.GetType());

            Assert.Equal(
                expected.PlayerId,
                actual.PlayerId);

            Assert.Equal(
                expected.Prompt,
                actual.Prompt);

            // Add properties common to PendingChoice here.

            // Then add type-specific assertions for each
            // concrete PendingChoice type.
        }
    }
}
