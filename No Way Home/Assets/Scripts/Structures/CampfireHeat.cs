using UnityEngine;

public class CampfireHeat : MonoBehaviour
{
    [SerializeField] private float heatRadius = 5f;
    [SerializeField] private bool showHeatRadius = true;
    [SerializeField] private float maxHeatBonus = 20f;

    private CircleCollider2D heatZone;
    private PlayerStatsManager currentPlayer;
    private float heatIntensity = 0f;

    private void Start()
    {
        heatZone = gameObject.AddComponent<CircleCollider2D>();
        heatZone.radius = heatRadius;
        heatZone.isTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerStatsManager player = other.GetComponent<PlayerStatsManager>();
        if (player != null)
        {
            // Calculate distance-based heat intensity
            float distance = Vector2.Distance(transform.position, other.transform.position);
            float normalizedDistance = 1f - Mathf.Clamp01(distance / heatRadius);

            // Exponential falloff for more realistic heat distribution
            heatIntensity = Mathf.Pow(normalizedDistance, 1.5f);

            // Apply heat intensity to player
            player.SetHeatSource(heatIntensity * maxHeatBonus);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerStatsManager player = other.GetComponent<PlayerStatsManager>();
        if (player != null)
        {
            player.SetHeatSource(0f);
        }
    }

    private void OnDrawGizmos()
    {
        if (showHeatRadius)
        {
            // Outer radius
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, heatRadius);

            // Inner effective radius
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, heatRadius * 0.5f);
        }
    }
}