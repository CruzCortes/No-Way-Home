using UnityEngine;
using System.Collections.Generic;

public class BunnyMovement : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minHopHeight = 0.2f;
    [SerializeField] private float maxHopHeight = 0.4f;
    [SerializeField] private float hopDistance = 0.5f; // Distance per hop
    [SerializeField] private float wanderRadius = 5f;

    [Header("AI Parameters")]
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;
    [SerializeField] private float hopDuration = 0.4f; // Time for each individual hop

    [Header("Collider Settings")]
    [SerializeField] private float colliderRadius = 0.1f;
    [SerializeField] private Vector2 colliderOffset = Vector2.zero;

    [Header("Sprite Settings")]
    [SerializeField] private Vector2 spriteScale = new Vector2(2f, 2f);

    private Vector2 currentTarget;
    private Vector2 startPosition;
    private bool isMoving = false;
    private float waitTimer;
    private CircleCollider2D circleCollider;

    // Hopping variables
    private List<Vector2> hopPoints = new List<Vector2>();
    private int currentHopIndex = 0;
    private float currentHopTime = 0f;
    private Vector2 currentHopStart;
    private Vector2 currentHopEnd;
    private float currentHopHeight;

    private void Awake()
    {
        circleCollider = gameObject.GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        UpdateColliderSettings();
        transform.localScale = spriteScale;
    }

    private void UpdateColliderSettings()
    {
        if (circleCollider != null)
        {
            circleCollider.radius = colliderRadius;
            circleCollider.offset = colliderOffset;
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

        // Handle hopping movement
        if (currentHopIndex < hopPoints.Count - 1)
        {
            currentHopTime += Time.deltaTime;
            float hopProgress = currentHopTime / hopDuration;

            if (hopProgress >= 1f)
            {
                // Move to next hop
                currentHopIndex++;
                if (currentHopIndex < hopPoints.Count - 1)
                {
                    StartNewHop();
                }
                else
                {
                    // Reached final destination
                    isMoving = false;
                    waitTimer = Random.Range(minWaitTime, maxWaitTime);
                    transform.position = hopPoints[hopPoints.Count - 1];
                }
            }
            else
            {
                // Calculate current position in hop
                Vector2 currentPos = Vector2.Lerp(currentHopStart, currentHopEnd, hopProgress);

                // Add arc movement
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

        // Generate hop points along the path
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

        // Add start point
        hopPoints.Add((Vector2)transform.position);

        // Generate intermediate hop points
        for (int i = 1; i < numberOfHops; i++)
        {
            float distance = i * hopDistance;
            if (distance > totalDistance) break;

            // Add some randomness to hop points
            Vector2 idealPoint = (Vector2)transform.position + direction * distance;
            float randomOffset = Random.Range(-0.1f, 0.1f);
            Vector2 perpendicular = new Vector2(-direction.y, direction.x) * randomOffset;

            hopPoints.Add(idealPoint + perpendicular);
        }

        // Add end point
        hopPoints.Add(currentTarget);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        // Draw hop points and path
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

    private void OnValidate()
    {
        UpdateColliderSettings();
    }
}