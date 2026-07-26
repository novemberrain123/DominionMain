import type {
    GameResultDto,
    PlayerDto,
} from "./api/game";

interface GameResultModalProps {
    result: GameResultDto;
    players: PlayerDto[];
}

export default function GameResultModal({
    result,
    players,
}: GameResultModalProps) {
    const playersById = new Map(
        players.map(player => [player.id, player]),
    );

    const winners = result.playerResults
        .filter(player => player.rank === 1)
        .map(player => playersById.get(player.playerId))
        .filter(
            (player): player is PlayerDto =>
                player !== undefined,
        );

    const title =
        winners.length === 1
            ? `${winners[0].name} won!`
            : `${winners
                .map(player => player.name)
                .join(", ")} tied!`;

    const sortedResults = [...result.playerResults].sort(
        (a, b) => a.rank - b.rank,
    );

    return (
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4"
            role="dialog"
            aria-modal="true"
            aria-labelledby="game-result-title"
        >
            <div className="w-full max-w-lg rounded-2xl bg-white p-6 text-black shadow-2xl">
                <h2
                    id="game-result-title"
                    className="text-2xl font-bold"
                >
                    {title}
                </h2>

                <table className="mt-6 w-full border-collapse">
                    <thead>
                        <tr className="border-b">
                            <th className="pb-2 text-left">
                                Rank
                            </th>
                            <th className="pb-2 text-left">
                                Player
                            </th>
                            <th className="pb-2 text-right">
                                VP
                            </th>
                        </tr>
                    </thead>

                    <tbody>
                        {sortedResults.map(resultPlayer => {
                            const player = playersById.get(
                                resultPlayer.playerId,
                            );

                            return (
                                <tr
                                    key={resultPlayer.playerId}
                                    className="border-b last:border-0"
                                >
                                    <td className="py-2">
                                        {resultPlayer.rank}
                                    </td>

                                    <td className="py-2">
                                        {player?.name ??
                                            "Unknown"}
                                    </td>

                                    <td className="py-2 text-right">
                                        {
                                            resultPlayer.victoryPoints
                                        }
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>

                <div className="mt-6 flex justify-end">
                    <button
                        type="button"
                        onClick={() =>
                            window.location.reload()
                        }
                        className="rounded-xl bg-green-700 px-4 py-2 font-semibold text-white transition hover:bg-green-600"
                    >
                        New Game
                    </button>
                </div>
            </div>
        </div>
    );
}