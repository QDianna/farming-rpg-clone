using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages crafting recipes, unlock progression, and recipe availability.
/// Handles recipe unlocking based on prerequisites and tracks crafted recipes for persistence.
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
            recipe.isUnlocked = recipe.startsUnlocked || craftedRecipeNames.Contains(recipe.recipeName);
        }
    }
    
    public void MarkRecipeAsCrafted(CraftingRecipe recipe)
    {
        if (!recipe.isUnlocked)
        {
            recipe.isUnlocked = true;
            craftedRecipeNames.Add(recipe.recipeName);
            NotificationSystem.ShowNotification($"New recipe unlocked: {recipe.recipeName}!");
            CheckForNewUnlocks();
        }
    }
    
    public bool CanUnlockRecipe(CraftingRecipe recipe)
    {
        if (recipe.isUnlocked || recipe.startsUnlocked) return true;
        
        foreach (var prerequisite in recipe.prerequisiteRecipes)
        {
            if (!prerequisite.isUnlocked) return false;
        }
        return true;
    }
    
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
    
    public bool TryCraft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe))
        {
            string reason = !CanUnlockRecipe(recipe) ? "recipe locked" : "missing ingredients";
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
    
    private void CheckForNewUnlocks()
    {
        foreach (var recipe in recipes)
        {
            if (!recipe.isUnlocked && CanUnlockRecipe(recipe))
                NotificationSystem.ShowNotification($"Recipe discovered: {recipe.recipeName}");
        }
    }
    
    public List<CraftingRecipe> GetUnlockedRecipes()
    {
        List<CraftingRecipe> unlocked = new List<CraftingRecipe>();
        
        foreach (var recipe in recipes)
        {
            if (CanUnlockRecipe(recipe))
                unlocked.Add(recipe);
        }
        
        return unlocked;
    }
}