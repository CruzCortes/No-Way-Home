using UnityEngine;
using System.Collections.Generic;

public class WaterBody : MonoBehaviour
{
    public enum WaterBodyType
    {
        River,
        Lake
    }

    public WaterBodyType type;
    public List<Vector2> points = new List<Vector2>();
    public float width;
    public bool isFrozen = true;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public void Initialize(WaterBodyType bodyType, List<Vector2> pathPoints, float bodyWidth = 0f)
    {
        type = bodyType;
        points = new List<Vector2>(pathPoints);
        width = bodyWidth;

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Add collider for interaction
        if (GetComponent<Collider2D>() == null)
        {
            if (type == WaterBodyType.River)
            {
                EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
                Vector2[] edgePoints = new Vector2[points.Count];
                for (int i = 0; i < points.Count; i++)
                {
                    edgePoints[i] = points[i];
                }
                edgeCollider.points = edgePoints;
            }
            else
            {
                PolygonCollider2D polyCollider = gameObject.AddComponent<PolygonCollider2D>();
                Vector2[] polyPoints = new Vector2[points.Count];
                for (int i = 0; i < points.Count; i++)
                {
                    polyPoints[i] = points[i];
                }
                polyCollider.points = polyPoints;
            }
        }

        UpdateMesh();
    }

    public void UpdateMesh()
    {
        if (meshFilter == null || points.Count < 2) return;

        Mesh mesh = new Mesh();
        if (type == WaterBodyType.River)
        {
            GenerateRiverMesh(mesh);
        }
        else
        {
            GenerateLakeMesh(mesh);
        }

        meshFilter.mesh = mesh;
    }

    private void GenerateRiverMesh(Mesh mesh)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 current = points[i];
            Vector2 next = points[i + 1];
            Vector2 direction = (next - current).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x) * width * 0.5f;

            vertices.Add(new Vector3(current.x + perpendicular.x, current.y + perpendicular.y, 0));
            vertices.Add(new Vector3(current.x - perpendicular.x, current.y - perpendicular.y, 0));
            vertices.Add(new Vector3(next.x + perpendicular.x, next.y + perpendicular.y, 0));
            vertices.Add(new Vector3(next.x - perpendicular.x, next.y - perpendicular.y, 0));

            int vertIndex = i * 4;
            triangles.AddRange(new int[] {
                vertIndex, vertIndex + 2, vertIndex + 1,
                vertIndex + 2, vertIndex + 3, vertIndex + 1
            });

            float uvY = i / (float)(points.Count - 1);
            uvs.Add(new Vector2(0, uvY));
            uvs.Add(new Vector2(1, uvY));
            uvs.Add(new Vector2(0, uvY + 1f / (points.Count - 1)));
            uvs.Add(new Vector2(1, uvY + 1f / (points.Count - 1)));
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }

    private void GenerateLakeMesh(Mesh mesh)
    {
        List<Vector3> vertices = new List<Vector3>();
        foreach (Vector2 point in points)
        {
            vertices.Add(new Vector3(point.x, point.y, 0));
        }

        // Add center point
        Vector3 center = Vector3.zero;
        foreach (Vector3 vertex in vertices)
        {
            center += vertex;
        }
        center /= vertices.Count;
        vertices.Add(center);

        // Generate triangles
        List<int> triangles = new List<int>();
        int centerIndex = vertices.Count - 1;
        for (int i = 0; i < vertices.Count - 1; i++)
        {
            triangles.Add(i);
            triangles.Add((i + 1) % (vertices.Count - 1));
            triangles.Add(centerIndex);
        }

        // Generate UVs
        List<Vector2> uvs = new List<Vector2>();
        float maxSize = 15f; // This should match your lakeMaxSize setting
        foreach (Vector3 vertex in vertices)
        {
            uvs.Add(new Vector2(
                (vertex.x - center.x) / maxSize + 0.5f,
                (vertex.y - center.y) / maxSize + 0.5f
            ));
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }

    public bool ContainsPoint(Vector2 point)
    {
        if (type == WaterBodyType.River)
        {
            // For rivers, check distance to line segments
            for (int i = 0; i < points.Count - 1; i++)
            {
                float distance = DistancePointToLineSegment(point, points[i], points[i + 1]);
                if (distance <= width / 2)
                    return true;
            }
            return false;
        }
        else
        {
            // For lakes, use point in polygon test
            return IsPointInPolygon(point);
        }
    }

    private static float DistancePointToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;
        float length = line.magnitude;
        if (length == 0f)
            return Vector2.Distance(point, lineStart);

        Vector2 normalized = line / length;
        float projection = Vector2.Dot(point - lineStart, normalized);
        projection = Mathf.Clamp(projection, 0f, length);

        Vector2 projectedPoint = lineStart + normalized * projection;
        return Vector2.Distance(point, projectedPoint);
    }

    private bool IsPointInPolygon(Vector2 point)
    {
        bool inside = false;
        for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
        {
            if (((points[i].y > point.y) != (points[j].y > point.y)) &&
                (point.x < (points[j].x - points[i].x) * (point.y - points[i].y) /
                (points[j].y - points[i].y) + points[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
}