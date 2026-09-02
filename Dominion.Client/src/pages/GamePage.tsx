import { useEffect, useState, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import GameResultModal from "../components/GameResultModal";
import * as GameDto from "../api/game";
import Lobby from "../components/Lobby";
import {
    HubConnectionBuilder,
    HubConnectionState,
    LogLevel,
} from "@microsoft/signalr";


const API_BASE_URL = "https://localhost:7268";
const SERVER_URL = "https://localhost:7268";


export default function GamePage() {
    const navigate = useNavigate();
    const { gameId } = useParams<{ gameId: string }>();

    const [game, setGame] = useState<GameDto.GameStateDto | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [playerName, setPlayerName] = useState("");
    const [playerId, setPlayerId] = useState<string | null>(() => {
        if (!gameId) {
            return null;
        }

        return localStorage.getItem(`dominion-player-${gameId}`);
    });
    const [playerToken, setPlayerToken] =
        useState<string | null>(() => {
            if (!gameId) {
                return null;
            }

            return localStorage.getItem(
                `dominion-player-token-${gameId}`,
            );
        });
    const [isSubmitting, setIsSubmitting] = useState(false);

    const pendingChoice = game?.pendingChoice ?? null;


    const isMyPendingChoice =
        pendingChoice?.playerId === playerId;

    const gainChoice =
        isMyPendingChoice &&
            pendingChoice?.type === "gainCards"
            ? pendingChoice
            : null;

    const cardChoice =
        isMyPendingChoice &&
            pendingChoice?.type === "trashCards"
            ? pendingChoice
            : null;

    const [selectedDefinitionIds, setSelectedDefinitionIds] =
        useState<string[]>([]);

    const [selectedCardIds, setSelectedCardIds] =
        useState<string[]>([]);

    const isMyTurn =
        game?.currentPlayerId === playerId;

    const canBuyCards =
        isMyTurn &&
        game.phase === "buy";

    const isJoined = playerId !== null;

    const refreshGame = useCallback(async () => {
        if (!gameId) {
            return;
        }

        try {
            const response = await fetch(
                `${API_BASE_URL}/games/${gameId}`,
                {
                    method: "GET",
                    headers: {
                        ...(playerToken && {
                            "X-Player-Token": playerToken,
                        }),
                    },
                },
            );

            if (!response.ok) {
                const message = await response.text();

                throw new Error(
                    message ||
                    `Failed to load game: ${response.status}`,
                );
            }

            const loadedGame: GameDto.GameStateDto =
                await response.json();

            setGame(loadedGame);
            setError(null);

        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : "Failed to load game.",
            );
        } finally {
            setIsLoading(false);
        }
    }, [gameId, playerToken]);

    useEffect(() => {
        if (!gameId) {
            return;
        }

        // eslint-disable-next-line react-hooks/set-state-in-effect
        void refreshGame();
    }, [gameId, refreshGame]);


    useEffect(() => {
        if (!gameId) {
            return;
        }

        const connection = new HubConnectionBuilder()
            .withUrl(`${SERVER_URL}/hubs/game`)
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        connection.on("GameUpdated", () => {
            void refreshGame();
        });

        connection.onreconnected(async () => {
            try {
                await connection.invoke(
                    "JoinGame",
                    gameId,
                );

                await refreshGame();
            } catch (error) {
                console.error(
                    "Failed to rejoin game hub:",
                    error,
                );
            }
        });

        connection.onreconnecting(error => {
            console.warn(
                "SignalR reconnecting:",
                error,
            );
        });

        connection.onclose(error => {
            if (error) {
                console.error(
                    "SignalR connection closed:",
                    error,
                );
            }
        });

        async function startConnection() {
            try {
                await connection.start();

                await connection.invoke(
                    "JoinGame",
                    gameId,
                );
            } catch (error) {
                console.error(
                    "SignalR connection failed:",
                    error,
                );
            }
        }

        void startConnection();

        return () => {
            connection.off("GameUpdated");

            if (
                connection.state !==
                HubConnectionState.Disconnected
            ) {
                void connection.stop();
            }
        };
    }, [gameId, refreshGame]);




    async function joinGame() {
        if (!gameId || !playerName.trim()) {
            return;
        }

        try {
            setError(null);
            setIsSubmitting(true);

            const response = await fetch(
                `${API_BASE_URL}/games/${gameId}/join`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        playerName: playerName.trim(),
                    }),
                },
            );

            if (!response.ok) {
                const message = await response.text();

                throw new Error(
                    message || `Failed to join game: ${response.status}`,
                );
            }

            const result: {
                gameId: string;
                playerId: string;
                playerToken: string;
            } = await response.json();

            setPlayerId(result.playerId);
            setPlayerToken(result.playerToken);

            localStorage.setItem(
                `dominion-player-${gameId}`,
                result.playerId,
            );

            localStorage.setItem(
                `dominion-player-token-${gameId}`,
                result.playerToken,
            );
        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : "Failed to join game.",
            );
        } finally {
            setIsSubmitting(false);
        }
    }

    async function resolveChoice() {
        if (!gameId || !playerToken || (!gainChoice && !cardChoice)) {
            return;
        }

        try {
            setError(null);
            setIsSubmitting(true);

            const response = await fetch(
                `${API_BASE_URL}/games/${gameId}/resolve-choice`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "X-Player-Token": playerToken,
                    },
                    body: JSON.stringify({
                        SelectedCardInstanceIds: selectedCardIds,
                        SelectedDefinitionIds: selectedDefinitionIds,
                    }),
                },
            );

            if (!response.ok) {
                const message = await response.text();

                throw new Error(
                    message ||
                    `Failed to resolve choice: ${response.status}`,
                );
            }

            const updatedGame: GameDto.GameStateDto =
                await response.json();

            setGame(updatedGame);
            setSelectedCardIds([]);
            setSelectedDefinitionIds([]);
        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : "Failed to resolve choice.",
            );
        } finally {
            setIsSubmitting(false);
        }
    }
    function toggleCardSelection(cardId: string) {
        if (!cardChoice)
            return;

        console.log("Toggling card selection:", cardId, selectedCardIds);
        setSelectedCardIds(current => {

            if (current.includes(cardId))
                return current.filter(x => x !== cardId);

            if (cardChoice.maximum === 1)
                return [cardId];

            if (current.length >= cardChoice.maximum)
                return current;

            return [...current, cardId];
        });
    }

    function toggleGainSelection(definitionId: string) {
        if (!gainChoice) {
            return;
        }

        console.log("Toggling gain selection:", definitionId, selectedDefinitionIds);

        setSelectedDefinitionIds(current => {
            if (current.includes(definitionId)) {
                return current.filter(id => id !== definitionId);
            }

            if (gainChoice.maximum === 1) {
                return [definitionId];
            }

            if (current.length >= gainChoice.maximum) {
                return current;
            }

            return [...current, definitionId];
        });
    }

    async function playAllTreasures() {
        try {
            setError(null);

            if (!playerToken) {
                throw new Error("You are not joined to this game.");
            }


            const response = await fetch(
                `${API_BASE_URL}/games/${gameId}/play-all-treasures`,
                {
                    method: "POST",
                    headers: {
                        "X-Player-Token": playerToken,
                    },
                },
            );

            if (!response.ok) {
                const message = await response.text();

                throw new Error(
                    message ||
                    `Failed to play treasures: ${response.status}`,
                );
            }

            const updatedGame: GameDto.GameStateDto =
                await response.json();

            setGame(updatedGame);
        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : "Failed to play treasures.",
            );
        }
    }

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

            if (!playerToken) {
                throw new Error("You are not joined to this game.");
            }

            const response = await fetch(
                `${API_BASE_URL}/games/${game?.gameId}/buy-card`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "X-Player-Token": playerToken,
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
                `${API_BASE_URL}/games/${game?.gameId}/end-turn`,
                {
                    method: "POST",
                    headers: {
                        ...(playerToken && {
                            "X-Player-Token": playerToken,
                        }),
                    },
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
                `${API_BASE_URL}/games/${game?.gameId}/end-action-phase`,
                {
                    method: "POST",
                    headers: {
                        ...(playerToken && {
                            "X-Player-Token": playerToken,
                        }),
                    },
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

            if (!playerToken) {
                throw new Error("You are not joined to this game.");
            }

            const response = await fetch(
                `${API_BASE_URL}/games/${game?.gameId}/play-card`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "X-Player-Token": playerToken,
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

    async function startGame() {
        if (!gameId) {
            return;
        }

        try {
            setError(null);
            setIsSubmitting(true);

            const response = await fetch(
                `${API_BASE_URL}/games/${gameId}/start`,
                {
                    method: "POST",
                    headers: {
                        ...(playerToken && {
                            "X-Player-Token": playerToken,
                        }),
                    },
                },
            );

            if (!response.ok) {
                const message = await response.text();

                throw new Error(
                    message || `Failed to start game: ${response.status}`,
                );
            }

            await refreshGame();
        } catch (error) {
            setError(
                error instanceof Error
                    ? error.message
                    : "Failed to start game.",
            );
        } finally {
            setIsSubmitting(false);
        }
    }

    if (isLoading) {
        return <main>Loading game...</main>;
    }

    if (!game) {
        return <main>{error ?? "Game not found."}</main>;
    }

    console.log("Game status:", game.status);
    if (game.status === "lobby") {
        return (
            <Lobby
                game={game}
                playerName={playerName}
                isJoined={isJoined}
                isSubmitting={isSubmitting}
                error={error}
                onPlayerNameChange={setPlayerName}
                onJoin={joinGame}
                onStart={startGame}
            />
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

                    <div className="flex flex-wrap gap-3">
                        {game.phase === "buy" && (
                            <button
                                type="button"
                                onClick={() => void playAllTreasures()}
                                disabled={
                                    game.status === "finished" ||
                                    !isMyTurn ||
                                    game.pendingChoice !== null
                                }
                                className="rounded-xl bg-amber-600 px-4 py-2 font-semibold text-white transition hover:bg-amber-500 disabled:cursor-not-allowed disabled:opacity-50"
                            >
                                Play All Treasures
                            </button>
                        )}

                        <button
                            type="button"
                            onClick={() => void nextPhase()}
                            disabled={
                                game.status === "finished" ||
                                !isMyTurn ||
                                game.pendingChoice !== null
                            }
                            className="rounded-xl bg-yellow-300 px-4 py-2 font-semibold text-black transition hover:bg-yellow-200 disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            {game.phase === "action"
                                ? "End Action Phase"
                                : "End Turn"}
                        </button>
                    </div>

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
                {(gainChoice || cardChoice) && (
                    <section className="rounded-2xl border border-yellow-300 bg-yellow-300/10 p-4">
                        <h2 className="text-xl font-semibold">
                            {pendingChoice?.prompt}
                        </h2>

                        <p className="mt-2 text-sm text-white/60">
                            Select{" "}
                            {pendingChoice?.minimum === pendingChoice?.maximum
                                ? pendingChoice?.minimum
                                : `${pendingChoice?.minimum}–${pendingChoice?.maximum}`}{" "}
                            {pendingChoice?.maximum === 1 ? "card" : "cards"}.
                        </p>

                        <div className="mt-4 flex gap-3">
                            <button
                                type="button"
                                onClick={() => void resolveChoice()}
                                disabled={
                                    isSubmitting ||
                                    (cardChoice
                                        ? selectedCardIds.length < cardChoice.minimum
                                        : selectedDefinitionIds.length <
                                        (gainChoice?.minimum ?? 0))
                                }
                                className="rounded-xl bg-yellow-300 px-4 py-2 font-semibold text-black disabled:cursor-not-allowed disabled:opacity-50"
                            >
                                Confirm
                            </button>

                            {pendingChoice?.minimum === 0 && (
                                <button
                                    type="button"
                                    onClick={() => void resolveChoice()}
                                    disabled={isSubmitting}
                                    className="rounded-xl bg-white/10 px-4 py-2 font-semibold text-white disabled:cursor-not-allowed disabled:opacity-50"
                                >
                                    Skip
                                </button>
                            )}
                        </div>
                    </section>
                )}
                <section>
                    <h2 className="mb-4 text-2xl font-semibold">
                        Supply
                    </h2>

                    <div className="flex flex-wrap gap-3">
                        {game.supply.map((pile) => {
                            const isEligibleGain =
                                gainChoice?.eligibleDefinitionIds?.includes(
                                    pile.definitionId,
                                ) ?? false;

                            const isSelectedGain =
                                selectedDefinitionIds.includes(
                                    pile.definitionId,
                                );

                            let onClick: (() => void) | undefined;

                            if (gainChoice && isEligibleGain) {
                                onClick = () =>
                                    toggleGainSelection(pile.definitionId);
                            }
                            else if (!pendingChoice && canBuyCards) {
                                onClick = () =>
                                    void buyCard(pile.definitionId);
                            }

                            return (
                                <SupplyCard
                                    key={pile.definitionId}
                                    pile={pile}
                                    onClick={onClick}
                                    isEligible={isEligibleGain}
                                    isSelected={isSelectedGain}
                                    isChoiceActive={gainChoice !== null}
                                />
                            );
                        })}
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
                            isViewingPlayer={
                                player.id === playerId
                            }
                            onPlayCard={
                                game.pendingChoice === null
                                    ? playCard
                                    : undefined
                            }
                            onSelectCard={
                                cardChoice
                                    ? toggleCardSelection
                                    : undefined
                            }
                            selectedCardIds={selectedCardIds}
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

            {game.status == "finished" && game.result && (
                <GameResultModal
                    result={game.result}
                    players={game.players}
                    onNewGame={() => navigate("/")}
                />
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
    isViewingPlayer,
    onPlayCard,
    onSelectCard,
    selectedCardIds,
}: {
    player: GameDto.PlayerDto;
    isCurrentPlayer: boolean;
    isViewingPlayer: boolean;
    onPlayCard?: (cardInstanceId: string) => void;
    onSelectCard?: (cardInstanceId: string) => void;
    selectedCardIds?: string[];
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
                        {isViewingPlayer ? " (You)" : ""}
                    </h2>

                    <p className="text-sm text-white/70">
                        {isCurrentPlayer
                            ? "Current player"
                            : "Waiting"}
                    </p>
                </div>

                <div className="flex gap-3">
                    <Stat
                        label="Actions"
                        value={player.actions}
                    />
                    <Stat
                        label="Buys"
                        value={player.buys}
                    />
                    <Stat
                        label="Coins"
                        value={player.coins}
                    />
                </div>
            </div>

            <CardZone
                title={`Hand (${player.handCount})`}
                cards={player.hand}
                onCardClick={
                    onSelectCard ??
                    (isCurrentPlayer ? onPlayCard : undefined)
                }
                selectedCardIds={selectedCardIds ?? []}
            />

            <CardZone
                title="In Play"
                cards={player.inPlay}
            />

            <CardZone
                title={`Deck (${player.deckCount})`}
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
    selectedCardIds,
}: {
    title: string;
    cards: GameDto.CardDto[] | null;
    onCardClick?: (cardInstanceId: string) => void;
    selectedCardIds?: string[];
}) {
    if (cards === null) {
        return (
            <section className="mb-6 last:mb-0">
                <h3 className="mb-3 text-lg font-medium">
                    {title}
                </h3>

                <p className="text-sm text-white/50 italic">
                    Hidden
                </p>
            </section>
        );
    }

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
                            isSelected={
                                selectedCardIds !== undefined &&
                                selectedCardIds.includes(card.instanceId)
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
    isSelected,
}: {
    card: GameDto.CardDto;
    onClick?: () => void;
    isSelected: boolean;
}) {
    const className = [
        "flex h-40 w-28 flex-col rounded-2xl",
        "border border-black/20 bg-yellow-100 p-3 text-black shadow-lg",
        onClick
            ? "cursor-pointer transition hover:-translate-y-1 hover:shadow-xl"
            : "",
        isSelected
            ? "ring-4 ring-yellow-300"
            : ""
    ].join(" ");

    const content = (
        <>
            <div className="text-xs font-semibold uppercase tracking-wide text-black/60">
                {card.types.join(" · ")}
            </div>

            <div className="mt-1 text-center text-lg font-bold">
                {card.name}
            </div>

            <div className="my-auto flex flex-col gap-1 text-center text-xs">
                {card.effects.map((text, index) => (
                    <div key={`${text}-${index}`}>
                        {text}
                    </div>
                ))}
            </div>

            <div className="flex items-end justify-between text-xs font-medium text-black/60">
                {/*<span>{card.definitionId}</span>*/}
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
    isEligible = false,
    isSelected = false,
    isChoiceActive = false,
}: {
    pile: GameDto.SupplyPileDto;
    onClick?: () => void;
    isEligible?: boolean;
    isSelected?: boolean;
    isChoiceActive?: boolean;
}) {
    return (
        <div
            className={[
                "relative rounded-2xl",
                isSelected
                    ? "ring-4 ring-yellow-300"
                    : "",
                isChoiceActive && !isEligible
                    ? "opacity-40"
                    : "",
            ].join(" ")}
        >
            <Card
                card={{
                    instanceId: pile.definitionId,
                    definitionId: pile.definitionId,
                    name: pile.name,
                    cost: pile.cost,
                    types: pile.types,
                    effects: pile.effects,
                }}
                onClick={onClick}
                isSelected={false}
            />

            <div className="absolute -right-2 -top-2 rounded-full bg-black px-2 py-1 text-sm font-bold text-white">
                {pile.remaining}
            </div>
        </div>
    );
}

