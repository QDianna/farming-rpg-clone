using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton crafting system managing recipe unlocking and availability.
/// Integrates with research system to progressively unlock recipes based on ingredient knowledge.
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }
    
    [Header("Recipe Database")]
    [SerializeField] private List<CraftingRecipe> allRecipes;
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    // Marks recipe as unlocked when research requirements are met
    public void MarkRecipeAsUnlocked(CraftingRecipe recipe)
    {
        if (!recipe.isUnlocked)
        {
            recipe.isUnlocked = true;
        }
    }
    
    // Checks if specific recipe is currently unlocked
    public bool IsRecipeUnlocked(CraftingRecipe recipe)
    {
        return recipe.isUnlocked;
    }
    
    // Returns complete recipe collection for research system analysis
    public List<CraftingRecipe> GetAllRecipes()
    {
        return new List<CraftingRecipe>(allRecipes);
    }
    
    // Returns only unlocked recipes for crafting interface display
    public List<CraftingRecipe> GetUnlockedRecipes()
    {
        List<CraftingRecipe> unlockedRecipes = new List<CraftingRecipe>();
        
        foreach (var recipe in allRecipes)
        {
            if (IsRecipeUnlocked(recipe))
                unlockedRecipes.Add(recipe);
        }
        
        return unlockedRecipes;
    }
    
    // Sets up singleton instance
    private void InitializeSingleton()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}