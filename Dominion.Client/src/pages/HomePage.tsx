import { useState } from "react";
import { useNavigate } from "react-router-dom";
import type { GameStateDto } from "../api/game";

const API_BASE_URL = "https://localhost:7268/debug";

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
                    message || `Failed to create game: ${response.status}`,
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

    return (
        <main>
            <h1>Dominion</h1>

            <button
                type="button"
                onClick={() => void createGame()}
                disabled={isLoading}
            >
                {isLoading ? "Creating..." : "Create game"}
            </button>

            <hr />

            <label htmlFor="game-id">Game ID</label>

            <input
                id="game-id"
                value={gameIdInput}
                onChange={(event) =>
                    setGameIdInput(event.target.value)
                }
                placeholder="Paste a game ID"
            />

            <button
                type="button"
                onClick={joinGameById}
                disabled={!gameIdInput.trim()}
            >
                Join game
            </button>

            {error && <p role="alert">{error}</p>}
        </main>
    );
}