using System.Collections.Generic;
using System.Drawing;
using CellConquest.Models;
using Xunit;
using PointF = System.Drawing.PointF;

namespace CellConquest.Testing;

public class PointTests
{
    [Theory]
    [InlineData(0, 1, false)]
    [InlineData(1, 1, false)]
    [InlineData(2, 2, true)]
    [InlineData(5, 1, false)]
    [InlineData(8, 4, false)]
    [InlineData(3, 4, false)]
    [InlineData(8, 9, false)]
    [InlineData(6, 8, true)]
    [InlineData(7, 9, false)]
    [InlineData(1, 9, false)]
    public void IsPointInPolygon(float x1, float y1, bool expected)
    {
        PointF[] polygon =
        {
            new(1, 1),
            new(5, 1),
            new(8, 4),
            new(3, 4),
            new(8, 9),
            new(1, 9)
        };
        Assert.Equal(expected, PolygonHelper.IsPointInPolygon(new PointF(x1, y1), polygon));
    }
    [Theory]
    // [InlineData(0, 1, false)]
    // [InlineData(1, 1, true)]
    // [InlineData(2, 1, true)]
    // [InlineData(2, 2, false)]
    // [InlineData(5, 1, true)]
    [InlineData(5, 2, false)]
    // [InlineData(8, 4, true)]
    // [InlineData(3, 4, true)]
    // [InlineData(8, 9, true)]
    // [InlineData(6, 8, true)]
    // [InlineData(7, 9, true)]
    // [InlineData(1, 9, true)]
    public void IsPointOnPolygon(float x1, float y1, bool expected)
    {
        PointF[] polygon =
        {
            new(1, 1),
            new(5, 1),
            new(8, 4),
            new(3, 4),
            new(8, 9),
            new(1, 9)
        };
        Assert.Equal(expected, PolygonHelper.IsPointOnPolygon(new PointF(x1, y1), polygon));
    }

}