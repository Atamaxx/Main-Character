using System.Collections.Generic;
using UnityEngine;

public static class Geometry
{
    public static void DrawCurve(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, LineRenderer lineRenderer, int numberOfPoints)
    {
        lineRenderer.positionCount = numberOfPoints;

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            Vector3 position = CalculateQuadraticBezierPoint(t, startPoint, controlPoint, endPoint);
            lineRenderer.SetPosition(i, position);
        }
    }

    public static List<Vector3> CalculateBezierPoints(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int numberOfPoints)
    {
        List<Vector3> points = new();
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            Vector3 position = CalculateQuadraticBezierPoint(t, startPoint, controlPoint, endPoint);
            points.Add(position);
        }
        return points;
    }

    public static List<Vector3> CalculateBezierLocalPoints(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int numberOfPoints, Transform localRef)
    {
        List<Vector3> points = new();
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            Vector3 position = CalculateQuadraticBezierPoint(t, startPoint, controlPoint, endPoint);
            position = localRef.InverseTransformPoint(position);
            points.Add(position);
        }
        return points;
    }

    private static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float ut = u * t;

        Vector3 p = uu * p0;
        p += 2 * ut * p1;
        p += tt * p2;

        return p;
    }


    public static void PredictNextPoints(List<Vector3> bezierPoints, Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint, int predictionSteps)
    {
        // Get the last known points of the curve
        Vector3 lastPoint = bezierPoints[bezierPoints.Count - 1];
        Vector3 lastControl = controlPoint;
        Vector3 lastEnd = endPoint;

        for (int i = 1; i <= predictionSteps; i++)
        {
            // Extrapolate the next point by extending the last segment
            Vector3 nextPoint = lastEnd + (lastEnd - lastControl);

            bezierPoints.Add(nextPoint);

            // Update the last known points for the next iteration
            lastControl = lastEnd;
            lastEnd = nextPoint;
        }
    }

    public static List<Vector3> GetInterpolatedLine(Vector2 start, Vector2 end, int numberOfPoints)
    {
        List<Vector3> linePoints = new();
        Vector2 vector = end - start;
        for (int i = 0; i <= numberOfPoints; i++)
        {
            Vector3 interpolatedPoint = start + vector * i / numberOfPoints;
            linePoints.Add(interpolatedPoint);
        }

        return linePoints;
    }

    public static List<Vector3> GetInterpolatedCurve(Vector2 point1, Vector2 point2, Vector2 point3, int numberOfPoints)
    {
        List<Vector3> curvePoints = new ();
        
        for (int i = 0; i <= numberOfPoints; i++)
        {
            float t = i / (float)numberOfPoints;
            Vector3 interpolatedPoint = CalculateInterpolatedPoint(point1, point2, point3, t);
            curvePoints.Add(interpolatedPoint);
        }

        return curvePoints;
    }

    private static Vector3 CalculateInterpolatedPoint(Vector2 point1, Vector2 point2, Vector2 point3, float t)
    {
        Vector2 interpolatedPoint = Vector2.Lerp(Vector2.Lerp(point1, point2, t), Vector2.Lerp(point2, point3, t), t);
        return interpolatedPoint;
    }

    public static List<Vector3> GetInterpolatedAndExtrapolatedCurve(Vector2 point1, Vector2 point2, Vector2 point3, int numberOfPoints, int numberOfExtrapolatedPoints)
    {
        List<Vector3> curvePoints = new List<Vector3>();

        // Interpolating existing points
        for (int i = 0; i <= numberOfPoints; i++)
        {
            float t = i / (float)numberOfPoints;
            Vector3 interpolatedPoint = CalculateInterpolatedPoint(point1, point2, point3, t);
            curvePoints.Add(interpolatedPoint);
        }

        // Polynomial fitting
        float[] xs = { 0, 1, 2 }; // X values for the three known points
        float[] ys = { point1.y, point2.y, point3.y }; // Y values for the three known points

        // Fit a polynomial of degree 2 (or higher if needed) using the known points
        float[] coefficients = FitPolynomial(xs, ys, 2); // Change the degree as needed

        // Extrapolate additional points
        float lastX = 2; // X value of the last known point
        for (int i = 1; i <= numberOfExtrapolatedPoints; i++)
        {
            float newX = lastX + i;
            float newY = EvaluatePolynomial(coefficients, newX); // Evaluate the polynomial
            Vector3 extrapolatedPoint = new Vector3(newX, newY, 0); // Creating a new Vector3 for the extrapolated point
            curvePoints.Add(extrapolatedPoint);
        }

        return curvePoints;
    }

    // Function to fit a polynomial using least squares fitting
    private static float[] FitPolynomial(float[] x, float[] y, int degree)
    {
        int n = x.Length;
        int m = degree + 1;

        var matrix = new float[m, m];
        var vector = new float[m];

        for (int row = 0; row < m; row++)
        {
            for (int col = 0; col < m; col++)
            {
                float sum = 0;
                for (int i = 0; i < n; i++)
                {
                    sum += (float)Mathf.Pow(x[i], row + col);
                }
                matrix[row, col] = sum;
            }
            float sumY = 0;
            for (int i = 0; i < n; i++)
            {
                sumY += (float)(y[i] * Mathf.Pow(x[i], row));
            }
            vector[row] = sumY;
        }

        var coefficients = SolveEquation(matrix, vector);
        return coefficients;
    }

    // Function to solve a system of equations
    private static float[] SolveEquation(float[,] matrix, float[] vector)
    {
        int n = vector.Length;

        for (int i = 0; i < n - 1; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                float factor = matrix[j, i] / matrix[i, i];
                for (int k = i; k < n; k++)
                {
                    matrix[j, k] -= factor * matrix[i, k];
                }
                vector[j] -= factor * vector[i];
            }
        }

        float[] result = new float[n];
        for (int i = n - 1; i >= 0; i--)
        {
            float sum = 0;
            for (int j = i + 1; j < n; j++)
            {
                sum += matrix[i, j] * result[j];
            }
            result[i] = (vector[i] - sum) / matrix[i, i];
        }

        return result;
    }

    // Function to evaluate a polynomial given its coefficients and an x value
    private static float EvaluatePolynomial(float[] coefficients, float x)
    {
        float result = 0;
        for (int i = 0; i < coefficients.Length; i++)
        {
            result += coefficients[i] * (float)Mathf.Pow(x, i);
        }
        return result;
    }

    public static bool IsPointInPolygon(Vector2 point, LineRenderer lineRenderer)
    {
        int numPoints = lineRenderer.positionCount;
        Vector3[] vertices = new Vector3[numPoints];
        lineRenderer.GetPositions(vertices);

        bool isInside = false;
        for (int i = 0, j = numPoints - 1; i < numPoints; j = i++)
        {
            Vector3 vi = vertices[i];
            Vector3 vj = vertices[j];

            if (((vi.y > point.y) != (vj.y > point.y)) &&
                (point.x < (vj.x - vi.x) * (point.y - vi.y) / (vj.y - vi.y) + vi.x))
            {
                isInside = !isInside;
            }
        }
        return isInside;
    }

    public static bool IsPointInPolygon(Vector3[] polygon, Vector3 point)
    {
        int polygonLength = polygon.Length;
        bool inside = false;
        Vector3 p1, p2;

        for (int i = 0, j = polygonLength - 1; i < polygonLength; j = i++)
        {
            p1 = polygon[i];
            p2 = polygon[j];

            if (((p1.y > point.y) != (p2.y > point.y)) &&
                (point.x < (p2.x - p1.x) * (point.y - p1.y) / (p2.y - p1.y) + p1.x))
            {
                inside = !inside;
            }
        }

        return inside;
    }

}
