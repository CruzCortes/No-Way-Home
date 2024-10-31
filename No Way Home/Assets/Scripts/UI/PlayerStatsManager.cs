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

    [Header("UI References")]
    [SerializeField] private RectTransform statusPanel;

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
        // Get panel dimensions from RectTransform
        float panelWidth = statusPanel.rect.width;
        float panelHeight = statusPanel.rect.height;

        // Calculate bar dimensions
        float totalBarsHeight = panelHeight * 0.9f; // Use 90% of panel height
        float barHeight = totalBarsHeight / 3f; // Divide by number of bars
        float barWidth = panelWidth * 0.9f; // Use 90% of panel width
        float verticalSpacing = (totalBarsHeight - (barHeight * 3)) / 2; // Space between bars

        // Create bars within the existing status panel
        healthBar = CreateBar("HealthBar", new Color(1, 0, 0, 0.8f), 0, barHeight, barWidth, verticalSpacing);
        hungerBar = CreateBar("HungerBar", new Color(0, 1, 0, 0.8f), 1, barHeight, barWidth, verticalSpacing);
        temperatureBar = CreateBar("TemperatureBar", new Color(0, 0, 1, 0.8f), 2, barHeight, barWidth, verticalSpacing);

        // Create texts
        healthText = CreateText(healthBar.transform, "100%");
        hungerText = CreateText(hungerBar.transform, "100%");
        temperatureText = CreateText(temperatureBar.transform, "100%");
    }

    private Image CreateBar(string name, Color color, int index, float height, float barWidth, float spacing)
    {
        GameObject barObj = new GameObject(name);
        RectTransform barRect = barObj.AddComponent<RectTransform>();
        barRect.SetParent(statusPanel);

        // Keep bars within the status panel bounds
        barRect.localPosition = Vector3.zero;
        barRect.localScale = Vector3.one;

        // Position bars relative to the panel's top
        float topOffset = (height + spacing) * index;
        barRect.anchoredPosition = new Vector2(0, -topOffset - (height * 0.5f));

        // Set size and anchors
        barRect.anchorMin = new Vector2(0.05f, 1f);
        barRect.anchorMax = new Vector2(0.95f, 1f);
        barRect.sizeDelta = new Vector2(0, height);

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
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

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
        fillImage.color = color;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        return fillImage;
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
        currentHunger = Mathf.Min(maxHunger, currentHunger + 30f);
        UpdateUI();
    }

    public void SetNearFire(bool near)
    {
        isNearFire = near;
    }
}