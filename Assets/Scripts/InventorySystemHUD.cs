using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Inventory UI manager handling selected item HUD and full inventory overlay.
/// Automatically displays full inventory when interacting with crafting, market, or research systems.
/// </summary>
public class InventorySystemHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private InteractionCraftRecipe craftingBench;
    [SerializeField] private InteractionMarket market;
    [SerializeField] private InteractionResearchItem researchTable;

    private Label tooltipsLabel;
    private VisualElement selectedItemContainer;
    private VisualElement inventoryContainer;
    private VisualElement inventoryItemsContainer;
    private readonly List<VisualElement> inventorySlots = new List<VisualElement>();
    private bool isInventoryOpenForTab = false;

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
    
    // Sets up UI element references
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        selectedItemContainer = root.Q<VisualElement>("SelectedItemContainer");
        inventoryContainer = root.Q<VisualElement>("InventoryContainer");
        inventoryItemsContainer = inventoryContainer.Q<VisualElement>("InventoryItemsContainer");
        tooltipsLabel = root.Q<Label>("Tooltips");
        tooltipsLabel.style.display = DisplayStyle.None;
        
        HideFullInventory();
    }

    // Sets up all event subscriptions for inventory and interaction systems
    private void SubscribeToEvents()
    {
        SubscribeToInventoryEvents();
        SubscribeToInteractionEvents();
    }
    
    // Removes all event subscriptions
    private void UnsubscribeFromEvents()
    {
        UnsubscribeFromInventoryEvents();
        UnsubscribeFromInteractionEvents();
    }
    
    // Subscribes to inventory system events
    private void SubscribeToInventoryEvents()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnSelectedItemChange += UpdateSelectedItemDisplay;
            InventorySystem.Instance.OnInventoryChanged += UpdateFullInventoryDisplay;
            InventorySystem.Instance.OnInventoryItemClicked += OnInventoryItemClicked;
        }
    }
    
    // Subscribes to interaction system events
    private void SubscribeToInteractionEvents()
    {
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
        
        if (researchTable != null)
        {
            researchTable.OnTableOpened += ShowFullInventory;
            researchTable.OnTableClosed += HideFullInventory;
        }
    }
    
    // Unsubscribes from inventory system events
    private void UnsubscribeFromInventoryEvents()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnSelectedItemChange -= UpdateSelectedItemDisplay;
            InventorySystem.Instance.OnInventoryChanged -= UpdateFullInventoryDisplay;
            InventorySystem.Instance.OnInventoryItemClicked -= OnInventoryItemClicked;
        }
    }
    
    // Unsubscribes from interaction system events
    private void UnsubscribeFromInteractionEvents()
    {
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
        
        if (researchTable != null)
        {
            researchTable.OnTableOpened -= ShowFullInventory;
            researchTable.OnTableClosed -= HideFullInventory;
        }
    }
    private void RegisterTooltip(VisualElement target, string text)
    {
        target.RegisterCallback<MouseEnterEvent>(evt =>
        {
            tooltipsLabel.text = text;
            tooltipsLabel.style.display = DisplayStyle.Flex;
        });

        target.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            tooltipsLabel.style.display = DisplayStyle.None;
        });

        target.RegisterCallback<MouseMoveEvent>(evt =>
        {
            tooltipsLabel.style.left = evt.mousePosition.x + 10;
            tooltipsLabel.style.top = evt.mousePosition.y + 10;
        });
    }

    
    
    // Updates selected item HUD with current selection
    private void UpdateSelectedItemDisplay()
    {
        var selectedItemIcon = selectedItemContainer.Q<VisualElement>("ItemIcon");
        var selectedItemQuantity = selectedItemContainer.Q<Label>("ItemQuantity");
        
        var selectedItem = InventorySystem.Instance.GetSelectedItem();
        if (selectedItem?.sprite != null)
        {
            selectedItemIcon.style.backgroundImage = new StyleBackground(selectedItem.sprite);
            selectedItemQuantity.text = "x" + InventorySystem.Instance.GetSelectedItemQuantity();
            
            RegisterTooltip(selectedItemContainer, selectedItem.newName);
            
        }
        else
        {
            selectedItemIcon.style.backgroundImage = null;
            selectedItemQuantity.text = "";
        }
    }
    
    // Shows full inventory overlay
    private void ShowFullInventory()
    {
        inventoryContainer.style.display = DisplayStyle.Flex;
        UpdateFullInventoryDisplay();
    }
    
    // Hides full inventory overlay
    private void HideFullInventory()
    {
        inventoryContainer.style.display = DisplayStyle.None;
    }
    
    // Rebuilds full inventory display with current items
    private void UpdateFullInventoryDisplay()
    {
        if (InventorySystem.Instance == null) 
            return;
        
        ClearInventorySlots();
        
        var allItems = InventorySystem.Instance.GetAllItems();
        foreach (var entry in allItems)
        {
            CreateInventorySlot(entry);
        }
    }
    
    // Removes all existing inventory slot UI elements
    private void ClearInventorySlots()
    {
        foreach (var slot in inventorySlots)
        {
            inventoryItemsContainer.Remove(slot);
        }
        inventorySlots.Clear();
    }
    
    // Creates UI slot for inventory entry
    private void CreateInventorySlot(InventoryEntry entry)
    {
        if (entry?.item == null) 
            return;
        
        var slotContainer = CreateSlotContainer();
        var slotIcon = CreateSlotIcon(entry.item);
        var quantityLabel = CreateQuantityLabel(entry.quantity);
        
        slotContainer.RegisterCallback<ClickEvent>(_ => OnInventorySlotClicked(entry.item));
        
        slotContainer.Add(slotIcon);
        slotContainer.Add(quantityLabel);
        
        inventoryItemsContainer.Add(slotContainer);
        inventorySlots.Add(slotContainer);
        
        RegisterTooltip(slotContainer, entry.item.newName);

    }
    
    // Creates the main slot container element
    private VisualElement CreateSlotContainer()
    {
        var slotContainer = new VisualElement();
        slotContainer.AddToClassList("item-slot");
        return slotContainer;
    }
    
    // Creates the item icon element
    private VisualElement CreateSlotIcon(InventoryItem item)
    {
        var slotIcon = new VisualElement();
        slotIcon.AddToClassList("item-icon");
        
        if (item.sprite != null)
        {
            slotIcon.style.backgroundImage = new StyleBackground(item.sprite);
        }
        
        return slotIcon;
    }
    
    // Creates the quantity label element
    private Label CreateQuantityLabel(int quantity)
    {
        var quantityLabel = new Label("x" + quantity);
        quantityLabel.AddToClassList("item-quantity");
        return quantityLabel;
    }
    
    // Handles inventory slot click events
    private void OnInventorySlotClicked(InventoryItem item)
    {
        InventorySystem.Instance.TriggerItemClick(item);
    }
    
    // Handles forwarded inventory item click events
    private void OnInventoryItemClicked(InventoryItem item)
    {
        // Event forwarded to systems that need it (crafting/market/research)
    }
}