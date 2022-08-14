import { TRPCError } from '@trpc/server';
import * as admin from 'firebase-admin';
import { isEqual, some } from 'lodash';
import { z } from 'zod';
import { createCellsAndMembranesForASquareGrid } from '../../board';
import { Schema } from '../../schemas';
import { Domain, API } from '../../types';
import { trpcInstance } from '../trpcInstance';

const authenticatedProcedure = trpcInstance.procedure.use(
    trpcInstance.middleware(({ next, ctx }) => {
        if (!ctx.user) {
            throw new TRPCError({
                code: 'UNAUTHORIZED',
                message: 'You are not authenticated',
            });
        }
        return next({
            ctx: {
                user: { ...ctx.user },
            },
        });
    })
);
const isPlayerInPlayerList = (
    players: {
        userUID: string;
    }[],
    user: Domain.Player
) => players.findIndex(p => p.userUID === user.userUID) !== -1;

const isPlayerOwner = (owner: Domain.Player, user: Domain.Player) => owner.userUID === user.userUID;

const areThereSufficientPlayersInTheGame = (
    players: {
        userUID: string;
    }[]
) => players.length > 1;

const createRulesEvaluator = (
    rules: {
        predicate: boolean;
        error: string;
    }[]
) => {
    const errors = rules.filter(rule => rule.predicate === false).map(rule => rule.error);
    return { isSuccessFul: errors.length === 0, errors };
};

const canUserJoinGameRule = (gameState: Domain.GameState, players: Domain.Player[], user: Domain.Player) =>
    createRulesEvaluator([
        { predicate: isPlayerInPlayerList(players, user) === false, error: 'User already exits in game' },
        { predicate: gameState === 'setup', error: 'Game is not in setup state' },
    ]);

const canUserLeaveGameRule = (gameState: Domain.GameState, players: Domain.Player[], owner: Domain.Player, user: Domain.Player) =>
    createRulesEvaluator([
        { predicate: isPlayerOwner(owner, user) === false, error: "Owner can't leave the game" },
        { predicate: isPlayerInPlayerList(players, user), error: "User doesn't exist in game" },
        { predicate: gameState === 'setup', error: 'Game is not in setup state' },
    ]);

const canUserStartGameRule = (gameState: Domain.GameState, players: Domain.Player[], owner: Domain.Player, user: Domain.Player) =>
    createRulesEvaluator([
        { predicate: isPlayerOwner(owner, user), error: 'User is not the owner of the game' },
        { predicate: areThereSufficientPlayersInTheGame(players), error: 'Insufficient players in the game' },
        { predicate: gameState === 'setup', error: 'Game is not in setup state' },
    ]);
const canUserTouchGameRule = (gameState: Domain.GameState, currentPlayer: Domain.CurrentPlayer, membrane: Domain.Membrane, user: Domain.Player) =>
    createRulesEvaluator([
        { predicate: currentPlayer !== 'noOne' && currentPlayer.userUID === user.userUID, error: 'User is not the current player' },
        { predicate: gameState === 'inProgress', error: 'Game is not in progress state' },
        { predicate: membrane.touchedBy === 'noOne', error: 'Membrane is already touched' },
    ]);

const addGameActionWhen = (condition: boolean, gameActionType: API.GameActionType): API.GameAction[] => {
    // TODO Refactor
    return condition ? [{ name: gameActionType }] : [];
};
const addMembraneActionWhen = (condition: boolean, membraneActionType: API.MembraneActionType): API.MembraneAction[] => {
    // TODO Refactor
    return condition ? [{ name: membraneActionType }] : [];
};

