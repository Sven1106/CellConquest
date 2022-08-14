import { trpc } from "../utils/trpc";

const GamePage = ({ gameId }: { gameId: string }) => {
  const { data, error } = trpc.proxy.game.byId.useQuery({ gameId });
  if (error || !data) {
    return <div>Redirect to previous page</div>;
  }
  return <>{data.gameId}</>;
};
export default GamePage;
