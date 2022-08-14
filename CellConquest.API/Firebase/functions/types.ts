import { z } from 'zod';
import { Schema } from './schemas';

export namespace Domain {
    export type GameConfig = z.infer<typeof Schema.Domain.gameConfig>;
    export type Game = z.infer<typeof Schema.Domain.game>;
    export type Player = z.infer<typeof Schema.Domain.player>;
    export type GameState = z.infer<typeof Schema.Domain.gameState>;
    export type CurrentPlayer = z.infer<typeof Schema.Domain.currentPlayer>;
    export type Membrane = z.infer<typeof Schema.Domain.membrane>;
    export type Coordinate = z.infer<typeof Schema.Domain.coordinate>;
    export type Cell = z.infer<typeof Schema.Domain.cell>;
}
export namespace API {
    export type Game = z.infer<typeof Schema.API.game>;
    export type GameForList = z.infer<typeof Schema.API.gameForList>;
    export type GameActionType = z.infer<typeof Schema.API.gameActionType>;
    export type GameAction = z.infer<typeof Schema.API.gameAction>;
    export type Membrane = z.infer<typeof Schema.API.membrane>;
    export type MembraneActionType = z.infer<typeof Schema.API.membraneActionType>;
    export type MembraneAction = z.infer<typeof Schema.API.membraneAction>;
}
