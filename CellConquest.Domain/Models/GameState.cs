﻿namespace CellConquest.Domain.Models;

public enum GameState
{
    NotSet,
    WaitForPlayers,
    Playing,
    Paused,
    Finished
}