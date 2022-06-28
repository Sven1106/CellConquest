using CellConquest.Domain.Entities;
using CellConquest.Domain.ValueObjects;

namespace CellConquest.Domain.Models;

public enum GameState
{
    NotSet,
    WaitForPlayers,
    Playing,
    Paused,
    Finished
}