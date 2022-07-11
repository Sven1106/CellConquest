using System;

namespace CellConquest.Domain.Exceptions;

public class CoordinatesInvalidException : ArgumentException
{
    public CoordinatesInvalidException(string message) : base(message)
    {
    }
}