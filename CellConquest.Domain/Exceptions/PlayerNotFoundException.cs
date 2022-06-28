using System;

namespace CellConquest.Domain.Exceptions;

public class PlayerNotFoundException : ArgumentException
{
    public PlayerNotFoundException(string message) : base(message)
    {
    }
}