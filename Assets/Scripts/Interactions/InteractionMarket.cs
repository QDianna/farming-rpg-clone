using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interactive market system with seed tier progression, daily refreshes, and crafting bench upgrades.
/// </summary>
public class InteractionMarket : MonoBehaviour, IInteractable
{
    [Header("Market Settings")]
    [SerializeField] private int sellSlotCount = 3;
    [SerializeField] private int dailySeedVariety = 4; // How many different seeds to show daily
    
    [Header("Upgrade Settings")]
    [SerializeField] private int craftingBenchUpgradeCost = 500;
    [SerializeField] private CraftingSystemHUD craftingSystemHUD; // Reference to the crafting HUD
    
    [Header("System References")]
    public PlayerEconomy playerEconomy;
    public InventorySystem inventorySystem;
    
    // Current daily seed selection
    private List<InventoryItem> currentDailySeeds = new();
    public List<MarketSellSlot> sellSlots;
    private bool isMarketOpen;
    private bool craftingBenchUpgraded = false;

    public event System.Action OnMarketOpened;
    public event System.Action OnMarketClosed;
    public event System.Action OnMarketSlotsChanged;
    public event System.Action OnTransactionCompleted;

    [System.Serializable]
    public class MarketSellSlot
    {
        public InventoryItem item;
        public int quantity;
        public int totalValue;
        
        public bool IsEmpty => item == null || quantity <= 0;
        
        public void SetItem(InventoryItem newItem, int newQuantity, int value)
        {
            item = newItem;
            quantity = newQuantity;
            totalValue = value;
        }
        
        public void Clear()
        {
            item = null;
            quantity = 0;
            totalValue = 0;
        }
    }
    
    private void Awake()
    {
        InitializeSlots();
    }
    
    private void Start()
    {
        // Subscribe to day changes for daily seed refresh
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange += RefreshDailySeeds;
        }
        
        // Subscribe to tier unlocks for immediate seed refresh
        if (ResearchSystem.Instance != null)
        {
            ResearchSystem.Instance.OnTierUnlocked += OnTierUnlocked;
        }
        
