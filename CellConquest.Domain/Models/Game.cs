using System.Collections.Immutable;
using CellConquest.Domain.ValueObjects;

namespace CellConquest.Domain.Models;

public record Game
{
    public GameId Id { get; init; }
    public GameState GameState { get; init; } = GameState.WaitForPlayers; // Should this be handled through finite state machine?
    public Board Board { get; init; }
    public PlayerName Owner { get; init; }
    public PlayerName CurrentPlayerTurn { get; init; } = StaticGameValues.NoOne; // Should this only be accessible in certain states?
    public ImmutableList<PlayerName> Players { get; init; }

    public Game(GameConfig gameConfig)
    {
        var (gameId, owner, pointFs) = gameConfig;
        Id = gameId;
        Board = new Board(pointFs);
        Owner = owner;
        Players = ImmutableList<PlayerName>.Empty.Add(owner);
    }
}