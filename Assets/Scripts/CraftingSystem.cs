using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages crafting recipes, unlock progression, and recipe availability.
/// Handles recipe unlocking based on research completion and prerequisites.
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    #region Singleton
    public static CraftingSystem Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    #endregion
    
    [Header("Crafting System Data")]
    [SerializeField] private List<CraftingRecipe> recipes;
    
    
    #region Recipe Unlocking
    
    /// <summary>
    /// Mark a recipe as unlocked - called by ResearchSystem when all ingredients researched
    /// </summary>
    public void MarkRecipeAsUnlocked(CraftingRecipe recipe)
    {
        if (!recipe.isUnlocked)
        {
            recipe.isUnlocked = true;
        }
    }
    
    /// <summary>
    /// Check if a recipe is currently unlocked
    /// </summary>
    public bool IsRecipeUnlocked(CraftingRecipe recipe)
    {
        return recipe.isUnlocked;
    }

    #endregion
    
    
    #region Public API
    
    /// <summary>
    /// Get all recipes (for ResearchSystem to analyze)
    /// </summary>
    public List<CraftingRecipe> GetAllRecipes()
    {
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
    
    #endregion
    
}