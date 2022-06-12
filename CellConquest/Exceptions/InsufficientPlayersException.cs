using System;

namespace CellConquest.Exceptions;

public class InsufficientPlayersException : ArgumentException
{
    public InsufficientPlayersException(string message) : base(message)
    {
    }
}