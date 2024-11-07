using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float maxTemperature = 100f;

    [Header("Decay Rates")]
    [SerializeField] private float hungerDecayRate = 10f;
    [SerializeField] private float healthDecayRate = 8f;

    [Header("UI Settings")]
    [SerializeField] private RectTransform statusPanel;
    [SerializeField] private float verticalPadding = 10f;  // Padding between bars
    [SerializeField] private float horizontalPadding = 20f;  // Padding from edges
    [SerializeField] private float barHeight = 5f;  // Height of each bar

    private float currentHealth;
    private float currentHunger;
    private float currentTemperature;
    private bool isDead = false;
    public bool isNearFire = false;

    private Image healthBar;
    private Image hungerBar;
    private Image temperatureBar;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI hungerText;
    private TextMeshProUGUI temperatureText;

    private void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentTemperature = maxTemperature;
        CreateStatusBars();
    }

    private void CreateStatusBars()
    {
        // Set up the container
        GameObject containerObj = new GameObject("BarsContainer");
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.SetParent(statusPanel, false);

        // Set container to fill parent with padding
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.offsetMin = new Vector2(horizontalPadding, verticalPadding);
        containerRect.offsetMax = new Vector2(-horizontalPadding, -verticalPadding);

        // Calculate total height needed
        float totalHeight = (barHeight * 3) + (verticalPadding * 2);
        float spacing = (containerRect.rect.height - totalHeight) / 4; // Divide remaining space

        // Create bars with new colors
        Color healthColor = new Color(0.8f, 0.2f, 0.2f, 0.95f);  // Strong red
        Color hungerColor = new Color(1f, 0.8f, 0.2f, 0.95f);    // Yellow
        Color tempColor = new Color(0.1f, 0.2f, 0.4f, 0.95f);    // Dark navy blue

        healthBar = CreateBar("HealthBar", healthColor, 0, spacing, containerRect);
        hungerBar = CreateBar("HungerBar", hungerColor, 1, spacing, containerRect);
        temperatureBar = CreateBar("TemperatureBar", tempColor, 2, spacing, containerRect);

        // Create texts with improved visibility
        healthText = CreateText(healthBar.transform, "100%");
        hungerText = CreateText(hungerBar.transform, "100%");
        temperatureText = CreateText(temperatureBar.transform, "100%");

        // Add drop shadow to text for better readability
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

        // Calculate position from top
        float topPosition = -(spacing * (index + 1) + barHeight * index);

        // Set anchors to stretch horizontally but maintain fixed height
        barRect.anchorMin = new Vector2(0, 1);
        barRect.anchorMax = new Vector2(1, 1);
        barRect.anchoredPosition = new Vector2(0, topPosition);
        barRect.sizeDelta = new Vector2(0, barHeight);

        // Create background
        GameObject bgObj = new GameObject("Background");
        Image bgImage = bgObj.AddComponent<Image>();
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.SetParent(barRect);
        bgRect.localPosition = Vector3.zero;
        bgRect.localScale = Vector3.one;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Dark semi-transparent background
        bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        bgImage.sprite = CreateRoundedRectSprite(400, 40, 8); // Width, height, and corner radius

        // Create fill
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

        // Fill texture with transparent pixels initially
        Color[] colors = new Color[width * height];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.clear;
        }
        texture.SetPixels(colors);

        // Draw the rounded rectangle
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Check if we're in a corner region
                bool inCorner = false;
                float distanceFromCorner = 0f;

                // Top-left corner
                if (x < cornerRadius && y < cornerRadius)
                {
                    distanceFromCorner = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, cornerRadius));
                    inCorner = true;
                }
                // Top-right corner
                else if (x >= width - cornerRadius && y < cornerRadius)
                {
                    distanceFromCorner = Vector2.Distance(new Vector2(x, y), new Vector2(width - cornerRadius, cornerRadius));
                    inCorner = true;
                }
                // Bottom-left corner
                else if (x < cornerRadius && y >= height - cornerRadius)
                {
                    distanceFromCorner = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, height - cornerRadius));
                    inCorner = true;
                }
                // Bottom-right corner
                else if (x >= width - cornerRadius && y >= height - cornerRadius)
                {
                    distanceFromCorner = Vector2.Distance(new Vector2(x, y), new Vector2(width - cornerRadius, height - cornerRadius));
                    inCorner = true;
                }

                if (inCorner)
                {
                    // Set pixel based on distance from corner
                    if (distanceFromCorner <= cornerRadius)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                }
                else
                {
                    // Non-corner pixels are always white
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

        // Hunger decreases faster for testing
        currentHunger = Mathf.Max(0, currentHunger - (hungerDecayRate * Time.deltaTime));

        // Health decreases when hunger is 0
        if (currentHunger <= 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - (healthDecayRate * Time.deltaTime));
        }

        // Temperature handling
        if (isNearFire && currentTemperature < maxTemperature)
        {
            currentTemperature = Mathf.Min(maxTemperature, currentTemperature + (10f * Time.deltaTime));
        }
        else if (!isNearFire && currentTemperature > 0)
        {
            currentTemperature = Mathf.Max(0, currentTemperature - (5f * Time.deltaTime));
        }

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            Debug.Log("Player has died!");
        }

        UpdateUI();
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
        float recoveryAmount = maxHunger * 0.25f; // 25% of max hunger
        currentHunger = Mathf.Min(maxHunger, currentHunger + recoveryAmount);
        UpdateUI();
    }

    public void SetNearFire(bool near)
    {
        isNearFire = near;
    }
}