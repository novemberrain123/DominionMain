import { useEffect, useState } from "react";
import GameResultModal from "./GameResultModal";
import * as GameDto from "./api/game";


const API_BASE_URL = "https://localhost:7268";

export default function DominionBoard() {
    const [game, setGame] = useState<GameDto.GameStateDto | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    async function nextPhase() {
        if (!game) {
            return;
        }

        if (game.phase === "action") {
            await endActionPhase();
        } else if (game.phase === "buy") {
            await endTurn();
        }
    }

    async function buyCard(definitionId: string) {
        try {
            setError(null);

            const response = await fetch(
                `${API_BASE_URL}/debug/buy-card`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        definitionId,
                    }),
                },
            );

            if (!response.ok) {
                const message = await response.text();

                throw new Error(
                    message ||
                    `Failed to buy card: ${response.status}`,
                );
            }

            const updatedGame: GameDto.GameStateDto = await response.json();
            setGame(updatedGame);
        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : "Failed to buy card.",
            );
        }
    }

    async function endTurn() {
        try {
            setError(null);

            const response = await fetch(
                `${API_BASE_URL}/debug/end-turn`,
                {
                    method: "POST",
                },
            );

            if (!response.ok) {
                const message = await response.text();

                throw new Error(
                    message ||
                    `Failed to end turn: ${response.status}`,
                );
            }

            const updatedGame: GameDto.GameStateDto = await response.json();
            setGame(updatedGame);
        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : "Failed to end turn.",
            );
        }
    }

    async function endActionPhase() {
        try {
            setError(null);

            const response = await fetch(
                `${API_BASE_URL}/debug/end-action-phase`,
                {
                    method: "POST",
                },
            );

            if (!response.ok) {
                const message = await response.text();

                throw new Error(
                    message ||
                    `Failed to end action phase: ${response.status}`,
                );
            }

            const updatedGame: GameDto.GameStateDto = await response.json();
            setGame(updatedGame);
        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : "Failed to end action phase.",
            );
        }
    }


    async function playCard(cardInstanceId: string) {
        try {
            setError(null);

            const response = await fetch(
                `${API_BASE_URL}/debug/play-card`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        cardInstanceId,
                    }),
                },
            );

            if (!response.ok) {
                const message = await response.text();

                throw new Error(
                    message || `Failed to play card: ${response.status}`,
                );
            }

            const updatedGame: GameDto.GameStateDto = await response.json();
            setGame(updatedGame);
        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : "Failed to play card.",
            );
        }
    }

    useEffect(() => {
        async function loadGame() {
            try {
                const response = await fetch(`${API_BASE_URL}/debug/game`);

                if (!response.ok) {
                    throw new Error(
                        `Failed to load game: ${response.status}`,
                    );
                }

                const data: GameDto.GameStateDto = await response.json();
                setGame(data);
            } catch (error) {
                setError(
                    error instanceof Error
                        ? error.message
                        : "Failed to load game.",
                );
            } finally {
                setIsLoading(false);
            }
        }

        void loadGame();
    }, []);

    if (isLoading) {
        return (
            <main className="min-h-screen bg-green-950 p-6 text-white">
                Loading game...
            </main>
        );
    }


    if (!game) {
        return (
            <main className="min-h-screen bg-green-950 p-6 text-white">
                No game loaded.
            </main>
        );
    }

    return (
        <main className="min-h-screen bg-green-950 p-6 text-white">
            <div className="mx-auto flex max-w-7xl flex-col gap-8">
                <header className="flex flex-wrap items-center justify-between gap-4">
                    <div>
                        <h1 className="text-4xl font-bold tracking-tight">
                            Dominion
                        </h1>

                        <p className="mt-1 text-white/70">
                            Turn {game.turnNumber} · Phase: {game.phase}
                        </p>
                    </div>

                    <button
                        type="button"
                        onClick={nextPhase}
                        className="rounded-xl bg-yellow-300 px-4 py-2 font-semibold text-black transition hover:bg-yellow-200"
                    >
                        {game.phase === "action"
                            ? "End Action Phase"
                            : "End Turn"}
                    </button>

                    <div className="flex gap-3">
                        <Stat
                            label="Current Player"
                            value={game.currentPlayerIndex + 1}
                        />

                        <Stat
                            label="Trash"
                            value={game.trashCount}
                        />
                    </div>
                </header>

                <section>
                    <h2 className="mb-4 text-2xl font-semibold">
                        Supply
                    </h2>

                    <div className="flex flex-wrap gap-3">
                        {game.supply.map((pile) => (
                            <SupplyCard
                                key={pile.definitionId}
                                pile={pile}
                                onClick={
                                    game.phase === "buy"
                                        ? () => buyCard(pile.definitionId)
                                        : undefined
                                }
                            />
                        ))}
                    </div>
                </section>

                <section className="grid grid-cols-1 gap-6 xl:grid-cols-2">
                    {game.players.map((player) => (
                        <PlayerBoard
                            key={player.id}
                            player={player}
                            isCurrentPlayer={
                                player.id === game.currentPlayerId
                            }
                            onPlayCard={playCard}
                        />
                    ))}
                </section>
            </div>
            {error && (
                <ErrorModal
                    message={error}
                    onClose={() => setError(null)}
                />
            )}

            {game.isGameOver && game.result && (
                <GameResultModal result={game.result} players={game.players} />
            )}
        </main>
    );
}

