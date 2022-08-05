import { z } from 'zod';
import { Schema } from './schemas';

export namespace Domain {
    export type Coordinate = z.infer<typeof Schema.coordinate>;

    export type GameState = z.infer<typeof Schema.gameState>;

    export type GameValues = z.infer<typeof Schema.gameValues>;

    export type GameActions = z.infer<typeof Schema.gameActionsDto>;

    export type MembraneActions = z.infer<typeof Schema.membraneActionsDto>;

    export type Player = z.infer<typeof Schema.player>;

    export type Cell = z.infer<typeof Schema.cell>;

    export type Membrane = z.infer<typeof Schema.membrane>;

    export type GameConfig = z.infer<typeof Schema.gameConfig>;

    export type Game = z.infer<typeof Schema.game>;
}

export namespace DTO {
    export type Game = z.infer<typeof Schema.gameDto>;
    export type GameAction = z.infer<typeof Schema.gameActionDto>;
    export type Membrane = z.infer<typeof Schema.membraneDto>;
    export type MembraneAction = z.infer<typeof Schema.membraneActionDto>;
}
