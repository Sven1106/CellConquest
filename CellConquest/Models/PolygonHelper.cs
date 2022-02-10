using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace CellConquest.Models;

public static class PolygonHelper
{
    public static RectangleF GetBounds(List<PointF> polygon)
    {
        float biggestX = default;
        float biggestY = default;
        float smallestX = default;
        float smallestY = default;
        for (var i = 0; i < polygon.Count; i++)
        {
            var currentPoint = polygon[i];
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

    public static float GetSlopeOfSegment(LineSegment segment)
    {
        var slopeOfSegment = (segment.Point2.Y - segment.Point1.Y) / (segment.Point2.X - segment.Point1.X);
        return slopeOfSegment;
    }

    // Given three collinear points p, q, r,
    // the function checks if point q lies
    // on line segment 'pr'
    public static bool IsPointOnSegment3(PointF q, LineSegment pr)
    {
        if (q.X <= Math.Max(pr.Point1.X, pr.Point2.X) && q.X >= Math.Min(pr.Point1.X, pr.Point2.X) &&
            q.Y <= Math.Max(pr.Point1.Y, pr.Point2.Y) && q.Y >= Math.Min(pr.Point1.Y, pr.Point2.Y))
        {
            return true;
        }

        return false;
    }

    public static bool IsPointOnSegment(PointF q, LineSegment pr)
    {
        var AB = Math.Sqrt((pr.Point2.X - pr.Point1.X) * (pr.Point2.X - pr.Point1.X) +
                           (pr.Point2.Y - pr.Point1.Y) * (pr.Point2.Y - pr.Point1.Y));
        var AP = Math.Sqrt((q.X - pr.Point1.X) * (q.X - pr.Point1.X) + (q.Y - pr.Point1.Y) * (q.Y - pr.Point1.Y));
        var PB = Math.Sqrt((pr.Point2.X - q.X) * (pr.Point2.X - q.X) + (pr.Point2.Y - q.Y) * (pr.Point2.Y - q.Y));
        return AB == AP + PB;
    }


    // Given two line segments,
    // the function checks if line segment 1 lies
    // on line segment 2
    public static bool IsSegmentOnSegment(LineSegment segment1, LineSegment segment2)
    {
        var areSegmentsCollinear = GetSlopeOfSegment(segment1) == GetSlopeOfSegment(segment2);
        if (areSegmentsCollinear == false)
        {
            return false;
        }

        if (segment1.Point1.X <= Math.Max(segment2.Point1.X, segment2.Point2.X) &&
            segment1.Point1.X >= Math.Min(segment2.Point1.X, segment2.Point2.X) &&
            segment1.Point1.Y <= Math.Max(segment2.Point1.Y, segment2.Point2.Y) &&
            segment1.Point1.Y >= Math.Min(segment2.Point1.Y, segment2.Point2.Y) &&
            segment1.Point2.X <= Math.Max(segment2.Point1.X, segment2.Point2.X) &&
            segment1.Point2.X >= Math.Min(segment2.Point1.X, segment2.Point2.X) &&
            segment1.Point2.Y <= Math.Max(segment2.Point1.Y, segment2.Point2.Y) &&
            segment1.Point2.Y >= Math.Min(segment2.Point1.Y, segment2.Point2.Y)
           )
        {
            return true;
        }

        return false;
    }

    public static bool IsPointOnPolygon(PointF point, PointF[] polygon)
    {
        var isPointOnSegment = false;
        for (var i = 0; i < polygon.Length; i++) //Check if wall is on any polygon segments
        {
            var next = (i + 1) % polygon.Length;
            var polygonSegment = new LineSegment(polygon[i], polygon[next]);

            isPointOnSegment = IsPointOnSegment(point, polygonSegment);
            if (isPointOnSegment)
            {
                break;
            }
        }

        return isPointOnSegment;
    }


    // The function that returns true if
    // line segment 'segment1' and 'segment2' intersect.
    public static bool DoesSegmentsIntersect(LineSegment segment1, LineSegment segment2)
    {
        // Find the four orientations needed for
        // general and special cases
        int o1 = GetOrientation(segment1.Point1, segment1.Point2, segment2.Point1);
        int o2 = GetOrientation(segment1.Point1, segment1.Point2, segment2.Point2);
        int o3 = GetOrientation(segment2.Point1, segment2.Point2, segment1.Point1);
        int o4 = GetOrientation(segment2.Point1, segment2.Point2, segment1.Point2);

        // General case
        if (o1 != o2 && o3 != o4)
        {
            return true;
        }

        // Special Cases
        // segment1.Point1, segment1.Point2 and segment2.Point1 are collinear and
        // segment2.Point1 lies on segment segment1
        if (o1 == 0 && IsPointOnSegment(segment2.Point1, segment1))
        {
            return true;
        }

        // segment1.Point1, segment1.Point2 and segment2.Point1 are collinear and
        // segment2.Point2 lies on segment segment1
        if (o2 == 0 && IsPointOnSegment(segment2.Point2, segment1))
        {
            return true;
        }

        // segment2.Point1, segment2.Point2 and segment1.Point1 are collinear and
        // segment1.Point1 lies on segment segment2
        if (o3 == 0 && IsPointOnSegment(segment1.Point1, segment2))
        {
            return true;
        }

        // segment2.Point1, segment2.Point2 and segment1.Point2 are collinear and
        // segment1.Point2 lies on segment segment2
        if (o4 == 0 && IsPointOnSegment(segment1.Point2, segment2))
        {
            return true;
        }

        // Doesn't fall in any of the above cases
        return false;
    }

    public static bool IsPointInPolygon(PointF pnt, PointF[] polygon)
    {
        var v = GetEnlargedPolygon(polygon, 0.01f);
        var inside = false;
        var len = v.Length;
        for (var i = 0; i < len; i++)
        {
            if (Intersects(v[i], v[(i + 1) % len], pnt))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    public static PointF[] GetEnlargedPolygon(PointF[] old_points, float offset)
    {
        List<PointF> enlarged_points = new();
        int num_points = old_points.Length;
        for (int j = 0; j < num_points; j++)
        {
            // Find the new location for point j.
            // Find the points before and after j.
            int i = (j - 1);
            if (i < 0) i += num_points;
            int k = (j + 1) % num_points;

            // Move the points by the offset.
            Vector2 v1 = new(
                old_points[j].X - old_points[i].X,
                old_points[j].Y - old_points[i].Y);
            v1 = Vector2.Normalize(v1);
            v1 *= offset;
            Vector2 n1 = new(-v1.Y, v1.X);

            PointF pij1 = new(
                old_points[i].X + n1.X,
                old_points[i].Y + n1.Y);
            PointF pij2 = new(
                old_points[j].X + n1.X,
                old_points[j].Y + n1.Y);

            Vector2 v2 = new(
                old_points[k].X - old_points[j].X,
                old_points[k].Y - old_points[j].Y);
            v2 = Vector2.Normalize(v2);
            v2 *= offset;
            Vector2 n2 = new(-v2.Y, v2.X);

            PointF pjk1 = new(
                old_points[j].X + n2.X,
                old_points[j].Y + n2.Y);
            PointF pjk2 = new(
                old_points[k].X + n2.X,
                old_points[k].Y + n2.Y);

            // See where the shifted lines ij and jk intersect.
            bool lines_intersect, segments_intersect;
            PointF poi, close1, close2;
            FindIntersection(pij1, pij2, pjk1, pjk2,
                out lines_intersect, out segments_intersect,
                out poi, out close1, out close2);
            if (lines_intersect) enlarged_points.Add(poi);
        }

        return enlarged_points.ToArray();
    }

    public static bool IsSegmentOnPolygon(LineSegment wall, PointF[] polygon)
    {
        var isSegmentOnPolygon = false;
        for (var i = 0; i < polygon.Length; i++) //Check if wall is on any polygon segments
        {
            var next = (i + 1) % polygon.Length;
            var polygonSegment = new LineSegment(polygon[i], polygon[next]);

            isSegmentOnPolygon = IsSegmentOnSegment(wall, polygonSegment);
            if (isSegmentOnPolygon)
            {
                break;
            }
        }

        return isSegmentOnPolygon;
    }


    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are collinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    private static int GetOrientation(PointF p, PointF q, PointF r)
    {
        var val = (q.Y - p.Y) * (r.X - q.X) -
                  (q.X - p.X) * (r.Y - q.Y);

        if (val == 0)
        {
            return 0; // collinear
        }

        return val > 0 ? 1 : 2; // clock or counterclock wise
    }

    // Find the point of intersection between
    // the lines p1 --> p2 and p3 --> p4.
    private static void FindIntersection(
        PointF p1, PointF p2, PointF p3, PointF p4,
        out bool lines_intersect, out bool segments_intersect,
        out PointF intersection,
        out PointF close_p1, out PointF close_p2)
    {
        // Get the segments' parameters.
        float dx12 = p2.X - p1.X;
        float dy12 = p2.Y - p1.Y;
        float dx34 = p4.X - p3.X;
        float dy34 = p4.Y - p3.Y;

        // Solve for t1 and t2
        float denominator = (dy12 * dx34 - dx12 * dy34);
        bool lines_parallel = (Math.Abs(denominator) < 0.001);

        float t1 =
            ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
            / denominator;
        if (float.IsNaN(t1) || float.IsInfinity(t1))
            lines_parallel = true;

        if (lines_parallel)
        {
            // The lines are parallel (or close enough to it).
            lines_intersect = false;
            segments_intersect = false;
            intersection = new(float.NaN, float.NaN);
            close_p1 = new(float.NaN, float.NaN);
            close_p2 = new(float.NaN, float.NaN);
            return;
        }

        lines_intersect = true;

        float t2 =
            ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
            / -denominator;

        // Find the point of intersection.
        intersection = new(p1.X + dx12 * t1, p1.Y + dy12 * t1);

        // The segments intersect if t1 and t2 are between 0 and 1.
        segments_intersect =
            ((t1 >= 0) && (t1 <= 1) &&
             (t2 >= 0) && (t2 <= 1));

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

        close_p1 = new(p1.X + dx12 * t1, p1.Y + dy12 * t1);
        close_p2 = new(p3.X + dx34 * t2, p3.Y + dy34 * t2);
    }

    private static bool Intersects(PointF A, PointF B, PointF P)
    {
        if (A.Y > B.Y)
            return Intersects(B, A, P);

        if (P.Y == A.Y || P.Y == B.Y)
            P.Y += 0.0001f;

        if (P.Y > B.Y || P.Y < A.Y || P.X >= Math.Max(A.X, B.X))
            return false;

        if (P.X < Math.Min(A.X, B.X))
            return true;

        var red = (P.Y - A.Y) / (P.X - A.X);
        var blue = (B.Y - A.Y) / (B.X - A.X);
        return red >= blue;
    }
}