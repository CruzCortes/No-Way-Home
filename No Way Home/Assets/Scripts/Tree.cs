using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tree : MonoBehaviour
{
    public SpriteRenderer stump;
    public SpriteRenderer top;
    public BoxCollider2D treeCollider;  // Main collider for physics
    private BoxCollider2D interactionCollider; // Trigger collider for stump interaction

    [Header("Tree Cutting Settings")]
    public int maxHealth = 3;
    public float fallDuration = 1.5f;
    public float woodScatterRadius = 2f;
    public int woodDropCount = 3;

    private int currentHealth;
    private bool isBeingCut = false;
    private bool isFalling = false;
    private PlayerController nearbyPlayer = null;

    public void Initialize()
    {
        if (stump != null) stump.sortingOrder = 0;
        if (top != null) top.sortingOrder = 2;

        // Setup main collision collider
        if (treeCollider == null)
        {
            treeCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        treeCollider.isTrigger = false; // This is the main physics collider

        // Set up the stump interaction trigger collider
        interactionCollider = gameObject.AddComponent<BoxCollider2D>();
        interactionCollider.isTrigger = true;

        if (stump != null)
        {
            Vector2 stumpSize = stump.sprite.bounds.size;

            // Main collider setup (for the whole tree)
            treeCollider.size = new Vector2(stumpSize.x * 0.1f, stumpSize.y * 0.01f);
            float mainOffsetY = (-stumpSize.y / 2) + (treeCollider.size.y / 2);
            mainOffsetY += -2f;
            treeCollider.offset = new Vector2(0, mainOffsetY);

            // Interaction collider setup (wider area around the stump)
            interactionCollider.size = new Vector2(stumpSize.x * 1.5f, stumpSize.y * 0.5f);
            float interactionOffsetY = mainOffsetY - 0.5f; // Position it at the base of the tree
            interactionCollider.offset = new Vector2(0, interactionOffsetY);
        }

        currentHealth = maxHealth;
        Debug.Log($"Tree initialized at {transform.position} with health: {currentHealth}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.GetComponent<Collider2D>() != treeCollider)
        {
            Debug.Log("Player entered tree interaction zone");
            nearbyPlayer = other.GetComponent<PlayerController>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.GetComponent<Collider2D>() != treeCollider)
        {
            Debug.Log("Player exited tree interaction zone");
            nearbyPlayer = null;
        }
    }

    void Update()
    {
        if (isFalling || isBeingCut) return;

        if (nearbyPlayer != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("E key pressed near tree");
                HitTree();
            }
        }
    }

    void HitTree()
    {
        if (isBeingCut || isFalling) return;

        currentHealth--;
        isBeingCut = true;
        Debug.Log($"Tree hit! Health remaining: {currentHealth}");

        StartCoroutine(ShakeTree());

        if (currentHealth <= 0)
        {
            Debug.Log("Tree is falling!");
            StartCoroutine(FallTree());
        }
        else
        {
            isBeingCut = false;
        }
    }

    IEnumerator ShakeTree()
    {
        if (top == null) yield break;

        Vector3 originalPosition = top.transform.localPosition;
        float shakeAmount = 0.1f;
        float shakeDuration = 0.2f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = originalPosition.x + Random.Range(-shakeAmount, shakeAmount);
            top.transform.localPosition = new Vector3(x, originalPosition.y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        top.transform.localPosition = originalPosition;
    }

    IEnumerator FallTree()
    {
        if (top == null) yield break;

        isFalling = true;
        float elapsed = 0f;
        Quaternion startRotation = top.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, -90f);

        // Determine fall direction (random left/right)
        float randomDirection = Random.Range(0, 2) == 0 ? -90f : 90f;
        targetRotation = Quaternion.Euler(0, 0, randomDirection);

        while (elapsed < fallDuration)
        {
            // Use smoothstep for acceleration
            float t = elapsed / fallDuration;
            t = t * t * (3f - 2f * t); // Smoothstep formula

            top.transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is exact
        top.transform.localRotation = targetRotation;

        // Scatter wood
        ScatterWood();

        // Remove tree top
        Destroy(top.gameObject);
    }

    void ScatterWood()
    {
        if (WorldGenerator.Instance != null && WorldGenerator.Instance.woodPrefab != null)
        {
            for (int i = 0; i < woodDropCount; i++)
            {
                float angle = Random.Range(0f, 360f);
                float radius = Random.Range(0f, woodScatterRadius);
                Vector2 position = (Vector2)transform.position + (Random.insideUnitCircle * radius);

                GameObject wood = Instantiate(WorldGenerator.Instance.woodPrefab, position, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));

                // Add necessary components for wood pickup
                Wood woodComponent = wood.AddComponent<Wood>();
                if (wood.GetComponent<Collider2D>() == null)
                {
                    wood.AddComponent<CircleCollider2D>().isTrigger = true;
                }
            }
        }
    }
}