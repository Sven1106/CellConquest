using System;

namespace CellConquest.Domain.Exceptions;

public class PlayerNameIsNullOrEmptyException : ArgumentException
{
    public PlayerNameIsNullOrEmptyException() : base("Player name can't be null or empty")
    {
    }
}