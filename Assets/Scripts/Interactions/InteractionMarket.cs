using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player interactions with the market - buying, selling, and upgrades.
/// Bridges between MarketSystem and UI.
/// </summary>
public class InteractionMarket : MonoBehaviour, IInteractable
{
    #region Settings
    [Header("Sell Settings")]
    [SerializeField] private int sellSlotCount = 5;
    
    [Header("References")]
    [SerializeField] private CraftingSystemHUD craftingSystemHUD; // For upgrades
    public PlayerEconomy playerEconomy;
    #endregion
    
    #region Current State
    public List<MarketSellSlot> sellSlots;
    private bool isMarketOpen;
    #endregion
    
    #region Events
    public event System.Action OnMarketOpened;
    public event System.Action OnMarketClosed;
    public event System.Action OnSellSlotsChanged;
    public event System.Action OnTransactionCompleted;
    #endregion
    
    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSellSlots();
    }
    #endregion
    
    #region Interaction
    public void Interact(PlayerController player)
    {
        if (isMarketOpen)
            CloseMarket();
        else
            OpenMarket(player);
    }
    
    private void OpenMarket(PlayerController player)
    {
        isMarketOpen = true;
        
        if (playerEconomy == null)
            playerEconomy = player.GetComponent<PlayerEconomy>();
        
        OnMarketOpened?.Invoke();
        Debug.Log("Market opened!");
    }
    
    private void CloseMarket()
    {
        isMarketOpen = false;
        ReturnAllSellItems();
        OnMarketClosed?.Invoke();
        Debug.Log("Market closed");
    }
    #endregion
    
    #region Selling Operations
    
    public bool TryAddItemToSell(InventoryItem item, int quantity)
    {
        if (item == null || quantity <= 0) return false;
        if (!InventorySystem.Instance.HasItem(item, quantity)) return false;
        
        // Try to stack with existing item
        foreach (var slot in sellSlots)
        {
            if (!slot.IsEmpty && slot.item == item)
            {
                slot.quantity += quantity;
                slot.totalValue = playerEconomy.GetTotalSellValue(item, slot.quantity);
                InventorySystem.Instance.RemoveItem(item, quantity);
                OnSellSlotsChanged?.Invoke();
                return true;
            }
        }
        
        // Try to find empty slot
        foreach (var slot in sellSlots)
        {
            if (slot.IsEmpty)
            {
                int sellValue = playerEconomy.GetTotalSellValue(item, quantity);
                slot.SetItem(item, quantity, sellValue);
                InventorySystem.Instance.RemoveItem(item, quantity);
                OnSellSlotsChanged?.Invoke();
                return true;
            }
        }
        
        NotificationSystem.ShowNotification("No more empty slots, sell or remove some of the items.");
        return false;
    }
    
    public bool TryRemoveItemFromSell(int slotIndex, int quantity)
    {
        if (slotIndex < 0 || slotIndex >= sellSlots.Count) return false;
        
        var slot = sellSlots[slotIndex];
        if (slot.IsEmpty || slot.quantity < quantity) return false;
        
        InventorySystem.Instance.AddItem(slot.item, quantity);
        
        slot.quantity -= quantity;
        if (slot.quantity > 0)
        {
            slot.totalValue = playerEconomy.GetTotalSellValue(slot.item, slot.quantity);
        }
        else
        {
            slot.Clear();
        }
        
        OnSellSlotsChanged?.Invoke();
        return true;
    }
    
    public bool ConfirmSale()
    {
        int totalEarnings = GetTotalSellValue();
        if (totalEarnings <= 0) 
            return false;
        
        playerEconomy.AddMoney(totalEarnings);
        ClearAllSellSlots();
        
        OnSellSlotsChanged?.Invoke();
        OnTransactionCompleted?.Invoke();
        NotificationSystem.ShowNotification($"Sold items for {totalEarnings} coins!");
        return true;
    }
    
    #endregion
    
    #region Buying Operations
    
    public bool TryBuyItem(InventoryItem item, int quantity)
    {
        if (item == null || quantity <= 0) return false;
        if (MarketSystem.Instance == null) return false;
        
        if (!MarketSystem.Instance.IsItemAvailable(item))
        {
            Debug.Log("Item not available today!");
            return false;
        }
        
        bool success = playerEconomy.BuyItem(item, quantity);
        if (success)
        {
            OnTransactionCompleted?.Invoke();
        }
        return success;
    }
    
    public bool TryBuyUpgrade()
    {
        if (MarketSystem.Instance == null) return false;

        if (ResearchSystem.Instance.currentSeedsTier < 2)
        {
            NotificationSystem.ShowNotification("Unlock tier 2 seeds first!");
            return false;
        }
        bool success = MarketSystem.Instance.PurchaseCraftingBenchUpgrade(playerEconomy);
        
        if (success && craftingSystemHUD != null)
        {
            craftingSystemHUD.UnlockAllUpgradeSlots();
            NotificationSystem.ShowNotification("Crafting bench was upgraded, check it out!");
            OnTransactionCompleted?.Invoke();
        }
        
        return success;
    }
    
    #endregion
    
    #region Getters
    
    public bool IsOpen() => isMarketOpen;
    public bool HasItemsToSell() => sellSlots.Exists(slot => !slot.IsEmpty);
    
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
    
    public List<InventoryItem> GetAvailableItems()
    {
        return MarketSystem.Instance?.GetAvailableSeeds() ?? new List<InventoryItem>();
    }
    
    public bool IsCraftingBenchUpgradeAvailable()
    {
        return MarketSystem.Instance?.IsCraftingBenchUpgradeAvailable() ?? false;
    }
    
    public int GetCraftingBenchUpgradeCost()
    {
        return MarketSystem.Instance?.GetCraftingBenchUpgradeCost() ?? 0;
    }
    
    #endregion
    
    #region Private Helpers
    
    private void InitializeSellSlots()
    {
        sellSlots = new List<MarketSellSlot>();
        for (int i = 0; i < sellSlotCount; i++)
        {
            sellSlots.Add(new MarketSellSlot());
        }
    }
    
    private void ReturnAllSellItems()
    {
        foreach (var slot in sellSlots)
        {
            if (!slot.IsEmpty)
            {
                InventorySystem.Instance.AddItem(slot.item, slot.quantity);
                slot.Clear();
            }
        }
        OnSellSlotsChanged?.Invoke();
    }
    
    private void ClearAllSellSlots()
    {
        foreach (var slot in sellSlots)
        {
            slot.Clear();
        }
    }
    
    #endregion
    
    #region IInteractable
    
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
    
    #endregion
}

#region Data Structures
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
#endregion