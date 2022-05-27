using System;
using System.Collections.Generic;
using System.Linq;
using CellConquest.DTOs;

namespace CellConquest.Models;

public class Game
{
    public string Id { get; }
    public GameState GameState { get; private set; } = GameState.WaitForPlayers;
    public Board Board { get; }
    public string Owner { get; }
    public string CurrentPlayerTurn { get; private set; } = StaticGameValues.NoOne;
    public List<string> Players { get; } = new();

    public Game(GameConfig gameConfig)
    {
        var (gameId, owner, pointFs) = gameConfig;
        Id = gameId;
        Board = new Board(pointFs);
        Owner = owner;
        Players.Add(owner);
    }

    #region Setup Phase

    public void AddPlayer(string playerName)
    {
        if (GameState != GameState.WaitForPlayers) // TODO Generalize Method guards.
        {
            throw new Exception("Player can only join a game Setup");
        }

        if (StaticGameValues.Contains(playerName))
        {
            throw new Exception("Not a valid player name");
        }

        if (Players.Exists(x => x == playerName))
        {
            throw new Exception("Player with that name already exists");
        }

        Players.Add(playerName);
        // Emit PlayerJoinedGameEvent
    }

    public void RemovePlayer(string playerName)
    {
        if (Players.Exists(x => x == playerName) == false)
        {
            throw new Exception("Player with that name doesn't exist");
        }

        Players.Remove(playerName);
        // Emit PlayerLeftGameEvent
    }

    public void Start()
    {
        if (Players.Count <= 1)
        {
            throw new Exception("It needs at least two players to start a game");
        }

        Random random = new();
        var randomPlayerIndex = random.Next(0, Players.Count);
        CurrentPlayerTurn = Players[randomPlayerIndex];
        GameState = GameState.Playing;
        // Emit GameStartedEvent
        // Emit PlayerTurnChangedEvent
    }

    #endregion

    #region Game Phase

    public void TouchMembrane(string playerName, string membraneId)
    {
        if (CurrentPlayerTurn != playerName)
        {
            throw new Exception($"It's not {playerName}'s turn");
        }

        var membrane = Board.GetMembraneById(membraneId);
        if (membrane is null)
        {
            throw new Exception($"Membrane with id: {membraneId} doesn't exist");
        }

        membrane.Touch(playerName);

        // Check if membrane is connected to Cells with no untouched membranes and mark the cells as conquered

        var conquerableCellsConnectedToMembraneId = Board.GetConquerableCellsConnectedToMembraneId(membraneId);
        foreach (var cell in conquerableCellsConnectedToMembraneId)
        {
            cell.Conquer(playerName);
        }

        if (conquerableCellsConnectedToMembraneId.Any())
        {
            return;
        }

        var nextPlayerTurn = Players[(Players.IndexOf(playerName) + 1) % Players.Count];
        CurrentPlayerTurn = nextPlayerTurn;
        // Emit CurrentPlayerTurnChanged
    }

    #endregion
}