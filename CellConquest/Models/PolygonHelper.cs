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

    public static bool IsPointInPolygon(PointF point, PointF[] polygon)
    {
        // Get the angle between the point and the
        // first and last vertices.
        var max_point = polygon.Length - 1;
        var total_angle = GetAngle(
            polygon[max_point].X, polygon[max_point].Y,
            point.X, point.Y,
            polygon[0].X, polygon[0].Y);

        // Add the angles from the point
        // to each other pair of vertices.
        for (var i = 0; i < max_point; i++)
        {
            total_angle += GetAngle(
                polygon[i].X, polygon[i].Y,
                point.X, point.Y,
                polygon[i + 1].X, polygon[i + 1].Y);
        }

        // The total angle should be 2 * PI or -2 * PI if
        // the point is in the polygon and close to zero
        // if the point is outside the polygon.
        return (Math.Abs(total_angle) > 1);
    }

    // Return the angle ABC.
    // Return a value between PI and -PI.
    // Note that the value is the opposite of what you might
    // expect because Y coordinates increase downward.
    public static float GetAngle(float Ax, float Ay, float Bx, float By, float Cx, float Cy)
    {
        // Get the dot product.
        var dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);

        // Get the cross product.
        var cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

        // Calculate the angle.
        return (float)Math.Atan2(cross_product, dot_product);
    }


    // Return the cross product AB x BC.
    // The cross product is a vector perpendicular to AB
    // and BC having length |AB| * |BC| * Sin(theta) and
    // with direction given by the right-hand rule.
    // For two vectors in the X-Y plane, the result is a
    // vector with X and Y components 0 so the Z component
    // gives the vector's length and direction.
    public static float CrossProductLength(float Ax, float Ay, float Bx, float By, float Cx, float Cy)
    {
        // Get the vectors' coordinates.
        var BAx = Ax - Bx;
        var BAy = Ay - By;
        var BCx = Cx - Bx;
        var BCy = Cy - By;

        // Calculate the Z coordinate of the cross product.
        return (BAx * BCy - BAy * BCx);
    }

    // Return the dot product AB · BC.
    // Note that AB · BC = |AB| * |BC| * Cos(theta).
    private static float DotProduct(float Ax, float Ay, float Bx, float By, float Cx, float Cy)
    {
        // Get the vectors' coordinates.
        var BAx = Ax - Bx;
        var BAy = Ay - By;
        var BCx = Cx - Bx;
        var BCy = Cy - By;

        // Calculate the dot product.
        return (BAx * BCx + BAy * BCy);
    }

    // Return points representing an enlarged polygon.
    public static PointF[] GetEnlargedPolygon(PointF[] old_points, float offset)
    {
        List<PointF> enlarged_points = new();
        var num_points = old_points.Length;
        for (var j = 0; j < num_points; j++)
        {
            // Find the new location for point j.
            // Find the points before and after j.
            var i = (j - 1);
            if (i < 0) i += num_points;
            var k = (j + 1) % num_points;

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

    // Find the point of intersection between
    // the lines p1 --> p2 and p3 --> p4.
    public static void FindIntersection(
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
            intersection = new PointF(float.NaN, float.NaN);
            close_p1 = new PointF(float.NaN, float.NaN);
            close_p2 = new PointF(float.NaN, float.NaN);
            return;
        }

        lines_intersect = true;

        float t2 =
            ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
            / -denominator;

        // Find the point of intersection.
        intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

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

        close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
        close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
    }
}