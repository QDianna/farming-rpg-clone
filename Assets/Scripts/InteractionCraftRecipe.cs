using System.Collections.Generic;
using UnityEngine;

public class InteractionCraftRecipe : MonoBehaviour, IInteractable
{
    [Header("Crafting Bench Settings")]
    [SerializeField] private int inputSlotCount = 3; // How many ingredient slots
    
    [Header("Current Crafting State")]
    public List<InventoryItemSlot> inputSlots;  // Made public for UI access
    public InventoryItemSlot outputSlot;        // Made public for UI access
    
    private CraftingSystem playerCraftingSystem;
    private CraftingSystemHUD uiController;

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
    
    private void Start()
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
        
        if (isBenchOpen)
        {
            // Close crafting bench
            isBenchOpen = false;
            OnCraftingBenchClosed?.Invoke();
        }
        else
        {
            // Open crafting bench
            isBenchOpen = true;
            OpenCraftingBench();
        }
    }
    
    private void OpenCraftingBench()
    {
        Debug.Log("Crafting interaction start...");
        OnCraftingBenchOpened?.Invoke();
        
        // Ask CraftingSystem what recipes are available
        var availableRecipes = playerCraftingSystem.GetUnlockedRecipes();
        
        foreach (var recipe in availableRecipes)
        {
            bool canCraft = playerCraftingSystem.CanUnlockRecipe(recipe);
            if (canCraft)
                Debug.Log($"Recipe {recipe.recipeName} is available for crafting!");
            else 
                Debug.Log($"Recipe {recipe.recipeName} is NOT available for crafting.");
        }
    }
    
    /// <summary>
    /// Add item to the first available input slot
    /// </summary>
    public bool TryAddIngredient(InventoryItem item, int quantity)
    {
        // Find first empty slot or slot with same item
        foreach (var slot in inputSlots)
        {
            if (slot.IsEmpty)
            {
                slot.SetItem(item, quantity);
                CheckForValidRecipe();
                OnCraftingSlotsChanged?.Invoke();  // Notify UI
                return true;
            }
            else if (slot.item == item)
            {
                slot.quantity += quantity;
                CheckForValidRecipe();
                OnCraftingSlotsChanged?.Invoke();  // Notify UI
                return true;
            }
        }
        
        Debug.Log("No available slots for ingredient");
        return false;
    }
    
    /// <summary>
    /// Remove item from input slot
    /// </summary>
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
        OnCraftingSlotsChanged?.Invoke();  // Notify UI
        return true;
    }
    
    /// <summary>
    /// Check if current ingredients match any recipe
    /// </summary>
    private void CheckForValidRecipe()
    {
        outputSlot.Clear();
        
        // Get available recipes from CraftingSystem
        var availableRecipes = playerCraftingSystem.GetUnlockedRecipes();
        
        foreach (var recipe in availableRecipes)
        {
            if (playerCraftingSystem != null && !playerCraftingSystem.CanUnlockRecipe(recipe))
                continue;
            
            if (DoIngredientsMatchRecipe(recipe))
            {
                outputSlot.SetItem(recipe.result, recipe.resultQuantity);
                Debug.Log($"Valid recipe found: {recipe.recipeName}");
                OnCraftingSlotsChanged?.Invoke();  // Notify UI
                return;
            }
        }
        
        OnCraftingSlotsChanged?.Invoke();  // Notify UI even if no recipe found
    }
    
    /// <summary>
    /// Check if current input slots match recipe requirements
    /// </summary>
    private bool DoIngredientsMatchRecipe(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            int totalQuantity = 0;
            
            // Count total quantity of this ingredient in input slots
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
    
    /// <summary>
    /// Take the crafted item from output slot
    /// </summary>
    public bool TryTakeOutput()
    {
        if (outputSlot.IsEmpty)
        {
            Debug.Log("No item to take from output");
            return false;
        }
        
        // Get available recipes from CraftingSystem
        var availableRecipes = playerCraftingSystem.GetUnlockedRecipes();
        
        // Find which recipe was used
        CraftingRecipe usedRecipe = null;
        foreach (var recipe in availableRecipes)
        {
            if (recipe.result == outputSlot.item && DoIngredientsMatchRecipe(recipe))
            {
                usedRecipe = recipe;
                break;
            }
        }
        
        if (usedRecipe == null)
            return false;
        
        // Add item to player inventory
        InventorySystem.Instance.AddItem(outputSlot.item, outputSlot.quantity);
        
        // Remove consumed ingredients
        foreach (var ingredient in usedRecipe.ingredients)
        {
            RemoveIngredientsFromSlots(ingredient.item, ingredient.quantity);
        }
        
        // Unlock recipe if first time crafting
        if (playerCraftingSystem != null)
        {
            playerCraftingSystem.MarkRecipeAsCrafted(usedRecipe);
        }
        
        // Clear output
        outputSlot.Clear();
        OnCraftingSlotsChanged?.Invoke();  // Notify UI
        
        Debug.Log($"Successfully crafted {usedRecipe.recipeName}");
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
    
    // IInteractable implementation - KEPT!
    public void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("Press E to use crafting bench!");
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }
    
    public void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("exited interaction");
            InteractionSystem.Instance.SetCurrentInteractable(null);
        }
    }
}