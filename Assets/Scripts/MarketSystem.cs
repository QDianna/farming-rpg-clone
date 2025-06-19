using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton market system managing daily seed availability and structure purchases.
/// Handles automatic daily refreshes, tier-based selection, wood/money costs, and upgrade purchases.
/// </summary>
public class MarketSystem : MonoBehaviour
{
    public static MarketSystem Instance { get; private set; }
    
    [Header("Market Configuration")]
    [SerializeField] private int dailySeedVariety ;
    private readonly List<InventoryItem> currentDailySeeds = new();
    private readonly List<InventoryItem> currentDailyCrops = new();

    [Header("Unlock-able Structures")]
    [SerializeField] private GameObject researchTable;
    [SerializeField] private GameObject craftingBench;
    
    [Header("Structure Costs")]
    [SerializeField] private int researchTableCost;
    [SerializeField] private int researchTableWoodCost;
    [SerializeField] private int craftingBenchCost;
    [SerializeField] private int craftingBenchWoodCost;
    [SerializeField] private int craftingBenchUpgradeCost;
    
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
    
    // DAILY SEEDS AND CROPS METHODS
    
    public List<InventoryItem> GetAvailableSeeds()
    {
        return new List<InventoryItem>(currentDailySeeds);
    }

    public List<InventoryItem> GetAvailableCrops()
    {
        return new List<InventoryItem>(currentDailyCrops);
    }
    
    public bool IsItemAvailable(InventoryItem item)
    {
        return currentDailySeeds.Contains(item) || currentDailyCrops.Contains(item);
    }
    
    // RESEARCH TABLE METHODS
    
    public void UnlockResearchTable()
    {
        isResearchTableUnlocked = true;
        OnMarketDataChanged?.Invoke();
    }

    public bool IsResearchTableAvailable()
    {
        return isResearchTableUnlocked;
    }

    public int GetResearchTableCost()
    {
        return researchTableCost;
    }
    
    public int GetResearchTableWoodCost()
    {
        return researchTableWoodCost;
    }

    // Purchase with money and wood cost verification
    public bool PurchaseResearchTable(PlayerEconomy playerEconomy)
    {
        if (!isResearchTableUnlocked || 
            !playerEconomy.CanAfford(researchTableCost) || 
            !HasEnoughWood(researchTableWoodCost))
            return false;
        
        playerEconomy.SpendMoney(researchTableCost);
        ConsumeWood(researchTableWoodCost);
        UnhideResearchTableInWorld();
        
        isResearchTableUnlocked = false; // One-time purchase
        OnMarketDataChanged?.Invoke();
        return true;
    }
    
    // CRAFTING BENCH METHODS
    
    public void UnlockCraftingBench()
    {
        isCraftingBenchUnlocked = true;
        OnMarketDataChanged?.Invoke();
    }

    public bool IsCraftingBenchAvailable()
    {
        return isCraftingBenchUnlocked && !isCraftingBenchPurchased;
    }

    public int GetCraftingBenchCost()
    {
        return craftingBenchCost;
    }
    
    public int GetCraftingBenchWoodCost()
    {
        return craftingBenchWoodCost;
    }

    // Purchase with money and wood cost verification
    public bool PurchaseCraftingBench(PlayerEconomy playerEconomy)
    {
        if (!isCraftingBenchUnlocked || 
            isCraftingBenchPurchased || 
            !playerEconomy.CanAfford(craftingBenchCost) || 
            !HasEnoughWood(craftingBenchWoodCost))
            return false;
        
        playerEconomy.SpendMoney(craftingBenchCost);
        ConsumeWood(craftingBenchWoodCost);
        UnhideCraftingBenchInWorld();
        
        isCraftingBenchPurchased = true;
        OnMarketDataChanged?.Invoke();
        return true;
    }
    
    // CRAFTING BENCH UPGRADE METHODS
    
    // Only available after bench is purchased and witch quest completed
    public bool IsCraftingBenchUpgradeAvailable()
    {
        return isCraftingBenchPurchased && 
               !isCraftingBenchUpgraded && 
               QuestsSystem.Instance != null && 
               QuestsSystem.Instance.HasCompletedWitchQuest;
    }
    
    public int GetCraftingBenchUpgradeCost()
    {
        return craftingBenchUpgradeCost;
    }
    
