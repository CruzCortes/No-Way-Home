using UnityEngine;
using System.Collections;

public class EyeEnemyAI : MonoBehaviour
{
    [Header("Target Settings")]
    public float detectionRange = 10f;
    public float attackRange = 3f;
    public float maxDistanceFromStart = 15f;  // Original wander radius
    public float maxTrackingDistance = 50f;   // Larger tracking radius
    private Transform player;

    [Header("Movement Settings")]
    public float flyingSpeed = 1.5f;
    public float soarAmplitude = 0.8f;
    public float soarFrequency = 0.8f;
    public float wanderRadius = 4f;
    public float maxRotationSpeed = 90f;

    [Header("Attack Settings")]
    public float chargeSpeed = 6f;
    public float chargeCooldown = 3f;
    public float chargeDistance = 3f;
    public float knockbackForce = 5f;
    public float baseAttackProbability = 0.05f;

    [Header("Damage Settings")]
    public float minDamage = 5f;
    public float maxDamage = 20f;
    private float damageInterval = 0.5f;
    private float lastDamageTime;
    private PlayerStatsManager playerStats;

    [Header("Aggression Settings")]
    public float aggressionLevel = 0f;              // Starts at 0
    public float naturalAggressionIncrease = 0.01f; // Increases slowly over time
    public float aggressionMultiplier = 1f;         // Multiplier based on player's actions
    public float maxAggressionLevel = 1f;           // Max aggression level
    public float environmentalImpactCounter = 0f;   // Player's destructive actions

    [Header("Flight Pattern Settings")]
    public float oscillationSpeed = 1f;
    public float oscillationAmplitude = 0.3f;
    public float spiralRadius = 0.5f;
    public float spiralSpeed = 1.5f;

    // State Management
    private bool isCharging = false;
    private bool canAttack = true;
    private Vector3 targetPosition;
    private Vector3 chargeDirection;
    private Vector3 startPosition;
    private float flightTime;
    private Matrix4x4 rotationMatrix;
    private Vector3 currentVelocity;
    private float currentRotation;
    private Vector3 originalScale;
    private float wanderChangeTimer;
    private float wanderChangeInterval = 3f;

    private void Start()
    {
        FindAndSetupPlayer();
        startPosition = transform.position;
        targetPosition = GetRandomWanderPoint();
        rotationMatrix = Matrix4x4.identity;
        currentVelocity = Vector3.zero;
        originalScale = transform.localScale;
        wanderChangeTimer = 0f;
        lastDamageTime = 0f;
    }

