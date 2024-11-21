using UnityEngine;

public class CampfireHeat : MonoBehaviour
{
    [Header("Heat Settings")]
    [SerializeField] private float maxHeatBonus = 30f;
    [SerializeField] private float heatMultiplier = 5f;

    [Header("Wood Settings")]
    [SerializeField] private GameObject woodPrefab; // Reference to wood prefab
    [SerializeField] private Vector2 woodOffset = new Vector2(0f, -0.1f); // Offset from fire position
    [SerializeField] private int woodSortingOrder = -1; // Making wood render behind fire
    [SerializeField] private int numberOfWoodPieces = 3; // Number of wood pieces to spawn
    [SerializeField] private float woodSpreadRadius = 0.2f; // How far apart the wood pieces can be

    private CircleCollider2D heatZone;
    private PlayerStatsManager nearbyPlayer;
    private float currentHeatAmount = 0f;

    private void Start()
    {
        heatZone = GetComponent<CircleCollider2D>();
        if (heatZone == null)
        {
            Debug.LogError("No CircleCollider2D found on campfire!");
            return;
        }

        SpawnWood();
        Debug.Log($"Campfire initialized with radius: {heatZone.radius}");
    }

    private void SpawnWood()
    {
        if (woodPrefab == null)
        {
            Debug.LogError("Wood prefab not assigned!");
            return;
        }

        // Spawn single wood bundle at the offset position
        Vector2 woodPosition = (Vector2)transform.position + woodOffset;

        // Instantiate wood piece with no rotation
        GameObject woodPiece = Instantiate(woodPrefab, woodPosition, Quaternion.identity);

        // Set wood as child of fire
        woodPiece.transform.SetParent(transform);

        // Set sorting order for wood sprite
        SpriteRenderer woodSprite = woodPiece.GetComponent<SpriteRenderer>();
        if (woodSprite != null)
        {
            woodSprite.sortingOrder = woodSortingOrder;
        }
        else
        {
            Debug.LogWarning("No SpriteRenderer found on wood prefab!");
        }
    }

    private void Update()
    {
        if (nearbyPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, nearbyPlayer.transform.position);
            float normalizedDistance = 1f - Mathf.Clamp01(distance / heatZone.radius);
            currentHeatAmount = normalizedDistance * maxHeatBonus * heatMultiplier;
            nearbyPlayer.SetHeatSource(currentHeatAmount);
            Debug.Log($"Applying heat: {currentHeatAmount} at distance: {distance}");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered heat zone");
            nearbyPlayer = other.GetComponent<PlayerStatsManager>();
            if (nearbyPlayer != null)
            {
                Debug.Log("PlayerStatsManager found on player");
                float distance = Vector2.Distance(transform.position, other.transform.position);
                float normalizedDistance = 1f - Mathf.Clamp01(distance / heatZone.radius);
                currentHeatAmount = normalizedDistance * maxHeatBonus * heatMultiplier;
                nearbyPlayer.SetHeatSource(currentHeatAmount);
            }
            else
            {
                Debug.LogWarning("PlayerStatsManager not found on player!");
            }
        }
        else
        {
            Debug.Log($"Non-player object entered heat zone: {other.name}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && nearbyPlayer != null)
        {
            Debug.Log("Player exited heat zone");
            nearbyPlayer.SetHeatSource(0f);
            currentHeatAmount = 0f;
            nearbyPlayer = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (heatZone != null)
        {
            // Show heat radius in editor
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, heatZone.radius);

            // Show wood placement area
            Gizmos.color = new Color(0.6f, 0.4f, 0.2f, 0.2f);
            Gizmos.DrawWireSphere(transform.position + (Vector3)woodOffset, woodSpreadRadius);
        }
    }
}