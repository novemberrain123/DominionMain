import { useState } from "react";
import { useNavigate } from "react-router-dom";
import type { GameStateDto } from "../api/game";

const API_BASE_URL = "https://localhost:7268";

export default function HomePage() {
    const navigate = useNavigate();

    const [gameIdInput, setGameIdInput] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    async function createGame() {
        try {
            setError(null);
            setIsLoading(true);

            const response = await fetch(`${API_BASE_URL}/games`, {
                method: "POST",
            });

            if (!response.ok) {
                const message = await response.text();

                throw new Error(
                    message ||
                    `Failed to create game: ${response.status}`,
                );
            }

            const game: GameStateDto = await response.json();

            navigate(`/games/${game.gameId}`);
        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : "Failed to create game.",
            );
        } finally {
            setIsLoading(false);
        }
    }

    function joinGameById() {
        const gameId = gameIdInput.trim();

        if (!gameId) {
            return;
        }

        navigate(`/games/${gameId}`);
    }

    function handleJoinSubmit(
        event: React.SubmitEvent<HTMLFormElement>,
    ) {
        event.preventDefault();
        joinGameById();
    }

    return (
        <main className="flex min-h-screen items-center justify-center bg-green-950 p-6 text-white">
            <div className="w-full max-w-4xl">
                <header className="mb-10 text-center">
                    <p className="mb-2 text-sm font-semibold uppercase tracking-[0.3em] text-yellow-300">
                        A deck-building game
                    </p>

                    <h1 className="text-5xl font-bold tracking-tight sm:text-7xl">
                        Dominion
                    </h1>

                    <p className="mx-auto mt-4 max-w-xl text-white/65">
                        Create a new kingdom or join an existing game
                        using its invite code.
                    </p>
                </header>

                <div className="grid gap-6 md:grid-cols-2">
                    <section className="flex flex-col rounded-3xl border border-white/10 bg-black/20 p-7 shadow-2xl">
                        <div className="mb-6">
                            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-yellow-300 text-2xl text-black">
                                ♛
                            </div>

                            <h2 className="text-2xl font-semibold">
                                Create a game
                            </h2>

                            <p className="mt-2 text-sm text-white/60">
                                Start a new lobby and share the generated
                                invite link with another player.
                            </p>
                        </div>

                        <button
                            type="button"
                            onClick={() => void createGame()}
                            disabled={isLoading}
                            className="mt-auto rounded-xl bg-yellow-300 px-5 py-3 font-semibold text-black transition hover:bg-yellow-200 disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            {isLoading
                                ? "Creating kingdom..."
                                : "Create game"}
                        </button>
                    </section>

                    <section className="rounded-3xl border border-white/10 bg-black/20 p-7 shadow-2xl">
                        <div className="mb-6">
                            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-white/10 text-2xl">
                                ↗
                            </div>

                            <h2 className="text-2xl font-semibold">
                                Join a game
                            </h2>

                            <p className="mt-2 text-sm text-white/60">
                                Paste the game ID sent to you by the host.
                            </p>
                        </div>

                        <form
                            className="space-y-4"
                            onSubmit={handleJoinSubmit}
                        >
                            <div>
                                <label
                                    htmlFor="game-id"
                                    className="mb-2 block text-sm font-medium text-white/80"
                                >
                                    Game ID
                                </label>

                                <input
                                    id="game-id"
                                    type="text"
                                    value={gameIdInput}
                                    onChange={(event) => {
                                        setGameIdInput(
                                            event.target.value,
                                        );
                                        setError(null);
                                    }}
                                    placeholder="Paste a game ID"
                                    autoComplete="off"
                                    spellCheck={false}
                                    className="w-full rounded-xl border border-white/15 bg-black/20 px-4 py-3 text-white outline-none transition placeholder:text-white/35 focus:border-yellow-300 focus:ring-2 focus:ring-yellow-300/20"
                                />
                            </div>

                            <button
                                type="submit"
                                disabled={!gameIdInput.trim()}
                                className="w-full rounded-xl bg-white/10 px-5 py-3 font-semibold text-white transition hover:bg-white/20 disabled:cursor-not-allowed disabled:opacity-40"
                            >
                                Join game
                            </button>
                        </form>
                    </section>
                </div>

                {error && (
                    <p
                        role="alert"
                        className="mt-6 rounded-2xl border border-red-400/30 bg-red-950/50 px-5 py-4 text-center text-red-100"
                    >
                        {error}
                    </p>
                )}
            </div>
        </main>
    );
}