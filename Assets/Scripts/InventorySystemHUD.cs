using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manages inventory UI display with selected item HUD and full inventory overlay.
/// Automatically shows/hides full inventory when crafting bench or market opens.
/// </summary>
public class InventorySystemHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private InteractionCraftRecipe craftingBench;
    [SerializeField] private InteractionMarket market;

    private VisualElement selectedItemContainer;
    private VisualElement inventoryContainer;
    private VisualElement inventoryItemsContainer;
    private List<VisualElement> inventorySlots = new List<VisualElement>();

    private void Awake()
    {
        InitializeUI();
    }
    
    private void Start()
    {
        SubscribeToEvents();
        UpdateSelectedItemDisplay();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        selectedItemContainer = root.Q<VisualElement>("SelectedItemContainer");
        inventoryContainer = root.Q<VisualElement>("InventoryContainer");
        inventoryItemsContainer = inventoryContainer.Q<VisualElement>("InventoryItemsContainer");
        
        HideFullInventory();
    }

    private void SubscribeToEvents()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnSelectedItemChange += UpdateSelectedItemDisplay;
            InventorySystem.Instance.OnInventoryChanged += UpdateFullInventoryDisplay;
            InventorySystem.Instance.OnInventoryItemClicked += OnInventoryItemClicked;
        }
        
        if (craftingBench != null)
        {
            craftingBench.OnCraftingBenchOpened += ShowFullInventory;
            craftingBench.OnCraftingBenchClosed += HideFullInventory;
        }

        if (market != null)
        {
            market.OnMarketOpened += ShowFullInventory;
            market.OnMarketClosed += HideFullInventory;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnSelectedItemChange -= UpdateSelectedItemDisplay;
            InventorySystem.Instance.OnInventoryChanged -= UpdateFullInventoryDisplay;
            InventorySystem.Instance.OnInventoryItemClicked -= OnInventoryItemClicked;
        }
        
        if (craftingBench != null)
        {
            craftingBench.OnCraftingBenchOpened -= ShowFullInventory;
            craftingBench.OnCraftingBenchClosed -= HideFullInventory;
        }
        
        if (market != null)
        {
            market.OnMarketOpened -= ShowFullInventory;
            market.OnMarketClosed -= HideFullInventory;
        }
    }
    
    private void UpdateSelectedItemDisplay()
    {
        var selectedItemIcon = selectedItemContainer.Q<VisualElement>("ItemIcon");
        var selectedItemQuantity = selectedItemContainer.Q<Label>("ItemQuantity");
        
        var selectedItem = InventorySystem.Instance.GetSelectedItem();
        if (selectedItem?.itemSprite != null)
        {
            selectedItemIcon.style.backgroundImage = new StyleBackground(selectedItem.itemSprite);
            selectedItemQuantity.text = "x" + InventorySystem.Instance.GetSelectedItemQuantity();
        }
        else
        {
            selectedItemIcon.style.backgroundImage = null;
            selectedItemQuantity.text = "";
        }
    }
    
    private void ShowFullInventory()
    {
        inventoryContainer.style.display = DisplayStyle.Flex;
        UpdateFullInventoryDisplay();
    }
    
    private void HideFullInventory()
    {
        inventoryContainer.style.display = DisplayStyle.None;
    }
    
    private void UpdateFullInventoryDisplay()
    {
        if (InventorySystem.Instance == null) return;
        
        ClearInventorySlots();
        
        var allItems = InventorySystem.Instance.GetAllItems();
        foreach (var entry in allItems)
        {
            CreateInventorySlot(entry);
        }
    }
    
    private void ClearInventorySlots()
    {
        foreach (var slot in inventorySlots)
        {
            inventoryItemsContainer.Remove(slot);
        }
        inventorySlots.Clear();
    }
    
    private void CreateInventorySlot(InventoryEntry entry)
    {
        if (entry?.item == null) return;
        
        var slotContainer = new VisualElement();
        slotContainer.AddToClassList("item-slot");
        
        var slotIcon = new VisualElement();
        slotIcon.AddToClassList("item-icon");
        
        if (entry.item.itemSprite != null)
        {
            slotIcon.style.backgroundImage = new StyleBackground(entry.item.itemSprite);
        }
        
        var quantityLabel = new Label(entry.quantity.ToString());
        quantityLabel.AddToClassList("item-quantity");
        
        slotContainer.tooltip = $"{entry.quantity}x {entry.item.itemName ?? "Unknown"}";
        slotContainer.RegisterCallback<ClickEvent>(evt => OnInventorySlotClicked(entry.item));
        
        slotContainer.Add(slotIcon);
        slotContainer.Add(quantityLabel);
        
        inventoryItemsContainer.Add(slotContainer);
        inventorySlots.Add(slotContainer);
    }
    
    private void OnInventorySlotClicked(InventoryItem item)
    {
        InventorySystem.Instance.TriggerItemClick(item);
    }
    
    private void OnInventoryItemClicked(InventoryItem item)
    {
        // Event forwarded to systems that need it (crafting/market)
    }
}