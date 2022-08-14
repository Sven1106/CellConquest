import type { AppRouter } from "../../../functions/trpc/routers/app";
import { createReactQueryHooks, createReactQueryHooksProxy } from "@trpc/react";

const hooks = createReactQueryHooks<AppRouter>();
const proxy = createReactQueryHooksProxy<AppRouter>(hooks);
export const trpc = {
  proxy,
  ...hooks,
};
