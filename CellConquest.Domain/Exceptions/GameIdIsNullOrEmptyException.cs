using System;

namespace CellConquest.Domain.Exceptions;

public class GameIdIsNullOrEmptyException : ArgumentException
{
    public GameIdIsNullOrEmptyException() : base("Game id can't be null or empty")
    {
    }
}