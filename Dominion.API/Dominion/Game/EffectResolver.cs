using Dominion.Dominion.Players;
using Dominion.Dominion.Serialization.EffectDatas;
using Dominion.Dominion.Serialization;

namespace Dominion.Dominion.Game
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

                default:
                    throw new InvalidOperationException($"Unknown effect: {effect.GetType().Name}");
            }
        }

    }
}
