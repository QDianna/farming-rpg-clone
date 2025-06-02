using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventorySystemHUD : MonoBehaviour
{
    [Header("Main References")]
    // [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private InteractionCraftRecipe craftingBench;  // Reference to listen to crafting events

    [Header("Selected Item UI")] private VisualElement SelectedItemContainer;
    
    [Header("Full Inventory UI")]
    private VisualElement InventoryContainer;           // Container for full inventory (shown during crafting)
    private VisualElement InventoryItemsContainer;      // Where inventory items will be displayed
    
    // Dynamic inventory slots (created at runtime)
    private List<VisualElement> InventorySlots = new List<VisualElement>();

    private void Start()
    {
        InitializeUI();
        
        // Subscribe to inventory system events
        InventorySystem.Instance.OnSelectedItemChange += UpdateSelectedItemDisplay;
        InventorySystem.Instance.OnInventoryChanged += UpdateFullInventoryDisplay;
        
        // Subscribe to crafting bench events (to auto-open/close full inventory)
        if (craftingBench != null)
        {
            craftingBench.OnCraftingBenchOpened += ShowFullInventory;
            craftingBench.OnCraftingBenchClosed += HideFullInventory;
        }
        
        // Subscribe to inventory item clicks (for crafting integration)
        InventorySystem.Instance.OnInventoryItemClicked += OnInventoryItemClicked;
        
        UpdateSelectedItemDisplay();  // remove initial (test) values from ui builder
    }
    
    private void OnDisable()
    {
        // Unsubscribe from inventory system events
        InventorySystem.Instance.OnSelectedItemChange -= UpdateSelectedItemDisplay;
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= UpdateFullInventoryDisplay;
            InventorySystem.Instance.OnInventoryItemClicked -= OnInventoryItemClicked;
        }
        
        // Unsubscribe from crafting bench events
        if (craftingBench != null)
        {
            craftingBench.OnCraftingBenchOpened -= ShowFullInventory;
            craftingBench.OnCraftingBenchClosed -= HideFullInventory;
        }
    }
    
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Get selected item UI elements
        SelectedItemContainer = root.Q<VisualElement>("SelectedItemContainer");
        
        // Container children
        var selectedItemIcon = SelectedItemContainer.Q<VisualElement>("ItemIcon");
        var selectedItemQuantity = SelectedItemContainer.Q<Label>("ItemQuantity");
        
        // Get inventory container UI elements
        InventoryContainer = root.Q<VisualElement>("InventoryContainer");
        
        // Containter children
        InventoryItemsContainer = InventoryContainer.Q<VisualElement>("InventoryItemsContainer");
        
        // Hide full inventory at start
        HideFullInventory();
    }

    #region Selected Item Display (existing functionality)
    
    private void UpdateSelectedItemDisplay()
    {
        /*var root = GetComponent<UIDocument>().rootVisualElement;

        // Get selected item UI elements (existing)
        SelectedItemContainer = root.Q<VisualElement>("SelectedItem");*/
        
        // Container children
        var selectedItemIcon = SelectedItemContainer.Q<VisualElement>("ItemIcon");
        var selectedItemQuantity = SelectedItemContainer.Q<Label>("ItemQuantity");
        
        var selectedItem = InventorySystem.Instance.GetSelectedItem();
        if (selectedItem != null && selectedItem.itemSprite != null)
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
    
    #endregion
    
    #region Full Inventory Display (new functionality for crafting)
    
    public void ShowFullInventory()
    {
        InventoryContainer.style.display = DisplayStyle.Flex;
        Debug.Log("Full inventory opened for crafting!");
        UpdateFullInventoryDisplay();
    }
    
    public void HideFullInventory()
    {
        InventoryContainer.style.display = DisplayStyle.None;
        Debug.Log("Full inventory closed!");
    }
    
    private void UpdateFullInventoryDisplay()
    {
        // Clear existing slots
        ClearInventorySlots();
        
        // Get all items from inventory system
        var allItems = InventorySystem.Instance.GetAllItems();
        
        // Create a slot for each inventory entry
        foreach (var entry in allItems)
        {
            CreateInventorySlot(entry);
        }
        
        Debug.Log($"Full inventory updated: {allItems.Count} different items");
    }
    
    private void ClearInventorySlots()
    {
        foreach (var slot in InventorySlots)
        {
            InventoryItemsContainer.Remove(slot);
        }
        InventorySlots.Clear();
    }
    
    private void CreateInventorySlot(InventoryEntry entry)
    {
        // Create slot container
        var slotContainer = new VisualElement();
        slotContainer.AddToClassList("inventory-slot");  // Add CSS class for styling
        
        // Create inner element for sprite
        var slotIcon = new VisualElement();
        slotIcon.AddToClassList("inventory-slot-icon");
        
        // Set background image
        if (entry.item.itemSprite != null)
        {
            slotIcon.style.backgroundImage = new StyleBackground(entry.item.itemSprite);
        }
        
        // Create quantity label
        var quantityLabel = new Label(entry.quantity.ToString());
        quantityLabel.AddToClassList("inventory-slot-quantity");
        
        // Set tooltip
        slotContainer.tooltip = $"{entry.quantity}x {entry.item.name}";
        
        // Add click event
        slotContainer.RegisterCallback<ClickEvent>(evt => OnInventorySlotClicked(entry.item));
        
        // Assemble slot
        slotContainer.Add(slotIcon);
        slotContainer.Add(quantityLabel);
        
        // Add to container and track
        InventoryItemsContainer.Add(slotContainer);
        InventorySlots.Add(slotContainer);
    }
    
    private void OnInventorySlotClicked(InventoryItem item)
    {
        // Trigger the inventory system's click event
        InventorySystem.Instance.TriggerItemClick(item);
    }
    
    // This gets called when InventorySystem.OnInventoryItemClicked is triggered
    private void OnInventoryItemClicked(InventoryItem item)
    {
        Debug.Log($"Inventory item {item.name} was clicked - this will be handled by CraftingSystemHUD");
        // CraftingSystemHUD will handle the actual crafting logic
    }
    
    #endregion
}