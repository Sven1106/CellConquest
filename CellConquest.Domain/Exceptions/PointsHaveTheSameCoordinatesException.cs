using System;

namespace CellConquest.Domain.Exceptions;

public class PointsHaveTheSameCoordinatesException : Exception
{
    public PointsHaveTheSameCoordinatesException(string message) : base(message)
    {
    }
}