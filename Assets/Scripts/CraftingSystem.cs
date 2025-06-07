using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages crafting recipes, unlock progression, and recipe availability.
/// Handles recipe unlocking based on research completion and prerequisites.
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    [Header("Crafting Recipes")]
    [SerializeField] private List<CraftingRecipe> recipes;
    
    [Header("Save Data")]
    [SerializeField] private List<string> craftedRecipeNames = new List<string>();

    private InventorySystem inventorySystem;
    
    private void Awake()
    {
        inventorySystem = GetComponent<InventorySystem>();
        InitializeRecipeUnlocks();
    }
    
    private void InitializeRecipeUnlocks()
    {
        foreach (var recipe in recipes)
        {
            // Set unlock status based on startsUnlocked flag or previous crafting
            recipe.isUnlocked = craftedRecipeNames.Contains(recipe.recipeName);
        }
    }
    
    #region Recipe Unlocking
    
    /// <summary>
    /// Mark a recipe as unlocked (called by ResearchSystem when all ingredients researched)
    /// </summary>
    public void MarkRecipeAsUnlocked(CraftingRecipe recipe)
    {
        if (!recipe.isUnlocked)
        {
            recipe.isUnlocked = true;
            NotificationSystem.ShowNotification($"New recipe available: {recipe.recipeName}!");
        }
    }
    
    /// <summary>
    /// Mark a recipe as crafted (called when player successfully crafts it)
    /// </summary>
    public void MarkRecipeAsCrafted(CraftingRecipe recipe)
    {
        if (!craftedRecipeNames.Contains(recipe.recipeName))
        {
            craftedRecipeNames.Add(recipe.recipeName);
            NotificationSystem.ShowNotification($"Recipe mastered: {recipe.recipeName}!");
        }
    }
    
    /// <summary>
    /// Check if a recipe can be unlocked based on prerequisites
    /// </summary>
    public bool CanUnlockRecipe(CraftingRecipe recipe)
    {
        if (recipe.isUnlocked) return true;
        
        // Check prerequisite recipes
        foreach (var prerequisite in recipe.prerequisiteRecipes)
        {
            if (!prerequisite.isUnlocked || !craftedRecipeNames.Contains(prerequisite.recipeName))
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// Check if a recipe is currently unlocked
    /// </summary>
    public bool IsRecipeUnlocked(CraftingRecipe recipe)
    {
        return recipe.isUnlocked && CanUnlockRecipe(recipe);
    }

    #endregion
    
    #region Crafting Operations
    
    /// <summary>
    /// Check if a recipe can be crafted (unlocked + has ingredients)
    /// </summary>
    public bool CanCraft(CraftingRecipe recipe)
    {
        if (!IsRecipeUnlocked(recipe)) return false;
        
        foreach (var ingredient in recipe.ingredients)
        {
            if (!inventorySystem.HasItem(ingredient.item, ingredient.quantity))
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// Attempt to craft a recipe
    /// </summary>
    public bool TryCraft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe))
        {
            string reason = !IsRecipeUnlocked(recipe) ? "recipe locked" : "missing ingredients";
            NotificationSystem.ShowNotification($"Cannot craft {recipe.recipeName} - {reason}");
            return false;
        }
        
        // Remove ingredients and add result
        foreach (var ingredient in recipe.ingredients)
            inventorySystem.RemoveItem(ingredient.item, ingredient.quantity);
        
        inventorySystem.AddItem(recipe.result, recipe.resultQuantity);
        MarkRecipeAsCrafted(recipe);
        
        NotificationSystem.ShowNotification($"Crafted {recipe.resultQuantity}x {recipe.recipeName}!");
        return true;
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Get all recipes (for ResearchSystem to analyze)
    /// </summary>
    public List<CraftingRecipe> GetAllRecipes()
    {
        Debug.Log(recipes);
        return new List<CraftingRecipe>(recipes);
    }
    
    /// <summary>
    /// Get only unlocked recipes (for CraftingSystemHUD to display)
    /// </summary>
    public List<CraftingRecipe> GetUnlockedRecipes()
    {
        List<CraftingRecipe> unlocked = new List<CraftingRecipe>();
        
        foreach (var recipe in recipes)
        {
            if (IsRecipeUnlocked(recipe))
                unlocked.Add(recipe);
        }
        
        return unlocked;
    }
    
    /// <summary>
    /// Find a recipe by its result item
    /// </summary>
    public CraftingRecipe FindRecipeByResult(InventoryItem result)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.result == result)
                return recipe;
        }
        return null;
    }
    
    #endregion
    
    /*#region Debug Methods
    
    [ContextMenu("Debug Crafting State")]
    private void DebugCraftingState()
    {
        Debug.Log($"Total Recipes: {recipes.Count}");
        Debug.Log($"Unlocked Recipes: {GetUnlockedRecipes().Count}");
        Debug.Log($"Crafted Recipes: {craftedRecipeNames.Count}");
        
        foreach (var recipe in recipes)
        {
            string status = recipe.isUnlocked ? "UNLOCKED" : "LOCKED";
            string crafted = craftedRecipeNames.Contains(recipe.recipeName) ? " (CRAFTED)" : "";
            Debug.Log($"- {recipe.recipeName}: {status}{crafted}");
        }
    }
    
        
    /// <summary>
    /// Check for recipes that can be unlocked due to dependencies being met
    /// </summary>
    private void CheckForDependencyUnlocks()
    {
        foreach (var recipe in recipes)
        {
            if (!recipe.isUnlocked && CanUnlockRecipe(recipe))
            {
                // This recipe's dependencies are met, but we need ResearchSystem to check ingredients
                Debug.Log($"Recipe {recipe.recipeName} dependencies met - awaiting ingredient research");
            }
        }
    }
    
    #endregion*/
}