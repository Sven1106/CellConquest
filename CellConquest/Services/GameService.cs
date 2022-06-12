using System;
using System.Collections.Immutable;
using System.Linq;
using CellConquest.Exceptions;
using CellConquest.Models;

namespace CellConquest.Services;

public static class GameService
{
    #region Setup Phase

    public static Game AddPlayerToGame(Game game, string playerName)
    {
        if (game.GameState != GameState.WaitForPlayers)
        {
            throw new InvalidGameStateException("Player can only be added in setup phase");
        }

        if (StaticGameValues.Contains(playerName))
        {
            throw new InvalidPlayerNameException("Not a valid player name");
        }

        if (game.Players.Contains(playerName))
        {
            throw new PlayerAlreadyExistException("Player with that name already exists");
        }

        return game with
        {
            Players = game.Players.Add(playerName)
        };
    }

    public static Game RemovePlayerFromGame(Game game, string playerName)
    {
        if (game.GameState != GameState.WaitForPlayers)
        {
            throw new InvalidGameStateException("Player can only be removed in setup phase");
        }

        if (game.Players.Contains(playerName) == false)
        {
            throw new PlayerNotFoundException("Player with that name not found");
        }

        if (game.Owner == playerName)
        {
            throw new OwnerCantBeRemovedException("Owner can't be removed from the game");
        }

        return game with
        {
            Players = game.Players.Remove(playerName)
        };
    }

    public static Game StartGame(Game game)
    {
        if (game.Players.Count <= 1)
        {
            throw new InsufficientPlayersException("It needs at least two players to start a game");
        }

        Random random = new();
        var randomPlayerIndex = random.Next(0, game.Players.Count);
        return game with
        {
            CurrentPlayerTurn = game.Players[randomPlayerIndex],
            GameState = GameState.Playing
        };
    }

    #endregion

    #region Game Phase

    public static Game TouchMembraneOnGame(Game game, string playerName, string membraneId)
    {
        if (game.CurrentPlayerTurn != playerName)
        {
            throw new IncorrectPlayerTurnException($"It's not {playerName}'s turn");
        }

        var membrane = game.Board.Membranes.FirstOrDefault(x => x.Id == membraneId);
        if (membrane is null)
        {
            throw new MembraneNotFoundException($"Membrane with id: {membraneId} doesn't exist");
        }

        if (membrane.IsTouched)
        {
            throw new MembraneAlreadyTouchedException($"Membrane with id: {membrane.Id} is already touched");
        }

        game = game with
        {
            Board = game.Board with
            {
                Membranes = game.Board.Membranes.Replace(membrane, membrane with
                {
                    TouchedBy = playerName
                })
            }
        };

        var conquerableCellsConnectedToMembraneId = GetConquerableCellsConnectedToMembraneId(game.Board.Cells, game.Board.Membranes, game.Board.CellMembranes, membraneId);
        if (conquerableCellsConnectedToMembraneId.Any())
        {
            return game with
            {
                Board = game.Board with
                {
                    Cells = game.Board.Cells
                        .Select(cell =>
                            conquerableCellsConnectedToMembraneId
                                .Any(conquerableCell => conquerableCell.Id == cell.Id)
                                ? cell  with
                                {
                                    ConqueredBy = playerName
                                }
                                : cell
                        )
                        .ToImmutableList()
                }
            };
        }

        var nextPlayerTurn = game.Players[(game.Players.IndexOf(playerName) + 1) % game.Players.Count];
        return game with { CurrentPlayerTurn = nextPlayerTurn };
    }

    #endregion

    private static ImmutableList<Cell> GetConquerableCellsConnectedToMembraneId(
        ImmutableList<Cell> cells,
        ImmutableList<Membrane> membranes,
        ImmutableList<CellMembrane> cellMembranes,
        string membraneId)
    {
        var conquerableCells =
            from cellMembrane
                in cellMembranes.Where(x => x.MembraneId == membraneId)
            let cell = cells.FirstOrDefault(x => x.Id == cellMembrane.CellId)
            where
                cell.IsConquered == false &&
                cellMembranes.Where(i => i.CellId == cellMembrane.CellId)
                    .All(x => membranes.FirstOrDefault(y => y.Id == x.MembraneId).IsTouched)
            select cell;
        return conquerableCells.ToImmutableList();
    }
}