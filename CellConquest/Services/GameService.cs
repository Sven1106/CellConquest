using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CellConquest.Models;

namespace CellConquest.Services;

public static class GameService
{
    #region Setup Phase

    public static Game AddPlayerToGame(Game game, string playerName)
    {
        if (game.GameState != GameState.WaitForPlayers) // TODO Generalize Method guards.
        {
            throw new Exception("Player can only join a game Setup");
        }

        if (StaticGameValues.Contains(playerName))
        {
            throw new Exception("Not a valid player name");
        }

        if (game.Players.Exists(x => x == playerName))
        {
            throw new Exception("Player with that name already exists");
        }

        game.Players.Add(playerName);
        return game;
    }

    public static Game RemovePlayerFromGame(Game game, string playerName)
    {
        if (game.Players.Exists(x => x == playerName) == false)
        {
            throw new Exception("Player with that name doesn't exist");
        }

        game.Players.Remove(playerName);
        return game;
    }

    public static Game StartGame(Game game)
    {
        if (game.Players.Count <= 1)
        {
            throw new Exception("It needs at least two players to start a game");
        }

        Random random = new();
        var randomPlayerIndex = random.Next(0, game.Players.Count);
        return game with
        {
            CurrentPlayerTurn = game.Players[randomPlayerIndex],
            GameState = GameState.Playing
        };
        // Emit GameStartedEvent
        // Emit PlayerTurnChangedEvent
    }

    #endregion

    #region Game Phase

    public static Game TouchMembraneOnGame(Game game, string playerName, string membraneId)
    {
        if (game.CurrentPlayerTurn != playerName)
        {
            throw new Exception($"It's not {playerName}'s turn");
        }

        var membrane = game.Board.Membranes.FirstOrDefault(x => x.Id == membraneId);
        if (membrane is null)
        {
            throw new Exception($"Membrane with id: {membraneId} doesn't exist");
        }

        if (membrane.IsTouched)
        {
            throw new Exception($"Membrane with id: {membrane.Id} is already touched");
        }

        var touchedMembrane = membrane with
        {
            TouchedBy = playerName
        };

        game = game with
        {
            Board = game.Board with
            {
                Membranes = game.Board.Membranes.Replace(membrane, touchedMembrane)
            }
        };

        // Check if membrane is connected to Cells with no untouched membranes and mark the cells as conquered

        Cell ConquerCell(Cell cell, string playerId)
            => cell.IsConquered ? throw new Exception($"Cell with id: {cell.Id} is already conquered") : cell with { ConqueredBy = playerId };


        var conquerableCellsConnectedToMembraneId = GetConquerableCellsConnectedToMembraneId(game.Board.Cells, game.Board.Membranes, game.Board.CellMembranes, membraneId);
        var conqueredCells = conquerableCellsConnectedToMembraneId.Select(cell => ConquerCell(cell, playerName)).ToImmutableList();
        game = game with
        {
            Board = game.Board with { Cells = game.Board.Cells.RemoveRange(conquerableCellsConnectedToMembraneId).AddRange(conqueredCells) }
        };
        if (conqueredCells.Any())
        {
            return game;
        }

        var nextPlayerTurn = game.Players[(game.Players.IndexOf(playerName) + 1) % game.Players.Count];
        game = game with { CurrentPlayerTurn = nextPlayerTurn };
        return game;
        // Emit CurrentPlayerTurnChanged
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