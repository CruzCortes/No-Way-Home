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

    // rocks:
    public GameObject rockPrefab1;
    public GameObject rockPrefab2;
    [Range(0f, 1f)]
    public float rockSpawnChance = 0.10f; // 1% chance per tile to spawn a rock


    // Trees:
    public GameObject treeStumpPrefab;
    public GameObject treeTopPrefab;
    [Range(0f, 1f)]
    public float treeSpawnChance = 0.05f; // 5% chance per tile to spawn a tree
    public GameObject woodPrefab;
    public static WorldGenerator Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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

        // Create separate containers for rocks and trees in this chunk
        GameObject rocksContainer = new GameObject("RocksContainer");
        rocksContainer.transform.parent = chunkObject.transform;

        GameObject treesContainer = new GameObject("TreesContainer");
        treesContainer.transform.parent = chunkObject.transform;

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

                // Use the same seed for consistent generation
                long seed = tilePosition.x + tilePosition.y * 10000L;
                Random.InitState((int)seed);

                // First check for tree spawn to avoid overlap
                if (Random.value < treeSpawnChance)
                {
                    GameObject treeTemplate = CreateTreePrefab();
                    GameObject tree = Instantiate(treeTemplate, worldPosition, Quaternion.identity, treesContainer.transform);
                    tree.name = $"Tree_{tilePosition.x}_{tilePosition.y}";
                    Destroy(treeTemplate);
                }
                // If no tree was spawned, try to spawn a rock
                else if (Random.value < rockSpawnChance)
                {
                    GameObject rockPrefab = Random.value < 0.5f ? rockPrefab1 : rockPrefab2;
                    GameObject rock = Instantiate(rockPrefab, worldPosition, Quaternion.Euler(0, 0, Random.Range(0, 360)), rocksContainer.transform);

                    // Add Rock component and set type
                    Rock rockComponent = rock.AddComponent<Rock>();
                    rockComponent.rockType = rockPrefab == rockPrefab1 ? 0 : 1;

                    // Add collider for pickup detection if not already present
                    if (rock.GetComponent<Collider2D>() == null)
                    {
                        rock.AddComponent<CircleCollider2D>().isTrigger = true;
                    }
                }

                // Set material seed for the ground tile
                Renderer renderer = tile.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.SetFloat("_Seed", seed);
                }
            }
        }
        generatedChunks.Add(chunkPosition, chunkObject);
    }

    // make tree:
    private GameObject CreateTreePrefab()
    {
        // Create parent object for the tree
        GameObject treeObject = new GameObject("Tree");
        Tree treeComponent = treeObject.AddComponent<Tree>();

        // Create and setup stump
        GameObject stump = Instantiate(treeStumpPrefab, Vector3.zero, Quaternion.identity);
        stump.transform.SetParent(treeObject.transform, false);
        stump.transform.localPosition = Vector3.zero;
        SpriteRenderer stumpRenderer = stump.GetComponent<SpriteRenderer>();
        stumpRenderer.sortingOrder = 0;

        // Create and setup top
        GameObject top = Instantiate(treeTopPrefab, Vector3.zero, Quaternion.identity);
        top.transform.SetParent(treeObject.transform, false);
        top.transform.localPosition = Vector3.zero;
        SpriteRenderer topRenderer = top.GetComponent<SpriteRenderer>();
        topRenderer.sortingOrder = 2;

        // Set up references in the Tree component
        treeComponent.stump = stumpRenderer;
        treeComponent.top = topRenderer;

        // Initialize the tree
        treeComponent.Initialize();

        return treeObject;
    }


}
