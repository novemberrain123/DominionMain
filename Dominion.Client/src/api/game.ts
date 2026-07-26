
type CardType = "action" | "treasure" | "victory" | "attack" | "reaction";

export interface CardDto  {
    instanceId: string;
    definitionId: string;
    name: string;
    cost: number;
    types: CardType[];
};

export interface SupplyPileDto  {
    definitionId: string;
    name: string;
    cost: number;
    types: CardType[];
    remaining: number;
};

export interface PlayerDto  {
    id: string;
    name: string;
    actions: number;
    buys: number;
    coins: number;
    hand: CardDto[];
    deck: CardDto[];
    discardPile: CardDto[];
    inPlay: CardDto[];
};

export interface GameStateDto  {
    turnNumber: number;
    phase: string;
    currentPlayerIndex: number;
    currentPlayerId: string;
    isGameOver: boolean;
    players: PlayerDto[];
    supply: SupplyPileDto[];
    trashCount: number;
    result?: GameResultDto;
};

export interface PlayerResultDto {
    playerId: string;
    victoryPoints: number;
    rank: number;
}

export interface GameResultDto {
    playerResults: PlayerResultDto[];
}
