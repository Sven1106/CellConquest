using System;

namespace CellConquest.Exceptions;

public class PlayerNotFoundException : ArgumentException
{
    public PlayerNotFoundException(string message) : base(message)
    {
    }
}