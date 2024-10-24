using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Footprint Settings")]
    public GameObject footprintPrefab;
    public float footprintSpawnInterval = 0.3f;

    [Header("Footprint Positioning")]
    [SerializeField] private float horizontalFootSpacing = 0.15f;  // Closer spacing for left/right movement
    [SerializeField] private float verticalFootSpacing = 0.3f;    // Wider spacing for up/down movement
    [SerializeField] private float footprintVerticalOffset = -0.9f;  // Vertical offset to place at feet

    [Header("Footprint Variation")]
    [SerializeField] private float scaleVariation = 0.2f;
    [SerializeField] private float rotationVariation = 15f;

    private float footprintTimer = 0f;
    private bool isLeftFoot = true;
    private Vector2 lastMovementDirection = Vector2.right;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement != Vector2.zero)
        {
            lastMovementDirection = movement.normalized;
        }

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