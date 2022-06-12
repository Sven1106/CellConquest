using System;

namespace CellConquest.Exceptions;

public class InvalidGameStateException : ArgumentException
{
    public InvalidGameStateException(string message) : base(message)
    {
    }
}