import type { GameStateDto } from "../api/game";

interface LobbyProps {
    game: GameStateDto;
    playerName: string;
    isJoined: boolean;
    isSubmitting: boolean;
    error: string | null;

    onPlayerNameChange: (name: string) => void;
    onJoin: () => Promise<void>;
    onStart: () => Promise<void>;
}

export default function Lobby({
    game,
    playerName,
    isJoined,
    isSubmitting,
    error,
    onPlayerNameChange,
    onJoin,
    onStart,
}: LobbyProps) {
    const canJoin =
        !isJoined &&
        !isSubmitting &&
        playerName.trim().length > 0;

    const canStart =
        isJoined &&
        !isSubmitting &&
        game.players.length >= 2;


    function handleSubmit(event: React.SubmitEvent<HTMLFormElement>) {
        event.preventDefault();

        if (canJoin) {
            void onJoin();
        }
    }

    return (
        <main className="lobby">
            <section className="lobby-panel">
                <header className="lobby-header">
                    <h1>Dominion</h1>

                    <p>
                        Game ID: <strong>{game.gameId}</strong>
                    </p>
                </header>

                <div>
                    <label htmlFor="invite-url">Invite URL</label>

                    <input
                        id="invite-url"
                        type="text"
                        value={window.location.href}
                        readOnly
                    />
                </div>

                {!isJoined ? (
                    <form
                        className="join-form"
                        onSubmit={handleSubmit}
                    >
                        <label htmlFor="player-name">
                            Player name
                        </label>

                        <div className="join-controls">
                            <input
                                id="player-name"
                                type="text"
                                value={playerName}
                                onChange={(event) =>
                                    onPlayerNameChange(event.target.value)
                                }
                                placeholder="Enter your name"
                                maxLength={30}
                                autoComplete="off"
                                disabled={isSubmitting}
                            />

                            <button
                                type="submit"
                                disabled={!canJoin}
                            >
                                {isSubmitting ? "Joining..." : "Join game"}
                            </button>
                        </div>
                    </form>
                ) : (
                    <p className="joined-message">
                        You have joined the game.
                    </p>
                )}

                <section className="player-list">
                    <h2>
                        Players ({game.players.length})
                    </h2>

                    {game.players.length === 0 ? (
                        <p>No players have joined yet.</p>
                    ) : (
                        <ul>
                            {game.players.map((player) => (
                                <li key={player.id}>
                                    {player.name}
                                </li>
                            ))}
                        </ul>
                    )}
                </section>

                {isJoined && (
                    <button
                        type="button"
                        className="start-button"
                        onClick={() => void onStart()}
                        disabled={!canStart}
                    >
                        {isSubmitting ? "Starting..." : "Start game"}
                    </button>
                )}

                {isJoined && game.players.length < 2 && (
                    <p className="lobby-hint">
                        At least two players are required to start.
                    </p>
                )}

                {error && (
                    <p
                        className="error-message"
                        role="alert"
                    >
                        {error}
                    </p>
                )}
            </section>
        </main>
    );
}