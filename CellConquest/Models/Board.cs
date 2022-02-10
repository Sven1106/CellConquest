using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CellConquest.Models;

public class Board
{
    private readonly Dictionary<Guid, List<Cell>> _cellsByMembraneId;
    private readonly Dictionary<Guid, List<Membrane>> _membranesByCellId;
    public PointF[] Outline { get; }
    public List<Cell> Cells { get; }

    public Board(PointF[] outline)
    {
        Outline = outline;
        Cells = CreateCellsFromPolygon(Outline);
        _cellsByMembraneId = CreateCellsByMembraneIdLookUpTable(Cells);
        _membranesByCellId = Cells.ToDictionary(x => x.Id, x => x.Membranes);
    }

    private static List<Cell> CreateCellsFromPolygon(PointF[] polygon)
    {
        Dictionary<(PointF, PointF), Membrane> membranesByPoint = new();
        List<Cell> cells = new();
        var boundingBox = PolygonHelper.GetBounds(polygon.ToList());
        for (var row = 0; row < boundingBox.Height; row++) // For creating cells that fits in a square grid
        {
            for (var column = 0; column < boundingBox.Width; column++)
            {
                var topLeft = new PointF(boundingBox.X + column, boundingBox.Y + row);
                var topRight = new PointF(boundingBox.X + column + 1, boundingBox.Y + row);
                var bottomRight = new PointF(boundingBox.X + column + 1, boundingBox.Y + row + 1);
                var bottomLeft = new PointF(boundingBox.X + column, boundingBox.Y + row + 1);
                var topWall = new LineSegment(topLeft, topRight);
                var rightWall = new LineSegment(topRight, bottomRight);
                var bottomWall = new LineSegment(bottomRight, bottomLeft);
                var leftWall = new LineSegment(bottomLeft, topLeft);
                List<LineSegment> walls = new()
                {
                    topWall,
                    rightWall,
                    bottomWall,
                    leftWall
                };

                var hasAnyInvalidWall = false;
                // Check if one wall point is not in polygon
                // Check if wall is not
                foreach (var wall in walls)
                {
                    var isPoint1InPolygon = PolygonHelper.IsPointInPolygon(wall.Point1, polygon);
                    var isPoint2InPolygon = PolygonHelper.IsPointInPolygon(wall.Point2, polygon);
                    var isPoint1OnPolygon = PolygonHelper.IsPointOnPolygon(wall.Point1, polygon);
                    var isPoint2OnPolygon = PolygonHelper.IsPointOnPolygon(wall.Point2, polygon);
                    if (isPoint1InPolygon && isPoint2InPolygon ||
                        isPoint1InPolygon && isPoint2OnPolygon ||
                        isPoint2InPolygon && isPoint1OnPolygon
                       )
                    {
                        continue;
                    }

                    var isWallOnPolygon = PolygonHelper.IsSegmentOnPolygon(wall, polygon);

                    if (isWallOnPolygon)
                    {
                        continue;
                    }

                    var midPoint = new PointF(
                        (wall.Point1.X + wall.Point2.X) / 2,
                        (wall.Point1.Y + wall.Point2.Y) / 2
                    );
                    var isMidPointInPolygon = PolygonHelper.IsPointInPolygon(midPoint, polygon);
                    hasAnyInvalidWall = isMidPointInPolygon == false;
                    if (hasAnyInvalidWall)
                    {
                        break;
                    }
                }

                if (hasAnyInvalidWall)
                {
                    continue;
                }


                Cell cell = new();
                for (var i = 0; i < walls.Count; i++)
                {
                    var wall = walls[i];
                    // Order points from lowest to highest to ensure that the key (1,1)(2,1) and (2,1)(1,1) is the key to the same membrane
                    var membraneCoordinates = new List<PointF> { wall.Point1, wall.Point2 }
                        .OrderBy(point => point.X)
                        .ThenBy(point => point.Y)
                        .ToList();
                    var key = (membraneCoordinates[0], membraneCoordinates[1]);
                    if (membranesByPoint.TryGetValue(key, out var membrane) == false)
                    {
                        var isWallAnOutline = PolygonHelper.IsSegmentOnPolygon(wall, polygon);
                        if (isWallAnOutline == false)
                        {
                            // TODO Get wall direction
                            var parallelCellWalls = i switch
                            {
                                0 => new List<LineSegment> // above
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
                                1 => new List<LineSegment> // to the right
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
                                2 => new List<LineSegment> // below
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
                                3 => new List<LineSegment> // to the left
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
                                _ => throw new Exception("There are more than 4 walls.")
                            };
                            // check if parallel neighbour wall is out of polygon.

                            var hasAnyInvalidWall1 = false;
                            // Check if one wall point is not in polygon
                            // Check if wall is not
                            foreach (var cellWall in parallelCellWalls)
                            {
                                var isPoint1InPolygon = PolygonHelper.IsPointInPolygon(cellWall.Point1, polygon);
                                var isPoint2InPolygon = PolygonHelper.IsPointInPolygon(cellWall.Point2, polygon);
                                var isPoint1OnPolygon = PolygonHelper.IsPointOnPolygon(cellWall.Point1, polygon);
                                var isPoint2OnPolygon = PolygonHelper.IsPointOnPolygon(cellWall.Point2, polygon);
                                if (isPoint1InPolygon && isPoint2InPolygon ||
                                    isPoint1InPolygon && isPoint2OnPolygon ||
                                    isPoint2InPolygon && isPoint1OnPolygon
                                   )
                                {
                                    hasAnyInvalidWall1 = false;
                                }
                                else
                                {
                                    var isWallOnPolygon = PolygonHelper.IsSegmentOnPolygon(cellWall, polygon);

                                    if (isWallOnPolygon)
                                    {
                                        hasAnyInvalidWall1 = false;
                                    }
                                    else
                                    {
                                        var midPoint = new PointF(
                                            (cellWall.Point1.X + cellWall.Point2.X) / 2,
                                            (cellWall.Point1.Y + cellWall.Point2.Y) / 2
                                        );
                                        var isMidPointInPolygon = PolygonHelper.IsPointInPolygon(midPoint, polygon);
                                        hasAnyInvalidWall1 = isMidPointInPolygon == false;
                                    }
                                }

                                if (hasAnyInvalidWall1)
                                {
                                    break;
                                }
                            }

                            isWallAnOutline = hasAnyInvalidWall1;
                        }

                        membrane = new Membrane(wall.Point1, wall.Point2, isWallAnOutline);
                        membranesByPoint.Add(key, membrane);
                    }

                    cell.Membranes.Add(membrane);
                    membrane.Cells.Add(cell);
                }

                cells.Add(cell);
            }
        }

        return cells;
    }

    private static Dictionary<Guid, List<Cell>> CreateCellsByMembraneIdLookUpTable(List<Cell> cells)
    {
        Dictionary<Guid, List<Cell>> cellsByMembraneId = new();
        foreach (var cell in cells)
        {
            foreach (var membrane in cell.Membranes)
            {
                if (cellsByMembraneId.ContainsKey(membrane.Id) == false)
                {
                    cellsByMembraneId.Add(
                        membrane.Id,
                        new List<Cell>
                        {
                            cell
                        }
                    );
                }
                else
                {
                    cellsByMembraneId[membrane.Id].Add(cell);
                }
            }
        }

        return cellsByMembraneId;
    }

    public List<Cell> GetCellsConnectedToMembrane(Guid membraneId)
    {
        return _cellsByMembraneId[membraneId];
    }
}