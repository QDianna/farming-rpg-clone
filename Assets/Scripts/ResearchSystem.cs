using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Pure data and logic system. Handles what's researched and unlocks recipes/tiers.
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
    [SerializeField] private List<ItemSeed> allSeeds = new();
    
    private HashSet<string> researchedIngredients = new();
    private List<CraftingRecipe> allRecipes = new();
    [HideInInspector] public int currentSeedsTier = 1;
    #endregion
    
    #region Events
    public event Action OnResearchDataChanged;
    public event Action<int> OnTierUnlocked;
    #endregion
    
    #region Initialization
    private void Start()
    {
        if (CraftingSystem.Instance != null)
        {
            allRecipes = CraftingSystem.Instance.GetAllRecipes();
        }
    }
    #endregion
    
    #region Core API
    
    /// <summary>
    /// Research an ingredient and handle unlocks
    /// </summary>
    public bool DoResearch(InventoryItem ingredient)
    {
        if (ingredient == null || IsResearched(ingredient.name)) 
            return false;
        
        // Add to researched
        researchedIngredients.Add(ingredient.name);
        
        // Handle unlocks based on item type
        if (ingredient is ItemSeed)
        {
            CheckTierProgression();
        }
        else
        {
            CheckRecipeUnlocks();
        }
        
        OnResearchDataChanged?.Invoke();
        return true;
    }
    
    /// <summary>
    /// Check if ingredient is researched
    /// </summary>
    public bool IsResearched(string ingredientName)
    {
        return researchedIngredients.Contains(ingredientName);
    }
    
    /// <summary>
    /// Get current research stats
    /// </summary>
    public ResearchProgress GetProgress()
    {
        return new ResearchProgress
        {
            researchedCount = researchedIngredients.Count,
            unlockedRecipes = CraftingSystem.Instance?.GetUnlockedRecipes().Count ?? 0,
            totalRecipes = allRecipes.Count,
            currentTier = currentSeedsTier,
            maxTier = allSeeds.Count > 0 ? allSeeds.Max(s => s.tier) : 1
        };
    }
    
    /// <summary>
    /// Get available seeds for market (current tier + season)
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
    
    #region Private Logic
    
    private void CheckRecipeUnlocks()
    {
        if (CraftingSystem.Instance == null) return;
        
        foreach (var recipe in allRecipes)
        {
            if (!CraftingSystem.Instance.IsRecipeUnlocked(recipe) && 
                recipe.ingredients.All(ing => IsResearched(ing.item.name)))
            {
                CraftingSystem.Instance.MarkRecipeAsUnlocked(recipe);
            }
        }
    }
    
    private void CheckTierProgression()
    {
        var currentTierSeeds = allSeeds.Where(s => s.tier == currentSeedsTier).ToList();
        
        if (currentTierSeeds.Count > 0 && 
            currentTierSeeds.All(seed => IsResearched(seed.name)))
        {
            currentSeedsTier++;
            OnTierUnlocked?.Invoke(currentSeedsTier);
            NotificationSystem.ShowNotification($"Congratulations, " +
                                                $"you've unlocked tier {currentSeedsTier} seeds!");
        }
    }
    
    #endregion
}

#region Simple Data Structures
[Serializable]
public class ResearchProgress
{
    public int researchedCount;
    public int unlockedRecipes;
    public int totalRecipes;
    public int currentTier;
    public int maxTier;
}
#endregion