    private void FindAndSetupPlayer()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerObj in playerObjects)
        {
            playerStats = playerObj.GetComponent<PlayerStatsManager>();

            if (playerStats == null)
            {
                playerStats = GameObject.FindObjectOfType<PlayerStatsManager>();
            }

            if (playerStats != null)
            {
                player = playerObj.transform;
                return;
            }
        }

        Debug.LogError("PlayerStatsManager not found in any Player object or scene!");
    }

    private void Update()
    {
        flightTime += Time.deltaTime;

        // Increase aggression over time
        aggressionLevel = Mathf.Min(aggressionLevel + Time.deltaTime * naturalAggressionIncrease * aggressionMultiplier, maxAggressionLevel);

        float distanceFromStart = Vector2.Distance(transform.position, startPosition);
        if (distanceFromStart > maxDistanceFromStart)
        {
            Vector3 directionToStart = (startPosition - transform.position).normalized;
            transform.position += directionToStart * flyingSpeed * Time.deltaTime;
        }

        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (isCharging)
            {
                ExecuteChargeAttack();
            }
            else if (distanceToPlayer <= attackRange && canAttack && Random.value < baseAttackProbability)
            {
                StartCoroutine(ChargeAttack());
            }
            else
            {
                // Decide whether to follow player or wander based on aggression level
                float followProbability = aggressionLevel; // Higher aggression means more likely to follow
                if (Random.value < followProbability)
                {
                    FollowPlayerWithMatrixTransform();
                }
                else
                {
                    WanderWithMatrixTransform();
                }

                // Teleportation based on aggression
                if (distanceToPlayer > maxTrackingDistance)
                {
                    TeleportNearPlayer();
                }
            }
        }
        else
        {
            FindAndSetupPlayer();
            WanderWithMatrixTransform();
        }
    }

    private void TeleportNearPlayer()
    {
        if (player == null) return;

        // Teleport to a position near the player but at a distance
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float teleportDistance = Random.Range(detectionRange, detectionRange + 5f);
        Vector3 teleportPosition = player.position + (Vector3)(randomDirection * teleportDistance);

        transform.position = teleportPosition;
        currentVelocity = Vector3.zero;
        wanderChangeTimer = 0f;
        targetPosition = GetRandomWanderPoint();
    }

    private void FollowPlayerWithMatrixTransform()
    {
        if (player == null) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 desiredVelocity = directionToPlayer * flyingSpeed * (1f + aggressionLevel); // Increase speed with aggression

        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, Time.deltaTime * 2f);

        float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        float angleDiff = Mathf.DeltaAngle(currentRotation, targetAngle);
        float rotationAmount = Mathf.Clamp(angleDiff, -maxRotationSpeed * Time.deltaTime, maxRotationSpeed * Time.deltaTime);
        currentRotation += rotationAmount;

        rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, currentRotation));

        Vector3 oscillation = new Vector3(
            0,
            Mathf.Sin(flightTime * oscillationSpeed) * oscillationAmplitude,
            0
        );

        Vector3 spiralOffset = new Vector3(
            Mathf.Cos(flightTime * spiralSpeed) * spiralRadius * 0.3f,
            Mathf.Sin(flightTime * spiralSpeed) * spiralRadius * 0.3f,
            0
        );

        Vector3 finalPosition = transform.position + (currentVelocity * Time.deltaTime);
        finalPosition += rotationMatrix.MultiplyPoint3x4(oscillation) * 0.3f;
        finalPosition += rotationMatrix.MultiplyPoint3x4(spiralOffset) * 0.2f;

        transform.position = finalPosition;
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }

    private void WanderWithMatrixTransform()
    {
        wanderChangeTimer += Time.deltaTime * (1f - aggressionLevel); // Wander less as aggression increases
        if (wanderChangeTimer >= wanderChangeInterval)
        {
            targetPosition = GetRandomWanderPoint();
            wanderChangeTimer = 0f;
        }

        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        Vector3 desiredVelocity = directionToTarget * (flyingSpeed * 0.3f);

        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, Time.deltaTime * 1f);

        Vector3 soarOffset = new Vector3(
            0,
            Mathf.Sin(flightTime * soarFrequency) * soarAmplitude,
            0
        );

        float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        float angleDiff = Mathf.DeltaAngle(currentRotation, targetAngle);
        float rotationAmount = Mathf.Clamp(angleDiff, -maxRotationSpeed * 0.3f * Time.deltaTime, maxRotationSpeed * 0.3f * Time.deltaTime);
        currentRotation += rotationAmount;

        rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, currentRotation));

        Vector3 finalPosition = transform.position + (currentVelocity * Time.deltaTime);
        finalPosition += rotationMatrix.MultiplyPoint3x4(soarOffset) * 0.5f;

        transform.position = finalPosition;
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }

    private Vector3 GetRandomWanderPoint()
    {
        Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
        Vector3 newTarget = startPosition + new Vector3(randomPoint.x, randomPoint.y, 0);

        if (Vector2.Distance(newTarget, startPosition) > maxDistanceFromStart)
        {
            Vector2 directionToTarget = ((Vector2)newTarget - (Vector2)startPosition).normalized;
            newTarget = startPosition + (Vector3)(directionToTarget * maxDistanceFromStart * 0.8f);
        }

        return newTarget;
    }

    private IEnumerator ChargeAttack()
    {
        isCharging = true;
        canAttack = false;
        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;

        if (player != null)
        {
            chargeDirection = (player.position - transform.position).normalized;
            Vector3 chargeStart = transform.position;
            Vector3 chargeEnd = chargeStart + chargeDirection * Mathf.Min(chargeDistance, maxDistanceFromStart * 0.5f);

            // Wind-up
            float windUpTime = 0.5f;
            float elapsed = 0f;

            while (elapsed < windUpTime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / windUpTime;
                float scale = 1f + Mathf.Sin(progress * Mathf.PI * 2f) * 0.2f;
                transform.localScale = originalScale * scale;
                float windupRotation = progress * 360f * 2f;
                transform.rotation = Quaternion.Euler(0, 0, windupRotation);
                yield return null;
            }

            // Charge
            float chargeTime = Vector3.Distance(chargeStart, chargeEnd) / chargeSpeed;
            elapsed = 0f;

            while (elapsed < chargeTime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / chargeTime;
                float easedProgress = progress * (2 - progress);
                transform.position = Vector3.Lerp(chargeStart, chargeEnd, easedProgress);
                transform.Rotate(0, 0, 720f * Time.deltaTime);
                yield return null;
            }
        }

        // Reset
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
        isCharging = false;
        currentVelocity = Vector3.zero;

        yield return new WaitForSeconds(chargeCooldown);
        canAttack = true;
    }

    private void ExecuteChargeAttack()
    {
        transform.Rotate(0, 0, 720f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerStats = other.gameObject.GetComponent<PlayerStatsManager>();

            if (playerStats == null)
            {
                playerStats = other.gameObject.GetComponentInParent<PlayerStatsManager>();
            }

            if (playerStats == null)
            {
                playerStats = GameObject.FindObjectOfType<PlayerStatsManager>();
            }

            if (playerStats != null && isCharging)
            {
                DealDamageToPlayer();
            }

            // Apply knockback
            Rigidbody2D playerRb = other.attachedRigidbody;
            if (playerRb != null)
            {
                playerRb.AddForce(chargeDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isCharging)
        {
            float currentTime = Time.time;
            if (currentTime - lastDamageTime >= damageInterval)
            {
                DealDamageToPlayer();
            }
        }
    }

    private void DealDamageToPlayer()
    {
        if (playerStats != null)
        {
            float damage = Random.Range(minDamage, maxDamage);
            playerStats.TakeDamage(damage);
            lastDamageTime = Time.time;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxDistanceFromStart);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, maxTrackingDistance);
    }

    // Method to be called when the player destroys something in the environment
    public void IncreaseAggressionDueToPlayerAction()
    {
        environmentalImpactCounter += 0.1f; // Increment counter slightly
        aggressionMultiplier = 1f + environmentalImpactCounter; // Adjust aggression multiplier
    }
}