    // Upgrade only costs money, no wood
    public bool PurchaseCraftingBenchUpgrade(PlayerEconomy playerEconomy)
    {
        if (!isCraftingBenchPurchased || isCraftingBenchUpgraded || !playerEconomy.CanAfford(craftingBenchUpgradeCost)) 
            return false;
        
        playerEconomy.SpendMoney(craftingBenchUpgradeCost);
        isCraftingBenchUpgraded = true;
        OnMarketDataChanged?.Invoke();
        return true;
    }
    
    // WOOD MANAGEMENT HELPERS
    
    // Check if player has enough wood using correct inventory methods
    private bool HasEnoughWood(int woodCost)
    {
        Debug.Log("not enough wood? " +  InventorySystem.Instance.HasItemByName("wood", woodCost));
        return InventorySystem.Instance != null && 
               InventorySystem.Instance.HasItemByName("wood", woodCost);
    }

    // Consume wood from inventory using correct methods
    private void ConsumeWood(int woodCost)
    {
        if (InventorySystem.Instance != null)
        {
            var woodItem = InventorySystem.Instance.FindItemByName("wood");
            if (woodItem != null)
            {
                InventorySystem.Instance.RemoveItem(woodItem, woodCost);
            }
        }
    }
    
    // WORLD OBJECT ACTIVATION
    
    private void UnhideResearchTableInWorld()
    {
        if (researchTable == null) return;
        
        researchTable.SetActive(true);
        NotificationSystem.ShowHelp("Research Table has been delivered to your farm!");
    }

    private void UnhideCraftingBenchInWorld()
    {
        if (craftingBench == null) return;
        
        craftingBench.SetActive(true);
        NotificationSystem.ShowHelp("Crafting Bench has been delivered to your farm!");
    }
    
    // DAILY MARKET REFRESH
    
    // Refreshes daily seed and crop selection based on current research tier
    private void RefreshDailyItems()
    {
        currentDailySeeds.Clear();
        currentDailyCrops.Clear();
        
        if (ResearchSystem.Instance == null) 
            return;
        
        var availableSeeds = ResearchSystem.Instance.GetAvailableSeeds();
        if (availableSeeds.Count == 0) 
            return;
        
        SelectRandomSeedsAndCrops(availableSeeds);
        OnMarketDataChanged?.Invoke();
    }
    
    // Randomly selects seeds and their corresponding crops
    private void SelectRandomSeedsAndCrops(List<ItemSeed> availableSeeds)
    {
        int seedTypesToShow = Mathf.Min(dailySeedVariety, availableSeeds.Count);
        
        // Fisher-Yates shuffle for seed selection
        for (int i = 0; i < seedTypesToShow; i++)
        {
            int randomIndex = Random.Range(i, availableSeeds.Count);
            (availableSeeds[i], availableSeeds[randomIndex]) = (availableSeeds[randomIndex], availableSeeds[i]);
        }
        
        // Add selected seeds and their corresponding crops
        for (int i = 0; i < seedTypesToShow; i++)
        {
            var selectedSeed = availableSeeds[i];
            currentDailySeeds.Add(selectedSeed);
            
            // Add the corresponding crop if it exists
            if (selectedSeed.resultedCrop != null)
            {
                currentDailyCrops.Add(selectedSeed.resultedCrop);
            }
        }
        
        Debug.Log($"[Market] Daily refresh: {currentDailySeeds.Count} seeds, {currentDailyCrops.Count} crops");
    }
    
    // SYSTEM SETUP AND EVENTS
    
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
        
        if (QuestsSystem.Instance != null)
        {
            QuestsSystem.Instance.OnWitchFirstMet += OnWitchFirstMet;
            QuestsSystem.Instance.OnWitchQuestCompleted += OnWitchQuestCompleted;
        }
    }
    
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
        
        if (QuestsSystem.Instance != null)
        {
            QuestsSystem.Instance.OnWitchFirstMet -= OnWitchFirstMet;
            QuestsSystem.Instance.OnWitchQuestCompleted -= OnWitchQuestCompleted;
        }
    }
    
    // EVENT HANDLERS
    
    private void OnTierUnlocked(int newTier)
    {
        RefreshDailyItems();
    }
    
    // Unlocks both structures when witch is first met
    private void OnWitchFirstMet()
    {
        UnlockResearchTable();
        UnlockCraftingBench();
    }
    
    // Makes upgrade available after quest completion
    private void OnWitchQuestCompleted()
    {
        if (isCraftingBenchPurchased)
        {
            OnMarketDataChanged?.Invoke();
            NotificationSystem.ShowHelp("Crafting bench upgrade is now available!");
        }
    }
}