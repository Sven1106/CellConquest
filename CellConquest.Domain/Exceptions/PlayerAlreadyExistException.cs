using System;

namespace CellConquest.Domain.Exceptions;

public class PlayerAlreadyExistException : ArgumentException
{
    public PlayerAlreadyExistException(string message) : base(message)
    {
    }
}