import { z } from 'zod';

export namespace Schema {
    export namespace Domain {
        export const coordinate = z.object({
            x: z.number(),
            y: z.number(),
        });

        export const outline = coordinate.array().min(3);

        export const membraneCoordinates = coordinate.array().length(2);

        export const cellCoordinates = coordinate.array().length(4);

        export const gameState = z.enum(['setup', 'inProgress', 'finished']);

        export const gameValues = z.enum(['noOne', 'board']);

        export const player = z.object({
            userUID: z.string(),
        });
        export const currentPlayer = z.union([player, z.literal(gameValues.Enum.noOne)]);

        export const cell = z.object({
            coordinates: cellCoordinates,
            conqueredBy: z.union([player, gameValues]),
        });

        export const membrane = z.object({
            coordinates: membraneCoordinates,
            touchedBy: z.union([player, gameValues]),
        });

        export const gameConfig = z.object({
            gameId: z.string().uuid(),
            owner: player,
            outline: outline,
        });
        export const game = z.object({
            gameId: z.string().uuid(),
            gameState: gameState,
            outline: outline,
            cells: cell.array(),
            membranes: membrane.array(),
            owner: player,
            currentPlayer: z.union([player, z.literal(gameValues.Enum.noOne)]),
            players: player.array(),
        });
    }
    export namespace API {
        export const coordinate = z.object({
            x: z.number(),
            y: z.number(),
        });

        export const outline = coordinate.array().min(3);

        export const membraneCoordinates = coordinate.array().length(2);

        export const cellCoordinates = coordinate.array().length(4);

        export const gameState = z.enum(['setup', 'inProgress', 'finished']);

        export const gameValues = z.enum(['noOne', 'board']);

        export const player = z.object({
            userUID: z.string(),
        });
        export const gameActionType = z.enum(['join', 'leave', 'start']);

        export const membraneActionType = z.enum(['touch']);

        export const gameAction = z.object({
            name: gameActionType,
        });

        export const membraneAction = z.object({
            name: membraneActionType,
        });
        export const cell = z.object({
            coordinates: cellCoordinates,
            conqueredBy: z.union([player, gameValues]),
        });
        export const membrane = z.object({
            coordinates: membraneCoordinates,
            touchedBy: z.union([player, gameValues]),
            actions: membraneAction.array(),
        });

        export const game = z.object({
            gameId: z.string().uuid(),
            gameState: gameState,
            outline: outline,
            cells: cell.array(),
            membranes: membrane.array(),
            owner: player,
            currentPlayer: z.union([player, z.literal(gameValues.Enum.noOne)]),
            players: player.array(),
            actions: gameAction.array(),
        });

        export const gameForList = z.object({
            gameId: z.string().uuid(),
            gameState: gameState,
            owner: player,
            currentPlayer: z.union([player, z.literal(gameValues.Enum.noOne)]),
            players: player.array(),
            actions: gameAction.array(),
        });

        export const gameId = z.object({ gameId: z.string() });
    }
}
