using System;
using System.Drawing;

namespace CellConquest.Models;

public sealed class Wall
{
    public PointF Point1 { get; }
    public PointF Point2 { get; }

    public Wall(PointF point1, PointF point2)
    {
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