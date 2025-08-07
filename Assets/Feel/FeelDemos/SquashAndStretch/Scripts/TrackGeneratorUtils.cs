
using System.Collections.Generic;
using UnityEngine;

public static class TrackGeneratorUtils
{
    public static List<Vector3> GenerateControlPoints(Vector3 center, int count, float radius, float noise, float minDistance)
    {
        var points = new List<Vector3>();
        var attempts = 0;

        while (points.Count < count && attempts < 1000)
        {
            var angle = points.Count * Mathf.PI * 2f / count;
            var r = radius + Random.Range(-noise, noise);
            var p = new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r) + center;

            var tooClose = false;
            foreach (var point in points)
            {
                if ((point - p).sqrMagnitude < minDistance * minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                points.Add(p);

            attempts++;
        }

        return points;
    }

    public static List<Vector3> GenerateSmoothedPoints(List<Vector3> controlPoints, float resolution)
    {
        var bezierPoints = new List<Vector3>();
        for (var i = 0; i < controlPoints.Count; i++)
        {
            var p0 = controlPoints[i];
            var p1 = controlPoints[(i + 1) % controlPoints.Count];
            var p2 = controlPoints[(i + 2) % controlPoints.Count];

            var mid1 = Vector3.Lerp(p0, p1, 0.5f);
            var mid2 = Vector3.Lerp(p1, p2, 0.5f);

            for (var t = 0f; t < 1f; t += 1f / resolution)
            {
                var point = QuadraticBezier(mid1, p1, mid2, t);
                bezierPoints.Add(point);
            }
        }

        return bezierPoints;
    }

    static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        var ab = Vector3.Lerp(a, b, t);
        var bc = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(ab, bc, t);
    }

    public static void DrawTurningRadiusGizmos(List<Vector3> points, float thresholdRadius)
    {
#if UNITY_EDITOR
        for (var i = 1; i < points.Count - 1; i++)
        {
            var prev = points[i - 1];
            var current = points[i];
            var next = points[i + 1];

            var v1 = (current - prev).normalized;
            var v2 = (next - current).normalized;

            var angle = Vector3.Angle(v1, v2);
            var turnRadius = Mathf.Abs(1f / Mathf.Max((1f - Mathf.Cos(angle * Mathf.Deg2Rad)), 0.01f));

            if (turnRadius < thresholdRadius)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(current + Vector3.up * 0.2f, 0.5f);
            }
        }
#endif
    }
}
