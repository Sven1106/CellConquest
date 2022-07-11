using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CellConquest.Domain.Helpers;

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

    private static PointF[] GetCoordinatesOfParallelCell(int wallIndex, IReadOnlyList<PointF> coordinates)
    {
        // This is tightly coupled to 4 wall.
        // Since we know walls are iterated from top > right > bottom > left,
        // we can use the index to map to the corresponding parallel cell walls.
        var parallelCellCoordinates = wallIndex switch
        {
            0 => new PointF[] // cell walls above
            {
                new(coordinates[0].X, coordinates[0].Y - 1),
                new(coordinates[1].X, coordinates[1].Y - 1),
                new(coordinates[1].X, coordinates[1].Y),
                new(coordinates[0].X, coordinates[0].Y),
            },
            1 => new PointF[] // cell walls to the right
            {
                new(coordinates[0].X, coordinates[0].Y),
                new(coordinates[0].X + 1, coordinates[0].Y),
                new(coordinates[1].X + 1, coordinates[1].Y),
                new(coordinates[1].X, coordinates[1].Y),
            },
            2 => new PointF[] // cell walls below
            {
                new(coordinates[1].X, coordinates[1].Y),
                new(coordinates[0].X, coordinates[0].Y),
                new(coordinates[0].X, coordinates[0].Y + 1),
                new(coordinates[1].X, coordinates[1].Y + 1)
            },
            3 => new PointF[] // cell walls to the left
            {
                new(coordinates[1].X - 1, coordinates[1].Y),
                new(coordinates[1].X, coordinates[1].Y),
                new(coordinates[0].X, coordinates[0].Y),
                new(coordinates[0].X - 1, coordinates[0].Y),
            },
            _ => throw new Exception("There should only be 4 walls: 0, 1, 2 ,3")
        };
        return parallelCellCoordinates;
    }

    private static bool IsAnyCoordinateInvalid(IReadOnlyList<PointF> coordinates, PointF[] polygon)
    {
        var enlargedPolygon = PolygonHelper.GetEnlargedPolygon(polygon, -0.001f);

        var hasAnyInvalidWall = false;
        for (var i = 0; i < coordinates.Count; i++)
        {
            var currentCoordinate = coordinates[i];
            var nextCoordinate = coordinates[(i + 1) % coordinates.Count];
            var isPoint1InPolygon = PolygonHelper.IsPointInPolygon(currentCoordinate, enlargedPolygon);
            var isPoint2InPolygon = PolygonHelper.IsPointInPolygon(nextCoordinate, enlargedPolygon);
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
                PolygonHelper.FindIntersection(currentCoordinate, nextCoordinate, currentPolygonPoint,
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

    private static PointF[] GetPredictedCellCoordinates(RectangleF boundingBox, int column, int row)
    {
        var topLeft = new PointF(boundingBox.X + column, boundingBox.Y + row);
        var topRight = new PointF(boundingBox.X + column + 1, boundingBox.Y + row);
        var bottomRight = new PointF(boundingBox.X + column + 1, boundingBox.Y + row + 1);
        var bottomLeft = new PointF(boundingBox.X + column, boundingBox.Y + row + 1);
        var coordinates = new[]
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
        var boundingBox = PolygonHelper.GetBounds(polygon);
        for (var row = 0; row < boundingBox.Height; row++) // For creating cells that fits in a square grid
        {
            for (var column = 0; column < boundingBox.Width; column++)
            {
                var predictedCoordinatesForCell = GetPredictedCellCoordinates(boundingBox, column, row);
                if (IsAnyCoordinateInvalid(predictedCoordinatesForCell, polygon)) // CONDITION
                {
                    continue;
                }

                var membranesWithSameCoordinatesAsCell = new List<Membrane>();
                var newMembranes = new List<Membrane>();
                for (var index = 0; index < predictedCoordinatesForCell.Length; index++)
                {
                    var edge = new[] { predictedCoordinatesForCell[index], predictedCoordinatesForCell[(index + 1) % predictedCoordinatesForCell.Length] };
                    var membrane = membranes.FirstOrDefault(x => x.Coordinates.All(edge.Contains));
                    if (membrane is null)
                    {
                        var isEdgeAnOutline = PolygonHelper.IsEdgeOnPolygon(edge, polygon);
                        var markMembraneAsOutline = isEdgeAnOutline;
                        if (markMembraneAsOutline == false)
                        {
                            var coordinatesOfParallelCell = GetCoordinatesOfParallelCell(index, edge);
                            // checks if parallel neighbour cell walls are out of polygon.
                            var isAnyCoordinateOfParallelCellInvalid =
                                IsAnyCoordinateInvalid(coordinatesOfParallelCell, polygon); // TODO This could be optimized.
                            markMembraneAsOutline = isAnyCoordinateOfParallelCellInvalid;
                        }

                        membrane = new Membrane(edge, markMembraneAsOutline);
                        newMembranes.Add(membrane);
                    }

                    membranesWithSameCoordinatesAsCell.Add(membrane);
                }


                if (membranesWithSameCoordinatesAsCell.All(cellMembrane => cellMembrane.TouchedBy == StaticGameValues.Board))
                    // Skips a cell if it is already captured
                {
                    continue;
                }


                cells.Add(new Cell(predictedCoordinatesForCell));
                membranes.AddRange(newMembranes);
            }
        }

        return (cells, membranes);
    }
}