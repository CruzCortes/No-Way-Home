using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tree : MonoBehaviour
{
    public SpriteRenderer stump;
    public SpriteRenderer top;
    public BoxCollider2D treeCollider;
    private BoxCollider2D interactionCollider;

    [Header("Tree Cutting Settings")]
    public int maxHealth = 3;
    public float fallDuration = 1.5f;
    public float woodScatterRadius = 2f;
    public int woodDropCount = 3;

    [Header("Animation Settings")]
    public float wiggleDuration = 0.5f;
    public float wiggleStrength = 5f;
    public float wiggleSpeed = 15f;

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
        treeCollider.isTrigger = false;

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
            float interactionOffsetY = mainOffsetY - 0.5f;
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
            StartCoroutine(WiggleAndFall());
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
        isBeingCut = false;
    }

    IEnumerator WiggleAndFall()
    {
        if (top == null) yield break;

        isFalling = true;
        float elapsed = 0f;

        // Calculate base point (bottom center of tree top)
        Vector3 topStartPosition = top.transform.position;
        float treeHeight = top.bounds.size.y;
        Vector3 basePoint = topStartPosition - new Vector3(0, treeHeight / 2, 0);
        float heightOffset = treeHeight / 2;

        // Wiggle phase
        while (elapsed < wiggleDuration)
        {
            float wiggleAngle = Mathf.Sin(elapsed * wiggleSpeed) * wiggleStrength * (1 - elapsed / wiggleDuration);

            // Calculate position offset for wiggle
            float angleRad = wiggleAngle * Mathf.Deg2Rad;
            Vector3 wiggleOffset = new Vector3(
                heightOffset * Mathf.Sin(angleRad),
                heightOffset * (1 - Mathf.Cos(angleRad)),
                0
            );

            top.transform.rotation = Quaternion.Euler(0, 0, wiggleAngle);
            top.transform.position = basePoint + new Vector3(0, heightOffset, 0) - wiggleOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fall phase
        elapsed = 0f;
        float startRotation = 0;
        float randomDirection = Random.Range(0, 2) == 0 ? -90f : 90f;
        float targetRotation = randomDirection;

        Vector3 lastPosition = top.transform.position;
        Quaternion lastRotation = top.transform.rotation;

        while (elapsed < fallDuration)
        {
            float t = elapsed / fallDuration;
            t = t * t * (3f - 2f * t); // Smoothstep formula

            float currentAngle = Mathf.Lerp(startRotation, targetRotation, t);
            float angleRad = currentAngle * Mathf.Deg2Rad;

            Vector3 rotationOffset = new Vector3(
                heightOffset * Mathf.Sin(angleRad),
                heightOffset * (1 - Mathf.Cos(angleRad)),
                0
            );

            if (randomDirection < 0)
            {
                rotationOffset.x *= -1;
            }

            top.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            top.transform.position = basePoint + new Vector3(0, heightOffset, 0) - rotationOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position and rotation
        float finalAngleRad = targetRotation * Mathf.Deg2Rad;
        Vector3 finalOffset = new Vector3(
            heightOffset * Mathf.Sin(finalAngleRad),
            heightOffset * (1 - Mathf.Cos(finalAngleRad)),
            0
        );
        if (randomDirection < 0) finalOffset.x *= -1;

        top.transform.rotation = Quaternion.Euler(0, 0, targetRotation);
        top.transform.position = basePoint + new Vector3(0, heightOffset, 0) - finalOffset;

        yield return new WaitForSeconds(0.2f);

        ScatterWood();
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

                GameObject wood = Instantiate(WorldGenerator.Instance.woodPrefab, position,
                    Quaternion.Euler(0, 0, Random.Range(0f, 360f)));

                Wood woodComponent = wood.AddComponent<Wood>();
                if (wood.GetComponent<Collider2D>() == null)
                {
                    CircleCollider2D collider = wood.AddComponent<CircleCollider2D>();
                    collider.isTrigger = true;
                    collider.radius = 0.5f;
                }
            }
        }
    }
}