const membraneRoute = trpcInstance.router({
    touch: authenticatedProcedure
        .input(
            Schema.API.gameId.merge(
                z.object({
                    coordinates: Schema.API.membraneCoordinates,
                })
            )
        )
        .mutation(async ({ input, ctx }) => {
            ctx.res.statusCode = 200;
            const gameDoc: admin.firestore.DocumentSnapshot<admin.firestore.DocumentData> = await admin
                .firestore()
                .doc(`games/${input.gameId}`)
                .get();
            if (!gameDoc.exists) {
                throw new TRPCError({
                    code: 'NOT_FOUND',
                    message: 'Game not found',
                });
            }
            const game = Schema.Domain.game.parse(gameDoc.data());
            const membraneIndex = game.membranes.findIndex(m => isEqual(m.coordinates, input.coordinates));

            if (membraneIndex === -1) {
                throw new TRPCError({
                    code: 'NOT_FOUND',
                    message: 'Membrane not found',
                });
            }
            const { isSuccessFul, errors } = canUserTouchGameRule(game.gameState, game.currentPlayer, game.membranes[membraneIndex], ctx.user);
            if (!isSuccessFul) {
                throw new TRPCError({
                    code: 'BAD_REQUEST',
                    message: errors.join('\n'),
                });
            }

            const currentPlayerChangedToNoOneOnGame: Domain.Game = { ...game, currentPlayer: 'noOne' };
            await gameDoc.ref.set(currentPlayerChangedToNoOneOnGame); // This prevents the user from making multiple requests at the same time

            const membraneTouchedOnGame: Domain.Game = {
                ...game,
                membranes: game.membranes.map(
                    (
                        membrane // TODO Find a better solution. this seems clunky
                    ) =>
                        isEqual(membrane.coordinates, input.coordinates)
                            ? {
                                  ...membrane,
                                  touchedBy: {
                                      userUID: ctx.user.userUID,
                                  },
                              }
                            : membrane
                ), // => membraneWasTouched
            };

            const conquerableCells = membraneTouchedOnGame.cells.filter(
                cell => {
                    const doesCellContainInputCoordinates = input.coordinates.every(coordinate => some(cell.coordinates, coordinate));
                    const isCellNotConquered = cell.conqueredBy === 'noOne';
                    const areAllMembranesWithSameCoordinatesAsCellTouched = cell.coordinates
                        .map((coordinate, index) => [coordinate, cell.coordinates[(index + 1) % cell.coordinates.length]])
                        .every(x => membraneTouchedOnGame.membranes.find(m => isEqual(m.coordinates, x))?.touchedBy !== 'noOne'); // Check if all membranes are touching the cell
                    return doesCellContainInputCoordinates && isCellNotConquered && areAllMembranesWithSameCoordinatesAsCellTouched;
                }
                // check if all membranes with same coordinates as cell are touched
            );

            if (conquerableCells.length <= 0) {
                // Change player turn if no cells are conquerable
                const nextPlayerTurn =
                    membraneTouchedOnGame.players[
                        (membraneTouchedOnGame.players.findIndex(p => p.userUID === ctx.user.userUID) + 1) % membraneTouchedOnGame.players.length
                    ];
                const currentPlayerChangedOnGame: Domain.Game = {
                    ...membraneTouchedOnGame,
                    currentPlayer: nextPlayerTurn,
                };

                return await gameDoc.ref.set(currentPlayerChangedOnGame); // => currentPlayerChanged
            }

            const cellsConqueredOnGame: Domain.Game = {
                ...membraneTouchedOnGame,
                cells: membraneTouchedOnGame.cells.map(cell =>
                    conquerableCells.some(conquerableCell => isEqual(cell.coordinates, conquerableCell.coordinates))
                        ? {
                              ...cell,
                              conqueredBy: {
                                  userUID: ctx.user.userUID,
                              },
                          }
                        : cell
                ),
            };

            const areAllCellsConquered = cellsConqueredOnGame.cells.every(cell => cell.conqueredBy !== 'noOne');
            const areAllMembranesTouched = cellsConqueredOnGame.membranes.every(membrane => membrane.touchedBy !== 'noOne');
            if (areAllCellsConquered != areAllMembranesTouched) {
                // This should never happen
                throw new TRPCError({
                    code: 'INTERNAL_SERVER_ERROR',
                    message: 'All cells should be conquered if all membranes are touched and vice versa',
                });
            }
            if (areAllCellsConquered && areAllMembranesTouched) {
                const gameFinished: Domain.Game = {
                    ...cellsConqueredOnGame,
                    gameState: 'finished',
                };

                return await gameDoc.ref.set(gameFinished); // => cells were conquered and game finished
            }
            const currentPlayerChangedToUserOnGame: Domain.Game = {
                ...cellsConqueredOnGame,
                currentPlayer: {
                    userUID: ctx.user.userUID,
                },
            };
            return await gameDoc.ref.set(currentPlayerChangedToUserOnGame); // => cells were conquered and current player changed
        }),
});

