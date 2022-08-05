import { z } from 'zod';

export namespace Schema {
    export const coordinate = z.object({
        x: z.number(),
        y: z.number(),
    });

    export const gameState = z.enum(['setup', 'inProgress', 'finished']);

    export const gameValues = z.enum(['noOne', 'board']);

    export const player = z.object({
        userUID: z.string(),
    });

    export const cell = z.object({
        coordinates: coordinate.array().length(4),
        conqueredBy: z.union([player, gameValues]),
    });

    export const membrane = z.object({
        coordinates: coordinate.array().length(2),
        touchedBy: z.union([player, gameValues]),
    });

    export const gameConfig = z.object({
        gameId: z.string().uuid(),
        owner: player,
        outline: coordinate.array().min(3),
    });
    // #region DOMAIN LAYER DATA LAYER
    export const game = z.object({
        gameId: z.string().uuid(),
        gameState,
        outline: coordinate.array().min(3),
        cells: cell.array(),
        membranes: membrane.array(),
        owner: player,
        currentPlayer: z.union([player, z.literal(gameValues.Enum.noOne)]),
        players: player.array(),
    });

    // #endregion

    export const gameActionsDto = z.enum(['join', 'leave', 'start']);

    export const membraneActionsDto = z.enum(['touch']);

    export const gameActionDto = z.object({
        name: gameActionsDto,
    });

    export const membraneActionDto = z.object({
        name: membraneActionsDto,
    });

    export const membraneDto = z.object({
        coordinates: coordinate.array().length(2),
        touchedBy: z.union([player, gameValues]),
        actions: membraneActionDto.array(),
    });

    export const gameDto = z.object({
        gameId: z.string().uuid(),
        gameState,
        outline: coordinate.array().min(3),
        cells: cell.array(),
        membranes: membraneDto.array(),
        owner: player,
        currentPlayer: z.union([player, z.literal(gameValues.Enum.noOne)]),
        players: player.array(),
        actions: gameActionDto.array(),
    });

    export const gameIdDto = z.object({ gameId: z.string() });
}
