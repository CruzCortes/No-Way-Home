using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CraftingUIManager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private RectTransform craftingPanel;
    [SerializeField] private PlayerController playerController;

    [Header("Colors")]
    [SerializeField] private Color structuresColor = new Color(0.2f, 0.6f, 0.8f, 0.9f);
    [SerializeField] private Color utilitiesColor = new Color(0.8f, 0.4f, 0.2f, 0.9f);
    [SerializeField] private Color foodColor = new Color(0.4f, 0.8f, 0.2f, 0.9f);
    [SerializeField] private Color systemsColor = new Color(0.6f, 0.2f, 0.8f, 0.9f);

    [Header("UI Settings")]
    [SerializeField] private float buttonCornerRadius = 10f;
    [SerializeField] private Color buttonNormalColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
    [SerializeField] private Color buttonDisabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

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

    // Crafting system variables
    private Button craftButton;
    private TextMeshProUGUI requirementsText;
    private TextMeshProUGUI selectedItemText;
    private const int CAMPFIRE_WOOD_COST = 3;
    private string selectedCraftableItem = null;
    private string currentCategory = "Structures";

    // wood wall 
    private const int WOODWALL_WOOD_COST = 4;

    private void Awake()
    {
        canvasGroup = craftingPanel.gameObject.AddComponent<CanvasGroup>();
        HidePanel();
    }

    private void Start()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController not found!");
            }
        }
        SetupCraftingUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }

        if (isVisible && selectedCraftableItem != null)
        {
            UpdateCraftingRequirements();
        }
    }

    private void TogglePanel()
    {
        if (isVisible) HidePanel();
        else ShowPanel();
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

        // Setup navigation bar background
        var navBarBackground = navBar.gameObject.AddComponent<Image>();
        navBarBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        SetupContentPanels();
        SetupNavigationButtons();
        SetupCraftingItems();
        SetCategory("Structures");
    }

    private void SetupContentPanels()
    {
        // Selection Panel (Left side)
        selectionPanel = CreateUIElement("SelectionPanel", contentContainer);
        selectionPanel.anchorMin = new Vector2(0, 0);
        selectionPanel.anchorMax = new Vector2(0.7f, 1);
        selectionPanel.sizeDelta = Vector2.zero;
        selectionPanelBackground = selectionPanel.gameObject.AddComponent<Image>();
        selectionPanelBackground.color = structuresColor;

        // Right side container
        var rightContainer = CreateUIElement("RightContainer", contentContainer);
        rightContainer.anchorMin = new Vector2(0.71f, 0);
        rightContainer.anchorMax = Vector2.one;
        rightContainer.sizeDelta = Vector2.zero;

        // Requirements Panel (Right side)
        requirementsPanel = CreateUIElement("RequirementsPanel", rightContainer);
        requirementsPanel.anchorMin = Vector2.zero;
        requirementsPanel.anchorMax = Vector2.one;
        requirementsPanel.sizeDelta = Vector2.zero;
        requirementsPanelBackground = requirementsPanel.gameObject.AddComponent<Image>();
        requirementsPanelBackground.color = structuresColor;

        // Selected Item Display
        var selectedItemObj = CreateUIElement("SelectedItem", requirementsPanel);
        selectedItemObj.anchorMin = new Vector2(0.1f, 0.8f);
        selectedItemObj.anchorMax = new Vector2(0.9f, 0.9f);
        selectedItemText = selectedItemObj.gameObject.AddComponent<TextMeshProUGUI>();
        selectedItemText.fontSize = 28;
        selectedItemText.color = Color.white;
        selectedItemText.alignment = TextAlignmentOptions.Center;
        selectedItemText.fontStyle = FontStyles.Bold;

        // Requirements Text - Centered
        var requirementsTextObj = CreateUIElement("RequirementsText", requirementsPanel);
        requirementsTextObj.anchorMin = new Vector2(0.1f, 0.2f);
        requirementsTextObj.anchorMax = new Vector2(0.9f, 0.8f);
        requirementsText = requirementsTextObj.gameObject.AddComponent<TextMeshProUGUI>();
        requirementsText.fontSize = 24;
        requirementsText.color = Color.white;
        requirementsText.alignment = TextAlignmentOptions.Center;
        requirementsText.text = "Select an item to craft";

        // Craft Button
        var craftButtonContainer = CreateUIElement("CraftButtonContainer", requirementsPanel);
        craftButtonContainer.anchorMin = new Vector2(0.2f, 0.05f);
        craftButtonContainer.anchorMax = new Vector2(0.8f, 0.15f);
        craftButton = CreateButton(craftButtonContainer, "Craft", new Vector2(0.05f, 0.1f));
        craftButton.onClick.AddListener(OnCraftButtonClicked);
        craftButton.interactable = false;
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

    private void SetupCraftingItems()
    {
        foreach (Transform child in selectionPanel)
        {
            if (child.name.Contains("Button"))
            {
                Destroy(child.gameObject);
            }
        }

        if (currentCategory == "Structures")
        {
            // First button (Campfire) - Position at top
            var campfireButton = CreateCraftingItemButton("Campfire", "A warm fire to keep you cozy\nCost: 3 Wood");
            var campfireRect = campfireButton.GetComponent<RectTransform>();
            campfireRect.anchorMin = new Vector2(0.05f, 0.85f);
            campfireRect.anchorMax = new Vector2(0.95f, 0.95f);
            campfireButton.onClick.AddListener(() => SelectCraftingItem("Campfire"));

            // Second button (Wooden Wall) - Position below Campfire
            var woodWallButton = CreateCraftingItemButton("Wooden Wall", "A sturdy wooden wall for protection\nCost: 4 Wood");
            var woodWallRect = woodWallButton.GetComponent<RectTransform>();
            woodWallRect.anchorMin = new Vector2(0.05f, 0.70f);
            woodWallRect.anchorMax = new Vector2(0.95f, 0.80f);
            woodWallButton.onClick.AddListener(() => SelectCraftingItem("Wooden Wall"));
        }
        else if (currentCategory == "Utilities")
        {
            var spearButton = CreateCraftingItemButton("Spear", "A throwing weapon\nCost: 1 Wood, 1 Rock Type 1");
            var spearRect = spearButton.GetComponent<RectTransform>();
            spearRect.anchorMin = new Vector2(0.05f, 0.85f);
            spearRect.anchorMax = new Vector2(0.95f, 0.95f);
            spearButton.onClick.AddListener(() => SelectCraftingItem("Spear"));
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
        buttonImage.color = buttonNormalColor;

        // Add button press effect
        var buttonEffect = button.gameObject.AddComponent<ButtonScaleEffect>();

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

    private Button CreateCraftingItemButton(string itemName, string description)
    {
        var buttonObj = CreateUIElement($"{itemName}Button", selectionPanel);
        buttonObj.anchorMin = new Vector2(0.05f, 0.8f);
        buttonObj.anchorMax = new Vector2(0.95f, 0.95f);
        buttonObj.sizeDelta = Vector2.zero;

        var button = buttonObj.gameObject.AddComponent<Button>();
        var buttonImage = buttonObj.gameObject.AddComponent<Image>();
        buttonImage.color = buttonNormalColor;

        // Add button press effect
        var buttonEffect = button.gameObject.AddComponent<ButtonScaleEffect>();

        // Text container
        var textContainer = CreateUIElement("TextContainer", buttonObj.transform);
        textContainer.anchorMin = Vector2.zero;
        textContainer.anchorMax = Vector2.one;
        textContainer.sizeDelta = Vector2.zero;

        // Item name
        var nameText = CreateUIElement("ItemName", textContainer);
        nameText.anchorMin = new Vector2(0, 0.6f);
        nameText.anchorMax = new Vector2(1, 1);
        var tmpName = nameText.gameObject.AddComponent<TextMeshProUGUI>();
        tmpName.text = itemName;
        tmpName.fontSize = 24;
        tmpName.color = Color.white;
        tmpName.alignment = TextAlignmentOptions.Center;

        // Description
        var descText = CreateUIElement("Description", textContainer);
        descText.anchorMin = new Vector2(0, 0);
        descText.anchorMax = new Vector2(1, 0.6f);
        var tmpDesc = descText.gameObject.AddComponent<TextMeshProUGUI>();
        tmpDesc.text = description;
        tmpDesc.fontSize = 18;
        tmpDesc.color = Color.white;
        tmpDesc.alignment = TextAlignmentOptions.Center;

        return button;
    }

    private void SetCategory(string category)
    {
        currentCategory = category;
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

        foreach (var pair in navigationButtons)
        {
            var buttonImage = pair.Value.GetComponent<Image>();
            buttonImage.color = buttonNormalColor;
        }

        if (navigationButtons.TryGetValue(category, out Button selectedButton))
        {
            var buttonImage = selectedButton.GetComponent<Image>();
            buttonImage.color = categoryColor;
        }

        selectedCraftableItem = null;
        selectedItemText.text = "";
        requirementsText.text = "Select an item to craft";
        craftButton.interactable = false;

        SetupCraftingItems();
    }

    private void SelectCraftingItem(string itemName)
    {
        selectedCraftableItem = itemName;
        selectedItemText.text = itemName;
        UpdateCraftingRequirements();
    }

    private void UpdateCraftingRequirements()
    {
        if (selectedCraftableItem == "Campfire")
        {
            bool canCraft = playerController.woodCount >= CAMPFIRE_WOOD_COST;
            requirementsText.text = $"Requirements:\n\nWood: {playerController.woodCount}/{CAMPFIRE_WOOD_COST}\n\n" +
                                  (canCraft ? "Ready to craft!" : "Not enough resources!");
            craftButton.interactable = canCraft;
            craftButton.GetComponent<Image>().color = canCraft ? buttonNormalColor : buttonDisabledColor;
        }
        else if (selectedCraftableItem == "Wooden Wall")
        {
            bool canCraft = playerController.woodCount >= WOODWALL_WOOD_COST;
            requirementsText.text = $"Requirements:\n\nWood: {playerController.woodCount}/{WOODWALL_WOOD_COST}\n\n" +
                                  (canCraft ? "Ready to craft!" : "Not enough resources!");
            craftButton.interactable = canCraft;
            craftButton.GetComponent<Image>().color = canCraft ? buttonNormalColor : buttonDisabledColor;
        }
        else if (selectedCraftableItem == "Spear")
        {
            bool hasWood = playerController.woodCount >= 1;
            bool hasRock = playerController.collectedRocks[0] >= 1;
            bool canCraft = hasWood && hasRock;

            requirementsText.text = $"Requirements:\n\nWood: {playerController.woodCount}/1\n" +
                                  $"Rock Type 1: {playerController.collectedRocks[0]}/1\n\n" +
                                  (canCraft ? "Ready to craft!" : "Not enough resources!");
            craftButton.interactable = canCraft;
            craftButton.GetComponent<Image>().color = canCraft ? buttonNormalColor : buttonDisabledColor;
        }
    }

    private void OnCraftButtonClicked()
    {
        if (selectedCraftableItem == "Campfire" && playerController.woodCount >= CAMPFIRE_WOOD_COST)
        {
            playerController.RemoveWood(CAMPFIRE_WOOD_COST);
            playerController.AddCampfire();
            UpdateCraftingRequirements();
            Debug.Log("Crafted a campfire!");
        }
        else if (selectedCraftableItem == "Wooden Wall" && playerController.woodCount >= WOODWALL_WOOD_COST)
        {
            playerController.RemoveWood(WOODWALL_WOOD_COST);
            playerController.AddWoodWall();
            UpdateCraftingRequirements();
            Debug.Log("Crafted a wooden wall!");
        }
        else if (selectedCraftableItem == "Spear" &&
                 playerController.woodCount >= 1 &&
                 playerController.collectedRocks[0] >= 1)
        {
            playerController.RemoveWood(1);
            playerController.collectedRocks[0]--;
            playerController.CollectSpear();
            UpdateCraftingRequirements();
            Debug.Log("Crafted a spear!");
        }
    }
}
public class ButtonScaleEffect : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private const float scaleAmount = 0.95f;
    private const float scaleSpeed = 10f;
    private bool isScalingDown = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;

        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        isScalingDown = true;
    }

    private void Update()
    {
        if (isScalingDown)
        {
            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                originalScale * scaleAmount,
                Time.deltaTime * scaleSpeed
            );

            if (Vector3.Distance(rectTransform.localScale, originalScale * scaleAmount) < 0.01f)
            {
                isScalingDown = false;
            }
        }
        else if (rectTransform.localScale != originalScale)
        {
            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                originalScale,
                Time.deltaTime * scaleSpeed
            );
        }
    }
}