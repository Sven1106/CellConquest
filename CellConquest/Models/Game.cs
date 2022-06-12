﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CellConquest.DTOs;

namespace CellConquest.Models;

public record Game
{
    public string Id { get; init; }
    public GameState GameState { get; init; } = GameState.WaitForPlayers;
    public Board Board { get; init; }
    public string Owner { get; init; }
    public string CurrentPlayerTurn { get; init; } = StaticGameValues.NoOne;
    public IImmutableList<string> Players { get; init; } = ImmutableList<string>.Empty;

    public Game(GameConfig gameConfig)
    {
        var (gameId, owner, pointFs) = gameConfig;
        Id = gameId;
        Board = new Board(pointFs);
        Owner = owner;
        Players = Players.Add(owner);
    }
}