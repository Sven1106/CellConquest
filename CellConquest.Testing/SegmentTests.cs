using System.Collections.Generic;
using System.Drawing;
using CellConquest.Models;
using Xunit;
using PointF = System.Drawing.PointF;

namespace CellConquest.Testing;

public class SegmentTests
{
    [Theory]
    [InlineData(-1, -1, 1, 1, 1)]
    [InlineData(3, 3, 4, 1, -2)]
    [InlineData(4, 1, 3, 3, -2)]
    [InlineData(1, 2, 5, 2, 0)]
    [InlineData(1, 4, 6, 7, 0.6)]
    [InlineData(1, 1, 2, 3, 2)]
    [InlineData(-4, 4, 2, -6, -1.6666666)]
    [InlineData(2, 2, 2, 4, float.PositiveInfinity)]
    [InlineData(2, 4, 2, 2, float.NegativeInfinity)]
    public void CanCalculateSlopeOfSegment(float x1, float y1, float x2, float y2, float expectedSlope)
    {
        var lineSegment = new LineSegment(new PointF(x1, y1), new PointF(x2, y2));
        var slopeOfSegment = PolygonHelper.GetSlopeOfSegment(lineSegment);
        Assert.Equal(expectedSlope, slopeOfSegment);
    }

    [Theory]

    #region Diagonal

    [InlineData(0, 0, 1, 1, 3, 3, false)]
    [InlineData(2, 2, 1, 1, 3, 3, true)]
    [InlineData(4, 4, 1, 1, 3, 3, false)]
    [InlineData(0, 4, 1, 3, 3, 1, false)]
    [InlineData(2, 2, 1, 3, 3, 1, true)]
    [InlineData(4, 0, 1, 3, 3, 1, false)]

    #endregion

    #region Horizontal

    [InlineData(2, 6, 3, 6, 8, 6, false)]
    [InlineData(7, 6, 3, 6, 8, 6, true)]
    [InlineData(8, 6, 3, 6, 8, 6, true)]
    [InlineData(9, 6, 3, 6, 8, 6, false)]

    #endregion

    #region Vertical

    [InlineData(6, 2, 6, 3, 6, 8, false)]
    [InlineData(6, 7, 6, 3, 6, 8, true)]
    [InlineData(6, 8, 6, 3, 6, 8, true)]
    [InlineData(6, 9, 6, 3, 6, 8, false)]
    [InlineData(5, 2, 5, 1, 8, 4, false)]

    #endregion

    public void IsPointOnSegment(float x3, float y3, float x1, float y1, float x2, float y2, bool expected)
    {
        Assert.Equal(expected, PolygonHelper.IsPointOnSegment(new PointF(x3, y3), new LineSegment(new PointF(x1, y1), new PointF(x2, y2))));
    }

    [Theory]

    #region Diagonal

    [InlineData(0, 0, 1, 1, 2, 2, 5, 5, false)]
    [InlineData(1, 1, 2, 2, 2, 2, 5, 5, false)]
    [InlineData(1, 1, 3, 3, 2, 2, 5, 5, false)]
    [InlineData(2, 2, 3, 3, 2, 2, 5, 5, true)]
    [InlineData(3, 3, 4, 4, 2, 2, 5, 5, true)]
    [InlineData(4, 4, 5, 5, 2, 2, 5, 5, true)]
    [InlineData(4, 4, 6, 6, 2, 2, 5, 5, false)]
    [InlineData(5, 5, 6, 6, 2, 2, 5, 5, false)]
    [InlineData(6, 6, 7, 7, 2, 2, 5, 5, false)]

    #endregion

    #region Horizontal

