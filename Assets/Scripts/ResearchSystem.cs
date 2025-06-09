using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Pure data and logic system. Handles what's researched, what recipes exist, unlocking logic.
/// NOW ALSO handles tier progression for seeds/plants.
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
    [SerializeField] private List<ItemSeed> allSeeds = new(); // ADD THIS - drag your 21 seeds here
    
    private HashSet<string> researchedIngredients = new();
    [HideInInspector] public int currentSeedsTier = 1; // ADD THIS - tracks unlocked tier
    #endregion
    
    #region Events
    public event Action<ResearchResult> OnResearchCompleted;
    public event Action OnResearchDataChanged;
    public event Action<int> OnTierUnlocked; // ADD THIS - for market system
    #endregion
    
    #region Initialization
    private void Start()
    {
        // Get recipes from CraftingSystem singleton
        if (CraftingSystem.Instance != null)
        {
            allRecipes = CraftingSystem.Instance.GetAllRecipes();
            Debug.Log($"ResearchSystem: Found {allRecipes.Count} recipes");
        }
        
        Debug.Log($"ResearchSystem: Found {allSeeds.Count} seeds across {GetMaxTier()} tiers");
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
        
        var result = new ResearchResult
        {
            ingredientName = ingredientName,
            wasNewResearch = wasNew,
            availableRecipes = new List<ResearchFeedback>(),
            newlyUnlockedRecipes = new List<CraftingRecipe>()
        };
        
        // Check if this is a seed - handle tier progression
        if (ingredient is ItemSeed)
        {
            CheckTierProgression();
            // Don't show recipe information for seeds
        }
        else
        {
            // For non-seeds, find recipes and check unlocks
            result.availableRecipes = GetRecipesUsingIngredient(ingredient);
            result.newlyUnlockedRecipes = CheckAndUnlockRecipes();
        }
        
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
            unlockedRecipes = CraftingSystem.Instance?.GetUnlockedRecipes().Count ?? 0,
            totalRecipes = allRecipes.Count,
            currentTier = currentSeedsTier,
            maxTier = GetMaxTier()
        };
    }
    
    #endregion
    
    #region Tier Progression Logic - ADD THIS SECTION
    
    /// <summary>
    /// Check if we can unlock the next tier based on current tier research completion
    /// </summary>
    private void CheckTierProgression()
    {
        int maxTier = GetMaxTier();
        
        // Don't check if we're already at max tier
        if (currentSeedsTier >= maxTier) return;
        
        // Get all seeds from current tier
        var currentTierSeeds = GetSeedsForTier(currentSeedsTier);
        
        // Check if ALL seeds of current tier are researched
        bool allCurrentTierResearched = currentTierSeeds.All(seed => 
            researchedIngredients.Contains(seed.name));
        
        if (allCurrentTierResearched && currentTierSeeds.Count > 0)
        {
            currentSeedsTier++;
            OnTierUnlocked?.Invoke(currentSeedsTier);
            NotificationSystem.ShowNotification($"Tier {currentSeedsTier} seeds unlocked in market!");
            
            Debug.Log($"ResearchSystem: Tier {currentSeedsTier} unlocked! " +
                     $"Researched all {currentTierSeeds.Count} seeds from tier {currentSeedsTier - 1}");
        }
    }
    
    /// <summary>
    /// Get all seeds of a specific tier
    /// </summary>
    private List<ItemSeed> GetSeedsForTier(int tier)
    {
        return allSeeds
            .Where(seed => seed.tier == tier)
            .ToList();
    }
    
    /// <summary>
    /// Get the maximum tier available in the game
    /// </summary>
    private int GetMaxTier()
    {
        if (allSeeds.Count == 0) return 1;
        return allSeeds.Max(seed => seed.tier);
    }
    
    /// <summary>
    /// Get seeds available for current season and unlocked tier
    /// </summary>
    public List<ItemSeed> GetAvailableSeeds()
    {
        var currentSeason = TimeSystem.Instance.GetSeason();
        bool isWarmSeason = TimeSystem.Instance.isWarmSeason(currentSeason);
        
        return allSeeds
            .Where(seed => seed.tier <= currentSeedsTier && 
                          TimeSystem.Instance.isWarmSeason(seed.season) == isWarmSeason)
            .ToList();
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
                    canCraftNow = CraftingSystem.Instance?.CanCraft(recipe) ?? false,
                    isRecipeUnlocked = CraftingSystem.Instance?.IsRecipeUnlocked(recipe) ?? false
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
        
        if (CraftingSystem.Instance == null) return newlyUnlocked;
        
        foreach (var recipe in allRecipes)
        {
            if (!CraftingSystem.Instance.IsRecipeUnlocked(recipe) && AllIngredientsResearched(recipe))
            {
                CraftingSystem.Instance.MarkRecipeAsUnlocked(recipe);
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
    public int currentTier;     // ADD THIS
    public int maxTier;         // ADD THIS
}
#endregion