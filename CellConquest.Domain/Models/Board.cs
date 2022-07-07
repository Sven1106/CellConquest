using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using CellConquest.Domain.Helpers;
using CellConquest.Domain.ValueObjects;

namespace CellConquest.Domain.Models;

public record Board
{
    public PointF[] Outline { get; }
    public ImmutableList<Cell> Cells { get; init; }
    public ImmutableList<Membrane> Membranes { get; init; }

    public Board(PointF[] outline)
    {
        Outline = outline;
        var (cells, membranes) = CreateCellsAndMembranesFromPolygon(Outline);
        Cells = ImmutableList<Cell>.Empty.AddRange(cells);
        Membranes = ImmutableList<Membrane>.Empty.AddRange(membranes);
    }

    private static IEnumerable<Wall> GetWallsOfParallelCell(int wallIndex, Wall wall)
    {
        // This is tightly coupled to 4 wall.
        // Since we know walls are iterated from top > right > bottom > left,
        // we can use the index to map to the corresponding parallel cell walls.
        var parallelCellWalls = wallIndex switch
        {
            0 => new Wall[] // cell walls above
            {
                new(
                    new PointF(wall.First.X, wall.First.Y - 1),
                    new PointF(wall.Second.X, wall.Second.Y - 1)
                ),
                new(
                    new PointF(wall.First.X + 1, wall.Second.Y - 1),
                    new PointF(wall.Second.X, wall.Second.Y)
                ),
                new(
                    new PointF(wall.Second.X, wall.Second.Y),
                    new PointF(wall.First.X, wall.First.Y)
                ),
                new(
                    new PointF(wall.First.X, wall.First.Y),
                    new PointF(wall.Second.X - 1, wall.Second.Y - 1)
                )
            },
            1 => new Wall[] // cell walls to the right
            {
                new(
                    new PointF(wall.First.X, wall.First.Y),
                    new PointF(wall.Second.X + 1, wall.Second.Y - 1)
                ),
                new(
                    new PointF(wall.First.X + 1, wall.First.Y),
                    new PointF(wall.Second.X + 1, wall.Second.Y)
                ),
                new(
                    new PointF(wall.First.X + 1, wall.First.Y + 1),
                    new PointF(wall.Second.X, wall.Second.Y)
                ),
                new(
                    new PointF(wall.Second.X, wall.Second.Y),
                    new PointF(wall.First.X, wall.First.Y)
                )
            },
            2 => new Wall[] // cell walls below
            {
                new( // above
                    new PointF(wall.Second.X, wall.Second.Y),
                    new PointF(wall.First.X, wall.First.Y)
                ),
                new( // right
                    new PointF(wall.First.X, wall.First.Y),
                    new PointF(wall.Second.X + 1, wall.Second.Y + 1)
                ),
                new( // bottom
                    new PointF(wall.First.X, wall.First.Y + 1),
                    new PointF(wall.Second.X, wall.Second.Y + 1)
                ),
                new( // left
                    new PointF(wall.First.X - 1, wall.First.Y + 1),
                    new PointF(wall.Second.X, wall.Second.Y)
                )
            },
            3 => new Wall[] // cell walls to the left
            {
                new( // above
                    new PointF(wall.First.X - 1, wall.First.Y - 1),
                    new PointF(wall.Second.X, wall.Second.Y)
                ),
                new( // right
                    new PointF(wall.Second.X, wall.Second.Y),
                    new PointF(wall.First.X, wall.First.Y)
                ),
                new( // bottom
                    new PointF(wall.First.X, wall.First.Y),
                    new PointF(wall.Second.X - 1, wall.Second.Y + 1)
                ),
                new( // left
                    new PointF(wall.First.X - 1, wall.First.Y),
                    new PointF(wall.Second.X - 1, wall.Second.Y)
                )
            },
            _ => throw new Exception("There should only be 4 walls: 0, 1, 2 ,3")
        };
        return parallelCellWalls;
    }

