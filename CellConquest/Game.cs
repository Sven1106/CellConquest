using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;

namespace CellConquest
{
	public static class StaticGameValues
	{
		public const string NoOne = "noOne";
		public const string Board = "board";
	}

	public class Cell
	{
		public Guid Id { get; } = Guid.NewGuid();
		public string ConqueredBy { get; set; } = StaticGameValues.NoOne;
		public List<Membrane> Membranes { get; } = new();
	}

	public class Membrane
	{
		public Guid Id { get; } = Guid.NewGuid();
		public string TouchedBy { get; set; } = StaticGameValues.NoOne;
		public PointF P1 { get; }
		public PointF P2 { get; }
		public List<Cell> Cells { get; } = new();

		public Membrane(PointF p1, PointF p2, bool isOnOutline) //TODO should the coordinate aspect get removed?
		{
			P1 = p1;
			P2 = p2;
			if (isOnOutline)
			{
				TouchedBy = StaticGameValues.Board;
			}
		}
	}

	public class Board
	{
		private readonly Dictionary<Guid, List<Cell>> _cellsByMembraneId = new();
		private readonly Dictionary<Guid, List<Membrane>> _membranesByCellId;
		public List<PointF> Polygon { get; }
		public List<Cell> Cells { get; }

		public Board(List<PointF> polygon, List<Cell> cells)
		{
			Polygon = polygon;
			Cells = cells;
			CreateCellsByMembraneIdLookUpTable();
			_membranesByCellId = Cells.ToDictionary(x => x.Id, x => x.Membranes);
		}

		private void CreateCellsByMembraneIdLookUpTable()
		{
			foreach (var cell in Cells)
			{
				foreach (var membrane in cell.Membranes)
				{
					if (_cellsByMembraneId.ContainsKey(membrane.Id) == false)
					{
						_cellsByMembraneId.Add(
							membrane.Id,
							new List<Cell>
							{
								cell
							}
						);
					}
					else
					{
						_cellsByMembraneId[membrane.Id].Add(cell);
					}
				}
			}
		}

		public List<Cell> GetCellsConnectedToMembrane(Guid membraneId)
		{
			return _cellsByMembraneId[membraneId];
		}
	}

	public class BoardGenerator
	{
		private static List<Cell> CreateCellsFromPolygon(List<PointF> polygon)
		{
			Dictionary<(PointF, PointF), Membrane> membranesByPoint = new();
			List<Cell> cells = new();
			var polygonHelper = new PolygonHelper(polygon.ToArray());
			var boundingBox = polygonHelper.GetBounds();
			for (var row = 0; row < boundingBox.Height; row++) // For creating cells that fits in a square grid
			{
				for (var column = 0; column < boundingBox.Width; column++)
				{
					var point1 = new PointF(boundingBox.X + column, boundingBox.Y + row);
					var point2 = new PointF(boundingBox.X + column + 1, boundingBox.Y + row);
					var point3 = new PointF(boundingBox.X + column + 1, boundingBox.Y + row + 1);
					var point4 = new PointF(boundingBox.X + column, boundingBox.Y + row + 1);
					var topWall = new PolygonHelper.Line(point1, point2);
					var rightWall = new PolygonHelper.Line(point2, point3);
					var bottomWall = new PolygonHelper.Line(point3, point4);
					var leftWall = new PolygonHelper.Line(point4, point1);
					List<PolygonHelper.Line> walls = new()
					{
						topWall,
						rightWall,
						bottomWall,
						leftWall
					};
					if (walls.Any(line => polygonHelper.IsLineInsidePolygon(line) == false))
					{
						continue;
					}

					Cell cell = new();
					foreach (var wall in walls)
					{
						List<PointF> membraneCoordinates
							= new
									List<PointF> // Order points from lowest to highest to ensure that the key (1,1)(2,1) and (2,1)(1,1) is the key to the same membrane
									{
										wall.P1,
										wall.P2
									}.OrderBy(point => point.X)
									 .ThenBy(point => point.Y)
									 .ToList();
						var key = (membraneCoordinates[0], membraneCoordinates[1]);
						if (membranesByPoint.TryGetValue(key, out Membrane membrane) == false)
						{
							var isOutline = false;
							if (polygonHelper.IsLineHorizontal(wall))
							{
								PolygonHelper.Line wallAbove = new(
									new PointF(wall.P1.X, wall.P1.Y - 1),
									new PointF(wall.P2.X, wall.P2.Y - 1)
								);
								if (polygonHelper.IsLineOutsidePolygon(wallAbove))
								{
									isOutline = true;
								}

								PolygonHelper.Line wallBelow = new(
									new PointF(wall.P1.X, wall.P1.Y + 1),
									new PointF(wall.P2.X, wall.P2.Y + 1)
								);
								if (polygonHelper.IsLineOutsidePolygon(wallBelow))
								{
									isOutline = true;
								}
							}

							if (polygonHelper.IsLineVertical(wall))
							{
								PolygonHelper.Line wallALeft = new(
									new PointF(wall.P1.X - 1, wall.P1.Y),
									new PointF(wall.P2.X - 1, wall.P2.Y)
								);
								if (polygonHelper.IsLineOutsidePolygon(wallALeft))
								{
									isOutline = true;
								}

								PolygonHelper.Line wallARight = new(
									new PointF(wall.P1.X + 1, wall.P1.Y),
									new PointF(wall.P2.X + 1, wall.P2.Y)
								);
								if (polygonHelper.IsLineOutsidePolygon(wallARight))
								{
									isOutline = true;
								}
							}

							membrane = new Membrane(wall.P1, wall.P2, isOutline);
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

		public Board GenerateBoard(List<PointF> polygon)
		{
			List<Cell> cells = CreateCellsFromPolygon(polygon);

			return new Board(polygon, cells);
		}
	}
}