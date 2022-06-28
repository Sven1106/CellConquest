using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public ImmutableList<CellMembrane> CellMembranes { get; init; }

    public Board(PointF[] outline)
    {
        Outline = outline;
        var (cells, membranes, cellMembranes) = CreateCellsFromPolygon(Outline);
        Cells = ImmutableList<Cell>.Empty.AddRange(cells);
        Membranes = ImmutableList<Membrane>.Empty.AddRange(membranes);
        CellMembranes = ImmutableList<CellMembrane>.Empty.AddRange(cellMembranes);
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
                    new PointF(wall.Point1.X, wall.Point1.Y - 1),
                    new PointF(wall.Point2.X, wall.Point2.Y - 1)
                ),
                new(
                    new PointF(wall.Point1.X + 1, wall.Point2.Y - 1),
                    new PointF(wall.Point2.X, wall.Point2.Y)
                ),
                new(
                    new PointF(wall.Point2.X, wall.Point2.Y),
                    new PointF(wall.Point1.X, wall.Point1.Y)
                ),
                new(
                    new PointF(wall.Point1.X, wall.Point1.Y),
                    new PointF(wall.Point2.X - 1, wall.Point2.Y - 1)
                )
            },
            1 => new Wall[] // cell walls to the right
            {
                new(
                    new PointF(wall.Point1.X, wall.Point1.Y),
                    new PointF(wall.Point2.X + 1, wall.Point2.Y - 1)
                ),
                new(
                    new PointF(wall.Point1.X + 1, wall.Point1.Y),
                    new PointF(wall.Point2.X + 1, wall.Point2.Y)
                ),
                new(
                    new PointF(wall.Point1.X + 1, wall.Point1.Y + 1),
                    new PointF(wall.Point2.X, wall.Point2.Y)
                ),
                new(
                    new PointF(wall.Point2.X, wall.Point2.Y),
                    new PointF(wall.Point1.X, wall.Point1.Y)
                )
            },
            2 => new Wall[] // cell walls below
            {
                new( // above
                    new PointF(wall.Point2.X, wall.Point2.Y),
                    new PointF(wall.Point1.X, wall.Point1.Y)
                ),
                new( // right
                    new PointF(wall.Point1.X, wall.Point1.Y),
                    new PointF(wall.Point2.X + 1, wall.Point2.Y + 1)
                ),
                new( // bottom
                    new PointF(wall.Point1.X, wall.Point1.Y + 1),
                    new PointF(wall.Point2.X, wall.Point2.Y + 1)
                ),
                new( // left
                    new PointF(wall.Point1.X - 1, wall.Point1.Y + 1),
                    new PointF(wall.Point2.X, wall.Point2.Y)
                )
            },
            3 => new Wall[] // cell walls to the left
            {
                new( // above
                    new PointF(wall.Point1.X - 1, wall.Point1.Y - 1),
                    new PointF(wall.Point2.X, wall.Point2.Y)
                ),
                new( // right
                    new PointF(wall.Point2.X, wall.Point2.Y),
                    new PointF(wall.Point1.X, wall.Point1.Y)
                ),
                new( // bottom
                    new PointF(wall.Point1.X, wall.Point1.Y),
                    new PointF(wall.Point2.X - 1, wall.Point2.Y + 1)
                ),
                new( // left
                    new PointF(wall.Point1.X - 1, wall.Point1.Y),
                    new PointF(wall.Point2.X - 1, wall.Point2.Y)
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
            var isPoint1InPolygon = PolygonHelper.IsPointInPolygon(wall.Point1, enlargedPolygon);
            var isPoint2InPolygon = PolygonHelper.IsPointInPolygon(wall.Point2, enlargedPolygon);
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
                PolygonHelper.FindIntersection(wall.Point1, wall.Point2, currentPolygonPoint,
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

    private static (List<Cell>, List<Membrane>, List<CellMembrane>) CreateCellsFromPolygon(PointF[] polygon)
    {
        var cells = new List<Cell>();
        var membranes = new List<Membrane>();
        var cellMembranes = new List<CellMembrane>();
        var boundingBox = PolygonHelper.GetBounds(polygon.ToList());
        for (var row = 0; row < boundingBox.Height; row++) // For creating cells that fits in a square grid
        {
            for (var column = 0; column < boundingBox.Width; column++)
            {
                var predictedWalls = GetPredictedWalls(boundingBox, column, row);
                var isPredictedWallsInvalid = IsAnyWallInvalid(predictedWalls, polygon);
                if (isPredictedWallsInvalid) // CONDITION
                {
                    continue;
                }

                Cell newCell = new(Convert.ToString(cells.Count + 1));
                var newMembranes = new List<Membrane>();
                var newCellMembranes = new List<CellMembrane>();
                for (var wallIndex = 0; wallIndex < predictedWalls.Length; wallIndex++)
                {
                    var wall = predictedWalls[wallIndex];
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


                        membrane = new Membrane(Convert.ToString(membranes.Count + newMembranes.Count + 1), wall,
                            markMembraneAsOutline);
                        newMembranes.Add(membrane);
                    }

                    newCellMembranes.Add(new CellMembrane(newCell.Id, membrane.Id));
                }

                if (newCellMembranes.All(cellMembrane =>
                        membranes.Concat(newMembranes).FirstOrDefault(x => x.Id == cellMembrane.MembraneId)
                            .TouchedBy ==
                        StaticGameValues.Board)
                   )
                    // Skips a cell if it is a 1by1
                {
                    continue;
                }

                cells.Add(newCell);
                membranes.AddRange(newMembranes);
                cellMembranes.AddRange(newCellMembranes);
            }
        }
        return (cells, membranes, cellMembranes);
    }
}