import type { AppRouter } from "../../functions/src/index";
import { createTRPCClient, createTRPCClientProxy } from "@trpc/client";

const client = createTRPCClient<AppRouter>({
  url: "http://localhost:5000/trpc",
});
const proxy = createTRPCClientProxy(client);
