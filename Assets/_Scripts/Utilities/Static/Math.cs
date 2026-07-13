using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace My
{
    public static class Math
    {
        public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            int polygonLength = polygon.Count;
            bool isInside = false;

            Vector2 p1, p2;
            for (int i = 0, j = polygonLength - 1; i < polygonLength; j = i++)
            {
                p1 = polygon[i];
                p2 = polygon[j];

                if ((p1.y > point.y) != (p2.y > point.y) &&
                    (point.x < (p2.x - p1.x) * (point.y - p1.y) / (p2.y - p1.y) + p1.x))
                {
                    isInside = !isInside;
                }
            }

            return isInside;
        }


        public static float TriangleAreaByCoord(List<Vector2> vertexPoints)
        {
            float area;
            float x1, x2, x3;
            float y1, y2, y3;

            x1 = vertexPoints[0].x;
            x2 = vertexPoints[1].x;
            x3 = vertexPoints[2].x;
            y1 = vertexPoints[0].y;
            y2 = vertexPoints[1].y;
            y3 = vertexPoints[2].y;

            area = (x1 * y2 - x1 * y3 + x2 * y3 - x2 * y1 + x3 * y1 - x3 * y2) / 2;

            return area;
        }

        public static float CalculatePolygonArea(List<Vector2> polygon)
        {
            int numVertices = polygon.Count;
            float area = 0f;

            for (int i = 0; i < numVertices; i++)
            {
                Vector2 currentVertex = polygon[i];
                Vector2 nextVertex = polygon[(i + 1) % numVertices];

                area += (currentVertex.x * nextVertex.y) - (currentVertex.y * nextVertex.x);
            }

            area *= 0.5f;
            return Mathf.Abs(area);
        }

        public static float CircleRadiusByArea(float desiredArea)
        {
            float radius = Mathf.Sqrt(desiredArea / Mathf.PI);
            return radius;
        }


        public static bool ArePointsOnSameLine(List<Vector2> points)
        {
            if (points.Count < 3)
                return false;

            float slopeRef = (points[1].y - points[0].y) / (points[1].x - points[0].x);

            for (int i = 2; i < points.Count; i++)
            {
                float slope = (points[i].y - points[0].y) / (points[i].x - points[0].x);

                if (!Mathf.Approximately(slopeRef, slope))
                    return false;
            }

            return true;
        }

        public static IEnumerator LerpRoutine(float num1, float num2, float duration, 
        System.Action<float> onValueChanged, System.Action onComplete)
        {
            float timePassed = 0;
            onValueChanged?.Invoke(num1);

            while (timePassed < duration)
            {
                float t = timePassed / duration;
                float currentValue = Mathf.Lerp(num1, num2, t);
                onValueChanged?.Invoke(currentValue);

                timePassed += Time.deltaTime;
                yield return null;
            }
            onValueChanged?.Invoke(num2);

            onComplete?.Invoke();
        }


        public static IEnumerator LerpRoutine(float num1, float num2, float duration,
            System.Action<float> onValueChanged)
        {
            float timePassed = 0;
            onValueChanged?.Invoke(num1);

            while (timePassed < duration)
            {
                float t = timePassed / duration;
                float currentValue = Mathf.Lerp(num1, num2, t);
                onValueChanged?.Invoke(currentValue);

                timePassed += Time.deltaTime;
                yield return null;
            }
            onValueChanged?.Invoke(num2);
        }

    }



}