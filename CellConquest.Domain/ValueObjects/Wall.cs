using System;
using System.Collections.Generic;
using System.Drawing;
using CellConquest.Domain.Exceptions;

namespace CellConquest.Domain.ValueObjects;

public sealed record Wall
{
    public PointF Point1 { get; }
    public PointF Point2 { get; }

    public Wall(PointF point1, PointF point2)
    {
        if (point1.Equals(point2))
        {
            throw new PointsHaveTheSameCoordinatesException($"{nameof(point1)} and {nameof(point2)} cant have the same coordinates");
        }

        Point1 = point1;
        Point2 = point2;
    }

    public bool Equals(Wall? other)
    {
        return other != null && (
            Point1 == other.Point1 && Point2 == other.Point2 ||
            Point1 == other.Point2 && Point2 == other.Point1
        );
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Point1, Point2);
    }
}