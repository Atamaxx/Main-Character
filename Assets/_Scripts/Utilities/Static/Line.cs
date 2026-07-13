using System.Collections.Generic;
using UnityEngine;

namespace My
{
    public class Line : MonoBehaviour
    {
        public static float CalculateLength(LineRenderer lineRenderer)
        {
            float length = 0f;
            for (int i = 1; i < lineRenderer.positionCount; i++)
            {
                length += Vector3.Distance(lineRenderer.GetPosition(i - 1), lineRenderer.GetPosition(i));
            }
            return length;
        }
        public static float CalculateLength(List<Vector2> points)
        {
            float length = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                length += Vector2.Distance(points[i - 1], points[i]);
            }
            return length;
        }
        public static Vector3 FindPointByLength(LineRenderer lineRenderer, float length)
        {
            float currLength = 0f;
            float subLength;
            int numberOfPoints = lineRenderer.positionCount;
            for (int i = 1; i < numberOfPoints; i++)
            {
                Vector2 point0 = lineRenderer.GetPosition(i - 1);
                Vector2 point1 = lineRenderer.GetPosition(i);
                subLength = currLength;
                currLength += Vector3.Distance(point0, point1);
                if (currLength > length)
                {
                    return PointByDistance(point0, length - subLength, point1);
                }
            }

            return lineRenderer.GetPosition(numberOfPoints - 1);
        }

        public static Vector2 FindPointByLength(List<Vector2> points, float length)
        {
            float currLength = 0f;
            float subLength;
            int numberOfPoints = points.Count;
            for (int i = 1; i < numberOfPoints; i++)
            {
                Vector2 point0 = points[i - 1];
                Vector2 point1 = points[i];
                subLength = currLength;
                currLength += Vector2.Distance(point0, point1);
                if (currLength > length)
                {
                    return PointByDistance(point0, length - subLength, point1);
                }
            }

            return points[numberOfPoints - 1];
        }
        public static Vector3 FindPointByLength(List<Vector3> points, float length, out int betweenPoint)
        {
            float currLength = 0f;
            float subLength;
            int numberOfPoints = points.Count;
            for (int i = 1; i < numberOfPoints; i++)
            {
                Vector2 point0 = points[i - 1];
                Vector2 point1 = points[i];
                subLength = currLength;
                currLength += Vector3.Distance(point0, point1);
                if (currLength > length)
                {
                    betweenPoint = i;
                    return PointByDistance(point0, length - subLength, point1);
                }
            }
            betweenPoint = numberOfPoints - 1;
            return points[numberOfPoints - 1];
        }
        public static Vector2 PointByDistance(Vector2 startPoint, float distance, Vector2 endPoint)
        {
            float fullDistance = Vector2.Distance(startPoint, endPoint);

            float x0 = startPoint.x;
            float y0 = startPoint.y;
            float x1 = endPoint.x;
            float y1 = endPoint.y;
            float t = distance / fullDistance;

            return new Vector2(x0 - t * x0 + t * x1, (y0 - t * y0 + t * y1));        //(((1?t)x0 + tx1),((1?t)y0 + ty1))
        }
        public static Vector2 PointByPercent(Vector2 startPoint, float percent, Vector2 endPoint)
        {
            if (startPoint == endPoint || percent == 0f)
                return startPoint;
            if (percent == 1)
                return endPoint;

            float fullDistance = Vector2.Distance(startPoint, endPoint);
            float distance = fullDistance * percent;
            float x0 = startPoint.x;
            float y0 = startPoint.y;
            float x1 = endPoint.x;
            float y1 = endPoint.y;
            float t = distance / fullDistance;

            return new Vector2(x0 - t * x0 + t * x1, y0 - t * y0 + t * y1);
        }

        public static float LengthFromStartByPoint(LineRenderer lineRenderer, Vector3 point, int segmentNum)
        {
            float length = 0f;
            for (int i = 1; i <= segmentNum; i++)
            {
                Vector2 point0 = lineRenderer.GetPosition(i - 1);
                Vector2 point1 = lineRenderer.GetPosition(i);
                length += Vector3.Distance(point0, point1);
            }
            length += Vector3.Distance(lineRenderer.GetPosition(segmentNum), point);

            return length;
        }

