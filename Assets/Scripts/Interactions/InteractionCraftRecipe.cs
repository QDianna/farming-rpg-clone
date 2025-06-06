using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interactive crafting bench that allows players to combine ingredients into new items.
/// Features 3 input slots, 1 output slot, and automatic recipe validation with visual feedback.
/// </summary>
public class InteractionCraftRecipe : MonoBehaviour, IInteractable
{
    [Header("Crafting Bench Settings")]
    [SerializeField] private int inputSlotCount = 3;
    
    [Header("Current Crafting State")]
    public List<InventoryItemSlot> inputSlots;
    public InventoryItemSlot outputSlot;
    
    private CraftingSystem playerCraftingSystem;
    private bool isBenchOpen;

    public event System.Action OnCraftingBenchOpened;
    public event System.Action OnCraftingBenchClosed;
    public event System.Action OnCraftingSlotsChanged;
    
    [System.Serializable]
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
    
    private void Awake()
    {
        InitializeSlots();
    }
    
    private void InitializeSlots()
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
        playerCraftingSystem = player.GetComponent<CraftingSystem>();
        
        isBenchOpen = !isBenchOpen;
        
        if (isBenchOpen)
        {
            NotificationSystem.ShowNotification("Crafting bench opened!");
            OnCraftingBenchOpened?.Invoke();
        }
        else
        {
            NotificationSystem.ShowNotification("Crafting bench closed");
            OnCraftingBenchClosed?.Invoke();
        }
    }
    
    public bool TryAddIngredient(InventoryItem item, int quantity)
    {
        foreach (var slot in inputSlots)
        {
            if (slot.IsEmpty)
            {
                slot.SetItem(item, quantity);
                CheckForValidRecipe();
                return true;
            }
            else if (slot.item == item)
            {
                slot.quantity += quantity;
                CheckForValidRecipe();
                return true;
            }
        }
        return false;
    }
    
    public bool TryRemoveIngredient(int slotIndex, int quantity)
    {
        if (slotIndex < 0 || slotIndex >= inputSlots.Count)
            return false;
        
        var slot = inputSlots[slotIndex];
        if (slot.IsEmpty || slot.quantity < quantity)
            return false;
        
        slot.quantity -= quantity;
        if (slot.quantity <= 0)
            slot.Clear();
        
        CheckForValidRecipe();
        return true;
    }
    
    public bool TryTakeOutput()
    {
        if (outputSlot.IsEmpty) 
        {
            NotificationSystem.ShowNotification("No crafted item to take");
            return false;
        }
        
        var usedRecipe = FindMatchingRecipe();
        if (usedRecipe == null) return false;
        
        // Add result to inventory
        InventorySystem.Instance.AddItem(outputSlot.item, outputSlot.quantity);
        
        // Remove consumed ingredients
        foreach (var ingredient in usedRecipe.ingredients)
        {
            RemoveIngredientsFromSlots(ingredient.item, ingredient.quantity);
        }
        
        // Mark recipe as crafted
        playerCraftingSystem?.MarkRecipeAsCrafted(usedRecipe);
        
        outputSlot.Clear();
        CheckForValidRecipe();
        
        NotificationSystem.ShowNotification($"Crafted {usedRecipe.recipeName}!");
        return true;
    }
    
    private void CheckForValidRecipe()
    {
        outputSlot.Clear();
        
        var availableRecipes = playerCraftingSystem?.GetUnlockedRecipes();
        if (availableRecipes == null) return;
        
        foreach (var recipe in availableRecipes)
        {
            if (playerCraftingSystem.CanUnlockRecipe(recipe) && DoIngredientsMatchRecipe(recipe))
            {
                outputSlot.SetItem(recipe.result, recipe.resultQuantity);
                break;
            }
        }
        
        OnCraftingSlotsChanged?.Invoke();
    }
    
    private CraftingRecipe FindMatchingRecipe()
    {
        var availableRecipes = playerCraftingSystem?.GetUnlockedRecipes();
        if (availableRecipes == null) return null;
        
        foreach (var recipe in availableRecipes)
        {
            if (recipe.result == outputSlot.item && DoIngredientsMatchRecipe(recipe))
                return recipe;
        }
        return null;
    }
    
    private bool DoIngredientsMatchRecipe(CraftingRecipe recipe)
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
    
    private void RemoveIngredientsFromSlots(InventoryItem item, int quantityToRemove)
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
    
    private void RemoveAllIngredients()
    {
        bool hadItems = false;
        foreach (var slot in inputSlots)
        {
            if (!slot.IsEmpty)
            {
                InventorySystem.Instance.AddItem(slot.item, slot.quantity);
                slot.Clear();
                hadItems = true;
            }
        }
        
        outputSlot.Clear();
        OnCraftingSlotsChanged?.Invoke();
        
        if (hadItems)
            NotificationSystem.ShowNotification("Items returned to inventory");
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
                RemoveAllIngredients();
                OnCraftingBenchClosed?.Invoke();
            }
        }
    }
}