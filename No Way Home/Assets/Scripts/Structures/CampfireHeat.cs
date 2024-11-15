using UnityEngine;

public class CampfireHeat : MonoBehaviour
{
    [Header("Heat Settings")]
    [SerializeField] private float maxHeatBonus = 30f;
    [SerializeField] private float heatMultiplier = 5f; // Added multiplier for stronger effect

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

        Debug.Log($"Campfire initialized with radius: {heatZone.radius}");
    }

    private void Update()
    {
        if (nearbyPlayer != null)
        {
            // Calculate heat every frame while player is in range
            float distance = Vector2.Distance(transform.position, nearbyPlayer.transform.position);
            float normalizedDistance = 1f - Mathf.Clamp01(distance / heatZone.radius);

            // Apply stronger heat effect with multiplier
            currentHeatAmount = normalizedDistance * maxHeatBonus * heatMultiplier;

            // Apply heat effect
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
                // Initial heat application
                float distance = Vector2.Distance(transform.position, other.transform.position);
                float normalizedDistance = 1f - Mathf.Clamp01(distance / heatZone.radius);
                currentHeatAmount = normalizedDistance * maxHeatBonus * heatMultiplier;
                nearbyPlayer.SetHeatSource(currentHeatAmount);
            }
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
        }
    }
}