using UnityEngine;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour
{
    public GameObject groundPrefab;
    public GameObject playerPrefab;
    public Camera mainCamera;
    public Transform worldContainer;
    public float tileSize = 3.2f;
    private int chunkSize = 16;
    private Vector2Int lastGeneratedChunk;
    private Dictionary<Vector2Int, GameObject> generatedTiles = new Dictionary<Vector2Int, GameObject>();
    private GameObject player;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (worldContainer == null)
            worldContainer = transform;

        SetupPlayer();
        GenerateInitialWorld();
    }

    void Update()
    {
        Vector2Int currentChunk = GetCurrentChunk();
        if (currentChunk != lastGeneratedChunk)
        {
            GenerateChunksAroundCamera();
            lastGeneratedChunk = currentChunk;
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

    void GenerateInitialWorld()
    {
        GenerateChunksAroundCamera();
    }

    void GenerateChunksAroundCamera()
    {
        Vector2Int currentChunk = GetCurrentChunk();
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector2Int chunkToGenerate = new Vector2Int(
                    currentChunk.x + x,
                    currentChunk.y + y
                );
                GenerateChunk(chunkToGenerate);
            }
        }
    }

    Vector2Int GetCurrentChunk()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        return new Vector2Int(
            Mathf.FloorToInt(cameraPosition.x / (chunkSize * tileSize)),
            Mathf.FloorToInt(cameraPosition.y / (chunkSize * tileSize))
        );
    }

    void GenerateChunk(Vector2Int chunkPosition)
    {
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                Vector2Int tilePosition = new Vector2Int(
                    chunkPosition.x * chunkSize + x,
                    chunkPosition.y * chunkSize + y
                );
                if (!IsTilePresent(tilePosition))
                {
                    Vector3 worldPosition = new Vector3(tilePosition.x * tileSize, tilePosition.y * tileSize, 0);
                    GameObject tile = Instantiate(groundPrefab, worldPosition, Quaternion.identity, worldContainer);
                    tile.name = $"GroundTile_{tilePosition.x}_{tilePosition.y}";

                    long seed = tilePosition.x + tilePosition.y * 10000L;

                    Renderer renderer = tile.GetComponent<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        renderer.material.SetFloat("_Seed", seed);
                    }

                    generatedTiles[tilePosition] = tile;
                }
            }
        }
    }

    bool IsTilePresent(Vector2Int position)
    {
        return generatedTiles.ContainsKey(position);
    }
}