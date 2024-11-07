using UnityEngine;
using System.Collections.Generic;

public class BunnyMovement : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float minHopHeight = 0.2f;
    [SerializeField] private float maxHopHeight = 0.4f;
    [SerializeField] private float hopDistance = 0.5f;
    [SerializeField] private float wanderRadius = 5f;

    [Header("AI Parameters")]
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;
    [SerializeField] private float hopDuration = 0.4f;

    [Header("Collider Settings")]
    [SerializeField] private float colliderRadius = 0.1f;
    [SerializeField] private Vector2 colliderOffset = Vector2.zero;

    [Header("Sprite Settings")]
    [SerializeField] private Vector2 spriteScale = new Vector2(2f, 2f);

    [Header("Hunting Settings")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Vector2 foodSpawnOffset = new Vector2(0f, 0.5f);

    private Vector2 currentTarget;
    private Vector2 startPosition;
    private bool isMoving = false;
    private float waitTimer;
    private CircleCollider2D circleCollider;
    private List<Vector2> hopPoints = new List<Vector2>();
    private int currentHopIndex = 0;
    private float currentHopTime = 0f;
    private Vector2 currentHopStart;
    private Vector2 currentHopEnd;
    private float currentHopHeight;

    private void Awake()
    {
        // Setup collider
        circleCollider = gameObject.GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        UpdateColliderSettings();
        transform.localScale = spriteScale;

        // Setup Rigidbody2D
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.isKinematic = true;

        Debug.Log("Bunny initialized with Rigidbody2D and CircleCollider2D");
    }

    private void UpdateColliderSettings()
    {
        if (circleCollider != null)
        {
            circleCollider.radius = colliderRadius;
            circleCollider.offset = colliderOffset;
            circleCollider.isTrigger = true;
            Debug.Log($"Bunny collider updated - radius: {colliderRadius}, isTrigger: true");
        }
    }

    private void Start()
    {
        startPosition = transform.position;
        SetNewRandomTarget();
    }

    private void Update()
    {
        if (!isMoving)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                SetNewRandomTarget();
            }
            return;
        }

        if (currentHopIndex < hopPoints.Count - 1)
        {
            currentHopTime += Time.deltaTime;
            float hopProgress = currentHopTime / hopDuration;

            if (hopProgress >= 1f)
            {
                currentHopIndex++;
                if (currentHopIndex < hopPoints.Count - 1)
                {
                    StartNewHop();
                }
                else
                {
                    isMoving = false;
                    waitTimer = Random.Range(minWaitTime, maxWaitTime);
                    transform.position = hopPoints[hopPoints.Count - 1];
                }
            }
            else
            {
                Vector2 currentPos = Vector2.Lerp(currentHopStart, currentHopEnd, hopProgress);
                float heightMultiplier = Mathf.Sin(hopProgress * Mathf.PI);
                Vector3 hopOffset = Vector3.up * (currentHopHeight * heightMultiplier);
                transform.position = new Vector3(currentPos.x, currentPos.y, 0) + hopOffset;

                // Update sprite direction
                Vector3 newScale = spriteScale;
                if (currentHopEnd.x < currentHopStart.x)
                {
                    newScale.x = -Mathf.Abs(spriteScale.x);
                }
                else
                {
                    newScale.x = Mathf.Abs(spriteScale.x);
                }
                transform.localScale = newScale;
            }
        }
    }

    private void StartNewHop()
    {
        currentHopStart = hopPoints[currentHopIndex];
        currentHopEnd = hopPoints[currentHopIndex + 1];
        currentHopTime = 0f;
        currentHopHeight = Random.Range(minHopHeight, maxHopHeight);
    }

    private void SetNewRandomTarget()
    {
        startPosition = transform.position;
        float randomAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(1f, wanderRadius);

        Vector2 offset = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomDistance,
            Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomDistance
        );

        currentTarget = (Vector2)transform.position + offset;
        GenerateHopPoints();

        currentHopIndex = 0;
        isMoving = true;
        StartNewHop();
    }

    private void GenerateHopPoints()
    {
        hopPoints.Clear();
        Vector2 direction = (currentTarget - (Vector2)transform.position).normalized;
        float totalDistance = Vector2.Distance(transform.position, currentTarget);
        int numberOfHops = Mathf.CeilToInt(totalDistance / hopDistance);

        hopPoints.Add((Vector2)transform.position);

        for (int i = 1; i < numberOfHops; i++)
        {
            float distance = i * hopDistance;
            if (distance > totalDistance) break;

            Vector2 idealPoint = (Vector2)transform.position + direction * distance;
            float randomOffset = Random.Range(-0.1f, 0.1f);
            Vector2 perpendicular = new Vector2(-direction.y, direction.x) * randomOffset;

            hopPoints.Add(idealPoint + perpendicular);
        }

        hopPoints.Add(currentTarget);
    }

    public void OnHit()
    {
        Debug.Log($"Bunny hit! Spawning food at {gameObject.name}");
        if (foodPrefab != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(foodSpawnOffset.x, foodSpawnOffset.y, 0);
            GameObject food = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Spawned food at position: {spawnPosition}");
        }
        else
        {
            Debug.LogError("Food prefab not assigned to bunny!");
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Bunny triggered with: {other.gameObject.name}");
        Spear spear = other.GetComponent<Spear>();
        if (spear != null)
        {
            Debug.Log("Bunny hit by spear!");
        }
    }

    private void OnValidate()
    {
        UpdateColliderSettings();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        if (hopPoints.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < hopPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(hopPoints[i], hopPoints[i + 1]);
                Gizmos.DrawWireSphere(hopPoints[i], 0.1f);
            }
            Gizmos.DrawWireSphere(hopPoints[hopPoints.Count - 1], 0.1f);
        }
    }
}