using System;
using UnityEngine;

/// <summary>
/// Manages player money, item trading, and price calculations.
/// Handles buy/sell transactions with configurable price modifiers.
/// </summary>
public class PlayerEconomy : MonoBehaviour
{
    [Header("Starting Economy")]
    [SerializeField] private int startingMoney = 500;
    
    [Header("Price Modifiers")]
    [SerializeField] private float sellPriceModifier = 0.7f;
    [SerializeField] private float buyPriceModifier = 1.2f;
    
    private int currentMoney;
    
    public int CurrentMoney => currentMoney;
    
    public event Action<int> OnMoneyChanged;
    public event Action<InventoryItem, int, int> OnItemSold;
    public event Action<InventoryItem, int, int> OnItemBought;
    
    private void Awake()
    {
        currentMoney = startingMoney;
        OnMoneyChanged?.Invoke(currentMoney);
    }
    
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
    }
    
    public bool SpendMoney(int amount)
    {
        if (amount <= 0 || currentMoney < amount)
            return false;
        
        currentMoney -= amount;
        OnMoneyChanged?.Invoke(currentMoney);
        return true;
    }
    
    public bool CanAfford(int amount)
    {
        return currentMoney >= amount;
    }
    
    public bool BuyItem(InventoryItem item, int quantity)
    {
        if (item == null || quantity <= 0)
        {
            Debug.Log("ERROR - invalid purchase was available");
            return false;
        }
        
        int totalPrice = GetTotalBuyValue(item, quantity);
        
        if (!CanAfford(totalPrice))
        {
            NotificationSystem.ShowNotification($"You don't have enough money for this, " +
                                                $"try selling some of your harvest!");
            return false;
        }
        
        SpendMoney(totalPrice);
        InventorySystem.Instance.AddItem(item, quantity);
        OnItemBought?.Invoke(item, quantity, totalPrice);
        return true;
    }
    
    public int GetSellPrice(InventoryItem item)
    {
        if (item == null) return 0;
        return Mathf.RoundToInt(item.basePrice * sellPriceModifier);
    }
    
    public int GetBuyPrice(InventoryItem item)
    {
        if (item == null) return 0;
        return Mathf.RoundToInt(item.basePrice * buyPriceModifier);
    }
    
    public int GetTotalSellValue(InventoryItem item, int quantity)
    {
        return GetSellPrice(item) * quantity;
    }
    
    public int GetTotalBuyValue(InventoryItem item, int quantity)
    {
        return GetBuyPrice(item) * quantity;
    }
    
    [ContextMenu("Add 100 Coins")]
    private void AddTestMoney() => AddMoney(100);
    
    [ContextMenu("Remove 50 Coins")]
    private void RemoveTestMoney() => SpendMoney(50);
}