import { Link } from 'react-router-dom';
import { trpc } from "../utils/trpc";

const GamesPage = () => {
  return (
    <>
      <h1>Games</h1>
      <GamesList />
    </>
  );
};
const GamesList = () => {
  const { data, isLoading, error } = trpc.game.list.useQuery();
  if (error) return <div>Error</div>;
  if (isLoading) return <div>Loading...</div>;
  if (data.length === 0) return <div>No games found</div>;
  return (
    <>
      {data.map((game: any) => (
        <div key={game.gameId}>
          {game.gameId}
          <Link to={`/games/${game.gameId}`}>View</Link>
        </div>
      ))}
    </>
  );
};
export default GamesPage;
