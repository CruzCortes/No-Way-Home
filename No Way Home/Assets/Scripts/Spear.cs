using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    [Header("Spear Settings")]
    private int usesRemaining = 10;
    private float throwSpeed = 7f;
    private float maxArcHeight = 2f;

    [Header("State")]
    private bool isFlying = false;
    private bool canBeCollected = false;
    private float throwTime = 0f;

    [Header("References")]
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;

    private void Awake()
    {
        // Get or add required components
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        SetupComponents();
    }

    private void SetupComponents()
    {
        // Setup Rigidbody
        rb.gravityScale = 0;
        rb.isKinematic = true;

        // Setup Collider
        circleCollider.radius = 0.06f;
        circleCollider.offset = new Vector2(0, 0.17f);
        circleCollider.isTrigger = true;

        Debug.Log($"Spear setup complete. Collider radius: {circleCollider.radius}, offset: {circleCollider.offset}");
    }

    public void InitializeThrow(Vector3 start, Vector3 target)
    {
        if (usesRemaining <= 0)
        {
            Debug.Log("Spear has no uses remaining!");
            Destroy(gameObject);
            return;
        }

        // Reset state
        startPosition = start;
        targetPosition = target;
        transform.position = startPosition;
        isFlying = true;
        canBeCollected = false;
        throwTime = 0f;

        // Set initial rotation
        Vector2 throwDirection = (target - start).normalized;
        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        Debug.Log($"Spear throw initialized from {start} to {target}");
    }

    private void Update()
    {
        if (!isFlying) return;

        throwTime += Time.deltaTime * throwSpeed;

        // Check if throw is complete
        if (throwTime >= 1f)
        {
            CompleteThrow();
            return;
        }

        UpdateSpearPosition();
    }

    private void CompleteThrow()
    {
        throwTime = 1f;
        isFlying = false;
        canBeCollected = true;
        transform.position = targetPosition;
        Debug.Log("Spear throw completed - ready for collection");
    }

    private void UpdateSpearPosition()
    {
        // Calculate throw arc
        Vector2 direction = (targetPosition - startPosition).normalized;
        float throwAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Calculate vertical factor for arc height
        float verticalFactor = 1f;
        float verticalThreshold = 30f;
        if (Mathf.Abs(throwAngle - 90) < verticalThreshold || Mathf.Abs(throwAngle + 90) < verticalThreshold)
        {
            verticalFactor = Mathf.Abs(throwAngle - 90) / verticalThreshold;
        }

        // Calculate current position
        Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, throwTime);
        float arcHeight = maxArcHeight * verticalFactor * Mathf.Sin(throwTime * Mathf.PI);
        currentPos.y += arcHeight;

        // Update rotation to match trajectory
        if (throwTime < 0.99f)
        {
            UpdateSpearRotation(currentPos, verticalFactor);
        }

        transform.position = currentPos;
    }

    private void UpdateSpearRotation(Vector3 currentPos, float verticalFactor)
    {
        Vector3 nextPos = Vector3.Lerp(startPosition, targetPosition, throwTime + 0.1f);
        float nextArcHeight = maxArcHeight * verticalFactor * Mathf.Sin((throwTime + 0.1f) * Mathf.PI);
        nextPos.y += nextArcHeight;

        Vector2 moveDirection = (nextPos - currentPos).normalized;
        float moveAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, moveAngle - 90);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HandleCollision(other);
    }

    private void HandleCollision(Collider2D other)
    {
        // Handle bunny hits while flying
        if (isFlying)
        {
            BunnyMovement bunny = other.GetComponent<BunnyMovement>();
            if (bunny != null)
            {
                Debug.Log($"Hit bunny: {other.gameObject.name}");
                isFlying = false;
                canBeCollected = true;
                bunny.OnHit();
                transform.position = other.transform.position;
            }
            return;
        }

        // Handle player collection when not flying
        if (canBeCollected && other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log("Player collecting spear");
                player.CollectSpear();
                Destroy(gameObject);
            }
        }
    }

    public void DecrementUses()
    {
        usesRemaining--;
        Debug.Log($"Spear uses remaining: {usesRemaining}");

        if (usesRemaining <= 0)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.gray;
                Debug.Log("Spear turned gray - no uses remaining");
            }
            Destroy(gameObject, 2f);
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Spear destroyed");
    }
}