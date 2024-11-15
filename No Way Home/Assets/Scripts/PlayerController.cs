using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Animation References")]
    public RuntimeAnimatorController animatorController;
    private Animator animator;
    private PlayerStatsManager playerStatsManager;

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

    [Header("Inventory")]
    public int[] collectedRocks = new int[2];  // Different rock types
    public int woodCount = 0;
    public int campfireCount = 0;
    public int woodWallCount = 0;
    public GameObject woodWallPrefab; // Assign in inspector

    [Header("Spear Settings")]
    public GameObject spearPrefab;
    private GameObject activeSpear;
    public int spearCount = 0;
    private const int SPEAR_WOOD_COST = 1;
    private const int SPEAR_ROCK_COST = 1;
    [SerializeField] private Vector3 spearOffset = new Vector3(0, 0.17f, 0);

    private HotBarManager hotBarManager;
    private string currentSelectedItem;

    [Header("Food Inventory")]
    public int foodCount = 0;  // Add this for food tracking

    #region Food Collection
    public void CollectFood()
    {
        foodCount++;
        Debug.Log($"Collected food. Total: {foodCount}");
    }
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        hotBarManager = FindObjectOfType<HotBarManager>();
        // Add this line to get the PlayerStatsManager
        playerStatsManager = GetComponent<PlayerStatsManager>();

        // If PlayerStatsManager is not on the same GameObject, try to find it in the scene
        if (playerStatsManager == null)
        {
            playerStatsManager = FindObjectOfType<PlayerStatsManager>();
            Debug.Log("Found PlayerStatsManager in scene");
        }

        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }

        if (animatorController != null && animator != null)
        {
            animator.runtimeAnimatorController = animatorController;
        }

        CreateSpearVisual();
    }


    private void CreateSpearVisual()
    {
        if (spearPrefab != null && activeSpear == null)
        {
            activeSpear = Instantiate(spearPrefab, transform.position + spearOffset, Quaternion.identity);
            activeSpear.transform.parent = transform;
            activeSpear.transform.localPosition = spearOffset;

            // Disable collider for the visual representation
            var collider = activeSpear.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // Remove any Spear component from the visual representation
            var spearComponent = activeSpear.GetComponent<Spear>();
            if (spearComponent != null)
            {
                Destroy(spearComponent);
            }

            activeSpear.SetActive(false);
        }
    }

    void Update()
    {
        // Get input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Check selected item from hotbar
        if (hotBarManager != null)
        {
            string selectedItem = hotBarManager.GetSelectedItem();
            if (selectedItem != currentSelectedItem)
            {
                currentSelectedItem = selectedItem;
                UpdateSpearVisibility();
            }
        }

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

        // Throw spear
        if (Input.GetMouseButtonDown(0) && currentSelectedItem == "Spear" && spearCount > 0)
        {
            ThrowSpear();
        }

        // Handle eating food
        if (Input.GetKeyDown(KeyCode.E) && currentSelectedItem == "Food" && foodCount > 0)
        {
            EatSelectedFood();
        }

        // place wood wall
        if (Input.GetMouseButtonDown(0) && currentSelectedItem == "Wooden Wall" && woodWallCount > 0)
        {
            PlaceWoodWall();
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

    private void PlaceWoodWall()
    {
        if (woodWallCount > 0 && woodWallPrefab != null)
        {
            // Get mouse position in world space
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Ensure it's placed on the 2D plane

            // Round the position to the nearest 0.5 units for grid-like placement
            mousePos.x = Mathf.Round(mousePos.x * 2) / 2;
            mousePos.y = Mathf.Round(mousePos.y * 2) / 2;

            // Check if there's already a wall at this position
            Collider2D[] colliders = Physics2D.OverlapCircleAll(mousePos, 0.25f);
            bool canPlace = true;

            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("WoodWall"))
                {
                    canPlace = false;
                    break;
                }
            }

            // Place the wall if the position is clear
            if (canPlace)
            {
                GameObject wall = Instantiate(woodWallPrefab, mousePos, Quaternion.identity);
                woodWallCount--;
                Debug.Log($"Placed wooden wall. Remaining: {woodWallCount}");
            }
            else
            {
                Debug.Log("Cannot place wall here - space is occupied");
            }
        }
    }

    private void EatSelectedFood()
    {
        if (foodCount > 0 && playerStatsManager != null)
        {
            playerStatsManager.EatFood();
            foodCount--;
            Debug.Log($"Ate food. Remaining: {foodCount}");
        }
        else if (playerStatsManager == null)
        {
            Debug.LogError("PlayerStatsManager is missing! Make sure it's attached to the player or exists in the scene.");
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    #region Spear Management
    public void CollectSpear()
    {
        spearCount++;
        UpdateSpearVisibility();
        Debug.Log($"Collected spear. Total: {spearCount}");
    }

    private void UpdateSpearVisibility()
    {
        if (activeSpear == null)
        {
            CreateSpearVisual();
        }

        if (activeSpear != null)
        {
            bool shouldShowSpear = currentSelectedItem == "Spear" && spearCount > 0;
            activeSpear.SetActive(shouldShowSpear);
        }
    }

    public void ThrowSpear()
    {
        if (spearCount > 0 && currentSelectedItem == "Spear")
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // Hide the visual spear
            if (activeSpear != null)
            {
                activeSpear.SetActive(false);
            }

            // Create and throw the actual spear
            GameObject thrownSpear = Instantiate(spearPrefab, transform.position + spearOffset, Quaternion.identity);
            Spear spearComponent = thrownSpear.GetComponent<Spear>();
            if (spearComponent != null)
            {
                spearComponent.InitializeThrow(transform.position + spearOffset, mousePos);
                spearComponent.DecrementUses();
            }

            spearCount--;
            UpdateSpearVisibility();
            Debug.Log($"Threw spear. Remaining: {spearCount}");
        }
    }
    #endregion

    #region Resource Collection
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
    #endregion

    // wood wall thing
    public void AddWoodWall()
    {
        woodWallCount++;
        Debug.Log($"Added wooden wall to inventory. Total: {woodWallCount}");
    }

    #region Inventory Management
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

    public int GetItemCount(string itemName)
    {
        switch (itemName.ToLower())
        {
            case "wood":
                return woodCount;
            case "campfire":
                return campfireCount;
            case "spear":
                return spearCount;
            case "food":
                return foodCount;
            case "rock 1":
                return collectedRocks[0];
            case "rock 2":
                return collectedRocks[1];
            case "wooden wall":
                return woodWallCount;
            default:
                return 0;
        }
    }
    #endregion

    #region Footprint System
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
    #endregion
}