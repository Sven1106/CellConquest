import { trpc } from "../utils/trpc";

const IndexPage = () => {
  return <GameList />;
};

const GameList = () => {
  const { data, isError, isLoading } = trpc.proxy.game.list.useQuery();
  if (isError) return <div>Error</div>;

  if (isLoading) return <div>Loading...</div>;

  if (data.length === 0) return <div>No games found</div>;
  return (
    <>
      {data.map((game) => (
        <div key={game.gameId}>
          {game.gameId}
        </div>
      ))}
    </>
  );
};
export default IndexPage;
