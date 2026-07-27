namespace Dominion.API.Dominion.Game
{
    public enum GameEventType
    {
        GameStarted,
        PlayerTurnStarted,
        PlayerTurnEnded,
        CardPlayed,
        CardBought,
        CardGained,
        CardTrashed,
        PhaseChanged,
        GameOver,
    }
}