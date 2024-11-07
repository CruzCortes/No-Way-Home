using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    private int usesRemaining = 10;
    private bool isFlying = false;
    private float throwSpeed = 7f;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float throwTime = 0f;
    private float maxArcHeight = 2f;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Ensure collider settings are correct
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }
        collider.radius = 0.06f;
        collider.offset = new Vector2(0, 0.17f);
        collider.isTrigger = true;
    }

    public void InitializeThrow(Vector3 start, Vector3 target)
    {
        if (usesRemaining <= 0)
        {
            Destroy(gameObject);
            return;
        }

        startPosition = start;
        targetPosition = target;
        transform.position = startPosition;
        isFlying = true;
        throwTime = 0f;

        // Calculate initial rotation based on throw direction
        Vector2 throwDirection = (target - start).normalized;
        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    private void Update()
    {
        if (isFlying)
        {
            throwTime += Time.deltaTime * throwSpeed;

            if (throwTime >= 1f)
            {
                throwTime = 1f;
                isFlying = false;
                transform.position = targetPosition;
                return;
            }

            // Calculate arc
            Vector2 direction = (targetPosition - startPosition).normalized;
            float throwAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Reduce arc height for near-vertical throws
            float verticalFactor = 1f;
            float verticalThreshold = 30f;
            if (Mathf.Abs(throwAngle - 90) < verticalThreshold || Mathf.Abs(throwAngle + 90) < verticalThreshold)
            {
                verticalFactor = Mathf.Abs(throwAngle - 90) / verticalThreshold;
            }

            // Calculate position with arc
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, throwTime);
            float arcHeight = maxArcHeight * verticalFactor * Mathf.Sin(throwTime * Mathf.PI);
            currentPos.y += arcHeight;

            // Update rotation to match trajectory
            if (throwTime < 0.99f)  // Don't update rotation at the very end
            {
                Vector3 nextPos = Vector3.Lerp(startPosition, targetPosition, throwTime + 0.1f);
                nextPos.y += maxArcHeight * verticalFactor * Mathf.Sin((throwTime + 0.1f) * Mathf.PI);
                Vector2 moveDirection = (nextPos - currentPos).normalized;
                float moveAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, moveAngle - 90);
            }

            transform.position = currentPos;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFlying && other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.CollectSpear();
                Destroy(gameObject);
            }
        }
    }

    public void DecrementUses()
    {
        usesRemaining--;
        if (usesRemaining <= 0)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.gray;
            }
            Destroy(gameObject, 2f);
        }
    }
}