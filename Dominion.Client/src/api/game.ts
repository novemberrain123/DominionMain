
type CardType = "action" | "treasure" | "victory" | "attack" | "reaction";
type GameStatus = "lobby" | "playing" | "finished";

export interface CardDto  {
    instanceId: string;
    definitionId: string;
    name: string;
    cost: number;
    types: CardType[];
    effects: string[];
};

export interface SupplyPileDto  {
    definitionId: string;
    name: string;
    cost: number;
    types: CardType[];
    remaining: number;
    effects: string[];
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
    handCount: number;
    deckCount: number;
};

export interface PendingChoiceDto {
    playerId: string;
    prompt: string;
    minimum: number;
    maximum: number;
}

export interface GainCardsPendingChoiceDto
    extends PendingChoiceDto {
    type: "gainCards";
    eligibleDefinitionIds: string[];
    minimumCardCost: number | null;
    maximumCardCost: number | null;
    destination: string;
}

export interface TrashCardsPendingChoiceDto
    extends PendingChoiceDto {
    type: "trashCards";
    eligibleCardIds: string[];
}

export type AnyPendingChoiceDto =
    | GainCardsPendingChoiceDto
    | TrashCardsPendingChoiceDto;

export interface GameStateDto  {
    gameId: string;
    turnNumber: number;
    phase: string;
    currentPlayerIndex: number;
    currentPlayerId: string;
    status: GameStatus;
    players: PlayerDto[];
    supply: SupplyPileDto[];
    trashCount: number;
    result?: GameResultDto;
    pendingChoice: AnyPendingChoiceDto | null;
};

export interface PlayerResultDto {
    playerId: string;
    victoryPoints: number;
    rank: number;
}

export interface GameResultDto {
    playerResults: PlayerResultDto[];
}

export interface JoinGameResponse {
    playerId: string;
}
