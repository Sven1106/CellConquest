using System;
using System.Drawing;
using CellConquest.Domain.Exceptions;

namespace CellConquest.Domain.ValueObjects;

public sealed record Wall // Convert to contain Coordinates.
{
    public PointF First { get; }
    public PointF Second { get; }

    public Wall(PointF first, PointF second)
    {
        if (first.Equals(second))
        {
            throw new PointsHaveTheSameCoordinatesException($"{nameof(first)} and {nameof(second)} cant have the same coordinates");
        }

        First = first;
        Second = second;
    }

    public bool Equals(Wall? other)
    {
        return other != null && (
            First == other.First && Second == other.Second ||
            First == other.Second && Second == other.First
        );
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(First, Second);
    }

    public override string ToString()
    {
        return $"({First.X},{First.Y})({Second.X},{Second.Y})";
    }
    //
    // public static implicit operator string(Wall wall) => wall.ToString();
}