using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CraftingUIManager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private RectTransform craftingPanel;

    [Header("Colors")]
    [SerializeField] private Color structuresColor = new Color(0.2f, 0.6f, 0.8f, 0.9f);
    [SerializeField] private Color utilitiesColor = new Color(0.8f, 0.4f, 0.2f, 0.9f);
    [SerializeField] private Color foodColor = new Color(0.4f, 0.8f, 0.2f, 0.9f);
    [SerializeField] private Color systemsColor = new Color(0.6f, 0.2f, 0.8f, 0.9f);

    [Header("Input Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Q;

    private RectTransform navBar;
    private RectTransform contentContainer;
    private RectTransform selectionPanel;
    private RectTransform requirementsPanel;
    private Image selectionPanelBackground;
    private Image requirementsPanelBackground;
    private Dictionary<string, Button> navigationButtons;
    private CanvasGroup canvasGroup;
    private bool isVisible = false;

    private void Awake()
    {
        // Add CanvasGroup for easy visibility control
        canvasGroup = craftingPanel.gameObject.AddComponent<CanvasGroup>();
        HidePanel(); // Hide by default
    }

    private void Start()
    {
        SetupCraftingUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }
    }

    private void TogglePanel()
    {
        if (isVisible)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }

    private void ShowPanel()
    {
        isVisible = true;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HidePanel()
    {
        isVisible = false;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void SetupCraftingUI()
    {
        // Create main containers
        contentContainer = CreateUIElement("ContentContainer", craftingPanel);
        contentContainer.anchorMin = new Vector2(0, 0.1f);
        contentContainer.anchorMax = Vector2.one;
        contentContainer.sizeDelta = Vector2.zero;

        navBar = CreateUIElement("NavigationBar", craftingPanel);
        navBar.anchorMin = Vector2.zero;
        navBar.anchorMax = new Vector2(1, 0.1f);
        navBar.sizeDelta = Vector2.zero;

        // Setup navigation bar background with rounded corners
        var navBarBackground = navBar.gameObject.AddComponent<Image>();
        navBarBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        // Add rounded corners to navigation bar
        var navBarMask = navBar.gameObject.AddComponent<Mask>();
        navBarMask.showMaskGraphic = true;

        // Create selection and requirements panels
        SetupContentPanels();

        // Create navigation buttons
        SetupNavigationButtons();

        // Set initial category
        SetCategory("Structures");
    }

    private void SetupContentPanels()
    {
        // Left panel (Selection)
        selectionPanel = CreateUIElement("SelectionPanel", contentContainer);
        selectionPanel.anchorMin = new Vector2(0, 0);
        selectionPanel.anchorMax = new Vector2(0.7f, 1);
        selectionPanel.sizeDelta = Vector2.zero;
        selectionPanel.anchoredPosition = Vector2.zero;

        selectionPanelBackground = selectionPanel.gameObject.AddComponent<Image>();
        selectionPanelBackground.color = structuresColor;

        // Right side container
        var rightContainer = CreateUIElement("RightContainer", contentContainer);
        rightContainer.anchorMin = new Vector2(0.71f, 0);
        rightContainer.anchorMax = Vector2.one;
        rightContainer.sizeDelta = Vector2.zero;

        // Requirements panel (bottom part of right container)
        requirementsPanel = CreateUIElement("RequirementsPanel", rightContainer);
        requirementsPanel.anchorMin = new Vector2(0, 0);
        requirementsPanel.anchorMax = Vector2.one;
        requirementsPanel.sizeDelta = Vector2.zero;

        requirementsPanelBackground = requirementsPanel.gameObject.AddComponent<Image>();
        requirementsPanelBackground.color = structuresColor;

        // Craft button container (top of requirements panel)
        var craftButtonContainer = CreateUIElement("CraftButtonContainer", requirementsPanel);
        craftButtonContainer.anchorMin = new Vector2(0, 0.9f);
        craftButtonContainer.anchorMax = Vector2.one;
        craftButtonContainer.sizeDelta = Vector2.zero;

        // Create craft button
        var craftButton = CreateButton(craftButtonContainer, "Craft", new Vector2(0.1f, 0.1f));
        craftButton.GetComponent<RectTransform>().sizeDelta = new Vector2(-20, -10);
    }

    private void SetupNavigationButtons()
    {
        navigationButtons = new Dictionary<string, Button>();
        string[] categories = { "Structures", "Utilities", "Food", "Systems" };
        float buttonWidth = 1f / categories.Length;

        for (int i = 0; i < categories.Length; i++)
        {
            var buttonContainer = CreateUIElement($"{categories[i]}ButtonContainer", navBar);
            buttonContainer.anchorMin = new Vector2(buttonWidth * i, 0);
            buttonContainer.anchorMax = new Vector2(buttonWidth * (i + 1), 1);
            buttonContainer.sizeDelta = Vector2.zero;

            var button = CreateButton(buttonContainer, categories[i], new Vector2(0.05f, 0.15f));
            var buttonRect = button.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(-10, -10);

            string category = categories[i];
            button.onClick.AddListener(() => SetCategory(category));
            navigationButtons[category] = button;
        }
    }

    private RectTransform CreateUIElement(string name, Transform parent)
    {
        var go = new GameObject(name);
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent);
        rect.localScale = Vector3.one;
        rect.localPosition = Vector3.zero;
        return rect;
    }

    private Button CreateButton(RectTransform parent, string text, Vector2 padding)
    {
        var buttonObj = new GameObject($"{text}Button");
        var buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.SetParent(parent);
        buttonRect.localScale = Vector3.one;
        buttonRect.localPosition = Vector3.zero;
        buttonRect.anchorMin = padding;
        buttonRect.anchorMax = Vector2.one - padding;
        buttonRect.sizeDelta = Vector2.zero;

        var button = buttonObj.AddComponent<Button>();
        var buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);

        var textObj = new GameObject("Text");
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.SetParent(buttonRect);
        textRect.localScale = Vector3.one;
        textRect.localPosition = Vector3.zero;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.color = Color.white;

        return button;
    }

    private void SetCategory(string category)
    {
        Color categoryColor = structuresColor;
        switch (category)
        {
            case "Structures":
                categoryColor = structuresColor;
                break;
            case "Utilities":
                categoryColor = utilitiesColor;
                break;
            case "Food":
                categoryColor = foodColor;
                break;
            case "Systems":
                categoryColor = systemsColor;
                break;
        }

        selectionPanelBackground.color = categoryColor;
        requirementsPanelBackground.color = categoryColor;

        // Update button visual states
        foreach (var button in navigationButtons.Values)
        {
            var buttonImage = button.GetComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        }

        if (navigationButtons.TryGetValue(category, out Button selectedButton))
        {
            var buttonImage = selectedButton.GetComponent<Image>();
            buttonImage.color = categoryColor;
        }
    }
}