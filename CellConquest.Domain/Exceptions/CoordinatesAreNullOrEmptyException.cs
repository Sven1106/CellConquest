using System;

namespace CellConquest.Domain.Exceptions;

public class CoordinatesAreNullOrEmptyException : ArgumentException
{
    public CoordinatesAreNullOrEmptyException() : base("Coordinates can't be null or empty")
    {
    }
}