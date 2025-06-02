using UnityEngine;
using UnityEngine.UIElements;

public class CraftingSystemHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private InteractionCraftRecipe craftingBench;  // Reference to listen to events
    
    // UI Elements - Containers
    private VisualElement craftingBenchContainer;
    private VisualElement slot00Container, slot01Container, slot02Container, slot03Container;
    
    // UI Elements - Inner elements (the actual slots where sprites go)
    private VisualElement slot00Item, slot01Item, slot02Item, slot03Item;
    private Label slot00Quantity, slot01Quantity, slot02Quantity, slot03Quantity;
    
    private void Start()
    {
        InitializeUI();
        
        // Subscribe to crafting bench events
        if (craftingBench != null)
        {
            craftingBench.OnCraftingBenchOpened += ShowCraftingBench;
            craftingBench.OnCraftingBenchClosed += HideCraftingBench;
            craftingBench.OnCraftingSlotsChanged += UpdateSlotsDisplay;
            InventorySystem.Instance.OnInventoryItemClicked += OnInventoryItemClicked;

        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        if (craftingBench != null)
        {
            craftingBench.OnCraftingBenchOpened -= ShowCraftingBench;
            craftingBench.OnCraftingBenchClosed -= HideCraftingBench;
            craftingBench.OnCraftingSlotsChanged -= UpdateSlotsDisplay;
            InventorySystem.Instance.OnInventoryItemClicked -= OnInventoryItemClicked;

        }
    }
    
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Get container elements
        craftingBenchContainer = root.Q<VisualElement>("CraftingBenchContainer");
        slot00Container = root.Q<VisualElement>("Slot00Container");
        slot01Container = root.Q<VisualElement>("Slot01Container");
        slot02Container = root.Q<VisualElement>("Slot02Container");
        slot03Container = root.Q<VisualElement>("Slot03Container");
        
        // Containters children
        slot00Item = slot00Container.Q<VisualElement>("ItemIcon");
        slot00Quantity = slot00Container.Q<Label>("Quantity");
        slot01Item = slot01Container.Q<VisualElement>("ItemIcon");
        slot01Quantity = slot01Container.Q<Label>("Quantity");
        slot02Item = slot02Container.Q<VisualElement>("ItemIcon");
        slot02Quantity = slot02Container.Q<Label>("Quantity");
        slot03Item = slot03Container.Q<VisualElement>("ItemIcon");
        slot03Quantity = slot03Container.Q<Label>("Quantity");
        
        // Hide crafting bench at start
        HideCraftingBench();
        
        // Setup click events for containers (easier to click)
        SetupSlotEvents();
    }
    
    private void SetupSlotEvents()
    {
        // Input slots - clicking on containers removes items
        slot00Container.RegisterCallback<ClickEvent>(evt => OnInputSlotClicked(0));
        slot01Container.RegisterCallback<ClickEvent>(evt => OnInputSlotClicked(1));
        slot02Container.RegisterCallback<ClickEvent>(evt => OnInputSlotClicked(2));
        
        // Output slot - clicking takes crafted item
        slot03Container.RegisterCallback<ClickEvent>(evt => OnOutputSlotClicked());
    }
    
    public void ShowCraftingBench()
    {
        craftingBenchContainer.style.display = DisplayStyle.Flex;
        
        Debug.Log("Crafting Bench UI opened!");
        Debug.Log("Click on your inventory items to add them to crafting slots");
        Debug.Log("Click on input slots to remove items");
        Debug.Log("Click on output slot to take crafted items");
        
        UpdateSlotsDisplay();
    }
    
    public void HideCraftingBench()
    {
        craftingBenchContainer.style.display = DisplayStyle.None;
        
        Debug.Log("Crafting Bench UI closed!");
    }
    
    public bool IsCraftingBenchOpen()
    {
        return craftingBench != null && craftingBenchContainer.style.display == DisplayStyle.Flex;
    }
    
    // Called when player clicks on inventory item
    public void OnInventoryItemClicked(InventoryItem item)
    {
        if (!IsCraftingBenchOpen()) return;
        
        // Try to add item to crafting bench
        bool success = craftingBench.TryAddIngredient(item, 1);
        if (success)
        {
            // Remove from player inventory
            InventorySystem.Instance.RemoveItem(item, 1);
            UpdateSlotsDisplay();
            Debug.Log($"Added {item.name} to crafting bench");
        }
        else
        {
            Debug.Log("No space in crafting bench for this item");
        }
    }
    
    private void OnInputSlotClicked(int slotIndex)
    {
        if (!IsCraftingBenchOpen()) return;
        
        // Try to remove 1 item from this slot
        bool success = craftingBench.TryRemoveIngredient(slotIndex, 1);
        if (success)
        {
            UpdateSlotsDisplay();
            Debug.Log($"Removed item from slot {slotIndex}");
        }
    }
    
    private void OnOutputSlotClicked()
    {
        if (!IsCraftingBenchOpen()) return;
        
        bool success = craftingBench.TryTakeOutput();
        if (success)
        {
            UpdateSlotsDisplay();
            Debug.Log("Took crafted item!");
        }
    }
    
    private void UpdateSlotsDisplay()
    {
        if (craftingBench == null) return;
        
        Debug.Log("Updating slots display...");
        
        // Update input slots (first 3 are input, last is output)
        UpdateSlotDisplay(slot00Item, craftingBench.inputSlots[0]);
        UpdateSlotDisplay(slot01Item, craftingBench.inputSlots[1]);
        UpdateSlotDisplay(slot02Item, craftingBench.inputSlots[2]);
        
        // Update output slot
        UpdateSlotDisplay(slot03Item, craftingBench.outputSlot);
    }
    
    private void UpdateSlotDisplay(VisualElement slotElement, InteractionCraftRecipe.InventoryItemSlot slot)
    {
        if (slot.IsEmpty)
        {
            slotElement.style.backgroundImage = null;
            slotElement.tooltip = "Empty slot";
        }
        else
        {
            // Set background image to item sprite
            if (slot.item.itemSprite != null)
            {
                slotElement.style.backgroundImage = new StyleBackground(slot.item.itemSprite);
            }
            
            slotElement.tooltip = $"{slot.quantity}x {slot.item.name}";
        }
    }
}