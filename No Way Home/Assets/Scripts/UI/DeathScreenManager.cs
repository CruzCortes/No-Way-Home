using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathScreenManager : MonoBehaviour
{
    private static DeathScreenManager instance;
    public static DeathScreenManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DeathScreenManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("DeathScreenManager");
                    instance = obj.AddComponent<DeathScreenManager>();
                }
            }
            return instance;
        }
    }

    private Canvas mainCanvas;
    private GameObject deathScreenPanel;
    private Image blackOverlay;
    private TextMeshProUGUI deathMessageText;
    private Button restartButton;
    private float fadeInDuration = 1f;
    private float currentFadeTime = 0f;
    private bool isFading = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            SetupDeathScreen();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void SetupDeathScreen()
    {
        // Find main canvas
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return;
        }

        // Create death screen panel
        deathScreenPanel = new GameObject("DeathScreenPanel");
        deathScreenPanel.transform.SetParent(mainCanvas.transform, false);

        // Setup RectTransform for full screen coverage
        RectTransform rectTransform = deathScreenPanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // Add semi-transparent black overlay
        GameObject overlayObj = new GameObject("BlackOverlay");
        overlayObj.transform.SetParent(deathScreenPanel.transform, false);
        blackOverlay = overlayObj.AddComponent<Image>();
        RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        blackOverlay.color = new Color(0, 0, 0, 0f);

        // Add death message
        GameObject messageObj = new GameObject("DeathMessage");
        messageObj.transform.SetParent(deathScreenPanel.transform, false);
        deathMessageText = messageObj.AddComponent<TextMeshProUGUI>();
        RectTransform messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.5f, 0.6f);
        messageRect.anchorMax = new Vector2(0.5f, 0.6f);
        messageRect.sizeDelta = new Vector2(800, 100);
        messageRect.anchoredPosition = Vector2.zero;

        deathMessageText.text = "You Died";
        deathMessageText.fontSize = 72;
        deathMessageText.color = new Color(1, 0, 0, 0); // Start fully transparent
        deathMessageText.font = TMP_Settings.defaultFontAsset;
        deathMessageText.alignment = TextAlignmentOptions.Center;

        // Add restart button
        GameObject buttonObj = new GameObject("RestartButton");
        buttonObj.transform.SetParent(deathScreenPanel.transform, false);
        restartButton = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRect.sizeDelta = new Vector2(200, 60);
        buttonRect.anchoredPosition = Vector2.zero;

        // Add button text
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        RectTransform textRect = buttonTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        buttonText.text = "Restart Game";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.font = TMP_Settings.defaultFontAsset;
        buttonText.alignment = TextAlignmentOptions.Center;

        // Setup button visuals and click handler
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0f); // Start fully transparent
        restartButton.onClick.AddListener(RestartGame);

        // Hide the death screen initially
        deathScreenPanel.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        deathScreenPanel.SetActive(true);
        isFading = true;
        currentFadeTime = 0f;
    }

    void Update()
    {
        if (isFading)
        {
            currentFadeTime += Time.deltaTime;
            float progress = currentFadeTime / fadeInDuration;

            if (progress <= 1f)
            {
                // Fade in the overlay
                float overlayAlpha = Mathf.Lerp(0f, 0.8f, progress);
                blackOverlay.color = new Color(0, 0, 0, overlayAlpha);

                // Fade in the text with a slight delay
                float textProgress = Mathf.Max(0, (progress - 0.3f) * 1.5f);
                if (textProgress <= 1f)
                {
                    deathMessageText.color = new Color(1, 0, 0, Mathf.Lerp(0, 1, textProgress));
                }

                // Fade in the button with more delay
                float buttonProgress = Mathf.Max(0, (progress - 0.6f) * 1.5f);
                if (buttonProgress <= 1f)
                {
                    Color buttonColor = restartButton.image.color;
                    buttonColor.a = Mathf.Lerp(0, 1, buttonProgress);
                    restartButton.image.color = buttonColor;

                    TextMeshProUGUI buttonText = restartButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        Color textColor = buttonText.color;
                        textColor.a = Mathf.Lerp(0, 1, buttonProgress);
                        buttonText.color = textColor;
                    }
                }
            }
            else
            {
                isFading = false;
            }
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}