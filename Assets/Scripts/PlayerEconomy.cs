using System;
using UnityEngine;

/// <summary>
/// Player economy system managing money, item trading, and price calculations.
/// Handles buy/sell transactions with configurable price modifiers and event notifications.
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
    
    private void Awake()
    {
        InitializeEconomy();
    }
    
    public void AddMoney(int amount)
    {
        if (amount <= 0) 
            return;
        
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
        if (!IsValidPurchase(item, quantity))
            return false;
        
        int totalPrice = GetTotalBuyValue(item, quantity);
        
        if (!CanAfford(totalPrice))
        {
            ShowInsufficientFundsMessage();
            return false;
        }
        
        CompletePurchase(item, quantity, totalPrice);
        return true;
    }
    
    // Price calculation methods
    public int GetSellPrice(InventoryItem item)
    {
        if (item == null) 
            return 0;
        return Mathf.RoundToInt(item.basePrice * sellPriceModifier);
    }
    
    public int GetBuyPrice(InventoryItem item)
    {
        if (item == null) 
            return 0;
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
    
    // Sets up initial money amount and triggers change event
    private void InitializeEconomy()
    {
        currentMoney = startingMoney;
        OnMoneyChanged?.Invoke(currentMoney);
    }
    
    // Validates purchase parameters
    private bool IsValidPurchase(InventoryItem item, int quantity)
    {
        return item != null && quantity > 0;
    }
    
    // Shows insufficient funds notification
    private void ShowInsufficientFundsMessage()
    {
        NotificationSystem.ShowHelp("You don't have enough money for this, try selling some of your harvest!");
    }
    
    // Processes successful purchase transaction
    private void CompletePurchase(InventoryItem item, int quantity, int totalPrice)
    {
        SpendMoney(totalPrice);
        InventorySystem.Instance.AddItem(item, quantity);
    }
    
    [ContextMenu("Add 100 Coins")]
    private void AddTestMoney() => AddMoney(100);
    
    [ContextMenu("Remove 50 Coins")]
    private void RemoveTestMoney() => SpendMoney(50);
}