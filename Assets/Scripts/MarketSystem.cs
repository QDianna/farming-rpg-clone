using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton market system managing daily seed availability and crafting upgrades.
/// Handles automatic daily refreshes, tier-based selection, and upgrade purchases.
/// </summary>
public class MarketSystem : MonoBehaviour
{
    public static MarketSystem Instance { get; private set; }
    
    [Header("Market Configuration")]
    [SerializeField] private int dailySeedVariety = 3;
    [SerializeField] private int seedQuantityPerType = 2;
    [SerializeField] private int craftingBenchUpgradeCost = 1500;
    
    private List<InventoryItem> currentDailySeeds = new();
    private bool craftingBenchUpgraded;
    
    public event System.Action OnMarketDataChanged;
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        SubscribeToEvents();
        RefreshDailyItems();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    // Returns current daily seed selection
    public List<InventoryItem> GetAvailableSeeds()
    {
        return new List<InventoryItem>(currentDailySeeds);
    }
    
    // Checks if specific item is available today
    public bool IsItemAvailable(InventoryItem item)
    {
        return currentDailySeeds.Contains(item);
    }
    
    // Checks if crafting bench upgrade can be purchased
    public bool IsCraftingBenchUpgradeAvailable()
    {
        return !craftingBenchUpgraded;
    }
    
    // Returns crafting bench upgrade cost
    public int GetCraftingBenchUpgradeCost()
    {
        return craftingBenchUpgradeCost;
    }
    
    // Processes crafting bench upgrade purchase
    public bool PurchaseCraftingBenchUpgrade(PlayerEconomy playerEconomy)
    {
        if (craftingBenchUpgraded || !playerEconomy.CanAfford(craftingBenchUpgradeCost)) 
            return false;
        
        playerEconomy.SpendMoney(craftingBenchUpgradeCost);
        craftingBenchUpgraded = true;
        OnMarketDataChanged?.Invoke();
        return true;
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
    
    // Sets up event subscriptions for daily and tier updates
    private void SubscribeToEvents()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange += RefreshDailyItems;
        }
        
        if (ResearchSystem.Instance != null)
        {
            ResearchSystem.Instance.OnTierUnlocked += OnTierUnlocked;
        }
    }
    
    // Removes event subscriptions
    private void UnsubscribeFromEvents()
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
    
    // Refreshes daily seed selection based on available research
    private void RefreshDailyItems()
    {
        currentDailySeeds.Clear();
        
        if (ResearchSystem.Instance == null) 
            return;
        
        var availableSeeds = ResearchSystem.Instance.GetAvailableSeeds();
        if (availableSeeds.Count == 0) 
            return;
        
        SelectRandomSeeds(availableSeeds);
        OnMarketDataChanged?.Invoke();
    }
    
    // Randomly selects seeds for daily market
    private void SelectRandomSeeds(List<ItemSeed> availableSeeds)
    {
        int seedTypesToShow = Mathf.Min(dailySeedVariety, availableSeeds.Count);
        
        // Shuffle using Fisher-Yates algorithm
        for (int i = 0; i < seedTypesToShow; i++)
        {
            int randomIndex = Random.Range(i, availableSeeds.Count);
            (availableSeeds[i], availableSeeds[randomIndex]) = (availableSeeds[randomIndex], availableSeeds[i]);
        }
        
        // Add shuffled selection to daily seeds
        for (int i = 0; i < seedTypesToShow; i++)
        {
            currentDailySeeds.Add(availableSeeds[i]);
        }
    }
    
    // Handles tier unlock events by refreshing market
    private void OnTierUnlocked(int newTier)
    {
        RefreshDailyItems();
    }
}