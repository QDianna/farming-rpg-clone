using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure data and logic system. Handles what's researched, what recipes exist, unlocking logic.
/// NO UI, NO interaction handling - just the brain of research.
/// </summary>
public class ResearchSystem : MonoBehaviour
{
    #region Singleton
    public static ResearchSystem Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    
    #region Data
    [SerializeField] private List<CraftingRecipe> allRecipes = new();
    private HashSet<string> researchedIngredients = new();
    private CraftingSystem craftingSystem;
    #endregion
    
    #region Events
    public event Action<ResearchResult> OnResearchCompleted;
    public event Action OnResearchDataChanged;
    #endregion
    
    #region Initialization
    private void Start()
    {
        craftingSystem = FindObjectOfType<CraftingSystem>();
        if (craftingSystem != null)
        {
            allRecipes = craftingSystem.GetAllRecipes();
            Debug.Log($"ResearchSystem: Found {allRecipes.Count} recipes");
        }
    }
    #endregion
    
    #region Public API - Simple and Clear
    
    /// <summary>
    /// Research an ingredient. Returns result with what was discovered.
    /// </summary>
    public ResearchResult DoResearch(InventoryItem ingredient)
    {
        if (ingredient == null) return null;
        
        string ingredientName = ingredient.name;
        bool wasNew = !researchedIngredients.Contains(ingredientName);
        
        // Add to researched
        researchedIngredients.Add(ingredientName);
        
        // Find recipes using this ingredient
        var recipes = GetRecipesUsingIngredient(ingredient);
        
        // Check for newly unlocked recipes
        var newlyUnlocked = CheckAndUnlockRecipes();
        
        var result = new ResearchResult
        {
            ingredientName = ingredientName,
            wasNewResearch = wasNew,
            availableRecipes = recipes,
            newlyUnlockedRecipes = newlyUnlocked
        };
        
        // Fire events
        OnResearchCompleted?.Invoke(result);
        OnResearchDataChanged?.Invoke();
        
        return result;
    }
    
    /// <summary>
    /// Check if ingredient is already researched
    /// </summary>
    public bool IsResearched(string ingredientName)
    {
        return researchedIngredients.Contains(ingredientName);
    }
    
    /// <summary>
    /// Get research progress stats
    /// </summary>
    public ResearchProgress GetProgress()
    {
        return new ResearchProgress
        {
            researchedIngredients = researchedIngredients.Count,
            totalIngredients = GetAllIngredients().Count,
            unlockedRecipes = craftingSystem?.GetUnlockedRecipes().Count ?? 0,
            totalRecipes = allRecipes.Count
        };
    }
    
    #endregion
    
    #region Helper Methods
    
    private List<ResearchFeedback> GetRecipesUsingIngredient(InventoryItem ingredient)
    {
        var results = new List<ResearchFeedback>();
        
        foreach (var recipe in allRecipes)
        {
            if (RecipeUsesIngredient(recipe, ingredient))
            {
                results.Add(new ResearchFeedback
                {
                    recipeName = recipe.recipeName,
                    canCraftNow = craftingSystem?.CanCraft(recipe) ?? false,
                    isRecipeUnlocked = craftingSystem?.IsRecipeUnlocked(recipe) ?? false
                });
            }
        }
        
        return results;
    }
    
    private bool RecipeUsesIngredient(CraftingRecipe recipe, InventoryItem ingredient)
    {
        foreach (var recipeIngredient in recipe.ingredients)
        {
            if (recipeIngredient.item == ingredient)
                return true;
        }
        return false;
    }
    
    private List<CraftingRecipe> CheckAndUnlockRecipes()
    {
        var newlyUnlocked = new List<CraftingRecipe>();
        
        if (craftingSystem == null) return newlyUnlocked;
        
        foreach (var recipe in allRecipes)
        {
            if (!craftingSystem.IsRecipeUnlocked(recipe) && AllIngredientsResearched(recipe))
            {
                craftingSystem.MarkRecipeAsUnlocked(recipe);
                newlyUnlocked.Add(recipe);
            }
        }
        
        return newlyUnlocked;
    }
    
    private bool AllIngredientsResearched(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (!researchedIngredients.Contains(ingredient.item.name))
                return false;
        }
        return true;
    }
    
    private HashSet<string> GetAllIngredients()
    {
        var ingredients = new HashSet<string>();
        foreach (var recipe in allRecipes)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                ingredients.Add(ingredient.item.name);
            }
        }
        return ingredients;
    }
    
    #endregion
}

#region Data Structures
[Serializable]
public class ResearchResult
{
    public string ingredientName;
    public bool wasNewResearch;
    public List<ResearchFeedback> availableRecipes;
    public List<CraftingRecipe> newlyUnlockedRecipes;
}

[Serializable]
public class ResearchFeedback
{
    public string recipeName;
    public bool canCraftNow;
    public bool isRecipeUnlocked;
}

[Serializable]
public class ResearchProgress
{
    public int researchedIngredients;
    public int totalIngredients;
    public int unlockedRecipes;
    public int totalRecipes;
}
#endregion