// Start writing Firebase Functions
// https://firebase.google.com/docs/functions/typescript
import { inferAsyncReturnType, initTRPC, TRPCError } from '@trpc/server';
import * as trpcExpress from '@trpc/server/adapters/express';
import express from 'express';
import * as admin from 'firebase-admin';
import * as functions from 'firebase-functions';
import { isEqual, some } from 'lodash';
import { z } from 'zod';
import { createCellsAndMembranesForASquareGrid } from '../board';
import { Schema } from '../schemas';
import { DTO, Domain } from '../types';

admin.initializeApp();
const app = express();

const createContext = async ({ req, res }: trpcExpress.CreateExpressContextOptions) => {
    const idToken = req.headers.authorization?.split(' ')[1];
    if (!idToken) {
        return { req, res };
    }

    try {
        const { uid } = await admin.auth().verifyIdToken(idToken);
        return { req, res, userUID: uid };
    } catch (error) {
        return { req, res };
    }
}; // no context
type Context = inferAsyncReturnType<typeof createContext>;

const trpcInstance = initTRPC<{ ctx: Context }>()();

const isPlayerPartOfGame = (game: Domain.Game, userUID: string) => game.players.findIndex(p => p.userUID === userUID) !== -1;

const isPlayerOwner = (game: Domain.Game, userUID: string) => game.owner.userUID === userUID;

const isPlayerCurrentPlayer = (game: Domain.Game, userUID: string) => game.currentPlayer !== userUID;

const areThereSufficientPlayersInTheGame = (game: Domain.Game) => game.players.length > 1;

const authenticatedProcedure = trpcInstance.procedure.use(
    trpcInstance.middleware(({ next, ctx }) => {
        if (!ctx.userUID) {
            throw new TRPCError({
                code: 'UNAUTHORIZED',
                message: 'You are not authenticated',
            });
        }
        return next({ ctx: { userUID: ctx.userUID } });
    })
);

const membraneActionProcedures = {
    touch: authenticatedProcedure
        .input(Schema.gameIdDto.merge(z.object({ coordinates: Schema.coordinate.array().length(2) })))
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

            const game: Domain.Game = Schema.game.parse(gameDoc.data());

            if (!isPlayerPartOfGame(game, ctx.userUID)) {
                throw new TRPCError({
                    code: 'BAD_REQUEST',
                    message: 'Player is not part of this game',
                });
            }

            if (!isPlayerCurrentPlayer(game, ctx.userUID)) {
                throw new TRPCError({
                    code: 'BAD_REQUEST',
                    message: 'Player is not the current player',
                });
            }

            if (game.gameState !== 'inProgress') {
                throw new TRPCError({
                    code: 'BAD_REQUEST',
                    message: 'This game is not in progress',
                });
            }

            const membraneIndex = game.membranes.findIndex(m => isEqual(m.coordinates, input.coordinates));

            if (membraneIndex === -1) {
                throw new TRPCError({
                    code: 'NOT_FOUND',
                    message: 'Membrane not found',
                });
            }

            if (game.membranes[membraneIndex].touchedBy !== 'noOne') {
                throw new TRPCError({
                    code: 'BAD_REQUEST',
                    message: 'Membrane is already touched',
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
                                      userUID: ctx.userUID,
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
                        (membraneTouchedOnGame.players.findIndex(p => p.userUID === ctx.userUID) + 1) % membraneTouchedOnGame.players.length
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
                                  userUID: ctx.userUID,
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
                    userUID: ctx.userUID,
                },
            };
            return await gameDoc.ref.set(currentPlayerChangedToUserOnGame); // => cells were conquered and current player changed
        }),
};

