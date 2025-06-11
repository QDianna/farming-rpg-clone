using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a sell slot in the market interface with item, quantity, and calculated value.
/// </summary>
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

/// <summary>
/// Interactive market system for buying seeds, selling items, and purchasing crafting upgrades.
/// Manages sell slots, validates transactions, and integrates with player economy.
/// </summary>
public class InteractionMarket : MonoBehaviour, IInteractable
{
    [Header("Market Settings")]
    [SerializeField] private int sellSlotCount = 5;
    
    [Header("References")]
    [SerializeField] private CraftingSystemHUD craftingSystemHUD;
    public PlayerEconomy playerEconomy;
    
    public List<MarketSellSlot> sellSlots;
    private bool isMarketOpen;
    
    public event System.Action OnMarketOpened;
    public event System.Action OnMarketClosed;
    public event System.Action OnSellSlotsChanged;
    public event System.Action OnTransactionCompleted;
    
    private void Awake()
    {
        InitializeSellSlots();
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }
    
    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
            if (isMarketOpen)
                CloseMarket();
        }
    }
    
    public void Interact(PlayerController player)
    {
        if (isMarketOpen)
            CloseMarket();
        else
            OpenMarket(player);
    }
    
    public bool TryAddItemToSell(InventoryItem item, int quantity)
    {
        if (!CanAddItemToSell(item, quantity))
            return false;
        
        // Try stacking with existing item first
        if (TryStackWithExistingItem(item, quantity))
            return true;
        
        // Find empty slot
        return TryAddToEmptySlot(item, quantity);
    }
    
    public bool TryRemoveItemFromSell(int slotIndex, int quantity)
    {
        if (!IsValidSlotIndex(slotIndex))
            return false;
        
        var slot = sellSlots[slotIndex];
        if (slot.IsEmpty || slot.quantity < quantity)
            return false;
        
        InventorySystem.Instance.AddItem(slot.item, quantity);
        UpdateSlotAfterRemoval(slot, quantity);
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
    
    public bool TryBuyItem(InventoryItem item, int quantity)
    {
        if (!CanBuyItem(item, quantity))
            return false;
        
        bool success = playerEconomy.BuyItem(item, quantity);
        if (success)
        {
            OnTransactionCompleted?.Invoke();
        }
        return success;
    }
    
    public bool TryBuyUpgrade()
    {
        if (!CanBuyUpgrade())
            return false;
        
        bool success = MarketSystem.Instance.PurchaseCraftingBenchUpgrade(playerEconomy);
        
        if (success && craftingSystemHUD != null)
        {
            craftingSystemHUD.UnlockAllUpgradeSlots();
            NotificationSystem.ShowNotification("Crafting bench was upgraded, check it out!");
            OnTransactionCompleted?.Invoke();
        }
        
        return success;
    }
    
    // Market state getters
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
    
    // Market data getters
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
    
    // Opens market interface and caches player economy reference
    private void OpenMarket(PlayerController player)
    {
        isMarketOpen = true;
        
        if (playerEconomy == null)
            playerEconomy = player.GetComponent<PlayerEconomy>();
        
        OnMarketOpened?.Invoke();
    }
    
    // Closes market and returns all sell items to inventory
    private void CloseMarket()
    {
        isMarketOpen = false;
        ReturnAllSellItems();
        OnMarketClosed?.Invoke();
    }
    
    // Creates initial sell slot collection
    private void InitializeSellSlots()
    {
        sellSlots = new List<MarketSellSlot>();
        for (int i = 0; i < sellSlotCount; i++)
        {
            sellSlots.Add(new MarketSellSlot());
        }
    }
    
    // Validates if item can be added for selling
    private bool CanAddItemToSell(InventoryItem item, int quantity)
    {
        if (item == null || quantity <= 0)
            return false;
        
        return InventorySystem.Instance.HasItem(item, quantity);
    }
    
    // Attempts to stack item with existing slot
    private bool TryStackWithExistingItem(InventoryItem item, int quantity)
    {
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
        return false;
    }
    
    // Attempts to add item to first empty slot
    private bool TryAddToEmptySlot(InventoryItem item, int quantity)
    {
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
    
    // Updates slot after partial item removal
    private void UpdateSlotAfterRemoval(MarketSellSlot slot, int quantityRemoved)
    {
        slot.quantity -= quantityRemoved;
        if (slot.quantity > 0)
        {
            slot.totalValue = playerEconomy.GetTotalSellValue(slot.item, slot.quantity);
        }
        else
        {
            slot.Clear();
        }
    }
    
    // Validates buy item conditions
    private bool CanBuyItem(InventoryItem item, int quantity)
    {
        if (item == null || quantity <= 0 || MarketSystem.Instance == null)
            return false;
        
        if (!MarketSystem.Instance.IsItemAvailable(item))
        {
            return false;
        }
        
        return true;
    }
    
    // Validates upgrade purchase conditions
    private bool CanBuyUpgrade()
    {
        if (MarketSystem.Instance == null)
            return false;
        
        if (ResearchSystem.Instance.currentSeedsTier < 2)
        {
            NotificationSystem.ShowNotification("Unlock tier 2 seeds first!");
            return false;
        }
        
        return true;
    }
    
    // Validates slot index bounds
    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < sellSlots.Count;
    }
    
    // Returns all sell items to player inventory
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
    
    // Clears all sell slots without returning items
    private void ClearAllSellSlots()
    {
        foreach (var slot in sellSlots)
        {
            slot.Clear();
        }
    }
}