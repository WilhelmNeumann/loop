using System.Collections.Generic;
using UnityEngine;

public static class TrackMath
{
    // Projects a world point onto the centerline (splinePoints). Returns center position, forward dir, segment index and t in [0..1]
    public static Vector3 ClosestOnCenterlineXZ(
        IList<Vector3> centerlineWorld,
        Vector3 worldPoint,
        out int segIndex,
        out float segT,
        out Vector3 forward)
    {
        segIndex = -1;
        segT = 0f;
        var bestSqr = float.PositiveInfinity;
        Vector3 bestPos = Vector3.zero;
        forward = Vector3.forward;

        for (int i = 0; i < centerlineWorld.Count - 1; i++)
        {
            Vector3 a = centerlineWorld[i];
            Vector3 b = centerlineWorld[i + 1];

            // Project in XZ
            Vector2 p = new Vector2(worldPoint.x, worldPoint.z);
            Vector2 A = new Vector2(a.x, a.z);
            Vector2 B = new Vector2(b.x, b.z);
            Vector2 AB = B - A;

            float denom = Vector2.Dot(AB, AB);
            float u = denom > 1e-6f ? Vector2.Dot(p - A, AB) / denom : 0f;
            u = Mathf.Clamp01(u);

            Vector3 proj = Vector3.Lerp(a, b, u);
            float sqr = (new Vector2(proj.x, proj.z) - p).sqrMagnitude;

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                bestPos = proj;
                segIndex = i;
                segT = u;
                forward = (b - a).normalized;
            }
        }

        return bestPos;
    }
}