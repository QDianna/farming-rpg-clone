using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Crafting bench UI with upgradeable slots (3 base + 2 unlockable + 1 output).
/// Slots 3 and 4 are hidden until unlocked via upgrade interaction.
/// Prevents adding items to invisible/locked slots.
/// </summary>
public class CraftingSystemHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private InteractionCraftRecipe craftingBench;

    private static int totalSlots = 6; // 5 input + 1 output
    private static int baseInputSlots = 3; // Always available (0, 1, 2)
    
    private VisualElement craftingBenchContainer;
    private VisualElement[] slotContainers = new VisualElement[totalSlots];
    private VisualElement[] slotItems = new VisualElement[totalSlots];
    private Label[] slotQuantities = new Label[totalSlots];
    
    // Track which slots are unlocked
    private bool slot3Unlocked = false;
    private bool slot4Unlocked = false;
    
    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
        UpdateSlotVisibility(); // Hide locked slots initially
    }
    
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        craftingBenchContainer = root.Q<VisualElement>("CraftingBenchContainer");
        
        string[] slotNames = { 
            "Slot00Container", "Slot01Container", "Slot02Container", 
            "Slot03Container", "Slot04Container", "Slot05Container" 
        };
        
        for (int i = 0; i < totalSlots; i++)
        {
            slotContainers[i] = root.Q<VisualElement>(slotNames[i]);
            if (slotContainers[i] != null)
            {
                slotItems[i] = slotContainers[i].Q<VisualElement>("ItemIcon");
                slotQuantities[i] = slotContainers[i].Q<Label>("ItemQuantity");
                
                int slotIndex = i;
                // Input slots (0-4) get input click handling
                if (i < totalSlots - 1)
                    slotContainers[i].RegisterCallback<ClickEvent>(evt => OnInputSlotClicked(slotIndex));
                else // Output slot (5)
                    slotContainers[i].RegisterCallback<ClickEvent>(evt => OnOutputSlotClicked());
            }
            else
            {
                Debug.LogWarning($"Could not find UI element: {slotNames[i]}");
            }
        }
        
        HideCraftingBench();
    }
    
    /// <summary>
    /// Update visibility of slots based on unlock status
    /// </summary>
    private void UpdateSlotVisibility()
    {
        // Slot 3 visibility
        if (slotContainers[3] != null)
        {
            slotContainers[3].style.display = slot3Unlocked ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        // Slot 4 visibility
        if (slotContainers[4] != null)
        {
            slotContainers[4].style.display = slot4Unlocked ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// PUBLIC: Unlock slot 3 (called by upgrade interaction)
    /// </summary>
    public void UnlockSlot3()
    {
        if (!slot3Unlocked)
        {
            slot3Unlocked = true;
            UpdateSlotVisibility();
            NotificationSystem.ShowNotification("Crafting bench upgraded! Slot 4 unlocked!");
            Debug.Log("CraftingHUD: Slot 3 unlocked");
        }
    }
    
    /// <summary>
    /// PUBLIC: Unlock slot 4 (called by upgrade interaction)
    /// </summary>
    public void UnlockSlot4()
    {
        if (!slot4Unlocked)
        {
            slot4Unlocked = true;
            UpdateSlotVisibility();
            NotificationSystem.ShowNotification("Crafting bench upgraded! Slot 5 unlocked!");
            Debug.Log("CraftingHUD: Slot 4 unlocked");
        }
    }
    
    /// <summary>
    /// PUBLIC: Unlock both upgrade slots at once
    /// </summary>
    public void UnlockAllUpgradeSlots()
    {
        bool anyUnlocked = false;
        
        if (!slot3Unlocked)
        {
            slot3Unlocked = true;
            anyUnlocked = true;
        }
        
        if (!slot4Unlocked)
        {
            slot4Unlocked = true;
            anyUnlocked = true;
        }
        
        if (anyUnlocked)
        {
            UpdateSlotVisibility();
            NotificationSystem.ShowNotification("Crafting bench fully upgraded! All slots unlocked!");
            Debug.Log("CraftingHUD: All upgrade slots unlocked");
        }
    }
    
    /// <summary>
    /// Check if a slot is available for use (unlocked and bench supports it)
    /// </summary>
    private bool IsSlotUsable(int slotIndex)
    {
        if (slotIndex < baseInputSlots) return true; // Base slots always usable
        if (slotIndex == 3) return slot3Unlocked;
        if (slotIndex == 4) return slot4Unlocked;
        if (slotIndex == 5) return true; // Output slot always usable
        return false;
    }
    
    /// <summary>
    /// Get the number of currently available input slots
    /// </summary>
    private int GetAvailableInputSlotCount()
    {
        int count = baseInputSlots; // Always have base slots
        if (slot3Unlocked) count++;
        if (slot4Unlocked) count++;
        return count;
    }
    
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
    
    private void ShowCraftingBench()
    {
        craftingBenchContainer.style.display = DisplayStyle.Flex;
        UpdateSlotVisibility(); // Ensure correct visibility when opening
        UpdateSlotsDisplay();
    }
    
    private void HideCraftingBench()
    {
        craftingBenchContainer.style.display = DisplayStyle.None;
    }
    
    private bool IsCraftingBenchOpen()
    {
        return craftingBench != null && craftingBenchContainer.style.display == DisplayStyle.Flex;
    }
    
    private void OnInventoryItemClicked(InventoryItem item)
    {
        if (!IsCraftingBenchOpen()) return;
        
        // Only try to add if we have available slots that are actually usable
        if (TryAddToAvailableSlot(item))
        {
            InventorySystem.Instance.RemoveItem(item, 1);
        }
    }
    
    /// <summary>
    /// Try to add item only to unlocked/visible slots
    /// </summary>
    /// <summary>
    /// Try to add item only to unlocked/visible slots
    /// </summary>
    private bool TryAddToAvailableSlot(InventoryItem item)
    {
        // First, check for stacking in existing slots (prioritize stacking)
        for (int i = 0; i < baseInputSlots; i++)
        {
            if (craftingBench.inputSlots[i].item == item)
            {
                return craftingBench.TryAddIngredientToSlot(item, 1, i);
            }
        }
    
        if (slot3Unlocked && craftingBench.inputSlots[3].item == item)
        {
            return craftingBench.TryAddIngredientToSlot(item, 1, 3);
        }
    
        if (slot4Unlocked && craftingBench.inputSlots[4].item == item)
        {
            return craftingBench.TryAddIngredientToSlot(item, 1, 4);
        }
    
        // Then check for empty slots if no stacking available
        for (int i = 0; i < baseInputSlots; i++)
        {
            if (craftingBench.inputSlots[i].IsEmpty)
            {
                return craftingBench.TryAddIngredientToSlot(item, 1, i);
            }
        }
    
        // Check slot 3 if unlocked
        if (slot3Unlocked && craftingBench.inputSlots[3].IsEmpty)
        {
            return craftingBench.TryAddIngredientToSlot(item, 1, 3);
        }
    
        // Check slot 4 if unlocked
        if (slot4Unlocked && craftingBench.inputSlots[4].IsEmpty)
        {
            return craftingBench.TryAddIngredientToSlot(item, 1, 4);
        }
    
        return false; // No available slots
    }
    private void OnInputSlotClicked(int slotIndex)
    {
        if (!IsCraftingBenchOpen() || !IsSlotUsable(slotIndex)) return;
        
        var slot = craftingBench.inputSlots[slotIndex];
        if (!slot.IsEmpty)
        {
            var itemToReturn = slot.item;
            
            if (craftingBench.TryRemoveIngredient(slotIndex, 1))
            {
                InventorySystem.Instance.AddItem(itemToReturn, 1);
            }
        }
    }
    
    private void OnOutputSlotClicked()
    {
        if (IsCraftingBenchOpen())
        {
            craftingBench.TryTakeOutput();
        }
    }
    
    private void UpdateSlotsDisplay()
    {
        if (craftingBench == null) return;
        
        // Update all input slots (but only show unlocked ones)
        for (int i = 0; i < totalSlots - 1; i++)
        {
            if (IsSlotUsable(i) && slotItems[i] != null && slotQuantities[i] != null)
            {
                UpdateSlotDisplay(slotItems[i], slotQuantities[i], craftingBench.inputSlots[i]);
            }
        }
        
        // Update output slot
        if (slotItems[5] != null && slotQuantities[5] != null)
        {
            UpdateSlotDisplay(slotItems[5], slotQuantities[5], craftingBench.outputSlot);
        }
    }
    
    private void UpdateSlotDisplay(VisualElement slotElement, Label quantityLabel, InteractionCraftRecipe.InventoryItemSlot slot)
    {
        if (slot.IsEmpty)
        {
            slotElement.style.backgroundImage = null;
            quantityLabel.text = "";
            slotElement.tooltip = "Empty slot";
        }
        else
        {
            slotElement.style.backgroundImage = new StyleBackground(slot.item.sprite);
            quantityLabel.text = slot.quantity.ToString();
            slotElement.tooltip = $"{slot.quantity}x {slot.item.name}";
        }
    }
}
