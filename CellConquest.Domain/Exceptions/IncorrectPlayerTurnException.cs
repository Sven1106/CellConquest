using System;

namespace CellConquest.Domain.Exceptions;

public class IncorrectPlayerTurnException : ArgumentException
{
    public IncorrectPlayerTurnException(string message) : base(message)
    {
    }
}