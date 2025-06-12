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
    [SerializeField] private int dailySeedVariety = 2;
    [SerializeField] private int seedQuantityPerType = 3;
    private readonly List<InventoryItem> currentDailySeeds = new();

    [Header("Unlock-able Structures")]
    [SerializeField] private GameObject researchTable;
    [SerializeField] private GameObject craftingBench;
    [SerializeField] private int researchTableCost = 500;
    [SerializeField] private int craftingBenchCost = 1000;
    [SerializeField] private int craftingBenchUpgradeCost = 1500;
    
    private bool isResearchTableUnlocked;
    private bool isCraftingBenchUnlocked;
    private bool isCraftingBenchPurchased;
    private bool isCraftingBenchUpgraded;
    
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
    
    // CRAFTING BENCH METHODS
    
    /// <summary>
    /// Unlocks the crafting bench for purchase in the market
    /// </summary>
    public void UnlockCraftingBench()
    {
        isCraftingBenchUnlocked = true;
        OnMarketDataChanged?.Invoke();
    }

    /// <summary>
    /// Checks if the crafting bench is available for purchase
    /// </summary>
    public bool IsCraftingBenchAvailable()
    {
        return isCraftingBenchUnlocked && !isCraftingBenchPurchased;
    }

    /// <summary>
    /// Gets the cost of the crafting bench
    /// </summary>
    public int GetCraftingBenchCost()
    {
        return craftingBenchCost;
    }

    /// <summary>
    /// Processes crafting bench purchase and unhides the world object
    /// </summary>
    public bool PurchaseCraftingBench(PlayerEconomy playerEconomy)
    {
        if (!isCraftingBenchUnlocked || isCraftingBenchPurchased || !playerEconomy.CanAfford(craftingBenchCost))
            return false;
        
        // Process the purchase
        playerEconomy.SpendMoney(craftingBenchCost);
        
        // Unhide the crafting bench in the world
        UnhideCraftingBenchInWorld();
        
        // Mark as purchased
        isCraftingBenchPurchased = true;
        OnMarketDataChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Finds and unhides the crafting bench GameObject in the world
    /// </summary>
    private void UnhideCraftingBenchInWorld()
    {
        if (craftingBench != null)
        {
            craftingBench.SetActive(true);
            NotificationSystem.ShowNotification("Crafting Bench has been delivered to your farm!");
        }
        else
        {
            // Try finding by name as fallback
            GameObject foundBench = GameObject.Find("CraftingBench");
            if (foundBench != null)
            {
                foundBench.SetActive(true);
                NotificationSystem.ShowNotification("Crafting Bench has been delivered to your farm!");
            }
            else
            {
                Debug.LogError("Crafting Bench GameObject not found! Make sure it has the 'CraftingBench' tag or is named 'CraftingBench'");
            }
        }
    }

    // CRAFTING BENCH UPGRADE METHODS
    
    /// <summary>
    /// Checks if crafting bench upgrade can be purchased (only after bench is purchased)
    /// </summary>
    public bool IsCraftingBenchUpgradeAvailable()
    {
        return isCraftingBenchPurchased && !isCraftingBenchUpgraded;
    }
    
    /// <summary>
    /// Returns crafting bench upgrade cost
    /// </summary>
    public int GetCraftingBenchUpgradeCost()
    {
        return craftingBenchUpgradeCost;
    }
    
    /// <summary>
    /// Processes crafting bench upgrade purchase
    /// </summary>
    public bool PurchaseCraftingBenchUpgrade(PlayerEconomy playerEconomy)
    {
        if (!isCraftingBenchPurchased || isCraftingBenchUpgraded || !playerEconomy.CanAfford(craftingBenchUpgradeCost)) 
            return false;
        
        playerEconomy.SpendMoney(craftingBenchUpgradeCost);
        isCraftingBenchUpgraded = true;
        OnMarketDataChanged?.Invoke();
        return true;
    }

    // RESEARCH TABLE METHODS

    /// <summary>
    /// Unlocks the research table for purchase in the market
    /// </summary>
    public void UnlockResearchTable()
    {
        isResearchTableUnlocked = true;
        OnMarketDataChanged?.Invoke();
    }

    /// <summary>
    /// Checks if the research table is available for purchase
    /// </summary>
    public bool IsResearchTableAvailable()
    {
        return isResearchTableUnlocked;
    }

    /// <summary>
    /// Gets the cost of the research table
    /// </summary>
    public int GetResearchTableCost()
    {
        return researchTableCost;
    }

    /// <summary>
    /// Processes research table purchase and unhides the world object
    /// </summary>
    public bool PurchaseResearchTable(PlayerEconomy playerEconomy)
    {
        if (!isResearchTableUnlocked || !playerEconomy.CanAfford(researchTableCost))
            return false;
        
        // Process the purchase
        playerEconomy.SpendMoney(researchTableCost);
        
        // Unhide the research table in the world
        UnhideResearchTableInWorld();
        
        // Make it a one-time purchase
        isResearchTableUnlocked = false;
        OnMarketDataChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Finds and unhides the research table GameObject in the world
    /// </summary>
    private void UnhideResearchTableInWorld()
    {
        if (researchTable != null)
        {
            researchTable.SetActive(true);
            NotificationSystem.ShowNotification("Research Table has been delivered to your farm!");
        }
        else
        {
            // Try finding by name as fallback
            GameObject foundTable = GameObject.Find("ResearchTable");
            if (foundTable != null)
            {
                foundTable.SetActive(true);
                NotificationSystem.ShowNotification("Research Table has been delivered to your farm!");
            }
            else
            {
                Debug.LogError("Research Table GameObject not found! Make sure it has the 'ResearchTable' tag or is named 'ResearchTable'");
            }
        }
    }
    
    // SYSTEM SETUP METHODS
    
    /// <summary>
    /// Sets up singleton instance with persistence
    /// </summary>
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
    
    /// <summary>
    /// Sets up event subscriptions for daily and tier updates
    /// </summary>
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
    
    /// <summary>
    /// Removes event subscriptions
    /// </summary>
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
    
    /// <summary>
    /// Refreshes daily seed selection based on available research
    /// </summary>
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
    
    /// <summary>
    /// Randomly selects seeds for daily market
    /// </summary>
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
    
    /// <summary>
    /// Handles tier unlock events by refreshing market
    /// </summary>
    private void OnTierUnlocked(int newTier)
    {
        RefreshDailyItems();
    }
}