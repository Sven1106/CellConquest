using System;

namespace CellConquest.Exceptions;

public class InvalidPlayerNameException : ArgumentException
{
    public InvalidPlayerNameException(string message) : base(message)
    {
    }
}