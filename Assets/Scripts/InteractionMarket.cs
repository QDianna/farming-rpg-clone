using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interactive market system for buying and selling items with temporary sell slots.
/// Players can stage items for sale and confirm transactions to earn money.
/// </summary>
public class InteractionMarket : MonoBehaviour, IInteractable
{
    [Header("Market Settings")]
    [SerializeField] private List<InventoryItem> marketItems;
    [SerializeField] private int sellSlotCount = 3;
    
    [Header("System References")]
    public PlayerEconomy playerEconomy;
    public InventorySystem inventorySystem;
    
    public List<MarketSellSlot> sellSlots;
    private bool isMarketOpen;

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
    
    private void InitializeSlots()
    {
        sellSlots = new List<MarketSellSlot>();
        for (int i = 0; i < sellSlotCount; i++)
        {
            sellSlots.Add(new MarketSellSlot());
        }
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
    
    public bool IsItemAvailableForPurchase(InventoryItem item)
    {
        return marketItems.Contains(item);
    }
    
    public List<InventoryItem> GetAvailableItems()
    {
        return new List<InventoryItem>(marketItems);
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