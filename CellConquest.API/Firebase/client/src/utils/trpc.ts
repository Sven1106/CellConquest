import type { AppRouter } from "../../../functions/trpc/routers/app";
import { createTRPCReact } from "@trpc/react";

export const trpc = createTRPCReact<AppRouter>();
