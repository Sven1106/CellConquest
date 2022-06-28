using System;

namespace CellConquest.Domain.Exceptions;

public class InvalidGameStateException : ArgumentException
{
    public InvalidGameStateException(string message) : base(message)
    {
    }
}