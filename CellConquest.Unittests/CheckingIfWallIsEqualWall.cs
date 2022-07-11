// using System.Drawing;
// using CellConquest.Domain.Models;
// using CellConquest.Domain.ValueObjects;
// using Xunit;
//
// namespace CellConquest.Unittests;
//
// public class CheckingIfWallIsEqualWall
// {
//     [Theory]
//     [InlineData(1, 1, 1, 2, 1, 1, 1, 2)]
//     [InlineData(1, 2, 1, 1, 1, 1, 1, 2)]
//     [InlineData(1, 1, 2, 1, 1, 1, 2, 1)]
//     [InlineData(2, 1, 1, 1, 1, 1, 2, 1)]
//     public void ShouldSucceed(
//         int point1X, int point1Y, int point2X, int point2Y,
//         int expectedPoint1X, int expectedPoint1Y, int expectedPoint2X, int expectedPoint2Y)
//     {
//         //GIVEN
//         var wall = new Wall(new PointF(point1X, point1Y), new PointF(point2X, point2Y));
//         //WHEN
//         var otherWall = new Wall(new PointF(expectedPoint1X, expectedPoint1Y), new PointF(expectedPoint2X, expectedPoint2Y));
//         //THEN
//         Assert.True(wall.Equals(otherWall));
//     }
// }