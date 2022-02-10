using System.Drawing;

namespace CellConquest.Models;

public class LineSegment
{
    public System.Drawing.PointF Point1 { get; }
    public System.Drawing.PointF Point2 { get; }

    public LineSegment(System.Drawing.PointF point1, System.Drawing.PointF point2)
    {
        Point1 = point1;
        Point2 = point2;
    }
}