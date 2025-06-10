using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an entry into the crafting bench.
/// Used for input and output slots.
/// </summary>
public class InventoryItemSlot
{
    public InventoryItem item;
    public int quantity;
        
    public bool IsEmpty => item == null || quantity <= 0;
        
    public void SetItem(InventoryItem newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;
    }
        
    public void Clear()
    {
        item = null;
        quantity = 0;
    }
}

/// <summary>
/// Interactive crafting bench that allows players to combine ingredients into new items.
/// Features 5 input slots (3 base + 2 upgradeable), 1 output slot, and automatic recipe validation.
/// </summary>
public class InteractionCraftRecipe : MonoBehaviour, IInteractable
{
    [Header("Crafting Bench Settings")]
    private static int inputSlotCount = 5;
    private bool isBenchOpen;
    
    [HideInInspector] public List<InventoryItemSlot> inputSlots;
    [HideInInspector] public InventoryItemSlot outputSlot;

    public event System.Action OnCraftingBenchOpened, OnCraftingBenchClosed, OnCraftingSlotsChanged;
    
    private void Awake()
    {
        inputSlots = new List<InventoryItemSlot>();
        for (int i = 0; i < inputSlotCount; i++)
        {
            inputSlots.Add(new InventoryItemSlot());
        }
        outputSlot = new InventoryItemSlot();
    }
    
    public void Interact(PlayerController player)
    {
        isBenchOpen = !isBenchOpen;
        
        if (isBenchOpen)
        {
            OnCraftingBenchOpened?.Invoke();
        }
        else
        {
            OnCraftingBenchClosed?.Invoke();
        }
    }
    
    public bool TryAddIngredientToSlot(InventoryItem item, int quantity, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inputSlots.Count) return false;
        
        var slot = inputSlots[slotIndex];
        
        if (slot.IsEmpty)
        {
            slot.SetItem(item, quantity);
        }
        else if (slot.item == item)
        {
            slot.quantity += quantity;
        }
        else
        {
            return false; // Slot occupied with different item
        }
        
        UpdateRecipe();
        return true;
    }
    
    public bool TryRemoveIngredient(int slotIndex, int quantity)
    {
        if (slotIndex < 0 || slotIndex >= inputSlots.Count) return false;
        
        var slot = inputSlots[slotIndex];
        if (slot.IsEmpty || slot.quantity < quantity) return false;
        
        slot.quantity -= quantity;
        if (slot.quantity <= 0)
            slot.Clear();
        
        UpdateRecipe();
        return true;
    }
    
    public bool TryTakeOutput()
    {
        if (outputSlot.IsEmpty) return false;
        
        // Add result to inventory
        InventorySystem.Instance.AddItem(outputSlot.item, outputSlot.quantity);
        
        // Find recipe and consume ingredients
        var recipe = FindMatchingRecipe();
        if (recipe != null)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                ConsumeIngredient(ingredient.item, ingredient.quantity);
            }
        }
        
        // Clear output and update
        string craftedItemName = outputSlot.item.name;
        outputSlot.Clear();
        UpdateRecipe();
        
        NotificationSystem.ShowNotification($"Crafted {craftedItemName}!");
        return true;
    }
    
    private void UpdateRecipe()
    {
        outputSlot.Clear();
        
        var recipe = FindMatchingRecipe();
        if (recipe != null)
        {
            outputSlot.SetItem(recipe.result, recipe.resultQuantity);
        }
        
        OnCraftingSlotsChanged?.Invoke();
    }
    
    private CraftingRecipe FindMatchingRecipe()
    {
        if (CraftingSystem.Instance == null) return null;
        
        var unlockedRecipes = CraftingSystem.Instance.GetUnlockedRecipes();
        if (unlockedRecipes == null) return null;
        
        foreach (var recipe in unlockedRecipes)
        {
            if (HasAllIngredients(recipe))
                return recipe;
        }
        return null;
    }
    
    private bool HasAllIngredients(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            int totalQuantity = 0;
            foreach (var slot in inputSlots)
            {
                if (slot.item == ingredient.item)
                    totalQuantity += slot.quantity;
            }
            
            if (totalQuantity < ingredient.quantity)
                return false;
        }
        return true;
    }
    
    private void ConsumeIngredient(InventoryItem item, int quantityToRemove)
    {
        foreach (var slot in inputSlots)
        {
            if (slot.item == item && quantityToRemove > 0)
            {
                int removeFromThisSlot = Mathf.Min(slot.quantity, quantityToRemove);
                slot.quantity -= removeFromThisSlot;
                quantityToRemove -= removeFromThisSlot;
                
                if (slot.quantity <= 0)
                    slot.Clear();
            }
        }
    }
    
    private void ReturnAllIngredients()
    {
        foreach (var slot in inputSlots)
        {
            if (!slot.IsEmpty)
            {
                InventorySystem.Instance.AddItem(slot.item, slot.quantity);
                slot.Clear();
            }
        }
        
        outputSlot.Clear();
        OnCraftingSlotsChanged?.Invoke();
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }
    
    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
            if (isBenchOpen)
            {
                isBenchOpen = false;
                ReturnAllIngredients();
                OnCraftingBenchClosed?.Invoke();
            }
        }
    }
}