using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Animation References")]
    public RuntimeAnimatorController animatorController;
    private Animator animator;

    // Animation Trigger Parameters
    private static readonly string TRIGGER_IDLE = "Idle";
    private static readonly string TRIGGER_WALK_UP = "WalkUp";
    private static readonly string TRIGGER_WALK_DOWN = "WalkDown";
    private static readonly string TRIGGER_WALK_LEFT = "WalkLeft";
    private static readonly string TRIGGER_WALK_RIGHT = "WalkRight";

    [Header("Footprint Settings")]
    public GameObject footprintPrefab;
    public float footprintSpawnInterval = 0.3f;

    [Header("Footprint Positioning")]
    [SerializeField] private float horizontalFootSpacing = 0.15f;
    [SerializeField] private float verticalFootSpacing = 0.3f;
    [SerializeField] private float footprintVerticalOffset = -0.9f;

    [Header("Footprint Variation")]
    [SerializeField] private float scaleVariation = 0.2f;
    [SerializeField] private float rotationVariation = 15f;

    private float footprintTimer = 0f;
    private bool isLeftFoot = true;
    private Vector2 lastMovementDirection = Vector2.right;

    // Rocks and Wood collection
    // rocks have types. current: 2
    // Inventory
    [Header("Inventory")]
    public int[] collectedRocks = new int[2];  // Different rock types
    public int woodCount = 0;
    public int campfireCount = 0;  // Added campfire tracking

    public void CollectWood()
    {
        woodCount++;
        Debug.Log($"Collected wood. Total: {woodCount}");
    }

    public void CollectRock(int rockType)
    {
        if (rockType < collectedRocks.Length)
        {
            collectedRocks[rockType]++;
            Debug.Log($"Collected rock type {rockType}. Total: {collectedRocks[rockType]}");
        }
    }

    // Crafting-related methods
    public void RemoveWood(int amount)
    {
        woodCount = Mathf.Max(0, woodCount - amount);
        Debug.Log($"Used {amount} wood. Remaining: {woodCount}");
    }

    public void AddCampfire()
    {
        campfireCount++;
        Debug.Log($"Added campfire to inventory. Total: {campfireCount}");
    }

    public bool HasEnoughWood(int amount)
    {
        return woodCount >= amount;
    }

    // Additional utility methods for inventory management
    public int GetItemCount(string itemName)
    {
        switch (itemName.ToLower())
        {
            case "wood":
                return woodCount;
            case "campfire":
                return campfireCount;
            case "rock 1":
                return collectedRocks[0];
            case "rock 2":
                return collectedRocks[1];
            default:
                return 0;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }

        if (animatorController != null && animator != null)
        {
            animator.runtimeAnimatorController = animatorController;
        }
    }

    void Update()
    {
        // Get input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Handle animations
        if (animator != null)
        {
            if (movement.magnitude > 0.1f)
            {
                // Determine which animation to play based on dominant direction
                if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                {
                    // Horizontal movement is dominant
                    if (movement.x > 0)
                        animator.SetTrigger(TRIGGER_WALK_RIGHT);
                    else
                        animator.SetTrigger(TRIGGER_WALK_LEFT);
                }
                else
                {
                    // Vertical movement is dominant
                    if (movement.y > 0)
                        animator.SetTrigger(TRIGGER_WALK_UP);
                    else
                        animator.SetTrigger(TRIGGER_WALK_DOWN);
                }
            }
            else
            {
                animator.SetTrigger(TRIGGER_IDLE);
            }
        }

        if (movement != Vector2.zero)
        {
            lastMovementDirection = movement.normalized;
        }

        // Footprint logic
        if (movement.magnitude > 0.1f)
        {
            footprintTimer += Time.deltaTime;
            if (footprintTimer >= footprintSpawnInterval)
            {
                SpawnFootprint();
                footprintTimer = 0f;
            }
        }
        else
        {
            footprintTimer = footprintSpawnInterval;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void SpawnFootprint()
    {
        if (footprintPrefab != null)
        {
            Vector3 basePosition = transform.position;
            basePosition.y += footprintVerticalOffset;

            // Determine if movement is more horizontal or vertical
            bool isHorizontalMovement = Mathf.Abs(lastMovementDirection.x) > Mathf.Abs(lastMovementDirection.y);

            // Use appropriate spacing based on movement direction
            float currentOffset = isHorizontalMovement ? horizontalFootSpacing : verticalFootSpacing;
            currentOffset *= isLeftFoot ? -1 : 1;

            // Calculate perpendicular offset direction
            Vector2 perpendicularDirection = new Vector2(-lastMovementDirection.y, lastMovementDirection.x);

            // Apply the offset
            Vector3 offsetPosition = basePosition + (Vector3)(perpendicularDirection * currentOffset);
            offsetPosition.z += 0.1f;

            // Calculate rotation based on movement direction
            float angle = Mathf.Atan2(lastMovementDirection.y, lastMovementDirection.x) * Mathf.Rad2Deg;
            angle += Random.Range(-rotationVariation, rotationVariation);
            Quaternion rotation = Quaternion.Euler(0, 0, angle + 90);

            // Instantiate footprint
            GameObject footprint = Instantiate(footprintPrefab, offsetPosition, rotation);

            // Apply scale variation
            float randomScale = 1f + Random.Range(-scaleVariation, scaleVariation);
            footprint.transform.localScale = new Vector3(randomScale, randomScale, 1f);

            isLeftFoot = !isLeftFoot;
        }
        else
        {
            Debug.LogWarning("Footprint prefab not assigned in PlayerController.");
        }
    }
}