using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Data container for research progress tracking and statistics.
/// </summary>
[Serializable]
public class ResearchProgress
{
    public int researchedCount;
    public int unlockedRecipes;
    public int totalRecipes;
    public int currentTier;
    public int maxTier;
}

/// <summary>
/// Singleton research system managing ingredient research, recipe unlocking, and tier progression.
/// Handles research validation, seasonal seed availability, and crafting system integration.
/// </summary>
public class ResearchSystem : MonoBehaviour
{
    public static ResearchSystem Instance { get; private set; }
    
    [Header("Research Data")]
    [SerializeField] private List<ItemSeed> allSeeds = new();
    
    private HashSet<string> researchedIngredients = new();
    private List<CraftingRecipe> allRecipes = new();
    
    [HideInInspector] public int currentSeedsTier = 1;
    public int maxSeedsTier = 2;
    
    public event Action OnResearchDataChanged;
    public event Action<int> OnTierUnlocked;
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        LoadCraftingRecipes();
    }
    
    // Researches ingredient and processes unlocks, or shows existing research
    public bool DoResearch(InventoryItem ingredient)
    {
        if (ingredient == null) 
            return false;
        
        bool wasAlreadyResearched = IsResearched(ingredient.name);
        
        if (!wasAlreadyResearched)
        {
            researchedIngredients.Add(ingredient.name);
            ProcessResearchUnlocks(ingredient);
            OnResearchDataChanged?.Invoke();
        }
        
        return true;
    }
    
    // Checks if ingredient has been researched
    public bool IsResearched(string ingredientName)
    {
        return researchedIngredients.Contains(ingredientName);
    }
    
    // Returns current research progress statistics
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
    
    // Returns seeds available for current tier and season
    public List<ItemSeed> GetAvailableSeeds()
    {
        var currentSeason = TimeSystem.Instance.GetSeason();
        bool isWarmSeason = TimeSystem.Instance.IsWarmSeason(currentSeason);
        
        return allSeeds
            .Where(seed => seed.tier <= currentSeedsTier && 
                          TimeSystem.Instance.IsWarmSeason(seed.season) == isWarmSeason)
            .ToList();
    }
    
    // Sets up singleton instance with persistence
    private void InitializeSingleton()
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
    
    // Loads recipe data from crafting system
    private void LoadCraftingRecipes()
    {
        if (CraftingSystem.Instance != null)
        {
            allRecipes = CraftingSystem.Instance.GetAllRecipes();
        }
    }
    
    // Processes unlocks based on researched ingredient type
    private void ProcessResearchUnlocks(InventoryItem ingredient)
    {
        if (ingredient is ItemSeed)
        {
            CheckTierProgression();
        }
        else
        {
            CheckRecipeUnlocks();
        }
    }
    
    // Checks and unlocks recipes when all ingredients are researched
    private void CheckRecipeUnlocks()
    {
        if (CraftingSystem.Instance == null) 
            return;
        
        foreach (var recipe in allRecipes)
        {
            if (ShouldUnlockRecipe(recipe))
            {
                CraftingSystem.Instance.MarkRecipeAsUnlocked(recipe);
            }
        }
    }
    
    // Checks if recipe should be unlocked based on researched ingredients
    private bool ShouldUnlockRecipe(CraftingRecipe recipe)
    {
        return !CraftingSystem.Instance.IsRecipeUnlocked(recipe) && 
               recipe.ingredients.All(ingredient => IsResearched(ingredient.item.name));
    }
    
    // Checks tier progression and unlocks next tier if conditions met
    private void CheckTierProgression()
    {
        var currentTierSeeds = GetCurrentTierSeeds();
        
        if (AllCurrentTierSeedsResearched(currentTierSeeds))
        {
            UnlockNextTier();
        }
        else
        {
            ShowTierProgressMessage();
        }
    }
    
    // Gets all seeds for current tier
    private List<ItemSeed> GetCurrentTierSeeds()
    {
        return allSeeds.Where(seed => seed.tier == currentSeedsTier).ToList();
    }
    
    // Checks if all current tier seeds have been researched
    private bool AllCurrentTierSeedsResearched(List<ItemSeed> currentTierSeeds)
    {
        return currentTierSeeds.Count > 0 && 
               currentTierSeeds.All(seed => IsResearched(seed.name));
    }
    
    // Unlocks next tier and shows notification
    private void UnlockNextTier()
    {
        currentSeedsTier++;
        OnTierUnlocked?.Invoke(currentSeedsTier);
        NotificationSystem.ShowNotification($"Congratulations, you've unlocked tier {currentSeedsTier} seeds!");
    }
    
    // Shows tier progression message
    private void ShowTierProgressMessage()
    {
        if (currentSeedsTier < maxSeedsTier)
            NotificationSystem.ShowNotification($"Keep researching more tier {currentSeedsTier} seeds " +
                                                $"to unlock next tier!");
        NotificationSystem.ShowNotification(($"Research all {maxSeedsTier} seeds " +
                                             $"to unlock ???"));
    }
}