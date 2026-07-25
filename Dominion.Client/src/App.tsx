
import { useEffect, useState } from "react";
type PlayerDto = {
    id: string;
    name: string;

    actions: number;
    buys: number;
    coins: number;

    hand: CardDto[];

    deckCount: number;
    deck: string[];

    discardPile: string[];

    inPlay: string[];
};

type CardDto = {
    id: string;
    types: string[];
    cost: number;
};
export default function DominionBoard() {
    const [players, setPlayers] = useState<PlayerDto[]>([]);
    useEffect(() => {
        fetch("https://localhost:7268/debug/players")
            .then((res) => res.json())
            .then((data) => setPlayers(data));
    }, []);


    return (
        <div className="min-h-screen bg-green-900 p-6 text-white">
            <div className="mx-auto flex max-w-7xl flex-col gap-6">
                <h1 className="text-4xl font-bold tracking-tight">Dominion</h1>

                <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
                    {players.map((player) => (
                        <div
                            key={player.id}
                            className="rounded-3xl border border-white/10 bg-black/20 p-6 shadow-2xl backdrop-blur"
                        >
                            <div className="mb-4 flex items-center justify-between">
                                <div>
                                    <h2 className="text-2xl font-semibold">{player.name}</h2>
                                    <p className="text-sm text-white/70">Local Player</p>
                                </div>

                                <div className="flex gap-3 text-sm font-medium">
                                    <Stat label="Actions" value={player.actions} />
                                    <Stat label="Buys" value={player.buys} />
                                    <Stat label="Coins" value={player.coins} />
                                </div>
                            </div>

                            <div>
                                <h3 className="mb-3 text-lg font-medium">Hand</h3>

                                <div className="flex flex-wrap gap-3">
                                    {player.hand.map((card, index) => (
                                        <Card key={`${card}-${index}`} card={card} />
                                    ))}
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
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
            <div className="text-xl font-bold">{value}</div>
        </div>
    );
}

function Card({ card }: { card: CardDto }) {
    return (
        <div className="flex h-40 w-28 flex-col justify-between rounded-2xl border border-white/10 bg-yellow-100 p-3 text-black shadow-lg transition-transform hover:-translate-y-1">
            <div className="text-sm font-semibold uppercase tracking-wide text-black/60">
                {card.types.join(" \u2022 ")}
            </div>

            <div className="text-center text-lg font-bold capitalize">{card.id}</div>

            <div className="text-right text-sm font-medium text-black/60">Cost {card.cost}</div>
        </div>
    );
}
