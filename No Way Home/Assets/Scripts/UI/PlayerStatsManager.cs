using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerStatsManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float maxTemperature = 100f;

    [Header("Decay Rates")]
    [SerializeField] private float hungerDecayRate = 10f;
    [SerializeField] private float healthDecayRate = 8f;
    [SerializeField] private float temperatureDecayRate = 5f;

    [Header("Body Temperature Settings")]
    [SerializeField] private float bodyTemperatureInertia = 0.1f;
    [SerializeField] private float environmentalResistance = 0.8f;
    [SerializeField] private float optimalTemperature = 70f;
    [SerializeField] private float freezingTemperature = 0f;
    [SerializeField] private float environmentalTemperatureEffect = 0.5f;

    [Header("UI Settings")]
    [SerializeField] private RectTransform statusPanel;
    [SerializeField] private float verticalPadding = 10f;
    [SerializeField] private float horizontalPadding = 20f;
    [SerializeField] private float barHeight = 5f;

    private float currentHealth;
    private float currentHunger;
    private float currentTemperature;
    private float currentHeatFromSource = 0f;
    private float temperatureExposureTime = 0f;
    private float previousEnvironmentTemp = 20f;
    private bool isDead = false;

    private Image healthBar;
    private Image hungerBar;
    private Image temperatureBar;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI hungerText;
    private TextMeshProUGUI temperatureText;

    private PlayerController playerController;
    private float baseMovementSpeed;

    private void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentTemperature = optimalTemperature; // Start at optimal body temperature
        CreateStatusBars();

        playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            baseMovementSpeed = playerController.moveSpeed;
        }
    }

    private void CreateStatusBars()
    {
        GameObject containerObj = new GameObject("BarsContainer");
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.SetParent(statusPanel, false);

        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.offsetMin = new Vector2(horizontalPadding, verticalPadding);
        containerRect.offsetMax = new Vector2(-horizontalPadding, -verticalPadding);

        float totalHeight = (barHeight * 3) + (verticalPadding * 2);
        float spacing = (containerRect.rect.height - totalHeight) / 4;

        Color healthColor = new Color(0.8f, 0.2f, 0.2f, 0.95f);
        Color hungerColor = new Color(1f, 0.8f, 0.2f, 0.95f);
        Color tempColor = new Color(0.1f, 0.2f, 0.4f, 0.95f);

        healthBar = CreateBar("HealthBar", healthColor, 0, spacing, containerRect);
        hungerBar = CreateBar("HungerBar", hungerColor, 1, spacing, containerRect);
        temperatureBar = CreateBar("TemperatureBar", tempColor, 2, spacing, containerRect);

        healthText = CreateText(healthBar.transform, "100%");
        hungerText = CreateText(hungerBar.transform, "100%");
        temperatureText = CreateText(temperatureBar.transform, "100%");

        foreach (var text in new[] { healthText, hungerText, temperatureText })
        {
            text.enableVertexGradient = true;
            text.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
            text.fontSharedMaterial.SetFloat("_UnderlayOffsetX", 1);
            text.fontSharedMaterial.SetFloat("_UnderlayOffsetY", -1);
            text.fontSharedMaterial.SetFloat("_UnderlayDilate", 1);
            text.fontSharedMaterial.SetFloat("_UnderlaySoftness", 0);
            text.fontSharedMaterial.SetColor("_UnderlayColor", new Color(0, 0, 0, 0.5f));
        }
    }

    private Image CreateBar(string name, Color color, int index, float spacing, RectTransform container)
    {
        GameObject barObj = new GameObject(name);
        RectTransform barRect = barObj.AddComponent<RectTransform>();
        barRect.SetParent(container);

        float topPosition = -(spacing * (index + 1) + barHeight * index);

        barRect.anchorMin = new Vector2(0, 1);
        barRect.anchorMax = new Vector2(1, 1);
        barRect.anchoredPosition = new Vector2(0, topPosition);
        barRect.sizeDelta = new Vector2(0, barHeight);

        GameObject bgObj = new GameObject("Background");
        Image bgImage = bgObj.AddComponent<Image>();
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.SetParent(barRect);
        bgRect.localPosition = Vector3.zero;
        bgRect.localScale = Vector3.one;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        bgImage.sprite = CreateRoundedRectSprite(400, 40, 8);

        GameObject fillObj = new GameObject("Fill");
        Image fillImage = fillObj.AddComponent<Image>();
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.SetParent(barRect);
        fillRect.localPosition = Vector3.zero;
        fillRect.localScale = Vector3.one;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        fillImage.sprite = CreateRoundedRectSprite(400, 40, 8);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.color = color;

        return fillImage;
    }

    private Sprite CreateRoundedRectSprite(int width, int height, int cornerRadius)
    {
        Texture2D texture = new Texture2D(width, height);

        Color[] colors = new Color[width * height];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.clear;
        }
        texture.SetPixels(colors);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inCorner = false;
                float distanceFromCorner = 0f;

                if (x < cornerRadius && y < cornerRadius)
                {
                    distanceFromCorner = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, cornerRadius));
                    inCorner = true;
                }
                else if (x >= width - cornerRadius && y < cornerRadius)
                {
                    distanceFromCorner = Vector2.Distance(new Vector2(x, y), new Vector2(width - cornerRadius, cornerRadius));
                    inCorner = true;
                }
                else if (x < cornerRadius && y >= height - cornerRadius)
                {
                    distanceFromCorner = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, height - cornerRadius));
                    inCorner = true;
                }
                else if (x >= width - cornerRadius && y >= height - cornerRadius)
                {
                    distanceFromCorner = Vector2.Distance(new Vector2(x, y), new Vector2(width - cornerRadius, height - cornerRadius));
                    inCorner = true;
                }

                if (inCorner)
                {
                    if (distanceFromCorner <= cornerRadius)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                }
                else
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.Tight
        );
    }

    private TextMeshProUGUI CreateText(Transform parent, string initialText)
    {
        GameObject textObj = new GameObject("Text");
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.SetParent(parent);
        textRect.localPosition = Vector3.zero;
        textRect.localScale = Vector3.one;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = initialText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = parent.GetComponent<RectTransform>().rect.height * 0.6f;
        tmp.color = Color.white;
        tmp.enableWordWrapping = false;

        return tmp;
    }

    private void Update()
    {
        if (isDead) return;

        // Hunger system
        currentHunger = Mathf.Max(0, currentHunger - (hungerDecayRate * Time.deltaTime));

        // Health system
        if (currentHunger <= 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - (healthDecayRate * Time.deltaTime));
        }

        // Temperature system
        UpdateTemperature();

        if (currentHealth <= 0 && !isDead)
        {
            HandlePlayerDeath();
        }

        UpdateUI();
        UpdatePlayerSpeed();
    }

    private void HandlePlayerDeath()
    {
        isDead = true;

        // Get the Player GameObject (since we know it has the "Player" tag)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Disable components and fade out player
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.enabled = false;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.simulated = false;
            }

            // Fade out and disable the player sprite
            SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                StartCoroutine(FadeOutAndDisable(player, spriteRenderer));
            }
            else
            {
                player.SetActive(false);
            }
        }

        // Show death screen
        DeathScreenManager.Instance.ShowDeathScreen();
    }

    private IEnumerator FadeOutAndDisable(GameObject player, SpriteRenderer spriteRenderer)
    {
        float fadeTime = 1f;
        float elapsedTime = 0;
        Color startColor = spriteRenderer.color;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Disable the entire player GameObject
        player.SetActive(false);
    }

    public void ResetPlayer()
    {
        // Reset all stats
        isDead = false;
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentTemperature = optimalTemperature;
        currentHeatFromSource = 0f;
        temperatureExposureTime = 0f;

        // Update UI
        UpdateUI();

        // Reset player's components
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.moveSpeed = baseMovementSpeed; // Reset movement speed
        }

        // Make sure rigidbody is properly reset
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = false;
            rb.simulated = true;
        }
    }

    private void UpdateTemperature()
    {
        if (WeatherSystem.Instance == null) return;

        float envTemp = WeatherSystem.Instance.GetCurrentTemperature();
        float windSpeed = WeatherSystem.Instance.GetWindSpeed();

        // Track exposure time
        if (Mathf.Abs(previousEnvironmentTemp - envTemp) > 5f)
        {
            temperatureExposureTime = 0f;
        }
        else
        {
            temperatureExposureTime += Time.deltaTime;
        }
        previousEnvironmentTemp = envTemp;

        // Body's natural regulation toward optimal temperature
        float tempDifferenceFromOptimal = optimalTemperature - currentTemperature;
        float bodyRegulation = tempDifferenceFromOptimal * bodyTemperatureInertia * Time.deltaTime;

        // Environmental effect
        float environmentalDifference = envTemp - currentTemperature;
        float environmentalEffect = environmentalDifference *
                                  environmentalTemperatureEffect *
                                  (1f - environmentalResistance) *
                                  Mathf.Clamp01(temperatureExposureTime / 60f) *
                                  Time.deltaTime;

        // Wind chill effect
        float windChill = 0f;
        if (currentTemperature < optimalTemperature - 10f)
        {
            float coldnessFactor = (optimalTemperature - currentTemperature) / optimalTemperature;
            windChill = windSpeed * 0.2f * coldnessFactor * Mathf.Clamp01(temperatureExposureTime / 60f) * Time.deltaTime;
        }

        // Heat source effect
        float heatEffect = 0f;
        if (currentHeatFromSource > 0)
        {
            float coldnessFactor = 1f + Mathf.Abs(optimalTemperature - currentTemperature) / optimalTemperature;
            heatEffect = currentHeatFromSource * coldnessFactor * Time.deltaTime;
        }

        // Natural temperature decay
        float tempDifference = Mathf.Abs(optimalTemperature - currentTemperature);
        float decayFactor = tempDifference / optimalTemperature;
        float decay = temperatureDecayRate * decayFactor * Time.deltaTime;

        // Calculate final temperature change
        float tempChange = bodyRegulation + environmentalEffect + heatEffect - windChill - decay;

        // Apply change (increase the maximum change per frame)
        currentTemperature = Mathf.MoveTowards(
            currentTemperature,
            currentTemperature + tempChange,
            Time.deltaTime * 10f // Increased from 3f to 10f
        );

        // Clamp to valid range
        currentTemperature = Mathf.Clamp(currentTemperature, 0f, maxTemperature);

        // Debug.Log($"Temperature updated: {currentTemperature}, HeatEffect: {heatEffect}, TempChange: {tempChange}");
    }

    public void SetHeatSource(float heatAmount)
    {
        // Debug.Log($"SetHeatSource called with heatAmount: {heatAmount}");
        currentHeatFromSource = heatAmount;
    }

    private void UpdatePlayerSpeed()
    {
        if (playerController == null) return;

        float speedMultiplier = 1f;

        if (currentTemperature <= freezingTemperature)
        {
            speedMultiplier = 0.1f;
        }
        else if (currentTemperature < optimalTemperature - 20f)
        {
            float normalizedTemp = (currentTemperature - freezingTemperature) / (optimalTemperature - 20f - freezingTemperature);
            speedMultiplier = Mathf.Lerp(0.1f, 0.7f, normalizedTemp);
        }
        else if (currentTemperature < optimalTemperature - 10f)
        {
            speedMultiplier = 0.8f;
        }

        playerController.moveSpeed = baseMovementSpeed * speedMultiplier;
    }

    private void UpdateUI()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
        hungerBar.fillAmount = currentHunger / maxHunger;
        temperatureBar.fillAmount = currentTemperature / maxTemperature;

        healthText.text = $"{Mathf.Round(currentHealth)}%";
        hungerText.text = $"{Mathf.Round(currentHunger)}%";
        temperatureText.text = $"{Mathf.Round(currentTemperature)}%";
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateUI();
    }

    public void EatFood()
    {
        if (isDead) return;
        float recoveryAmount = maxHunger * 0.25f;
        currentHunger = Mathf.Min(maxHunger, currentHunger + recoveryAmount);
        UpdateUI();
    }

}