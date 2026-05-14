using System.Numerics;
using Dominion.Dominion.Game;
using Dominion.Dominion.Players;

namespace Dominion.Dominion.Cards
{
    public interface ICardEffect
    {
        void Execute(GameState state, Player player);
    }
}