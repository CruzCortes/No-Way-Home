using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaterBodySettings
{
    public float riverWidth = 1.5f;
    public float minRiverLength = 10f;
    public float riverWindiness = 0.8f;
    public float lakeMinSize = 5f;
    public float lakeMaxSize = 15f;
    public float lakeFrequency = 0.1f;
}

public class TerrainGenerator : MonoBehaviour
{
    [Header("Water Body Settings")]
    public WaterBodySettings waterSettings;
    public GameObject frozenWaterPrefab;

    // Noise settings for water body generation
    private FastNoiseLite waterNoise;
    private Dictionary<Vector2Int, List<WaterBody>> waterBodiesInChunk = new Dictionary<Vector2Int, List<WaterBody>>();

    // Called before chunk generation in WorldGenerator
    public void InitializeWaterGeneration()
    {
        waterNoise = new FastNoiseLite();
        waterNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        waterNoise.SetFrequency(0.01f);
    }

    public void GenerateWaterBodies(Vector2Int chunkPosition, GameObject chunkObject)
    {
        GameObject waterContainer = new GameObject("WaterContainer");
        waterContainer.transform.parent = chunkObject.transform;

        // Generate rivers
        GenerateRivers(chunkPosition, waterContainer);

        // Generate lakes
        GenerateLakes(chunkPosition, waterContainer);
    }

    private void GenerateRivers(Vector2Int chunkPosition, GameObject container)
    {
        float chunkWorldX = chunkPosition.x * WorldGenerator.Instance.chunkSize * WorldGenerator.Instance.tileSize;
        float chunkWorldY = chunkPosition.y * WorldGenerator.Instance.chunkSize * WorldGenerator.Instance.tileSize;

        // Use noise to determine river start points
        for (int x = 0; x < WorldGenerator.Instance.chunkSize; x++)
        {
            for (int y = 0; y < WorldGenerator.Instance.chunkSize; y++)
            {
                Vector2 worldPos = new Vector2(chunkWorldX + x * WorldGenerator.Instance.tileSize,
                                            chunkWorldY + y * WorldGenerator.Instance.tileSize);

                float riverValue = waterNoise.GetNoise(worldPos.x * 0.1f, worldPos.y * 0.1f);

                if (riverValue > 0.7f) // Threshold for river generation
                {
                    CreateRiver(worldPos, container);
                }
            }
        }
    }

    private void CreateRiver(Vector2 startPos, GameObject container)
    {
        List<Vector2> riverPoints = new List<Vector2>();
        riverPoints.Add(startPos);

        Vector2 currentPos = startPos;
        float angle = Random.Range(0f, 360f);

        for (int i = 0; i < 20; i++) // Maximum river segments
        {
            // Add some meandering to the river
            angle += Random.Range(-waterSettings.riverWindiness, waterSettings.riverWindiness) * 30f;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                                         Mathf.Sin(angle * Mathf.Deg2Rad));

            currentPos += direction * waterSettings.riverWidth;
            riverPoints.Add(currentPos);

            // Check if we should stop the river
            if (Vector2.Distance(startPos, currentPos) > waterSettings.minRiverLength &&
                Random.value < 0.2f)
                break;
        }

        // Create the river mesh
        GameObject riverObject = CreateWaterBodyMesh(riverPoints, waterSettings.riverWidth, container);
        riverObject.name = "River";
    }

    private void GenerateLakes(Vector2Int chunkPosition, GameObject container)
    {
        float chunkWorldX = chunkPosition.x * WorldGenerator.Instance.chunkSize * WorldGenerator.Instance.tileSize;
        float chunkWorldY = chunkPosition.y * WorldGenerator.Instance.chunkSize * WorldGenerator.Instance.tileSize;

        for (int x = 0; x < WorldGenerator.Instance.chunkSize; x += 4)
        {
            for (int y = 0; y < WorldGenerator.Instance.chunkSize; y += 4)
            {
                Vector2 worldPos = new Vector2(chunkWorldX + x * WorldGenerator.Instance.tileSize,
                                            chunkWorldY + y * WorldGenerator.Instance.tileSize);

                float lakeValue = waterNoise.GetNoise(worldPos.x * 0.05f, worldPos.y * 0.05f);

                if (lakeValue > 0.8f) // Threshold for lake generation
                {
                    CreateLake(worldPos, container);
                }
            }
        }
    }

    private void CreateLake(Vector2 center, GameObject container)
    {
        float radius = Random.Range(waterSettings.lakeMinSize, waterSettings.lakeMaxSize);
        int segments = 12;
        List<Vector2> lakePoints = new List<Vector2>();

        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * 360f;
            float radiusVariation = Random.Range(0.8f, 1.2f);
            Vector2 point = center + new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius * radiusVariation,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius * radiusVariation
            );
            lakePoints.Add(point);
        }

        GameObject lakeObject = CreateWaterBodyMesh(lakePoints, 0f, container);
        lakeObject.name = "Lake";
    }

    private GameObject CreateWaterBodyMesh(List<Vector2> points, float width, GameObject container)
    {
        GameObject waterBody = Instantiate(frozenWaterPrefab, Vector3.zero, Quaternion.identity, container.transform);
        MeshFilter meshFilter = waterBody.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = waterBody.GetComponent<MeshRenderer>();

        // Generate mesh based on points
        Mesh mesh = new Mesh();
        if (width > 0) // River
        {
            GenerateRiverMesh(points, width, mesh);
        }
        else // Lake
        {
            GenerateLakeMesh(points, mesh);
        }

        meshFilter.mesh = mesh;
        return waterBody;
    }

    private void GenerateRiverMesh(List<Vector2> points, float width, Mesh mesh)
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
            triangles.AddRange(new int[] { vertIndex, vertIndex + 2, vertIndex + 1,
                                         vertIndex + 2, vertIndex + 3, vertIndex + 1 });

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

    private void GenerateLakeMesh(List<Vector2> points, Mesh mesh)
    {
        // Convert points to Vector3 for the vertices
        List<Vector3> vertices = new List<Vector3>();
        foreach (Vector2 point in points)
        {
            vertices.Add(new Vector3(point.x, point.y, 0));
        }

        // Add center point for triangulation
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
        foreach (Vector3 vertex in vertices)
        {
            uvs.Add(new Vector2((vertex.x - center.x) / waterSettings.lakeMaxSize + 0.5f,
                               (vertex.y - center.y) / waterSettings.lakeMaxSize + 0.5f));
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }
}