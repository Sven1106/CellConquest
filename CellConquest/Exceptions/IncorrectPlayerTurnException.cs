using System;

namespace CellConquest.Exceptions;

public class IncorrectPlayerTurnException : ArgumentException
{
    public IncorrectPlayerTurnException(string message) : base(message)
    {
    }
}