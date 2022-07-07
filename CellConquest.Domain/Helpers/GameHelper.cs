using System;
using System.Collections.Immutable;
using System.Linq;
using CellConquest.Domain.Entities;
using CellConquest.Domain.Exceptions;
using CellConquest.Domain.Models;
using CellConquest.Domain.ValueObjects;

namespace CellConquest.Domain.Helpers;

public static class GameHelper
{
    #region Setup Phase

    public static Game AddPlayerToGame(Game game, PlayerName playerName)
    {
        if (game.GameState != GameState.WaitForPlayers)
        {
            throw new InvalidGameStateException("Player can only be added in WaitForPlayers state");
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

    public static Game RemovePlayerFromGame(Game game, PlayerName playerName)
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

    public static Game StartGame(Game game, PlayerName playerName)
    {
        if (game.Owner != playerName) // check if person is authorized.
        {
            throw new NotAuthorizedException("Player not authorized");
        }

        if (game.GameState != GameState.WaitForPlayers)
        {
            throw new InvalidGameStateException("Game can only be started in WaitForPlayers state");
        }

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

    public static Game TouchMembraneOnGame(Game game, PlayerName playerName, Wall selectedWall)
    {
        if (game.GameState != GameState.Playing)
        {
            throw new InvalidGameStateException("Player can only touch a membrane in Playing state");
        }

        if (game.CurrentPlayerTurn != playerName)
        {
            throw new IncorrectPlayerTurnException($"It's not {playerName}'s turn");
        }

        var membraneFound = game.Board.Membranes.FirstOrDefault(m => m.Wall == selectedWall);
        if (membraneFound is null)
        {
            throw new MembraneNotFoundException($"Membrane with id: {selectedWall} doesn't exist");
        }

        if (membraneFound.IsTouched)
        {
            throw new MembraneAlreadyTouchedException($"Membrane with id: {membraneFound.Wall} is already touched");
        }

        game = game with
        {
            Board = game.Board with
            {
                Membranes = game.Board.Membranes.Replace(membraneFound, membraneFound with
                {
                    TouchedBy = playerName
                })
            }
        };

        var conquerableCellsConnectedToMembraneWall = game.Board.Cells
            .Where(cell => cell.Walls.Contains(selectedWall)
                           && cell.IsConquered == false
                           && cell.Walls.All(wall =>
                               game.Board.Membranes.FirstOrDefault(membrane => membrane.Wall == wall).IsTouched)) // TODO Figure out why game is 'Captured variable is modified in the outer scope'
            .ToImmutableList();
        if (conquerableCellsConnectedToMembraneWall.IsEmpty)
        {
            var nextPlayerTurn = game.Players[(game.Players.IndexOf(playerName) + 1) % game.Players.Count];
            return game with
            {
                CurrentPlayerTurn = nextPlayerTurn
            };
        }

        game = game with
        {
            Board = game.Board with
            {
                Cells = game.Board.Cells
                    .Select(cell =>
                        conquerableCellsConnectedToMembraneWall
                            .Any(conquerableCell => conquerableCell.Walls == cell.Walls)
                            ? cell with
                            {
                                ConqueredBy = playerName
                            }
                            : cell
                    )
                    .ToImmutableList()
            }
        };
        var areAllCellsConquered = game.Board.Cells.All(cell => cell.IsConquered);
        var areAllMembranesTouched = game.Board.Membranes.All(cell => cell.IsTouched);
        if (areAllCellsConquered != areAllMembranesTouched)
        {
            throw new Exception("All cells should be conquered and all membranes should be touched");
        }

        if (areAllCellsConquered && areAllMembranesTouched)
        {
            game = game with
            {
                GameState = GameState.Finished
            };
        }

        return game;
    }

    #endregion
}