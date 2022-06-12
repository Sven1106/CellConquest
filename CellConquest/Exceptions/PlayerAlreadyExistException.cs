using System;

namespace CellConquest.Exceptions;

public class PlayerAlreadyExistException : ArgumentException
{
    public PlayerAlreadyExistException(string message) : base(message)
    {
    }
}