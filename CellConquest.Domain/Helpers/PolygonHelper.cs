using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using CellConquest.Domain.Models;
using CellConquest.Domain.ValueObjects;

namespace CellConquest.Domain.Helpers;

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

    // Return points representing an enlarged polygon.
    public static PointF[] GetEnlargedPolygon(PointF[] oldPoints, float offset)
    {
        List<PointF> enlargedPoints = new();
        var numPoints = oldPoints.Length;
        for (var j = 0; j < numPoints; j++)
        {
            // Find the new location for point j.
            // Find the points before and after j.
            var i = j - 1;
            if (i < 0) i += numPoints;
            var k = (j + 1) % numPoints;

            // Move the points by the offset.
            Vector2 v1 = new(
                oldPoints[j].X - oldPoints[i].X,
                oldPoints[j].Y - oldPoints[i].Y
            );
            v1 = Vector2.Normalize(v1);
            v1 *= offset;
            Vector2 n1 = new(-v1.Y, v1.X);

            PointF pij1 = new(
                oldPoints[i].X + n1.X,
                oldPoints[i].Y + n1.Y
            );
            PointF pij2 = new(
                oldPoints[j].X + n1.X,
                oldPoints[j].Y + n1.Y
            );

            Vector2 v2 = new(
                oldPoints[k].X - oldPoints[j].X,
                oldPoints[k].Y - oldPoints[j].Y
            );
            v2 = Vector2.Normalize(v2);
            v2 *= offset;
            Vector2 n2 = new(-v2.Y, v2.X);

            PointF pjk1 = new(
                oldPoints[j].X + n2.X,
                oldPoints[j].Y + n2.Y
            );
            PointF pjk2 = new(
                oldPoints[k].X + n2.X,
                oldPoints[k].Y + n2.Y
            );

            // See where the shifted lines ij and jk intersect.
            FindIntersection(pij1, pij2, pjk1, pjk2,
                out var linesIntersect, out _,
                out var poi);
            if (linesIntersect) enlargedPoints.Add(poi);
        }

        return enlargedPoints.ToArray();
    }

    // Find the point of intersection between
    // the lines p1 --> p2 and p3 --> p4.
    public static void FindIntersection(
        PointF p1, PointF p2, PointF p3, PointF p4,
        out bool linesIntersect, out bool segmentsIntersect,
        out PointF intersection
    )
    {
        // Get the segments' parameters.
        var dx12 = p2.X - p1.X;
        var dy12 = p2.Y - p1.Y;
        var dx34 = p4.X - p3.X;
        var dy34 = p4.Y - p3.Y;

        // Solve for t1 and t2
        var denominator = dy12 * dx34 - dx12 * dy34;
        var linesParallel = Math.Abs(denominator) < 0.001;

        var t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
        if (float.IsNaN(t1) || float.IsInfinity(t1))
        {
            linesParallel = true;
        }

        if (linesParallel)
        {
            // The lines are parallel (or close enough to it).
            linesIntersect = false;
            segmentsIntersect = false;
            intersection = new PointF(float.NaN, float.NaN);
            new PointF(float.NaN, float.NaN);
            new PointF(float.NaN, float.NaN);
            return;
        }

        linesIntersect = true;

        var t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

        // Find the point of intersection.
        intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

        // The segments intersect if t1 and t2 are between 0 and 1.
        segmentsIntersect =
            t1 >= 0 && t1 <= 1 &&
            t2 >= 0 && t2 <= 1;

        t1 = t1 switch
        {
            // Find the closest points on the segments.
            < 0 => 0,
            > 1 => 1,
            _ => t1
        };

        t2 = t2 switch
        {
            < 0 => 0,
            > 1 => 1,
            _ => t2
        };

        new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
        new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
    }

    public static bool IsSegmentOnPolygon(Wall wall, PointF[] polygon)
    {
        bool IsSegmentOnSegment(Wall segment1, Wall segment2)
        {
            float GetSlopeOfSegment(Wall segment) =>
                (segment.Point2.Y - segment.Point1.Y) / (segment.Point2.X - segment.Point1.X);

            var areSegmentsCollinear = GetSlopeOfSegment(segment1) == GetSlopeOfSegment(segment2);
            if (areSegmentsCollinear == false)
            {
                return false;
            }

            return segment1.Point1.X <= Math.Max(segment2.Point1.X, segment2.Point2.X) &&
                   segment1.Point1.X >= Math.Min(segment2.Point1.X, segment2.Point2.X) &&
                   segment1.Point1.Y <= Math.Max(segment2.Point1.Y, segment2.Point2.Y) &&
                   segment1.Point1.Y >= Math.Min(segment2.Point1.Y, segment2.Point2.Y) &&
                   segment1.Point2.X <= Math.Max(segment2.Point1.X, segment2.Point2.X) &&
                   segment1.Point2.X >= Math.Min(segment2.Point1.X, segment2.Point2.X) &&
                   segment1.Point2.Y <= Math.Max(segment2.Point1.Y, segment2.Point2.Y) &&
                   segment1.Point2.Y >= Math.Min(segment2.Point1.Y, segment2.Point2.Y);
        }

        var isSegmentOnPolygon = false;
        for (var i = 0; i < polygon.Length; i++) //Check if wall is on any polygon segments
        {
            var next = (i + 1) % polygon.Length;
            var polygonSegment = new Wall(polygon[i], polygon[next]);

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
        var maxPoint = polygon.Length - 1;
        var totalAngle = GetAngle(
            polygon[maxPoint].X, polygon[maxPoint].Y,
            point.X, point.Y,
            polygon[0].X, polygon[0].Y);

        // Add the angles from the point
        // to each other pair of vertices.
        for (var i = 0; i < maxPoint; i++)
        {
            totalAngle += GetAngle(
                polygon[i].X, polygon[i].Y,
                point.X, point.Y,
                polygon[i + 1].X, polygon[i + 1].Y);
        }

        // The total angle should be 2 * PI or -2 * PI if
        // the point is in the polygon and close to zero
        // if the point is outside the polygon.
        return Math.Abs(totalAngle) > 1;
    }

    // Return the angle ABC.
    // Return a value between PI and -PI.
    // Note that the value is the opposite of what you might
    // expect because Y coordinates increase downward.
    private static float GetAngle(float ax, float ay, float bx, float by, float cx, float cy)
    {
        // Get the dot product.
        var dotProduct = DotProduct(ax, ay, bx, by, cx, cy);

        // Get the cross product.
        var crossProduct = CrossProductLength(ax, ay, bx, by, cx, cy);

        // Calculate the angle.
        return (float)Math.Atan2(crossProduct, dotProduct);
    }

    // Return the cross product AB x BC.
    // The cross product is a vector perpendicular to AB
    // and BC having length |AB| * |BC| * Sin(theta) and
    // with direction given by the right-hand rule.
    // For two vectors in the X-Y plane, the result is a
    // vector with X and Y components 0 so the Z component
    // gives the vector's length and direction.
    private static float CrossProductLength(float ax, float ay, float bx, float @by, float cx, float cy)
    {
        // Get the vectors' coordinates.
        var bAx = ax - bx;
        var bAy = ay - @by;
        var bCx = cx - bx;
        var bCy = cy - @by;

        // Calculate the Z coordinate of the cross product.
        return bAx * bCy - bAy * bCx;
    }

    // Return the dot product AB · BC.
    // Note that AB · BC = |AB| * |BC| * Cos(theta).
    private static float DotProduct(float ax, float ay, float bx, float @by, float cx, float cy)
    {
        // Get the vectors' coordinates.
        var bAx = ax - bx;
        var bAy = ay - @by;
        var bCx = cx - bx;
        var bCy = cy - @by;

        // Calculate the dot product.
        return bAx * bCx + bAy * bCy;
    }
}