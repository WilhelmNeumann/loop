using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class ProceduralTrackGenerator : Singleton<ProceduralTrackGenerator>
{
    [SerializeField] private Transform _start;
    [SerializeField] private bool _showGizmos = true;

    [Header("Track Settings")] public int controlPointsCount = 12;
    [Range(0.01f, 1f)] public float fenceThickness = 0.2f;
    public float radius = 50f;
    public float noiseStrength = 5f;
    public float trackWidth = 6f;
    public float fenceHeight = 1.5f;
    public int segmentsPerCurve = 10;

    [Header("Mesh Filters")] public MeshFilter trackMeshFilter;
    public MeshFilter leftFenceMeshFilter;
    public MeshFilter rightFenceMeshFilter;

    private readonly List<Vector3> controlPoints = new();
    private readonly List<Vector3> splinePoints = new();

    [ContextMenu("Clear Meshes")]
    public void ClearMeshes()
    {
        controlPoints.Clear();
        splinePoints.Clear();
        trackMeshFilter.mesh.Clear();
        leftFenceMeshFilter.mesh.Clear();
        rightFenceMeshFilter.mesh.Clear();
    }
    
    
    [ContextMenu("Generate Track")]
    public Vector3 GenerateTrack()
    {
        GenerateControlPoints();
        GenerateSplinePoints();
        GenerateMeshes();
        if (_start != null)
            _start.position = transform.position + controlPoints.First();

        return _start.position;
    }

    private void GenerateControlPoints()
    {
        controlPoints.Clear();
        float angleStep = 2 * Mathf.PI / controlPointsCount;

        for (int i = 0; i < controlPointsCount; i++)
        {
            float angle = i * angleStep;
            Vector3 basePoint = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Vector3 radialOffset = basePoint.normalized * Random.Range(-noiseStrength, noiseStrength);
            controlPoints.Add(basePoint + radialOffset);
        }
    }

    private void GenerateSplinePoints()
    {
        splinePoints.Clear();
        int count = controlPoints.Count;

        for (int i = 0; i < count; i++)
        {
            Vector3 p0 = controlPoints[(i - 1 + count) % count];
            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[(i + 1) % count];
            Vector3 p3 = controlPoints[(i + 2) % count];

            for (int j = 0; j < segmentsPerCurve; j++)
            {
                float t = j / (float)segmentsPerCurve;
                splinePoints.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }

        splinePoints.Add(splinePoints[0]); // Ensure loop closure
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    private void GenerateMeshes()
    {
        List<Vector3> trackVerts = new();
        List<int> trackTris = new();

        List<Vector3> leftVerts = new();
        List<int> leftTris = new();

        List<Vector3> rightVerts = new();
        List<int> rightTris = new();

        int count = splinePoints.Count;

        for (int i = 0; i < count; i++)
        {
            var curr = splinePoints[i % count];
            var next = splinePoints[(i + 1) % count];
            var forward = (next - curr).normalized;
            var right = Vector3.Cross(Vector3.up, forward);

            var leftPoint = curr - right * (trackWidth / 2f);
            var rightPoint = curr + right * (trackWidth / 2f);

            var baseIdx = trackVerts.Count;
            trackVerts.Add(leftPoint);
            trackVerts.Add(rightPoint);

            if (i < count - 1)
            {
                var nextBaseIdx = (i == count - 2) ? 0 : baseIdx + 2;
                trackTris.AddRange(new[]
                {
                    baseIdx, nextBaseIdx, baseIdx + 1,
                    baseIdx + 1, nextBaseIdx, nextBaseIdx + 1
                });
            }

            // Fence geometry (full box)
            Vector3 up = Vector3.up * fenceHeight;

            // LEFT FENCE
            int lBase = leftVerts.Count;
            var liBot = leftPoint;
            var liTop = liBot + up;
            var loBot = liBot - right * fenceThickness;
            var loTop = loBot + up;

            leftVerts.AddRange(new[] { liBot, liTop, loBot, loTop });

            if (i < count - 1)
            {
                int n = (i == count - 2) ? 0 : lBase + 4;
                AddFenceTris(leftTris, lBase, n, flip: true);
            }

            // RIGHT FENCE
            int rBase = rightVerts.Count;
            var riBot = rightPoint;
            var riTop = riBot + up;
            var roBot = riBot + right * fenceThickness;
            var roTop = roBot + up;

            rightVerts.AddRange(new[] { riBot, riTop, roBot, roTop });

            if (i < count - 1)
            {
                int n = (i == count - 2) ? 0 : rBase + 4;
                AddFenceTris(rightTris, rBase, n, flip: false);
            }
        }

        trackMeshFilter.sharedMesh = BuildMesh(trackVerts, trackTris);
        leftFenceMeshFilter.sharedMesh = BuildMesh(leftVerts, leftTris);
        rightFenceMeshFilter.sharedMesh = BuildMesh(rightVerts, rightTris);
        
        AssignFenceColliders();
    }

    private void AddFenceTris(List<int> tris, int baseIdx, int nextIdx, bool flip = false)
    {
        if (!flip)
        {
            tris.AddRange(new[]
            {
                // Front
                baseIdx, nextIdx, baseIdx + 1, baseIdx + 1, nextIdx, nextIdx + 1,
                // Back
                baseIdx + 2, baseIdx + 3, nextIdx + 2, baseIdx + 3, nextIdx + 3, nextIdx + 2,
                // Top
                baseIdx + 1, nextIdx + 1, baseIdx + 3, baseIdx + 3, nextIdx + 1, nextIdx + 3,
                // Bottom
                baseIdx, baseIdx + 2, nextIdx, nextIdx, baseIdx + 2, nextIdx + 2,
                // Side 1
                baseIdx, baseIdx + 1, baseIdx + 2, baseIdx + 2, baseIdx + 1, baseIdx + 3,
                // Side 2
                nextIdx, nextIdx + 2, nextIdx + 1, nextIdx + 2, nextIdx + 3, nextIdx + 1
            });
        }
        else
        {
            tris.AddRange(new[]
            {
                // Front
                baseIdx + 1, nextIdx + 1, baseIdx, nextIdx + 1, nextIdx, baseIdx,
                // Back
                nextIdx + 2, baseIdx + 3, baseIdx + 2, nextIdx + 3, baseIdx + 3, nextIdx + 2,
                // Top
                baseIdx + 3, nextIdx + 3, baseIdx + 1, nextIdx + 3, nextIdx + 1, baseIdx + 1,
                // Bottom
                nextIdx, baseIdx + 2, baseIdx, nextIdx + 2, baseIdx + 2, nextIdx,
                // Side 1
                baseIdx + 2, baseIdx + 1, baseIdx, baseIdx + 3, baseIdx + 1, baseIdx + 2,
                // Side 2
                nextIdx + 1, nextIdx + 2, nextIdx, nextIdx + 2, nextIdx + 3, nextIdx
            });
        }
    }


    private Mesh BuildMesh(List<Vector3> verts, List<int> tris)
    {
        var mesh = new Mesh
        {
            vertices = verts.ToArray(),
            triangles = tris.ToArray()
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void OnValidate() => GenerateTrack();

    private void OnDrawGizmos()
    {
        if (!_showGizmos) return;

        Gizmos.color = Color.red;
        foreach (var point in controlPoints)
        {
            Gizmos.DrawSphere(transform.position + point, 1f);
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            Gizmos.DrawLine(transform.position + splinePoints[i], transform.position + splinePoints[i + 1]);
        }
    }
    
    private void AssignFenceColliders()
    {
        AssignMeshCollider(leftFenceMeshFilter);
        AssignMeshCollider(rightFenceMeshFilter);
    }

    private void AssignMeshCollider(MeshFilter meshFilter)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        var collider = meshFilter.GetComponent<MeshCollider>();
        if (collider == null)
            collider = meshFilter.gameObject.AddComponent<MeshCollider>();

        collider.sharedMesh = meshFilter.sharedMesh;
        Debug.Log($"{meshFilter.name} collider assigned with mesh: {meshFilter.sharedMesh?.name}, vertices: {meshFilter.sharedMesh?.vertexCount}");
        collider.convex = false; // Only set to true if needed for Rigidbody interaction
    }
}