    private static bool IsAnyWallInvalid(IEnumerable<Wall> walls, PointF[] polygon)
    {
        var enlargedPolygon = PolygonHelper.GetEnlargedPolygon(polygon, -0.001f);

        var hasAnyInvalidWall = false;
        foreach (var wall in walls)
        {
            var isPoint1InPolygon = PolygonHelper.IsPointInPolygon(wall.First, enlargedPolygon);
            var isPoint2InPolygon = PolygonHelper.IsPointInPolygon(wall.Second, enlargedPolygon);
            if (isPoint1InPolygon == false || isPoint2InPolygon == false)
            {
                hasAnyInvalidWall = true;
                break;
            }

            for (var polygonIndex = 0; polygonIndex < enlargedPolygon.Length; polygonIndex++)
            {
                var nextIndex = (polygonIndex + 1) % enlargedPolygon.Length;
                var currentPolygonPoint = enlargedPolygon[polygonIndex];
                var nextPolygonPoint = enlargedPolygon[nextIndex];
                PolygonHelper.FindIntersection(wall.First, wall.Second, currentPolygonPoint,
                    nextPolygonPoint, out var doesLinesIntersect, out var doesSegmentsIntersect,
                    out _);
                if (doesSegmentsIntersect == false || doesLinesIntersect == false)
                {
                    continue;
                }

                hasAnyInvalidWall = true;
                break;
            }
        }

        return hasAnyInvalidWall;
    }

    private static Wall[] GetPredictedWalls(RectangleF boundingBox, int column, int row)
    {
        var topLeft = new PointF(boundingBox.X + column, boundingBox.Y + row);
        var topRight = new PointF(boundingBox.X + column + 1, boundingBox.Y + row);
        var bottomRight = new PointF(boundingBox.X + column + 1, boundingBox.Y + row + 1);
        var bottomLeft = new PointF(boundingBox.X + column, boundingBox.Y + row + 1);
        var topWall = new Wall(topLeft, topRight);
        var rightWall = new Wall(topRight, bottomRight);
        var bottomWall = new Wall(bottomRight, bottomLeft);
        var leftWall = new Wall(bottomLeft, topLeft);
        var walls = new[]
        {
            topWall,
            rightWall,
            bottomWall,
            leftWall
        };
        return walls;
    }

    private static List<PointF> GetPredictedCoordinates(RectangleF boundingBox, int column, int row)
    {
        var topLeft = new PointF(boundingBox.X + column, boundingBox.Y + row);
        var topRight = new PointF(boundingBox.X + column + 1, boundingBox.Y + row);
        var bottomRight = new PointF(boundingBox.X + column + 1, boundingBox.Y + row + 1);
        var bottomLeft = new PointF(boundingBox.X + column, boundingBox.Y + row + 1);
        var coordinates = new List<PointF>
        {
            topLeft,
            topRight,
            bottomRight,
            bottomLeft
        };
        return coordinates;
    }

    private static (List<Cell>, List<Membrane>) CreateCellsAndMembranesFromPolygon(PointF[] polygon)
    {
        var cells = new List<Cell>();
        var membranes = new List<Membrane>();
        var boundingBox = PolygonHelper.GetBounds(polygon.ToList());
        for (var row = 0; row < boundingBox.Height; row++) // For creating cells that fits in a square grid
        {
            for (var column = 0; column < boundingBox.Width; column++)
            {
                var walls = GetPredictedWalls(boundingBox, column, row);
                var isPredictedWallsInvalid = IsAnyWallInvalid(walls, polygon);
                if (isPredictedWallsInvalid) // CONDITION
                {
                    continue;
                }

                var membranesWithSameWallsAsCell = new List<Membrane>();
                var newMembranes = new List<Membrane>();
                for (var wallIndex = 0; wallIndex < walls.Length; wallIndex++)
                {
                    var wall = walls[wallIndex];
                    var membrane = membranes.FirstOrDefault(x => x.Wall.Equals(wall));
                    if (membrane is null)
                    {
                        var isWallAnOutline = PolygonHelper.IsSegmentOnPolygon(wall, polygon);
                        var markMembraneAsOutline = isWallAnOutline;
                        if (markMembraneAsOutline == false)
                        {
                            var wallsOfParallelCell = GetWallsOfParallelCell(wallIndex, wall);
                            // checks if parallel neighbour cell walls are out of polygon.
                            var isAnyWallOfParallelCellInvalid =
                                IsAnyWallInvalid(wallsOfParallelCell, polygon); // TODO This could be optimized.
                            markMembraneAsOutline = isAnyWallOfParallelCellInvalid;
                        }

                        membrane = new Membrane(wall, new List<PointF> { wall.First, wall.Second }, markMembraneAsOutline);
                        newMembranes.Add(membrane);
                    }

                    membranesWithSameWallsAsCell.Add(membrane);
                }


                if (membranesWithSameWallsAsCell.All(cellMembrane => cellMembrane.TouchedBy == StaticGameValues.Board))
                    // Skips a cell if it is a 1by1
                {
                    continue;
                }


                Cell newCell = new(walls, GetPredictedCoordinates(boundingBox, column, row));
                cells.Add(newCell);
                membranes.AddRange(newMembranes);
            }
        }

        return (cells, membranes);
    }
}