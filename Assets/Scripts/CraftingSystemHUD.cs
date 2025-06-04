using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Crafting bench UI with 3 input slots and 1 output slot.
/// Handles inventory clicks to add ingredients and slot clicks to remove items.
/// </summary>
public class CraftingSystemHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private InteractionCraftRecipe craftingBench;
    
    private VisualElement craftingBenchContainer;
    private VisualElement[] slotContainers = new VisualElement[4];
    private VisualElement[] slotItems = new VisualElement[4];
    private Label[] slotQuantities = new Label[4];
    
    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        craftingBenchContainer = root.Q<VisualElement>("CraftingBenchContainer");
        
        string[] slotNames = { "Slot00Container", "Slot01Container", "Slot02Container", "Slot03Container" };
        for (int i = 0; i < 4; i++)
        {
            slotContainers[i] = root.Q<VisualElement>(slotNames[i]);
            slotItems[i] = slotContainers[i].Q<VisualElement>("ItemIcon");
            slotQuantities[i] = slotContainers[i].Q<Label>("ItemQuantity");
            
            int slotIndex = i;
            if (i < 3)
                slotContainers[i].RegisterCallback<ClickEvent>(evt => OnInputSlotClicked(slotIndex));
            else
                slotContainers[i].RegisterCallback<ClickEvent>(evt => OnOutputSlotClicked());
        }
        
        HideCraftingBench();
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
        
        if (craftingBench.TryAddIngredient(item, 1))
        {
            InventorySystem.Instance.RemoveItem(item, 1);
        }
    }
    
    private void OnInputSlotClicked(int slotIndex)
    {
        if (!IsCraftingBenchOpen()) return;
        
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
        
        for (int i = 0; i < 3; i++)
        {
            UpdateSlotDisplay(slotItems[i], slotQuantities[i], craftingBench.inputSlots[i]);
        }
        
        UpdateSlotDisplay(slotItems[3], slotQuantities[3], craftingBench.outputSlot);
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
            slotElement.style.backgroundImage = new StyleBackground(slot.item.itemSprite);
            quantityLabel.text = slot.quantity.ToString();
            slotElement.tooltip = $"{slot.quantity}x {slot.item.name}";
        }
    }
}