        // Initial seed setup
        RefreshDailySeeds();
    }
    
    private void OnDestroy()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange -= RefreshDailySeeds;
        }
        
        if (ResearchSystem.Instance != null)
        {
            ResearchSystem.Instance.OnTierUnlocked -= OnTierUnlocked;
        }
    }
    
    private void InitializeSlots()
    {
        sellSlots = new List<MarketSellSlot>();
        for (int i = 0; i < sellSlotCount; i++)
        {
            sellSlots.Add(new MarketSellSlot());
        }
    }
    
    /// <summary>
    /// Refresh the daily seed selection based on current tier and season
    /// </summary>
    private void RefreshDailySeeds()
    {
        currentDailySeeds.Clear();
        
        if (ResearchSystem.Instance == null)
        {
            Debug.LogWarning("ResearchSystem not found - cannot refresh seeds");
            return;
        }
        
        // Get available seeds for current tier and season
        var availableSeeds = ResearchSystem.Instance.GetAvailableSeeds();
        
        if (availableSeeds.Count == 0)
        {
            Debug.Log("No seeds available for current tier/season");
            return;
        }
        
        // Randomly select seeds for today (or show all if fewer than dailySeedVariety)
        int seedsToShow = Mathf.Min(dailySeedVariety, availableSeeds.Count);
        
        // Shuffle and take the first seedsToShow items
        for (int i = 0; i < seedsToShow; i++)
        {
            int randomIndex = Random.Range(i, availableSeeds.Count);
            var temp = availableSeeds[i];
            availableSeeds[i] = availableSeeds[randomIndex];
            availableSeeds[randomIndex] = temp;
        }
        
        // Add to current daily selection
        for (int i = 0; i < seedsToShow; i++)
        {
            currentDailySeeds.Add(availableSeeds[i]);
        }
        
        Debug.Log($"Market: Refreshed daily seeds - {currentDailySeeds.Count} seeds available");
        
        // Notify UI to update if market is open
        OnTransactionCompleted?.Invoke();
    }
    
    /// <summary>
    /// Called when a new tier is unlocked - immediately refresh seeds
    /// </summary>
    private void OnTierUnlocked(int newTier)
    {
        RefreshDailySeeds();
        NotificationSystem.ShowNotification($"New Tier {newTier} seeds now available in market!");
    }
    
    public void Interact(PlayerController player)
    {
        if (isMarketOpen)
        {
            CloseMarket();
        }
        else
        {
            OpenMarket(player);
        }
    }
    
    private void OpenMarket(PlayerController player)
    {
        isMarketOpen = true;
        
        if (playerEconomy == null)
            playerEconomy = player.GetComponent<PlayerEconomy>();
        
        NotificationSystem.ShowNotification("Market opened!");
        OnMarketOpened?.Invoke();
    }
    
    private void CloseMarket()
    {
        isMarketOpen = false;
        ReturnSellItems();
        NotificationSystem.ShowNotification("Market closed");
        OnMarketClosed?.Invoke();
    }
    
    public bool TryAddItemToSell(InventoryItem item, int quantity)
    {
        if (item == null || quantity <= 0 || !inventorySystem.HasItem(item, quantity))
            return false;
        
        foreach (var slot in sellSlots)
        {
            if (slot.IsEmpty)
            {
                int sellValue = playerEconomy.GetTotalSellValue(item, quantity);
                slot.SetItem(item, quantity, sellValue);
                inventorySystem.RemoveItem(item, quantity);
                OnMarketSlotsChanged?.Invoke();
                return true;
            }
            else if (slot.item == item)
            {
                int additionalValue = playerEconomy.GetTotalSellValue(item, quantity);
                slot.quantity += quantity;
                slot.totalValue += additionalValue;
                inventorySystem.RemoveItem(item, quantity);
                OnMarketSlotsChanged?.Invoke();
                return true;
            }
        }
        return false;
    }
    
    public bool TryRemoveItemFromSell(int slotIndex, int quantity)
    {
        if (slotIndex < 0 || slotIndex >= sellSlots.Count)
            return false;
        
        var slot = sellSlots[slotIndex];
        if (slot.IsEmpty || slot.quantity < quantity)
            return false;
        
        inventorySystem.AddItem(slot.item, quantity);
        
        slot.quantity -= quantity;
        if (slot.quantity > 0)
        {
            slot.totalValue = playerEconomy.GetTotalSellValue(slot.item, slot.quantity);
        }
        else
        {
            slot.Clear();
        }
        
        OnMarketSlotsChanged?.Invoke();
        return true;
    }
    
    public bool ConfirmSale()
    {
        int totalEarnings = GetTotalSellValue();
        if (totalEarnings <= 0) 
        {
            NotificationSystem.ShowNotification("No items to sell");
            return false;
        }
        
        playerEconomy.AddMoney(totalEarnings);
        
        foreach (var slot in sellSlots)
            slot.Clear();
        
        OnMarketSlotsChanged?.Invoke();
        OnTransactionCompleted?.Invoke();
        NotificationSystem.ShowNotification($"Sold items for {totalEarnings} coins!");
        return true;
    }
    
    public bool TryBuyItem(InventoryItem item, int quantity)
    {
        if (item == null || quantity <= 0 || !IsItemAvailableForPurchase(item))
            return false;
        
        return playerEconomy.BuyItem(item, quantity);
    }
    
    /// <summary>
    /// Try to purchase crafting bench upgrade
    /// </summary>
    public bool TryBuyCraftingBenchUpgrade()
    {
        if (craftingBenchUpgraded)
        {
            NotificationSystem.ShowNotification("Crafting bench already upgraded!");
            return false;
        }
        
        if (!playerEconomy.CanAfford(craftingBenchUpgradeCost))
        {
            NotificationSystem.ShowNotification($"Need {craftingBenchUpgradeCost} coins to upgrade crafting bench!");
            return false;
        }
        
        if (craftingSystemHUD == null)
        {
            NotificationSystem.ShowNotification("Crafting system not found!");
            return false;
        }
        
        // Purchase the upgrade
        playerEconomy.SpendMoney(craftingBenchUpgradeCost);
        craftingBenchUpgraded = true;
        
        // Unlock the slots
        craftingSystemHUD.UnlockAllUpgradeSlots();
        
        NotificationSystem.ShowNotification($"Crafting bench upgraded for {craftingBenchUpgradeCost} coins!");
        OnTransactionCompleted?.Invoke(); // Refresh UI
        
        return true;
    }
    
    private void ReturnSellItems()
    {
        foreach (var slot in sellSlots)
        {
            if (!slot.IsEmpty)
            {
                inventorySystem.AddItem(slot.item, slot.quantity);
                slot.Clear();
            }
        }
        OnMarketSlotsChanged?.Invoke();
    }
    
    /// <summary>
    /// Check if item is available for purchase (in today's seed selection)
    /// </summary>
    public bool IsItemAvailableForPurchase(InventoryItem item)
    {
        return currentDailySeeds.Contains(item);
    }
    
    /// <summary>
    /// Get today's available seeds for purchase
    /// </summary>
    public List<InventoryItem> GetAvailableItems()
    {
        return new List<InventoryItem>(currentDailySeeds);
    }
    
    /// <summary>
    /// Check if crafting bench upgrade is available for purchase
    /// </summary>
    public bool IsCraftingBenchUpgradeAvailable()
    {
        return !craftingBenchUpgraded;
    }
    
    /// <summary>
    /// Get the cost of the crafting bench upgrade
    /// </summary>
    public int GetCraftingBenchUpgradeCost()
    {
        return craftingBenchUpgradeCost;
    }
    
    public int GetTotalSellValue()
    {
        int total = 0;
        foreach (var slot in sellSlots)
        {
            if (!slot.IsEmpty)
                total += slot.totalValue;
        }
        return total;
    }
    
    public bool HasItemsToSell()
    {
        foreach (var slot in sellSlots)
        {
            if (!slot.IsEmpty) return true;
        }
        return false;
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }
    
    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
            if (isMarketOpen)
                CloseMarket();
        }
    }
}