const gamesProcedures = {
    get: authenticatedProcedure
        .input(z.union([Schema.gameIdDto, z.undefined()]))
        .output(Schema.gameDto.array())
        .query(async ({ input, ctx }) => {
            const gameCollection: admin.firestore.QuerySnapshot<admin.firestore.DocumentData> =
                input === undefined
                    ? await admin.firestore().collection('games').get()
                    : await admin.firestore().collection('games').where('gameId', '==', input.gameId).get();
            const games: Domain.Game[] = await Schema.game.array().parseAsync(gameCollection.docs.map(doc => doc.data()));
            const gamesWithActions: DTO.Game[] = Schema.gameDto.array().parse(
                games.map(game => {
                    const membranesWithActions: DTO.Membrane[] = game.membranes.map(membrane => {
                        const membraneActions: DTO.MembraneAction[] = [];
                        if (ctx.userUID) {
                            if (isPlayerCurrentPlayer(game, ctx.userUID) && game.gameState === 'inProgress' && membrane.touchedBy === 'noOne') {
                                membraneActions.push({ name: 'touch' });
                            }
                        }
                        return {
                            ...membrane,
                            actions: membraneActions,
                        };
                    });
                    const addActionWhen = (condition: boolean, action: DTO.GameAction) => {
                        return condition ? [action] : [];
                    };

                    const gameActions: DTO.GameAction[] = [
                        ...addActionWhen(
                            ctx.userUID !== undefined &&
                                isPlayerOwner(game, ctx.userUID) &&
                                game.gameState === 'setup' &&
                                areThereSufficientPlayersInTheGame(game),
                            {
                                name: 'start',
                            }
                        ),
                        ...addActionWhen(ctx.userUID !== undefined && isPlayerPartOfGame(game, ctx.userUID) === false, {
                            name: 'join',
                        }),
                        ...addActionWhen(
                            ctx.userUID !== undefined &&
                                isPlayerPartOfGame(game, ctx.userUID) &&
                                isPlayerOwner(game, ctx.userUID) === false &&
                                game.gameState === 'setup',
                            {
                                name: 'leave',
                            }
                        ),
                    ];
                    const gameWithActions: DTO.Game = {
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
                })
            );
            return gamesWithActions;
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
                owner: { userUID: ctx.userUID },
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
    membranes: trpcInstance.router(membraneActionProcedures),
    join: authenticatedProcedure.input(Schema.gameIdDto).mutation(async ({ input, ctx }) => {
        const gameDoc: admin.firestore.DocumentSnapshot<admin.firestore.DocumentData> = await admin.firestore().doc(`games/${input.gameId}`).get();
        if (!gameDoc.exists) {
            throw new TRPCError({ code: 'NOT_FOUND', message: 'Game not found' });
        }
        const game: Domain.Game = Schema.game.parse(gameDoc.data());
        if (isPlayerPartOfGame(game, ctx.userUID)) {
            throw new TRPCError({
                code: 'BAD_REQUEST',
                message: 'Player already exists in the game',
            });
        }
        if (game.gameState !== 'setup') {
            throw new TRPCError({
                code: 'BAD_REQUEST',
                message: 'Player can only join a game in setup',
            });
        }
        const updatedGame: Domain.Game = {
            ...game,
            players: [...game.players, { userUID: ctx.userUID }],
        };
        ctx.res.statusCode = 200;
        return await gameDoc.ref.set(updatedGame);
    }),
    leave: authenticatedProcedure.input(Schema.gameIdDto).mutation(async ({ input, ctx }) => {
        const gameDoc: admin.firestore.DocumentSnapshot<admin.firestore.DocumentData> = await admin.firestore().doc(`games/${input.gameId}`).get();
        if (!gameDoc.exists) {
            throw new TRPCError({ code: 'NOT_FOUND', message: 'Game not found' });
        }
        const game: Domain.Game = Schema.game.parse(gameDoc.data());
        if (isPlayerOwner(game, ctx.userUID)) {
            throw new TRPCError({
                code: 'BAD_REQUEST',
                message: "Owner can't be removed from the game",
            });
        }
        if (!isPlayerPartOfGame(game, ctx.userUID)) {
            throw new TRPCError({
                code: 'BAD_REQUEST',
                message: "Player doesn't exist in the game",
            });
        }
        if (game.gameState !== 'setup') {
            throw new TRPCError({
                code: 'BAD_REQUEST',
                message: 'Player can only leave a game in setup',
            });
        }

        const updatedGame: Domain.Game = {
            ...game,
            players: game.players.filter(player => player.userUID !== ctx.userUID),
        };

        ctx.res.statusCode = 200;
        return await gameDoc.ref.set(updatedGame);
    }),
    start: authenticatedProcedure.input(Schema.gameIdDto).mutation(async ({ input, ctx }) => {
        const gameDoc: admin.firestore.DocumentSnapshot<admin.firestore.DocumentData> = await admin.firestore().doc(`games/${input.gameId}`).get();
        if (!gameDoc.exists) {
            throw new TRPCError({ code: 'NOT_FOUND', message: 'Game not found' });
        }
        const game: Domain.Game = Schema.game.parse(gameDoc.data());
        if (!isPlayerOwner(game, ctx.userUID)) {
            throw new TRPCError({
                code: 'FORBIDDEN',
                message: 'Player is not the owner of the game',
            });
        }
        if (!areThereSufficientPlayersInTheGame(game)) {
            throw new TRPCError({
                code: 'BAD_REQUEST',
                message: 'There are not enough players in the game',
            });
        }
        if (game.gameState !== 'setup') {
            throw new TRPCError({
                code: 'BAD_REQUEST',
                message: "Player can't start a game that is not in setup",
            });
        }
        const updatedGame: Domain.Game = {
            ...game,
            gameState: 'inProgress',
            currentPlayer: game.players[Math.floor(Math.random() * game.players.length)],
        };
        ctx.res.statusCode = 200;
        return await gameDoc.ref.set(updatedGame);
    }),
};

const appRouter = trpcInstance.router({
    games: trpcInstance.router(gamesProcedures),
});

export type AppRouter = typeof appRouter;

app.use(
    '/',
    trpcExpress.createExpressMiddleware({
        router: appRouter,
        createContext,
    })
);

export const trpc = functions.region('europe-west1').https.onRequest(app);

/*
    Create game
    Mutation /trpc/games.create { gameId: string, outline: [{ x: number, y: number }] }

    Get games
    Query /trpc/games.get { gameId: string } | undefined

    Join game
    Mutation /trpc/games.join { gameId: string }

    Leave game
    Mutation /trpc/games.leave { gameId: string }

    Start game
    Mutation /trpc/games.start { gameId: string }

    Stop game
    Mutation /trpc/games.stop { gameId: string }

    Touch Membrane
    Mutation /trpc/games.membranes.touch { gameId: string, membraneId: string }
*/

// app.get('/:gameId', (request, response) => {
//     response.send(`GET Game with id: ${request.params.gameId}`);
// });
// app.post(
//     '/',
//     validateRequestBody(
//         z.object({
//             gameId: z.string(),
//             owner: z.string(),
//             outline: z.array(z.number().array().length(2)).nonempty(),
//         })
//     ),
//     async (request, response) => {
//
//     await admin.firestore().collection('games').add(gameConfig);

//         response.send(201);
//     }
// );