        // public static float LengthFromStartByPoint(List<Vector2> linePoints, Vector2 point)
        // {
        //     float length = 0f;

        //     for (int i = 1; i < linePoints.Count; i++)
        //     {
        //         Vector2 point0 = linePoints[i - 1];
        //         Vector2 point1 = linePoints[i];

        //         if (point0.x <= point.x && point.x <= point1.x && point0.y <= point.y && point.y <= point1.y
        //             || point1.x <= point.x && point.x <= point0.x && point1.y <= point.y && point.y <= point0.y)
        //         {
        //             length += Vector2.Distance(point0, point);
        //             break;
        //         }


        //         length += Vector2.Distance(point0, point1);
        //     }

        //     return length;
        // }

        public static float LengthFromStartByPoint(List<Vector2> linePoints, Vector2 point)
        {
            if (linePoints == null || linePoints.Count < 2)
                return 0f;

            float totalDistance = 0f;

            for (int i = 1; i < linePoints.Count; i++)
            {
                Vector2 p0 = linePoints[i - 1];
                Vector2 p1 = linePoints[i];
                Vector2 segment = p1 - p0;
                float segmentLength = segment.magnitude;

                // If the segment has zero length, skip it
                if (segmentLength <= Mathf.Epsilon)
                    continue;

                Vector2 p0ToPoint = point - p0;

                // Project p0ToPoint onto the segment (using dot product)
                float dot = Vector2.Dot(segment, p0ToPoint);

                // If dot < 0, projection is "behind" p0
                // If dot > segmentLength^2, projection is "beyond" p1
                // If 0 <= dot <= segmentLength^2, projection is within the segment
                if (dot >= 0 && dot <= segmentLength * segmentLength)
                {
                    // Normalized "t" along the segment (0 to 1)
                    float t = dot / (segmentLength * segmentLength);
                    // Distance along this segment from p0 to projection point
                    float distanceOnSegment = t * segmentLength;

                    // Return total distance so far plus distance on this segment
                    return totalDistance + distanceOnSegment;
                }
                else
                {
                    // The point is not projected on this segment, so add the full segment length and continue
                    totalDistance += segmentLength;
                }
            }

            return totalDistance;
        }

        public static float LengthByPointNum(LineRenderer lineRenderer, int pointNumber)
        {
            float length = 0f;

            for (int i = 1; i <= pointNumber; i++)
            {
                Vector2 point0 = lineRenderer.GetPosition(i - 1);
                Vector2 point1 = lineRenderer.GetPosition(i);
                length += Vector3.Distance(point0, point1);
            }
            return length;
        }

        public static List<Vector2> GetPoints(LineRenderer lineRenderer)
        {
            List<Vector2> points = new();
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                points.Add(lineRenderer.GetPosition(i));
            }
            return points;
        }

        public static Vector2 GetTangent(List<Vector2> linePoints, float distance)
        {
            if (linePoints == null || linePoints.Count < 2)
                return Vector2.zero;

            float accumulatedDistance = 0f;

            for (int i = 0; i < linePoints.Count - 1; i++)
            {
                Vector2 p0 = linePoints[i];
                Vector2 p1 = linePoints[i + 1];
                float segmentLength = Vector2.Distance(p0, p1);

                if (accumulatedDistance + segmentLength >= distance)
                {
                    // The requested distance is within this segment;
                    // the tangent is simply the direction from p0 to p1.
                    return (p1 - p0).normalized;
                }

                accumulatedDistance += segmentLength;
            }

            // If 'distance' is beyond the entire line, 
            // return the direction of the last segment.
            Vector2 lastDir = (linePoints[linePoints.Count - 1] - linePoints[linePoints.Count - 2]).normalized;
            return lastDir;
        }
    }
}