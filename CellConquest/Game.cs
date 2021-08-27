using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
		public string ConqueredBy { get; private set; } = StaticGameValues.NoOne;
	}

	public class BoardLayout
	{
		public Point[] Polygon { get; }
		public List<Cell> Cells { get; }

		public BoardLayout(Point[] polygon)
		{
			Polygon = polygon;

			Cells = CreateCells(Polygon);
		}

		private static bool IsPointInsidePolygon(IReadOnlyList<Point> polygon, Point point)
		{
			int k, j = polygon.Count - 1;
			var isPointInsidePolygon = false;
			for (k = 0; k < polygon.Count; k++)
			{
				//get adjacent points of polygon
				var pointK = polygon[k];
				var pointJ = polygon[j];
				//check the intersections
				if ((pointK.Y > point.Y) != (pointJ.Y > point.Y) &&
				    (point.X < (pointJ.X - pointK.X) * (point.Y - pointK.Y) / (pointJ.Y - pointK.Y) + pointK.X))
				{
					isPointInsidePolygon = !isPointInsidePolygon; //switch between odd and even
				}

				j = k;
			}

			return isPointInsidePolygon;
		}

		private static List<Cell> CreateCells(Point[] polygon)
		{
			List<Cell> cells = new List<Cell>();
			BoundingBox boundingBox = CreateBoundingBox(polygon);
			var size = GetSize(boundingBox);
			for (int i = 0; i < size.Width; i++)
			{
			}

			IsPointInsidePolygon(polygon, new Point());
			return cells;
		}

		public static Size GetSize(BoundingBox boundingBox)
		{
			return new Size(boundingBox.HighestX - boundingBox.LowestX, boundingBox.HighestY - boundingBox.LowestY);
		}


		public static BoundingBox CreateBoundingBox(Point[] polygon)
		{
			var highestX = 0;
			var lowestX = 0;
			var highestY = 0;
			var lowestY = 0;
			for (var i = 0; i < polygon.Length; i++)
			{
				var point = polygon[i];
				if (i == 0)
				{
					highestX = point.X;
					highestY = point.Y;
					lowestX = point.X;
					lowestY = point.Y;
					continue;
				}

				if (point.X > highestX)
				{
					highestX = point.X;
				}

				if (point.Y > highestY)
				{
					highestY = point.Y;
				}

				if (point.X < lowestX)
				{
					lowestX = point.X;
				}

				if (point.Y < lowestY)
				{
					lowestY = point.Y;
				}
			}

			return new BoundingBox(highestX, highestY, lowestX, lowestY);
		}
	}

	public class BoundingBox
	{
		public int HighestX { get; }
		public int HighestY { get; }
		public int LowestX { get; }
		public int LowestY { get; }

		public BoundingBox(int highestX, int highestY, int lowestX, int lowestY)
		{
			HighestX = highestX;
			HighestY = highestY;
			LowestX = lowestX;
			LowestY = lowestY;
		}
	}
}