// using System;
// using System.Drawing;
// using CellConquest.Domain.Exceptions;
// using CellConquest.Domain.ValueObjects;
// using Xunit;
//
// namespace CellConquest.Unittests;
//
// public class CheckingIfPointsInWallIsTheSame
// {
//     [Fact]
//     public void ShouldFail()
//     {
//         Assert.Throws<PointsHaveTheSameCoordinatesException>(() => new Wall(new PointF(1, 1), new PointF(1, 1)));
//     }
// }