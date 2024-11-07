using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotBarManager : MonoBehaviour
{
    [Header("Panel Reference")]
    [SerializeField] private RectTransform hotBarPanel;

    [Header("Hotbar Settings")]
    [SerializeField] private int numberOfSlots = 8;
    [SerializeField] private float slotSpacing = 10f;

    [Header("References")]
    [SerializeField] private PlayerController playerController;

    private RectTransform[] slotRects;
    private Image[] slotImages;
    private TextMeshProUGUI[] itemNameTexts;
    private TextMeshProUGUI[] quantityTexts;
    private int selectedSlotIndex = -1;

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

        CreateHotBarSlots();
        UpdateHotBarDisplay();
    }

    private void CreateHotBarSlots()
    {
        float panelWidth = hotBarPanel.rect.width;
        float panelHeight = hotBarPanel.rect.height;
        float slotSize = Mathf.Min(panelHeight * 0.9f, (panelWidth - (slotSpacing * (numberOfSlots + 1))) / numberOfSlots);

        slotRects = new RectTransform[numberOfSlots];
        slotImages = new Image[numberOfSlots];
        itemNameTexts = new TextMeshProUGUI[numberOfSlots];
        quantityTexts = new TextMeshProUGUI[numberOfSlots];

        float totalWidth = (slotSize * numberOfSlots) + (slotSpacing * (numberOfSlots - 1));
        float startX = -(totalWidth / 2);

        for (int i = 0; i < numberOfSlots; i++)
        {
            // Create slot container
            GameObject slotObj = new GameObject($"Slot_{i}");
            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.SetParent(hotBarPanel);
            slotRect.localScale = Vector3.one;

            float xPos = startX + (i * (slotSize + slotSpacing));
            slotRect.anchoredPosition = new Vector2(xPos, 0);
            slotRect.sizeDelta = new Vector2(slotSize, slotSize);
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);

            // Background
            GameObject bgObj = new GameObject("Background");
            Image bgImage = bgObj.AddComponent<Image>();
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.SetParent(slotRect);
            bgRect.localPosition = Vector3.zero;
            bgRect.localScale = Vector3.one;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            slotRects[i] = slotRect;
            slotImages[i] = bgImage;

            // Item Name Text
            GameObject nameObj = new GameObject("ItemName");
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.SetParent(slotRect);
            nameRect.localPosition = Vector3.zero;
            nameRect.localScale = Vector3.one;
            nameRect.anchorMin = new Vector2(0, 0.2f);
            nameRect.anchorMax = new Vector2(1, 0.8f);
            nameRect.sizeDelta = Vector2.zero;

            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontSize = slotSize * 0.25f;
            nameText.color = Color.white;
            nameText.enableWordWrapping = true;

            itemNameTexts[i] = nameText;

            // Quantity Text
            GameObject quantityObj = new GameObject("Quantity");
            TextMeshProUGUI quantityText = quantityObj.AddComponent<TextMeshProUGUI>();
            RectTransform quantityRect = quantityObj.GetComponent<RectTransform>();
            quantityRect.SetParent(slotRect);
            quantityRect.localPosition = Vector3.zero;
            quantityRect.localScale = Vector3.one;
            quantityRect.anchorMin = new Vector2(0.6f, 0);
            quantityRect.anchorMax = new Vector2(0.95f, 0.3f);
            quantityRect.sizeDelta = Vector2.zero;

            quantityText.alignment = TextAlignmentOptions.Right;
            quantityText.fontSize = slotSize * 0.25f;
            quantityText.color = Color.yellow;

            quantityTexts[i] = quantityText;

            // Click detection
            int slotIndex = i;
            Button button = slotObj.AddComponent<Button>();
            button.onClick.AddListener(() => OnSlotClicked(slotIndex));
        }
    }

    private void Update()
    {
        // Update display every frame to keep in sync with inventory
        UpdateHotBarDisplay();

        // Optional: Number key selection (1-8)
        for (int i = 0; i < numberOfSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                OnSlotClicked(i);
            }
        }
    }

    private void UpdateHotBarDisplay()
    {
        if (playerController == null) return;

        int currentSlot = 0;

        // Update display for rocks
        for (int i = 0; i < playerController.collectedRocks.Length; i++)
        {
            if (currentSlot < numberOfSlots)
            {
                itemNameTexts[currentSlot].text = $"Rock {i + 1}";
                quantityTexts[currentSlot].text = playerController.collectedRocks[i].ToString();
                UpdateSlotVisuals(currentSlot);
                currentSlot++;
            }
        }

        // Update display for wood
        if (currentSlot < numberOfSlots)
        {
            itemNameTexts[currentSlot].text = "Wood";
            quantityTexts[currentSlot].text = playerController.woodCount.ToString();
            UpdateSlotVisuals(currentSlot);
            currentSlot++;
        }

        // Update display for campfire
        if (currentSlot < numberOfSlots && playerController.campfireCount > 0)
        {
            itemNameTexts[currentSlot].text = "Campfire";
            quantityTexts[currentSlot].text = playerController.campfireCount.ToString();
            UpdateSlotVisuals(currentSlot);
            currentSlot++;
        }

        // Update display for spear
        if (currentSlot < numberOfSlots && playerController.spearCount > 0)
        {
            itemNameTexts[currentSlot].text = "Spear";
            quantityTexts[currentSlot].text = playerController.spearCount.ToString();
            UpdateSlotVisuals(currentSlot);
            currentSlot++;
        }

        // Clear remaining slots
        for (int i = currentSlot; i < numberOfSlots; i++)
        {
            itemNameTexts[i].text = "";
            quantityTexts[i].text = "";
            slotImages[i].color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        }
    }

    private void UpdateSlotVisuals(int index)
    {
        Color slotColor = (index == selectedSlotIndex)
            ? new Color(0.4f, 0.4f, 0.4f, 0.8f)
            : new Color(0.2f, 0.2f, 0.2f, 0.8f);
        slotImages[index].color = slotColor;
    }

    private void OnSlotClicked(int index)
    {
        if (index < numberOfSlots && !string.IsNullOrEmpty(itemNameTexts[index].text))
        {
            selectedSlotIndex = (selectedSlotIndex == index) ? -1 : index;
            UpdateHotBarDisplay();
            Debug.Log($"Selected slot {index}: {itemNameTexts[index].text}");
        }
    }

    public string GetSelectedItem()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < numberOfSlots)
        {
            return itemNameTexts[selectedSlotIndex].text;
        }
        return null;
    }
}