export const gameRouter = trpcInstance.router({
    list: trpcInstance.procedure.output(Schema.API.gameForList.array()).query(async ({ ctx }) => {
        const gameCollection: admin.firestore.QuerySnapshot<admin.firestore.DocumentData> = await admin.firestore().collection('games').get();
        const games = await Schema.Domain.game.array().parseAsync(gameCollection.docs.map(doc => doc.data()));
        const gamesWithActions: API.GameForList[] = games.map(game => {
            const gameActions = ctx.user
                ? [
                      ...addGameActionWhen(canUserStartGameRule(game.gameState, game.players, game.owner, ctx.user).isSuccessFul, 'start'),
                      ...addGameActionWhen(canUserJoinGameRule(game.gameState, game.players, ctx.user).isSuccessFul, 'join'),
                      ...addGameActionWhen(canUserLeaveGameRule(game.gameState, game.players, game.owner, ctx.user).isSuccessFul, 'leave'),
                  ]
                : [];
            return {
                gameId: game.gameId,
                gameState: game.gameState,
                owner: game.owner,
                currentPlayer: game.currentPlayer,
                players: game.players,
                actions: gameActions,
            };
        });
        return gamesWithActions;
    }),
    byId: trpcInstance.procedure
        .input(Schema.API.gameId)
        .output(Schema.API.game)
        .query(async ({ input, ctx }) => {
            const gameDoc: admin.firestore.DocumentSnapshot<admin.firestore.DocumentData> = await admin
                .firestore()
                .doc(`games/${input.gameId}`)
                .get();
            if (!gameDoc.exists) {
                throw new TRPCError({
                    code: 'NOT_FOUND',
                    message: 'Game not found',
                });
            }
            const game = await Schema.Domain.game.parseAsync(gameDoc.data());
            const membranesWithActions: API.Membrane[] = game.membranes.map(membrane => {
                const membraneActions = ctx.user
                    ? [...addMembraneActionWhen(canUserTouchGameRule(game.gameState, game.currentPlayer, membrane, ctx.user).isSuccessFul, 'touch')]
                    : [];

                return {
                    ...membrane,
                    actions: membraneActions,
                };
            });

            const gameActions = ctx.user
                ? [
                      ...addGameActionWhen(canUserStartGameRule(game.gameState, game.players, game.owner, ctx.user).isSuccessFul, 'start'),
                      ...addGameActionWhen(canUserJoinGameRule(game.gameState, game.players, ctx.user).isSuccessFul, 'join'),
                      ...addGameActionWhen(canUserLeaveGameRule(game.gameState, game.players, game.owner, ctx.user).isSuccessFul, 'leave'),
                  ]
                : [];
            const gameWithActions: API.Game = {
                gameId: game.gameId,
                gameState: game.gameState,
                owner: game.owner,
                outline: game.outline,
                cells: game.cells,
                membranes: membranesWithActions,
                currentPlayer: game.currentPlayer,
                players: game.players,
                actions: gameActions,
            };
            return gameWithActions;
        }),
    create: authenticatedProcedure
        .input(
            z.object({
                gameId: z.string().uuid(),
                outline: z
                    .object({
                        x: z.number(),
                        y: z.number(),
                    })
                    .array()
                    .nonempty()
                    .length(4),
            })
        )
        .mutation(async ({ input, ctx }) => {
            const gameConfig: Domain.GameConfig = {
                ...input,
                owner: { userUID: ctx.user.userUID },
            };
            // check if game already exists
            const gameDoc: admin.firestore.DocumentSnapshot<admin.firestore.DocumentData> = await admin
                .firestore()
                .doc(`games/${gameConfig.gameId}`)
                .get();
            if (gameDoc.exists) {
                throw new TRPCError({
                    code: 'BAD_REQUEST',
                    message: 'Game already exists',
                });
            }
            const { cells, membranes } = createCellsAndMembranesForASquareGrid(gameConfig.outline);
            const game: Domain.Game = {
                gameId: gameConfig.gameId,
                gameState: 'setup',
                owner: gameConfig.owner,
                outline: gameConfig.outline,
                cells,
                membranes,
                currentPlayer: 'noOne',
                players: [gameConfig.owner],
            };
            ctx.res.statusCode = 202;
            return await gameDoc.ref.set(game);
        }),
    join: authenticatedProcedure.input(Schema.API.gameId).mutation(async ({ input, ctx }) => {
        const gameDoc: admin.firestore.DocumentSnapshot<admin.firestore.DocumentData> = await admin.firestore().doc(`games/${input.gameId}`).get();
        if (!gameDoc.exists) {
            throw new TRPCError({ code: 'NOT_FOUND', message: 'Game not found' });
        }
        const game = Schema.Domain.game.parse(gameDoc.data());
        //#region  authorization guards
        const { isSuccessFul, errors } = canUserJoinGameRule(game.gameState, game.players, ctx.user);
        if (!isSuccessFul) {
            throw new TRPCError({ code: 'BAD_REQUEST', message: errors.join('\n') });
        }
        //#endregion
        const updatedGame: Domain.Game = {
            ...game,
            players: [...game.players, { userUID: ctx.user.userUID }],
        };
        ctx.res.statusCode = 200;
        return await gameDoc.ref.set(updatedGame);
    }),
    leave: authenticatedProcedure.input(Schema.API.gameId).mutation(async ({ input, ctx }) => {
        const gameDoc: admin.firestore.DocumentSnapshot<admin.firestore.DocumentData> = await admin.firestore().doc(`games/${input.gameId}`).get();
        if (!gameDoc.exists) {
            throw new TRPCError({ code: 'NOT_FOUND', message: 'Game not found' });
        }
        const game = Schema.Domain.game.parse(gameDoc.data());
        const { isSuccessFul, errors } = canUserLeaveGameRule(game.gameState, game.players, game.owner, ctx.user);
        if (!isSuccessFul) {
            throw new TRPCError({ code: 'BAD_REQUEST', message: errors.join('\n') });
        }

        const updatedGame: Domain.Game = {
            ...game,
            players: game.players.filter(player => player.userUID !== ctx.user.userUID),
        };

        ctx.res.statusCode = 200;
        return await gameDoc.ref.set(updatedGame);
    }),
    start: authenticatedProcedure.input(Schema.API.gameId).mutation(async ({ input, ctx }) => {
        const gameDoc: admin.firestore.DocumentSnapshot<admin.firestore.DocumentData> = await admin.firestore().doc(`games/${input.gameId}`).get();
        if (!gameDoc.exists) {
            throw new TRPCError({ code: 'NOT_FOUND', message: 'Game not found' });
        }
        const game = Schema.Domain.game.parse(gameDoc.data());
        const { isSuccessFul, errors } = canUserStartGameRule(game.gameState, game.players, game.owner, ctx.user);
        if (!isSuccessFul) {
            throw new TRPCError({ code: 'BAD_REQUEST', message: errors.join('\n') });
        }
        const updatedGame: Domain.Game = {
            ...game,
            gameState: 'inProgress',
            currentPlayer: game.players[Math.floor(Math.random() * game.players.length)],
        };
        ctx.res.statusCode = 200;
        return await gameDoc.ref.set(updatedGame);
    }),
    membrane: membraneRoute,
});
