using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace CellConquest
{
	public class PolygonHelper
	{
		public class Line
		{
			public PointF P1 { get; }
			public PointF P2 { get; }

			public Line(PointF p1, PointF p2)
			{
				P1 = p1;
				P2 = p2;
			}
		}

		public PointF[] Polygon { get; }
		private PointF[] PolygonWithOffset { get; }


		public PolygonHelper(PointF[] polygon)
		{
			Polygon = polygon;
			PolygonWithOffset = GetEnlargedPolygon(Polygon.ToList(), -0.1f).ToArray();
		}

		public RectangleF GetBounds()
		{
			float biggestX = default;
			float biggestY = default;
			float smallestX = default;
			float smallestY = default;
			for (var i = 0; i < Polygon.Length; i++)
			{
				var currentPoint = Polygon[i];
				if (i == 0)
				{
					smallestX = biggestX = currentPoint.X;
					smallestY = biggestY = currentPoint.Y;
				}

				if (currentPoint.X > biggestX)
				{
					biggestX = currentPoint.X;
				}

				if (currentPoint.X < smallestX)
				{
					smallestX = currentPoint.X;
				}

				if (currentPoint.Y > biggestY)
				{
					biggestY = currentPoint.Y;
				}

				if (currentPoint.Y < smallestY)
				{
					smallestY = currentPoint.Y;
				}
			}

			var bounds = new RectangleF(smallestX, smallestY, biggestX - smallestX, biggestY - smallestY);
			return bounds;
		}


		public bool IsLineVertical(Line line)
		{
			return IsCoordinatesAtSameAxis(line.P1.X, line.P2.X);
		}

		public bool IsLineHorizontal(Line line)
		{
			return IsCoordinatesAtSameAxis(line.P1.Y, line.P2.Y);
		}


		public bool IsLineOutsidePolygon(Line line)
		{
			var isPoint1InPolygon = IsPointInsidePolygon(line.P1);
			var isPoint2InPolygon = IsPointInsidePolygon(line.P2);
			var isOutside = isPoint1InPolygon == false || isPoint2InPolygon == false;
			return isOutside;
		}

		public bool IsLineInsidePolygon(Line line)
		{
			var isPoint1InPolygon = IsPointInsidePolygon(line.P1);
			var isPoint2InPolygon = IsPointInsidePolygon(line.P2);
			var isIn = isPoint1InPolygon && isPoint2InPolygon;
			return isIn;
		}

		private bool IsCoordinatesAtSameAxis(float coordinate1, float coordinate2)
		{
			const double tolerance = 0.000000001;
			return Math.Abs(coordinate1 - coordinate2) < tolerance;
		}

		private bool IsPointInsidePolygon(PointF point)
		{
			//Ray-cast algorithm is here onward
			int k, j = PolygonWithOffset.Length - 1;
			var isPointInsidePolygon = false;
			for (k = 0; k < PolygonWithOffset.Length; k++)
			{
				//get adjacent points of polygon
				var pointK = PolygonWithOffset[k];
				var pointJ = PolygonWithOffset[j];
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

		private static List<PointF> GetEnlargedPolygon(List<PointF> oldPoints, float offset)
		{
			List<PointF> enlargedPoints = new();
			var numPoints = oldPoints.Count;
			for (var j = 0; j < numPoints; j++)
			{
				// Find the new location for point j.
				// Find the points before and after j.
				var i = (j - 1);
				if (i < 0) i += numPoints;
				var k = (j + 1) % numPoints;

				// Move the points by the offset.
				var v1 = new Vector2(oldPoints[j].X - oldPoints[i].X, oldPoints[j].Y - oldPoints[i].Y);
				v1 = Vector2.Normalize(v1);
				v1 *= offset;
				var n1 = new Vector2(-v1.Y, v1.X);

				var pij1 = new PointF(oldPoints[i].X + n1.X, oldPoints[i].Y + n1.Y);
				var pij2 = new PointF(oldPoints[j].X + n1.X, oldPoints[j].Y + n1.Y);

				var v2 = new Vector2(oldPoints[k].X - oldPoints[j].X, oldPoints[k].Y - oldPoints[j].Y);
				v2 = Vector2.Normalize(v2);
				v2 *= offset;
				var n2 = new Vector2(-v2.Y, v2.X);

				var pjk1 = new PointF(oldPoints[j].X + n2.X, oldPoints[j].Y + n2.Y);
				var pjk2 = new PointF(oldPoints[k].X + n2.X, oldPoints[k].Y + n2.Y);

				// See where the shifted lines ij and jk intersect.
				FindIntersection(pij1, pij2, pjk1, pjk2, out var linesIntersect, out _, out var poi, out _, out _);
				if (linesIntersect) enlargedPoints.Add(poi);
			}

			return enlargedPoints;
		}

		private static void FindIntersection(
			PointF p1,
			PointF p2,
			PointF p3,
			PointF p4,
			out bool linesIntersect,
			out bool segmentsIntersect,
			out PointF intersection,
			out PointF closeP1,
			out PointF closeP2
		)
		{
			// Get the segments' parameters.
			var dx12 = p2.X - p1.X;
			var dy12 = p2.Y - p1.Y;
			var dx34 = p4.X - p3.X;
			var dy34 = p4.Y - p3.Y;

			// Solve for t1 and t2
			var denominator = (dy12 * dx34 - dx12 * dy34);
			var linesParallel = (Math.Abs(denominator) < 0.001);

			var t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
			if (float.IsNaN(t1) ||
			    float.IsInfinity(t1)) linesParallel = true;

			if (linesParallel)
			{
				// The lines are parallel (or close enough to it).
				linesIntersect = false;
				segmentsIntersect = false;
				intersection = new PointF(float.NaN, float.NaN);
				closeP1 = new PointF(float.NaN, float.NaN);
				closeP2 = new PointF(float.NaN, float.NaN);
				return;
			}

			linesIntersect = true;

			var t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

			// Find the point of intersection.
			intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

			// The segments intersect if t1 and t2 are between 0 and 1.
			segmentsIntersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

			// Find the closest points on the segments.
			if (t1 < 0)
			{
				t1 = 0;
			}
			else if (t1 > 1)
			{
				t1 = 1;
			}

			if (t2 < 0)
			{
				t2 = 0;
			}
			else if (t2 > 1)
			{
				t2 = 1;
			}

			closeP1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
			closeP2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
		}
	}
}