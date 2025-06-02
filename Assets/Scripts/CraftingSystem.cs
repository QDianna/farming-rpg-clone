using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    [Header("Crafting Recipes")]
    [SerializeField] private List<CraftingRecipe> recipes;
    
    [Header("Save Data")]
    [SerializeField] private List<string> craftedRecipeNames = new List<string>(); // For persistence
    
    private InventorySystem inventorySystem;
    
    private void Start()
    {
        inventorySystem = GetComponent<InventorySystem>();
        InitializeRecipeUnlocks();
    }
    
    private void InitializeRecipeUnlocks()
    {
        foreach (var recipe in recipes)
        {
            if (recipe.startsUnlocked)
                recipe.isUnlocked = true;
            else
                recipe.isUnlocked = craftedRecipeNames.Contains(recipe.recipeName);
        }
    }
        // Add these methods to your CraftingSystem class:

    /// <summary>
    /// Mark a recipe as crafted (called by crafting bench)
    /// </summary>
    public void MarkRecipeAsCrafted(CraftingRecipe recipe)
    {
        if (!recipe.isUnlocked)
        {
            recipe.isUnlocked = true;
            craftedRecipeNames.Add(recipe.recipeName);
            Debug.Log($"Recipe unlocked: {recipe.recipeName}");
            CheckForNewUnlocks();
        }
    }

    /// <summary>
    /// Check if recipe can be unlocked (for crafting bench)
    /// </summary>
    public bool CanUnlockRecipe(CraftingRecipe recipe)
    {
        if (recipe.isUnlocked) return true;
        if (recipe.startsUnlocked) return true;
    
        foreach (var prerequisite in recipe.prerequisiteRecipes)
        {
            if (!prerequisite.isUnlocked)
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// Check if player can craft a recipe (unlocked + has ingredients)
    /// </summary>
    public bool CanCraft(CraftingRecipe recipe)
    {
        if (!CanUnlockRecipe(recipe)) return false;
        
        foreach (var ingredient in recipe.ingredients)
        {
            if (!inventorySystem.HasItem(ingredient.item, ingredient.quantity))
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// Attempt to craft an item
    /// </summary>
    public bool TryCraft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe))
        {
            if (!CanUnlockRecipe(recipe))
                Debug.Log($"Recipe {recipe.recipeName} is still locked!");
            else
                Debug.Log($"Cannot craft {recipe.recipeName} - missing ingredients");
            return false;
        }
        
        // Remove ingredients from inventory
        foreach (var ingredient in recipe.ingredients)
        {
            inventorySystem.RemoveItem(ingredient.item, ingredient.quantity);
        }
        
        // Add crafted item to inventory
        inventorySystem.AddItem(recipe.result, recipe.resultQuantity);
        
        // Unlock this recipe and save progress
        if (!recipe.isUnlocked)
        {
            recipe.isUnlocked = true;
            craftedRecipeNames.Add(recipe.recipeName);
            Debug.Log($"Recipe unlocked: {recipe.recipeName}");
            
            // Check if new recipes can be unlocked
            CheckForNewUnlocks();
        }
        
        Debug.Log($"Successfully crafted {recipe.resultQuantity}x {recipe.recipeName}");
        return true;
    }
    
    /// <summary>
    /// Check if any new recipes can be unlocked after crafting
    /// </summary>
    private void CheckForNewUnlocks()
    {
        foreach (var recipe in recipes)
        {
            if (!recipe.isUnlocked && CanUnlockRecipe(recipe))
            {
                Debug.Log($"New recipe available: {recipe.recipeName}");
            }
        }
    }
    
    /// <summary>
    /// Get all unlocked recipes that can be crafted
    /// </summary>
    public List<CraftingRecipe> GetCraftableRecipes()
    {
        List<CraftingRecipe> craftable = new List<CraftingRecipe>();
        
        foreach (var recipe in recipes)
        {
            if (recipe.isUnlocked && CanCraft(recipe))
                craftable.Add(recipe);
        }
        
        return craftable;
    }
    
    /// <summary>
    /// Get all unlocked recipes (for UI display)
    /// </summary>
    public List<CraftingRecipe> GetUnlockedRecipes()
    {
        List<CraftingRecipe> unlocked = new List<CraftingRecipe>();
        
        foreach (var recipe in recipes)
        {
            if (recipe.isUnlocked || CanUnlockRecipe(recipe))
                unlocked.Add(recipe);
        }
        
        return unlocked;
    }
}

