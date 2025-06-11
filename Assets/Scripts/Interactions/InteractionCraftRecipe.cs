using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single slot in the crafting interface for holding items and quantities.
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
/// Interactive crafting bench with 5 input slots and 1 output slot.
/// Handles recipe validation, ingredient consumption, and automatic UI updates.
/// </summary>
public class InteractionCraftRecipe : MonoBehaviour, IInteractable
{
    [Header("Crafting Settings")]
    private static readonly int InputSlotCount = 5;
    
    [HideInInspector] public List<InventoryItemSlot> inputSlots;
    [HideInInspector] public InventoryItemSlot outputSlot;
    
    private bool isBenchOpen;
    
    public event System.Action OnCraftingBenchOpened;
    public event System.Action OnCraftingBenchClosed;
    public event System.Action OnCraftingSlotsChanged;
    
    private void Awake()
    {
        InitializeSlots();
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
            
            if (isBenchOpen)
            {
                CloseBench();
            }
        }
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
            CloseBench();
        }
    }
    
    public bool TryAddIngredientToSlot(InventoryItem item, int quantity, int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) 
            return false;
        
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
            return false;
        }
        
        UpdateRecipe();
        return true;
    }
    
    public bool TryRemoveIngredient(int slotIndex, int quantity)
    {
        if (!IsValidSlotIndex(slotIndex)) 
            return false;
        
        var slot = inputSlots[slotIndex];
        if (slot.IsEmpty || slot.quantity < quantity) 
            return false;
        
        slot.quantity -= quantity;
        if (slot.quantity <= 0)
            slot.Clear();
        
        UpdateRecipe();
        return true;
    }
    
    public bool TryTakeOutput()
    {
        if (outputSlot.IsEmpty) 
            return false;
        
        ProcessCraftingResult();
        return true;
    }
    
    // Creates all input and output slots
    private void InitializeSlots()
    {
        inputSlots = new List<InventoryItemSlot>();
        for (int i = 0; i < InputSlotCount; i++)
        {
            inputSlots.Add(new InventoryItemSlot());
        }
        outputSlot = new InventoryItemSlot();
    }
    
    // Closes the bench and returns ingredients to player
    private void CloseBench()
    {
        isBenchOpen = false;
        ReturnAllIngredients();
        OnCraftingBenchClosed?.Invoke();
    }
    
    // Validates slot index bounds
    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < inputSlots.Count;
    }
    
    // Updates output slot based on current ingredients
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
    
    // Finds recipe that matches current ingredients
    private CraftingRecipe FindMatchingRecipe()
    {
        if (CraftingSystem.Instance?.GetUnlockedRecipes() == null) 
            return null;
        
        foreach (var recipe in CraftingSystem.Instance.GetUnlockedRecipes())
        {
            if (HasAllIngredients(recipe))
                return recipe;
        }
        return null;
    }
    
    // Checks if all recipe ingredients are available in slots
    private bool HasAllIngredients(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            int totalQuantity = GetTotalQuantityInSlots(ingredient.item);
            if (totalQuantity < ingredient.quantity)
                return false;
        }
        return true;
    }
    
    // Counts total quantity of specific item across all slots
    private int GetTotalQuantityInSlots(InventoryItem item)
    {
        int total = 0;
        foreach (var slot in inputSlots)
        {
            if (slot.item == item)
                total += slot.quantity;
        }
        return total;
    }
    
    // Completes crafting by adding result and consuming ingredients
    private void ProcessCraftingResult()
    {
        InventorySystem.Instance.AddItem(outputSlot.item, outputSlot.quantity);
        
        var recipe = FindMatchingRecipe();
        if (recipe != null)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                ConsumeIngredient(ingredient.item, ingredient.quantity);
            }
        }
        
        string craftedItemName = outputSlot.item.name;
        outputSlot.Clear();
        UpdateRecipe();
        
        NotificationSystem.ShowNotification($"Crafted {craftedItemName}!");
    }
    
    // Removes specified quantity of item from input slots
    private void ConsumeIngredient(InventoryItem item, int quantityToRemove)
    {
        foreach (var slot in inputSlots)
        {
            if (slot.item == item && quantityToRemove > 0)
            {
                int removeFromSlot = Mathf.Min(slot.quantity, quantityToRemove);
                slot.quantity -= removeFromSlot;
                quantityToRemove -= removeFromSlot;
                
                if (slot.quantity <= 0)
                    slot.Clear();
            }
        }
    }
    
    // Returns all ingredients to player inventory when closing bench
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
}