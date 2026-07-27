using System.Numerics;
using Dominion.API.Dominion.Game;
using Dominion.API.Dominion.Players;

namespace Dominion.API.Dominion.Cards
{
    public interface ICardEffect
    {
        void Execute(GameState state, Player player);
    }
}