function ErrorModal({
    message,
    onClose,
}: {
    message: string;
    onClose: () => void;
}) {
    return (
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4"
            role="dialog"
            aria-modal="true"
            aria-labelledby="error-modal-title"
        >
            <div className="w-full max-w-md rounded-2xl bg-white p-6 text-black shadow-2xl">
                <h2
                    id="error-modal-title"
                    className="text-xl font-bold"
                >
                    Invalid action
                </h2>

                <p className="mt-3 whitespace-pre-wrap text-gray-700">
                    {message}
                </p>

                <div className="mt-6 flex justify-end">
                    <button
                        type="button"
                        onClick={onClose}
                        autoFocus
                        className="rounded-xl bg-red-600 px-4 py-2 font-semibold text-white transition hover:bg-red-500"
                    >
                        Close
                    </button>
                </div>
            </div>
        </div>
    );
}
function PlayerBoard({
    player,
    isCurrentPlayer,
    onPlayCard,
}: {
    player: GameDto.PlayerDto;
    isCurrentPlayer: boolean;
    onPlayCard: (cardInstanceId: string) => void;
}) {
    return (
        <article
            className={[
                "rounded-3xl border bg-black/20 p-6 shadow-2xl",
                isCurrentPlayer
                    ? "border-yellow-300"
                    : "border-white/10",
            ].join(" ")}
        >
            <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
                <div>
                    <h2 className="text-2xl font-semibold">
                        {player.name}
                    </h2>

                    <p className="text-sm text-white/70">
                        {isCurrentPlayer
                            ? "Current player"
                            : "Waiting"}
                    </p>
                </div>

                <div className="flex gap-3">
                    <Stat label="Actions" value={player.actions} />
                    <Stat label="Buys" value={player.buys} />
                    <Stat label="Coins" value={player.coins} />
                </div>
            </div>

            <CardZone
                title="Hand"
                cards={player.hand}
                onCardClick={
                    isCurrentPlayer
                        ? onPlayCard
                        : undefined
                }
            />

            <CardZone
                title="In Play"
                cards={player.inPlay}
            />

            <CardZone
                title={`Deck (${player.deck.length})`}
                cards={player.deck}
            />

            <CardZone
                title={`Discard Pile (${player.discardPile.length})`}
                cards={player.discardPile}
            />
        </article>
    );
}
function CardZone({
    title,
    cards,
    onCardClick,
}: {
    title: string;
    cards: GameDto.CardDto[];
    onCardClick?: (cardInstanceId: string) => void;
}) {
    return (
        <section className="mb-6 last:mb-0">
            <h3 className="mb-3 text-lg font-medium">
                {title}
            </h3>

            {cards.length === 0 ? (
                <p className="text-sm text-white/50">
                    Empty
                </p>
            ) : (
                <div className="flex flex-wrap gap-3">
                    {cards.map((card) => (
                        <Card
                            key={card.instanceId}
                            card={card}
                            onClick={
                                onCardClick
                                    ? () =>
                                        onCardClick(
                                            card.instanceId,
                                        )
                                    : undefined
                            }
                        />
                    ))}
                </div>
            )}
        </section>
    );
}

function Stat({
    label,
    value,
}: {
    label: string;
    value: number;
}) {
    return (
        <div className="rounded-2xl bg-white/10 px-4 py-2 text-center">
            <div className="text-xs uppercase tracking-wide text-white/60">
                {label}
            </div>

            <div className="text-xl font-bold">
                {value}
            </div>
        </div>
    );
}
function Card({
    card,
    onClick,
}: {
    card: GameDto.CardDto;
    onClick?: () => void;
}) {
    const className = [
        "flex h-40 w-28 flex-col justify-between rounded-2xl",
        "border border-black/20 bg-yellow-100 p-3 text-black shadow-lg",
        onClick
            ? "cursor-pointer transition hover:-translate-y-1 hover:shadow-xl"
            : "",
    ].join(" ");

    const content = (
        <>
            <div className="text-xs font-semibold uppercase tracking-wide text-black/60">
                {card.types.join(" · ")}
            </div>

            <div className="text-center text-lg font-bold">
                {card.name}
            </div>

            <div className="flex items-end justify-between text-sm font-medium text-black/60">
                <span>{card.definitionId}</span>
                <span>Cost {card.cost}</span>
            </div>
        </>
    );

    if (onClick) {
        return (
            <button
                type="button"
                className={className}
                onClick={onClick}
            >
                {content}
            </button>
        );
    }

    return <div className={className}>{content}</div>;
}
function SupplyCard({
    pile,
    onClick,
}: {
    pile: GameDto.SupplyPileDto;
    onClick?: () => void;
}) {
    return (
        <div className="relative">
            <Card
                card={{
                    instanceId: pile.definitionId,
                    definitionId: pile.definitionId,
                    name: pile.name,
                    cost: pile.cost,
                    types: pile.types,
                }}
                onClick={onClick}
            />

            <div className="absolute -right-2 -top-2 rounded-full bg-black px-2 py-1 text-sm font-bold text-white">
                {pile.remaining}
            </div>
        </div>
    );
}

