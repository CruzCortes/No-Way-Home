using UnityEngine;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject groundPrefab;
    public GameObject playerPrefab;
    public GameObject rockPrefab1;
    public GameObject rockPrefab2;
    public GameObject treeStumpPrefab;
    public GameObject treeTopPrefab;
    public GameObject woodPrefab;
    public GameObject bunnyPrefab; 

    [Header("Generation Settings")]
    public int chunkSize = 16;
    public float tileSize = 1f;
    public int chunkLoadRadius = 2;

    [Header("Decoration Settings")]
    [Range(0f, 1f)]
    public float rockSpawnChance = 0.10f;
    [Range(0f, 1f)]
    public float treeSpawnChance = 0.05f;
    [Range(0f, 1f)]
    public float bunnySpawnChance = 0.02f;

    [Header("References")]
    public Camera mainCamera;
    public Transform worldContainer;

    private Vector2Int lastPlayerChunk;
    private Dictionary<Vector2Int, GameObject> generatedChunks = new Dictionary<Vector2Int, GameObject>();
    private GameObject player;
    private const float DECORATION_HEIGHT_OFFSET = 0.1f;

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

        // Add weather and shadow systems
        if (FindObjectOfType<WeatherSystem>() == null)
        {
            GameObject weatherObj = new GameObject("WeatherSystem");
            weatherObj.AddComponent<WeatherSystem>();
        }

        gameObject.AddComponent<ShadowController>();

        SetupPlayer();
        UpdateChunks();
    }

    void Update()
    {
        if (player != null)
        {
            Vector2Int currentPlayerChunk = GetCurrentChunk(player.transform.position);
            if (currentPlayerChunk != lastPlayerChunk)
            {
                UpdateChunks();
                lastPlayerChunk = currentPlayerChunk;
            }
        }
    }

    void SetupPlayer()
    {
        player = GameObject.Find("Player");
        if (player == null && playerPrefab != null)
        {
            player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            player.name = "Player";
        }

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
            Debug.LogError("Player setup failed. Ensure either a Player object exists in the scene or a playerPrefab is assigned.");
        }
    }

    public Vector2Int GetCurrentChunk(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / (chunkSize * tileSize)),
            Mathf.FloorToInt(position.y / (chunkSize * tileSize))
        );
    }

    void UpdateChunks()
    {
        HashSet<Vector2Int> chunksToKeep = GetRequiredChunks();
        RemoveUnnecessaryChunks(chunksToKeep);
        GenerateNewChunks(chunksToKeep);
    }

    HashSet<Vector2Int> GetRequiredChunks()
    {
        HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>();
        Vector2Int playerChunk = GetCurrentChunk(player.transform.position);

        for (int x = -chunkLoadRadius; x <= chunkLoadRadius; x++)
        {
            for (int y = -chunkLoadRadius; y <= chunkLoadRadius; y++)
            {
                Vector2Int chunkPos = new Vector2Int(
                    playerChunk.x + x,
                    playerChunk.y + y
                );
                requiredChunks.Add(chunkPos);
            }
        }

        return requiredChunks;
    }

    void RemoveUnnecessaryChunks(HashSet<Vector2Int> chunksToKeep)
    {
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in generatedChunks)
        {
            if (!chunksToKeep.Contains(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunkPos in chunksToRemove)
        {
            if (generatedChunks.TryGetValue(chunkPos, out GameObject chunkObject))
            {
                Destroy(chunkObject);
                generatedChunks.Remove(chunkPos);
            }
        }
    }

    void GenerateNewChunks(HashSet<Vector2Int> requiredChunks)
    {
        foreach (Vector2Int chunkPos in requiredChunks)
        {
            if (!generatedChunks.ContainsKey(chunkPos))
            {
                GenerateChunk(chunkPos);
            }
        }
    }

    void GenerateChunk(Vector2Int chunkPosition)
    {
        GameObject chunkObject = new GameObject($"Chunk_{chunkPosition.x}_{chunkPosition.y}");
        chunkObject.transform.parent = worldContainer;
        chunkObject.transform.position = new Vector3(chunkPosition.x * chunkSize * tileSize,
                                                   chunkPosition.y * chunkSize * tileSize, 0);

        GameObject rocksContainer = new GameObject("RocksContainer");
        rocksContainer.transform.parent = chunkObject.transform;

        GameObject treesContainer = new GameObject("TreesContainer");
        treesContainer.transform.parent = chunkObject.transform;

        GameObject bunniesContainer = new GameObject("BunniesContainer"); // New container for bunnies
        bunniesContainer.transform.parent = chunkObject.transform;

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
                // The tag is already set in the prefab as "GroundTile"

                // Use the same seed for consistent generation
                long seed = tilePosition.x + tilePosition.y * 10000L;
                Random.InitState((int)seed);

                // Set material seed for the ground tile
                Renderer renderer = tile.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.SetFloat("_Seed", seed);
                }

                worldPosition.z = DECORATION_HEIGHT_OFFSET;  // Set decoration height

                // First check for bunny spawn to avoid overlap
                if (bunnyPrefab != null && Random.value < bunnySpawnChance)
                {
                    GameObject bunny = Instantiate(bunnyPrefab, worldPosition, Quaternion.identity, bunniesContainer.transform);
                    bunny.name = $"Bunny_{tilePosition.x}_{tilePosition.y}";
                }
                // Then check for tree spawn
                else if (Random.value < treeSpawnChance)
                {
                    GameObject treeTemplate = CreateTreePrefab();
                    GameObject tree = Instantiate(treeTemplate, worldPosition, Quaternion.identity, treesContainer.transform);
                    tree.name = $"Tree_{tilePosition.x}_{tilePosition.y}";
                    Tree treeComponent = tree.GetComponent<Tree>();
                    treeComponent.Initialize();  // THIS is the key line for tree interaction!
                    Destroy(treeTemplate);
                }
                // If no tree was spawned, try to spawn a rock
                else if (Random.value < rockSpawnChance)
                {
                    GameObject rockPrefab = Random.value < 0.5f ? rockPrefab1 : rockPrefab2;
                    GameObject rock = Instantiate(rockPrefab, worldPosition, Quaternion.Euler(0, 0, Random.Range(0, 360)),
                                               rocksContainer.transform);

                    Rock rockComponent = rock.AddComponent<Rock>();
                    rockComponent.rockType = rockPrefab == rockPrefab1 ? 0 : 1;

                    if (rock.GetComponent<Collider2D>() == null)
                    {
                        rock.AddComponent<CircleCollider2D>().isTrigger = true;
                    }
                }
            }
        }

        generatedChunks.Add(chunkPosition, chunkObject);
    }

    private GameObject CreateTreePrefab()
    {
        GameObject treeObject = new GameObject("Tree");
        Tree treeComponent = treeObject.AddComponent<Tree>();

        GameObject stump = Instantiate(treeStumpPrefab, Vector3.zero, Quaternion.identity);
        stump.transform.SetParent(treeObject.transform, false);
        stump.transform.localPosition = Vector3.zero;
        SpriteRenderer stumpRenderer = stump.GetComponent<SpriteRenderer>();
        stumpRenderer.sortingOrder = 0;

        GameObject top = Instantiate(treeTopPrefab, Vector3.zero, Quaternion.identity);
        top.transform.SetParent(treeObject.transform, false);
        top.transform.localPosition = Vector3.zero;
        SpriteRenderer topRenderer = top.GetComponent<SpriteRenderer>();
        topRenderer.sortingOrder = 2;

        treeComponent.stump = stumpRenderer;
        treeComponent.top = topRenderer;

        return treeObject;
    }

    public Vector3 GetWorldPosition(Vector2Int tilePosition)
    {
        return new Vector3(tilePosition.x * tileSize, tilePosition.y * tileSize, 0);
    }

    public Vector2Int GetTilePosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / tileSize),
            Mathf.FloorToInt(worldPosition.y / tileSize)
        );
    }

    public bool IsChunkGenerated(Vector2Int chunkPosition)
    {
        return generatedChunks.ContainsKey(chunkPosition);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        if (player != null)
        {
            foreach (var chunk in generatedChunks)
            {
                Vector3 chunkWorldPos = new Vector3(
                    chunk.Key.x * chunkSize * tileSize,
                    chunk.Key.y * chunkSize * tileSize,
                    0
                );
                Gizmos.DrawWireCube(
                    chunkWorldPos + new Vector3(chunkSize * tileSize * 0.5f, chunkSize * tileSize * 0.5f, 0),
                    new Vector3(chunkSize * tileSize, chunkSize * tileSize, 0)
                );
            }
        }
    }
}