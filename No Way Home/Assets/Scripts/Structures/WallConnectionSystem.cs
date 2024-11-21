using UnityEngine;

public class WallConnectionSystem : MonoBehaviour
{
    [SerializeField] private float gridSize = 0.5f; // Matches your PlayerController grid size
    private SpriteRenderer mainRenderer;
    private SpriteRenderer connectionRenderer;
    private Material connectionMaterial;

    private void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();

        // Create connection renderer
        GameObject connectionObj = new GameObject("ConnectionSprite");
        connectionObj.transform.SetParent(transform);
        connectionObj.transform.localPosition = Vector3.zero;

        connectionRenderer = connectionObj.AddComponent<SpriteRenderer>();
        connectionRenderer.sprite = mainRenderer.sprite;

        // Create material instance
        connectionMaterial = new Material(Shader.Find("Custom/WallConnection"));
        if (connectionMaterial != null)
        {
            connectionMaterial.SetColor("_Color", new Color(0.8f, 0.6f, 0.4f, 1f));
            connectionMaterial.SetFloat("_ConnectionWidth", 0.1f);
            connectionRenderer.material = connectionMaterial;
        }
        else
        {
            Debug.LogError("Could not find Custom/WallConnection shader!");
        }

        connectionRenderer.sortingOrder = mainRenderer.sortingOrder - 1;

        // Initial connection check
        UpdateConnections();
    }

    private void OnEnable()
    {
        UpdateConnections();
    }

    public void UpdateConnections()
    {
        if (!gameObject.activeInHierarchy || connectionMaterial == null) return;

        int connections = 0;
        Vector2[] checkDirections = new Vector2[]
        {
            Vector2.right,  // 1
            Vector2.left,   // 2
            Vector2.up,     // 4
            Vector2.down,   // 8
            new Vector2(1, 1),   // 16 (top-right)
            new Vector2(-1, 1),  // 32 (top-left)
            new Vector2(1, -1),  // 64 (bottom-right)
            new Vector2(-1, -1)  // 128 (bottom-left)
        };

        int[] connectionValues = new int[] { 1, 2, 4, 8, 16, 32, 64, 128 };

        for (int i = 0; i < checkDirections.Length; i++)
        {
            Vector2 checkPos = (Vector2)transform.position + (checkDirections[i] * gridSize);

            if (i >= 4) // Diagonal checks
            {
                Vector2 horizontal = new Vector2(checkDirections[i].x, 0) * gridSize;
                Vector2 vertical = new Vector2(0, checkDirections[i].y) * gridSize;

                bool hasHorizontal = HasWallAt((Vector2)transform.position + horizontal);
                bool hasVertical = HasWallAt((Vector2)transform.position + vertical);

                if (hasHorizontal && hasVertical && HasWallAt(checkPos))
                {
                    connections |= connectionValues[i];
                }
            }
            else if (HasWallAt(checkPos)) // Orthogonal checks
            {
                connections |= connectionValues[i];
            }
        }

        connectionMaterial.SetFloat("_ConnectionBits", connections);
    }

    private bool HasWallAt(Vector2 position)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(position);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.GetComponent<WoodWall>() != null)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDestroy()
    {
        Vector2[] checkDirections = new Vector2[]
        {
            Vector2.right, Vector2.left, Vector2.up, Vector2.down,
            new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1)
        };

        foreach (Vector2 direction in checkDirections)
        {
            Vector2 checkPos = (Vector2)transform.position + (direction * gridSize);
            Collider2D[] colliders = Physics2D.OverlapPointAll(checkPos);
            foreach (Collider2D collider in colliders)
            {
                if (collider.gameObject != gameObject)
                {
                    var connection = collider.GetComponent<WallConnectionSystem>();
                    if (connection != null)
                    {
                        connection.UpdateConnections();
                    }
                }
            }
        }
    }
}