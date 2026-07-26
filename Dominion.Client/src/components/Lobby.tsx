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

    const inviteUrl = window.location.href;

    function handleSubmit(
        event: React.SubmitEvent<HTMLFormElement>,
    ) {
        event.preventDefault();

        if (canJoin) {
            void onJoin();
        }
    }

    async function copyInviteUrl() {
        try {
            await navigator.clipboard.writeText(inviteUrl);
        } catch {
            // The URL remains available in the input for manual copying.
        }
    }

    return (
        <main className="min-h-screen bg-green-950 p-6 text-white">
            <div className="mx-auto w-full max-w-5xl">
                <header className="mb-8 flex flex-wrap items-end justify-between gap-4">
                    <div>
                        <p className="mb-1 text-sm font-semibold uppercase tracking-[0.25em] text-yellow-300">
                            Game lobby
                        </p>

                        <h1 className="text-4xl font-bold sm:text-5xl">
                            Dominion
                        </h1>

                        <p className="mt-2 text-white/60">
                            Gather your players and prepare the kingdom.
                        </p>
                    </div>

                    <div className="rounded-2xl bg-white/10 px-4 py-3">
                        <p className="text-xs uppercase tracking-wide text-white/50">
                            Players
                        </p>

                        <p className="text-2xl font-bold">
                            {game.players.length}
                        </p>
                    </div>
                </header>

                <div className="grid gap-6 lg:grid-cols-[1.3fr_0.7fr]">
                    <div className="space-y-6">
                        <section className="rounded-3xl border border-white/10 bg-black/20 p-6 shadow-2xl">
                            <div className="mb-5">
                                <h2 className="text-2xl font-semibold">
                                    Invite players
                                </h2>

                                <p className="mt-1 text-sm text-white/60">
                                    Share this link with anyone you want
                                    to invite.
                                </p>
                            </div>

                            <label
                                htmlFor="invite-url"
                                className="mb-2 block text-sm font-medium text-white/80"
                            >
                                Invite URL
                            </label>

                            <div className="flex flex-col gap-3 sm:flex-row">
                                <input
                                    id="invite-url"
                                    type="text"
                                    value={inviteUrl}
                                    readOnly
                                    onFocus={(event) =>
                                        event.currentTarget.select()
                                    }
                                    className="min-w-0 flex-1 rounded-xl border border-white/15 bg-black/20 px-4 py-3 text-sm text-white/80 outline-none focus:border-yellow-300"
                                />

                                <button
                                    type="button"
                                    onClick={() =>
                                        void copyInviteUrl()
                                    }
                                    className="rounded-xl bg-white/10 px-5 py-3 font-semibold transition hover:bg-white/20"
                                >
                                    Copy link
                                </button>
                            </div>

                            <p className="mt-3 break-all text-xs text-white/40">
                                Game ID: {game.gameId}
                            </p>
                        </section>

                        {!isJoined ? (
                            <section className="rounded-3xl border border-white/10 bg-black/20 p-6 shadow-2xl">
                                <div className="mb-5">
                                    <h2 className="text-2xl font-semibold">
                                        Join the kingdom
                                    </h2>

                                    <p className="mt-1 text-sm text-white/60">
                                        Choose the name other players
                                        will see.
                                    </p>
                                </div>

                                <form
                                    onSubmit={handleSubmit}
                                    className="space-y-4"
                                >
                                    <div>
                                        <label
                                            htmlFor="player-name"
                                            className="mb-2 block text-sm font-medium text-white/80"
                                        >
                                            Player name
                                        </label>

                                        <input
                                            id="player-name"
                                            type="text"
                                            value={playerName}
                                            onChange={(event) =>
                                                onPlayerNameChange(
                                                    event.target.value,
                                                )
                                            }
                                            placeholder="Enter your name"
                                            maxLength={30}
                                            autoComplete="off"
                                            disabled={isSubmitting}
                                            autoFocus
                                            className="w-full rounded-xl border border-white/15 bg-black/20 px-4 py-3 text-white outline-none transition placeholder:text-white/35 focus:border-yellow-300 focus:ring-2 focus:ring-yellow-300/20 disabled:opacity-50"
                                        />
                                    </div>

                                    <button
                                        type="submit"
                                        disabled={!canJoin}
                                        className="w-full rounded-xl bg-yellow-300 px-5 py-3 font-semibold text-black transition hover:bg-yellow-200 disabled:cursor-not-allowed disabled:opacity-50"
                                    >
                                        {isSubmitting
                                            ? "Joining..."
                                            : "Join game"}
                                    </button>
                                </form>
                            </section>
                        ) : (
                            <section className="rounded-3xl border border-yellow-300/40 bg-yellow-300/10 p-6">
                                <div className="flex items-center gap-4">
                                    <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-yellow-300 font-bold text-black">
                                        ✓
                                    </div>

                                    <div>
                                        <h2 className="font-semibold text-yellow-100">
                                            You have joined
                                        </h2>

                                        <p className="text-sm text-white/60">
                                            Wait for another player, then
                                            start the game.
                                        </p>
                                    </div>
                                </div>
                            </section>
                        )}
                    </div>

                    <aside className="rounded-3xl border border-white/10 bg-black/20 p-6 shadow-2xl">
                        <div className="mb-5 flex items-center justify-between">
                            <h2 className="text-2xl font-semibold">
                                Players
                            </h2>

                            <span className="rounded-full bg-white/10 px-3 py-1 text-sm text-white/70">
                                {game.players.length}
                            </span>
                        </div>

                        {game.players.length === 0 ? (
                            <div className="rounded-2xl border border-dashed border-white/15 px-4 py-10 text-center">
                                <p className="font-medium text-white/70">
                                    The lobby is empty
                                </p>

                                <p className="mt-1 text-sm text-white/40">
                                    Join using the form or share the
                                    invite URL.
                                </p>
                            </div>
                        ) : (
                            <ul className="space-y-3">
                                {game.players.map((player, index) => (
                                    <li
                                        key={player.id}
                                        className="flex items-center gap-3 rounded-2xl bg-white/10 px-4 py-3"
                                    >
                                        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-yellow-100 font-bold text-black">
                                            {player.name
                                                .charAt(0)
                                                .toUpperCase()}
                                        </div>

                                        <div className="min-w-0">
                                            <p className="truncate font-semibold">
                                                {player.name}
                                            </p>

                                            <p className="text-xs text-white/50">
                                                Player {index + 1}
                                            </p>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        )}

                        {isJoined && (
                            <div className="mt-6">
                                <button
                                    type="button"
                                    onClick={() => void onStart()}
                                    disabled={!canStart}
                                    className="w-full rounded-xl bg-yellow-300 px-5 py-3 font-semibold text-black transition hover:bg-yellow-200 disabled:cursor-not-allowed disabled:opacity-40"
                                >
                                    {isSubmitting
                                        ? "Starting..."
                                        : "Start game"}
                                </button>

                                {game.players.length < 2 && (
                                    <p className="mt-3 text-center text-sm text-white/50">
                                        At least two players are required
                                        to start.
                                    </p>
                                )}
                            </div>
                        )}
                    </aside>
                </div>

                {error && (
                    <p
                        className="mt-6 rounded-2xl border border-red-400/30 bg-red-950/50 px-5 py-4 text-red-100"
                        role="alert"
                    >
                        {error}
                    </p>
                )}
            </div>
        </main>
    );
}