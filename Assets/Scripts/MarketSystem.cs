using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure data and logic system for market operations.
/// Handles what's available for sale, daily refreshes, and upgrade availability.
/// </summary>
public class MarketSystem : MonoBehaviour
{
    #region Singleton
    public static MarketSystem Instance { get; private set; }
    
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
    
    #region Settings
    [Header("Market Settings")]
    [SerializeField] private int dailySeedVariety = 3; // How many different seed types per day
    [SerializeField] private int seedQuantityPerType = 2; // How many of each seed type
    [SerializeField] private int craftingBenchUpgradeCost = 1500;
    #endregion
    
    #region Data
    private List<InventoryItem> currentDailySeeds = new();
    private bool craftingBenchUpgraded = false;
    #endregion
    
    #region Events
    public event System.Action OnMarketDataChanged;
    #endregion
    
    #region Initialization
    private void Start()
    {
        // Subscribe to day changes for daily refresh
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange += RefreshDailyItems;
        }
        
        // Subscribe to tier unlocks for immediate refresh
        if (ResearchSystem.Instance != null)
        {
            ResearchSystem.Instance.OnTierUnlocked += OnTierUnlocked;
        }
        
        // Initial setup
        RefreshDailyItems();
    }
    
    private void OnDestroy()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange -= RefreshDailyItems;
        }
        
        if (ResearchSystem.Instance != null)
        {
            ResearchSystem.Instance.OnTierUnlocked -= OnTierUnlocked;
        }
    }
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Get today's available seeds for purchase
    /// </summary>
    public List<InventoryItem> GetAvailableSeeds()
    {
        return new List<InventoryItem>(currentDailySeeds);
    }
    
    /// <summary>
    /// Check if an item is available for purchase today
    /// </summary>
    public bool IsItemAvailable(InventoryItem item)
    {
        return currentDailySeeds.Contains(item);
    }
    
    /// <summary>
    /// Check if crafting bench upgrade is available
    /// </summary>
    public bool IsCraftingBenchUpgradeAvailable()
    {
        return !craftingBenchUpgraded;
    }
    
    /// <summary>
    /// Get crafting bench upgrade cost
    /// </summary>
    public int GetCraftingBenchUpgradeCost()
    {
        return craftingBenchUpgradeCost;
    }
    
    /// <summary>
    /// Purchase crafting bench upgrade
    /// </summary>
    public bool PurchaseCraftingBenchUpgrade(PlayerEconomy playerEconomy)
    {
        if (craftingBenchUpgraded) return false;
        if (!playerEconomy.CanAfford(craftingBenchUpgradeCost)) return false;
        
        playerEconomy.SpendMoney(craftingBenchUpgradeCost);
        craftingBenchUpgraded = true;
        OnMarketDataChanged?.Invoke();
        return true;
    }
    
    #endregion
    
    #region Private Logic
    
    private void RefreshDailyItems()
    {
        currentDailySeeds.Clear();
        
        if (ResearchSystem.Instance == null) return;
        
        // Get available seeds for current tier and season
        var availableSeeds = ResearchSystem.Instance.GetAvailableSeeds();
        if (availableSeeds.Count == 0) return;
        
        // Randomly select seeds for today
        int seedTypesToShow = Mathf.Min(dailySeedVariety, availableSeeds.Count);
        
        // Shuffle and take the first seedTypesToShow items
        for (int i = 0; i < seedTypesToShow; i++)
        {
            int randomIndex = Random.Range(i, availableSeeds.Count);
            (availableSeeds[i], availableSeeds[randomIndex]) = (availableSeeds[randomIndex], availableSeeds[i]);
        }
        
        // Add to current daily selection
        for (int i = 0; i < seedTypesToShow; i++)
        {
            currentDailySeeds.Add(availableSeeds[i]);
        }
        
        OnMarketDataChanged?.Invoke();
        Debug.Log($"MarketSystem: Refreshed daily items - {currentDailySeeds.Count} seed types available");
    }
    
    private void OnTierUnlocked(int newTier)
    {
        RefreshDailyItems();
    }
    
    private int GetSeedPrice(InventoryItem seed)
    {
        if (seed is ItemSeed itemSeed)
        {
            // Price based on tier - higher tier = more expensive
            return itemSeed.tier * 50 + 25;
        }
        return 50; // Default price
    }
    
    #endregion
}