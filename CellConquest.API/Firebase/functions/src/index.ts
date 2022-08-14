// Start writing Firebase Functions
// https://firebase.google.com/docs/functions/typescript
import * as trpcExpress from '@trpc/server/adapters/express';
import * as cors from 'cors';
import express from 'express';
import * as admin from 'firebase-admin';
import * as functions from 'firebase-functions';
import { appRouter } from '../trpc/routers/app';
import { createContext } from '../trpc/trpcInstance';

admin.initializeApp();
const app = express();
app.use(cors.default({ origin: true }));

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
