import { trpcInstance } from '../trpcInstance';
import { gameRouter } from './game';

const helloRoute = trpcInstance.procedure.query(async () => {
    return 'Hello world!!';
});
export const appRouter = trpcInstance.router({
    game: gameRouter,
    hello: helloRoute,
});

// export type definition of API
export type AppRouter = typeof appRouter;
