using UnityEngine;

public class WoodWall : MonoBehaviour
{
    public int maxHits = 3;
    private int currentHits = 0;
    private bool playerInRange = false;
    private float interactionRange = 2f;

    private void Start()
    {
        // Ensure the wall has the correct tag
        gameObject.tag = "WoodWall";
    }

    private void Update()
    {
        // Check if player is in range and presses E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TakeHit();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void TakeHit()
    {
        currentHits++;

        // Visual feedback (optional)
        StartCoroutine(FlashRed());

        if (currentHits >= maxHits)
        {
            DestroyWall();
        }
        else
        {
            Debug.Log($"Wall hit {currentHits}/{maxHits} times");
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
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
        // Optionally add particles or sound effects here
        Destroy(gameObject);
    }

    // Optional: Visualize the interaction range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}