using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Crafting bench UI system with upgradeable slot functionality.
/// Manages 3 base input slots plus 2 unlock-able upgrade slots and 1 output slot.
/// </summary>
public class CraftingSystemHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private InteractionCraftRecipe craftingBench;

    private static readonly int TotalSlots = 6; // 5 input + 1 output
    private static readonly int BaseInputSlots = 3; // Always available (0, 1, 2)
    
    private VisualElement craftingBenchContainer;
    private readonly VisualElement[] slotContainers = new VisualElement[TotalSlots];
    private readonly VisualElement[] slotItems = new VisualElement[TotalSlots];
    private Label[] slotQuantities = new Label[TotalSlots];
    
    private bool slot3Unlocked;
    private bool slot4Unlocked;
    
    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
        UpdateSlotVisibility();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    public void UnlockAllUpgradeSlots()
    {
        if (!slot3Unlocked)
            slot3Unlocked = true;
        
        if (!slot4Unlocked)
            slot4Unlocked = true;
        
        UpdateSlotVisibility();
    }
    
    // Sets up UI element references and click handlers
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        craftingBenchContainer = root.Q<VisualElement>("CraftingBenchContainer");
        
        string[] slotNames = { 
            "Slot00Container", "Slot01Container", "Slot02Container", 
            "Slot03Container", "Slot04Container", "Slot05Container" 
        };
        
        SetupSlotReferences(root, slotNames);
        HideCraftingBench();
    }
    
    // Creates references to slot UI elements and registers click events
    private void SetupSlotReferences(VisualElement root, string[] slotNames)
    {
        for (int i = 0; i < TotalSlots; i++)
        {
            slotContainers[i] = root.Q<VisualElement>(slotNames[i]);
            if (slotContainers[i] != null)
            {
                slotItems[i] = slotContainers[i].Q<VisualElement>("ItemIcon");
                slotQuantities[i] = slotContainers[i].Q<Label>("ItemQuantity");
                
                RegisterSlotClickHandlers(i);
            }
            else
            {
                Debug.LogWarning($"Could not find UI element: {slotNames[i]}");
            }
        }
    }
    
    // Registers appropriate click handlers for input and output slots
    private void RegisterSlotClickHandlers(int slotIndex)
    {
        if (slotIndex < TotalSlots - 1) // Input slots (0-4)
        {
            slotContainers[slotIndex].RegisterCallback<ClickEvent>(_ => OnInputSlotClicked(slotIndex));
        }
        else // Output slot (5)
        {
            slotContainers[slotIndex].RegisterCallback<ClickEvent>(_ => OnOutputSlotClicked());
        }
    }
    
    // Updates visibility of upgrade slots based on unlock status
    private void UpdateSlotVisibility()
    {
        if (slotContainers[3] != null)
        {
            slotContainers[3].style.display = slot3Unlocked ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        if (slotContainers[4] != null)
        {
            slotContainers[4].style.display = slot4Unlocked ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // Checks if specific slot is available for use
    private bool IsSlotUsable(int slotIndex)
    {
        if (slotIndex < BaseInputSlots) return true; // Base slots always usable
        if (slotIndex == 3) return slot3Unlocked;
        if (slotIndex == 4) return slot4Unlocked;
        if (slotIndex == 5) return true; // Output slot always usable
        return false;
    }
    
    // Sets up event subscriptions for crafting bench and inventory
    private void SubscribeToEvents()
    {
        if (craftingBench != null)
        {
            craftingBench.OnCraftingBenchOpened += ShowCraftingBench;
            craftingBench.OnCraftingBenchClosed += HideCraftingBench;
            craftingBench.OnCraftingSlotsChanged += UpdateSlotsDisplay;
        }
        
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryItemClicked += OnInventoryItemClicked;
        }
    }
    
    // Removes event subscriptions
    private void UnsubscribeFromEvents()
    {
        if (craftingBench != null)
        {
            craftingBench.OnCraftingBenchOpened -= ShowCraftingBench;
            craftingBench.OnCraftingBenchClosed -= HideCraftingBench;
            craftingBench.OnCraftingSlotsChanged -= UpdateSlotsDisplay;
        }
        
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryItemClicked -= OnInventoryItemClicked;
        }
    }
    
    // Shows crafting bench interface
    private void ShowCraftingBench()
    {
        craftingBenchContainer.style.display = DisplayStyle.Flex;
        UpdateSlotVisibility();
        UpdateSlotsDisplay();
    }
    
    // Hides crafting bench interface
    private void HideCraftingBench()
    {
        craftingBenchContainer.style.display = DisplayStyle.None;
    }
    
    // Checks if crafting bench UI is currently open
    private bool IsCraftingBenchOpen()
    {
        return craftingBench != null && craftingBenchContainer.style.display == DisplayStyle.Flex;
    }
    
    // Handles inventory item clicks when crafting bench is open
    private void OnInventoryItemClicked(InventoryItem item, bool no)
    {
        if (!IsCraftingBenchOpen()) 
            return;
        
        if (FindAvailableSlot(item))
        {
            InventorySystem.Instance.RemoveItem(item, 1);
        }
    }
    
    // Finds available slot for item, prioritizing stacking then empty slots
    private bool FindAvailableSlot(InventoryItem item)
    {
        // Try stacking first
        if (TryStackInExistingSlots(item))
            return true;
        
        // Then try empty slots
        return TryAddToEmptySlot(item);
    }
    
    // Attempts to stack item in existing slots with same item
    private bool TryStackInExistingSlots(InventoryItem item)
    {
        for (int i = 0; i < BaseInputSlots; i++)
            if (craftingBench.inputSlots[i].item == item)
                return craftingBench.TryAddIngredientToSlot(item, 1, i);
    
        if (slot3Unlocked && craftingBench.inputSlots[3].item == item)
            return craftingBench.TryAddIngredientToSlot(item, 1, 3);
    
        if (slot4Unlocked && craftingBench.inputSlots[4].item == item)
            return craftingBench.TryAddIngredientToSlot(item, 1, 4);
        
        return false;
    }
    
    // Attempts to add item to first available empty slot
    private bool TryAddToEmptySlot(InventoryItem item)
    {
        for (int i = 0; i < BaseInputSlots; i++)
            if (craftingBench.inputSlots[i].IsEmpty)
                return craftingBench.TryAddIngredientToSlot(item, 1, i);
    
        if (slot3Unlocked && craftingBench.inputSlots[3].IsEmpty)
            return craftingBench.TryAddIngredientToSlot(item, 1, 3);
    
        if (slot4Unlocked && craftingBench.inputSlots[4].IsEmpty)
            return craftingBench.TryAddIngredientToSlot(item, 1, 4);
    
        return false;
    }
    
    // Handles input slot clicks to remove items
    private void OnInputSlotClicked(int slotIndex)
    {
        if (!IsCraftingBenchOpen() || !IsSlotUsable(slotIndex)) 
            return;
        
        var slot = craftingBench.inputSlots[slotIndex];
        if (!slot.IsEmpty)
        {
            var itemToReturn = slot.item;
            
            if (craftingBench.TryRemoveIngredient(slotIndex, 1))
                InventorySystem.Instance.AddItem(itemToReturn, 1);
        }
    }
    
    // Handles output slot clicks to take crafted items
    private void OnOutputSlotClicked()
    {
        if (IsCraftingBenchOpen())
            craftingBench.TryTakeOutput();
    }
    
    // Updates all slot displays with current item data
    private void UpdateSlotsDisplay()
    {
        if (craftingBench == null) 
            return;
        
        // Update input slots (only usable ones)
        for (int i = 0; i < TotalSlots - 1; i++)
            if (IsSlotUsable(i) && slotItems[i] != null && slotQuantities[i] != null)
                UpdateSlotDisplay(slotItems[i], slotQuantities[i], craftingBench.inputSlots[i]);
        
        // Update output slot
        if (slotItems[5] != null && slotQuantities[5] != null)
            UpdateSlotDisplay(slotItems[5], slotQuantities[5], craftingBench.outputSlot);
    }
    
    // Updates individual slot display with item sprite and quantity
    private void UpdateSlotDisplay(VisualElement slotElement, Label quantityLabel, InventoryItemSlot slot)
    {
        if (slot.IsEmpty)
        {
            slotElement.style.backgroundImage = null;
            quantityLabel.text = "";
        }
        else
        {
            slotElement.style.backgroundImage = new StyleBackground(slot.item.sprite);
            quantityLabel.text = "x" + slot.quantity;
        }
    }
}