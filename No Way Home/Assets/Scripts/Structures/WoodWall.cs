using UnityEngine;

public class WoodWall : MonoBehaviour
{
    public int maxHits = 3;
    private int currentHits = 0;
    private bool playerInRange = false;
    private float interactionRange = 2f;

    private BoxCollider2D wallCollider;
    private BoxCollider2D interactionCollider;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        gameObject.tag = "WoodWall";
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Setup main collision collider
        wallCollider = GetComponent<BoxCollider2D>();
        if (wallCollider == null)
        {
            wallCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        wallCollider.isTrigger = false;

        // Setup interaction trigger collider
        interactionCollider = gameObject.AddComponent<BoxCollider2D>();
        interactionCollider.isTrigger = true;

        // Set up collider sizes
        if (spriteRenderer != null)
        {
            Vector2 wallSize = spriteRenderer.sprite.bounds.size;

            // Main collider setup (for the actual wall)
            wallCollider.size = wallSize * 0.19f; // Slightly smaller than sprite was 0.9

            // Interaction collider setup (wider area around the wall)
            interactionCollider.size = new Vector2(wallSize.x * 1.5f, wallSize.y * 1.5f);
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TakeHit();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only respond to trigger events from the interaction collider
        if (other.CompareTag("Player") && other.GetComponent<Collider2D>() != wallCollider)
        {
            Debug.Log("Player entered wall interaction zone");
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Only respond to trigger events from the interaction collider
        if (other.CompareTag("Player") && other.GetComponent<Collider2D>() != wallCollider)
        {
            Debug.Log("Player exited wall interaction zone");
            playerInRange = false;
        }
    }

    private void TakeHit()
    {
        currentHits++;
        Debug.Log($"Wall hit! Hits: {currentHits}/{maxHits}");

        StartCoroutine(FlashRed());

        if (currentHits >= maxHits)
        {
            DestroyWall();
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    private void DestroyWall()
    {
        Debug.Log("Wall destroyed!");
        Destroy(gameObject);
    }

    // Optional: Visualize the interaction range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (spriteRenderer != null)
        {
            Vector2 size = spriteRenderer.sprite.bounds.size;
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x * 1.5f, size.y * 1.5f, 0));
        }
    }
}