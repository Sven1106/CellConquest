import { inferAsyncReturnType, initTRPC } from '@trpc/server';
import * as trpcExpress from '@trpc/server/adapters/express';
import * as admin from 'firebase-admin';

export const createContext = async ({ req, res }: trpcExpress.CreateExpressContextOptions) => {
    const idToken = req.headers.authorization?.split(' ')[1];
    if (!idToken) {
        return { req, res };
    }

    try {
        const { uid } = await admin.auth().verifyIdToken(idToken);
        return { req, res, user: { userUID: uid } };
    } catch (error) {
        return { req, res };
    }
};
type Context = inferAsyncReturnType<typeof createContext>;

export const trpcInstance = initTRPC<{ ctx: Context }>()();
