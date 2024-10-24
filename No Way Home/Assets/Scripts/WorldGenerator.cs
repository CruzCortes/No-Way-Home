using UnityEngine;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    public GameObject groundPrefab;
    public GameObject playerPrefab;
    public Camera mainCamera;
    public Transform worldContainer;
    public float tileSize = 1f;
    private int chunkSize = 16;
    private Vector2Int lastPlayerChunk;
    private Dictionary<Vector2Int, GameObject> generatedChunks = new Dictionary<Vector2Int, GameObject>();
    private GameObject player;

    // Number of chunks beyond the camera view to load
    private int chunkLoadRadius = 2;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (worldContainer == null)
            worldContainer = transform;

        SetupPlayer();
        UpdateChunks();
    }

    void Update()
    {
        Vector2Int currentPlayerChunk = GetCurrentChunk(player.transform.position);

        if (currentPlayerChunk != lastPlayerChunk)
        {
            UpdateChunks();
            lastPlayerChunk = currentPlayerChunk;
        }
    }

    void SetupPlayer()
    {
        player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = Vector3.zero;
            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.target = player.transform;
            }
        }
        else
        {
            Debug.LogError("Player object not found in the scene. Make sure there's a GameObject named 'Player' in the hierarchy.");
        }
    }

    void UpdateChunks()
    {
        // Get camera bounds
        Bounds cameraBounds = GetCameraBounds();

        // Determine chunks to generate
        HashSet<Vector2Int> chunksToGenerate = GetChunksInBounds(cameraBounds, chunkLoadRadius);

        // Remove chunks not in view
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (Vector2Int chunkPos in generatedChunks.Keys)
        {
            if (!chunksToGenerate.Contains(chunkPos))
            {
                chunksToRemove.Add(chunkPos);
            }
        }
        foreach (Vector2Int chunkPos in chunksToRemove)
        {
            Destroy(generatedChunks[chunkPos]);
            generatedChunks.Remove(chunkPos);
        }

        // Generate new chunks
        foreach (Vector2Int chunkPos in chunksToGenerate)
        {
            if (!generatedChunks.ContainsKey(chunkPos))
            {
                GenerateChunk(chunkPos);
            }
        }
    }

    Bounds GetCameraBounds()
    {
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        Vector3 cameraCenter = mainCamera.transform.position;

        return new Bounds(cameraCenter, new Vector3(cameraWidth, cameraHeight, 0f));
    }

    HashSet<Vector2Int> GetChunksInBounds(Bounds bounds, int radius)
    {
        HashSet<Vector2Int> chunks = new HashSet<Vector2Int>();

        float minX = bounds.min.x - radius * chunkSize * tileSize;
        float maxX = bounds.max.x + radius * chunkSize * tileSize;
        float minY = bounds.min.y - radius * chunkSize * tileSize;
        float maxY = bounds.max.y + radius * chunkSize * tileSize;

        int startX = Mathf.FloorToInt(minX / (chunkSize * tileSize));
        int endX = Mathf.FloorToInt(maxX / (chunkSize * tileSize));
        int startY = Mathf.FloorToInt(minY / (chunkSize * tileSize));
        int endY = Mathf.FloorToInt(maxY / (chunkSize * tileSize));

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                chunks.Add(new Vector2Int(x, y));
            }
        }
        return chunks;
    }

    Vector2Int GetCurrentChunk(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / (chunkSize * tileSize)),
            Mathf.FloorToInt(position.y / (chunkSize * tileSize))
        );
    }

    void GenerateChunk(Vector2Int chunkPosition)
    {
        // Create chunk parent object
        GameObject chunkObject = new GameObject($"Chunk_{chunkPosition.x}_{chunkPosition.y}");
        chunkObject.transform.parent = worldContainer;
        chunkObject.transform.position = new Vector3(chunkPosition.x * chunkSize * tileSize, chunkPosition.y * chunkSize * tileSize, 0);

        // Generate tiles within the chunk
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                Vector2Int tilePosition = new Vector2Int(
                    chunkPosition.x * chunkSize + x,
                    chunkPosition.y * chunkSize + y
                );

                Vector3 worldPosition = new Vector3(tilePosition.x * tileSize, tilePosition.y * tileSize, 0);
                GameObject tile = Instantiate(groundPrefab, worldPosition, Quaternion.identity, chunkObject.transform);
                tile.name = $"GroundTile_{tilePosition.x}_{tilePosition.y}";

                long seed = tilePosition.x + tilePosition.y * 10000L;
                Renderer renderer = tile.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.SetFloat("_Seed", seed);
                }
            }
        }
        generatedChunks.Add(chunkPosition, chunkObject);
    }
}