    [InlineData(0, 5, 1, 5, 2, 5, 5, 5, false)]
    [InlineData(1, 5, 2, 5, 2, 5, 5, 5, false)]
    [InlineData(1, 5, 3, 5, 2, 5, 5, 5, false)]
    [InlineData(2, 5, 4, 5, 2, 5, 5, 5, true)]
    [InlineData(2, 5, 5, 5, 2, 5, 5, 5, true)]
    [InlineData(3, 5, 5, 5, 2, 5, 5, 5, true)]
    [InlineData(3, 5, 6, 5, 2, 5, 5, 5, false)]
    [InlineData(5, 5, 6, 5, 2, 5, 5, 5, false)]
    [InlineData(6, 5, 7, 5, 2, 5, 5, 5, false)]

    #endregion

    #region Vertical

    [InlineData(5, 0, 5, 1, 5, 2, 5, 5, false)]
    [InlineData(5, 1, 5, 2, 5, 2, 5, 5, false)]
    [InlineData(5, 1, 5, 3, 5, 2, 5, 5, false)]
    [InlineData(5, 2, 5, 4, 5, 2, 5, 5, true)]
    [InlineData(5, 2, 5, 5, 5, 2, 5, 5, true)]
    [InlineData(5, 3, 5, 5, 5, 2, 5, 5, true)]
    [InlineData(5, 3, 5, 6, 5, 2, 5, 5, false)]
    [InlineData(5, 5, 5, 6, 5, 2, 5, 5, false)]
    [InlineData(5, 6, 5, 7, 5, 2, 5, 5, false)]

    #endregion

    #region NonCollinear

    [InlineData(6, 2, 6, 5, 5, 2, 5, 5, false)]
    [InlineData(2, 6, 5, 6, 2, 5, 5, 5, false)]
    [InlineData(3, 4, 3, 6, 1, 5, 5, 5, false)]
    [InlineData(1, 1, 2, 1, 0, 2, 2, 0, false)]

    #endregion
    [InlineData(2, 1, 1, 1, 0, 2, 2, 0, false)]
    public void IsSegmentOnSegment(float x3, float y3, float x4, float y4, float x1, float y1, float x2, float y2, bool expected)
    {
        var segment1 = new LineSegment(new PointF(x3, y3), new PointF(x4, y4));
        var segment2 = new LineSegment(new PointF(x1, y1), new PointF(x2, y2));
        Assert.Equal(expected, PolygonHelper.IsSegmentOnSegment(segment1, segment2));
    }

    [Theory]

    #region Intersecting directly on segment.

    [InlineData(1, 1, 1, 0, 0, 0, 2, 2, true)]
    [InlineData(1, 1, 2, 1, 0, 0, 2, 2, true)]
    [InlineData(0, 1, 1, 1, 0, 2, 2, 0, true)]
    [InlineData(1, 0, 1, 1, 0, 2, 2, 0, true)]
    [InlineData(0, 1, 1, 1, 0, 0, 2, 2, true)]
    [InlineData(1, 1, 1, 2, 0, 0, 2, 2, true)]
    [InlineData(1, 1, 1, 2, 2, 0, 0, 2, true)]
    [InlineData(1, 1, 2, 1, 2, 0, 0, 2, true)]

    #endregion

    #region Intersecting across segment

    [InlineData(1, 1, 3, 1, 2, 1, 4, 1, true)]
    [InlineData(1, 2, 1, 0, 0, 0, 2, 2, true)]
    [InlineData(0, 1, 2, 1, 0, 0, 2, 2, true)]
    [InlineData(0, 1, 1, 2, 0, 2, 2, 0, true)]
    [InlineData(1, 0, 1, 2, 0, 2, 2, 0, true)]

    #endregion

    public void DoesSegmentsIntersect(float x3, float y3, float x4, float y4, float x1, float y1, float x2, float y2, bool expected)
    {
        var segment1 = new LineSegment(new PointF(x3, y3), new PointF(x4, y4));
        var segment2 = new LineSegment(new PointF(x1, y1), new PointF(x2, y2));
        Assert.Equal(expected, PolygonHelper.DoesSegmentsIntersect(segment1, segment2));
    }
}