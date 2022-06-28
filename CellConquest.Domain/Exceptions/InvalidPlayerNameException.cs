using System;

namespace CellConquest.Domain.Exceptions;

public class InvalidPlayerNameException : ArgumentException
{
    public InvalidPlayerNameException() : base("Not a valid player name")
    {
    }
}