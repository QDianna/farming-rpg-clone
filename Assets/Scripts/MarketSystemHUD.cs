using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Market UI manager handling buy/sell interface with slot-based item staging.
/// Integrates with inventory clicks to add items to sell slots.
/// </summary>
public class MarketSystemHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private InteractionMarket market;
    
    private VisualElement marketContainer;
    private VisualElement sellSlotsContainer;
    private VisualElement buyItemsContainer;
    private Button confirmSale;
    private Label totalValue;
    private Label playerMoney;
    
    private void Awake()
    {
        SetupUIReferences();
    }
    
    private void Start()
    {
        SubscribeToEvents();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    private void SetupUIReferences()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        marketContainer = root.Q<VisualElement>("MarketContainer");
        sellSlotsContainer = root.Q<VisualElement>("SellSlotsContainer");
        buyItemsContainer = root.Q<VisualElement>("BuyItemsContainer");
        confirmSale = root.Q<Button>("ConfirmSale");
        totalValue = root.Q<Label>("TotalValue");
        playerMoney = root.Q<Label>("PlayerMoney");
        
        if (marketContainer != null)
        {
            marketContainer.style.display = DisplayStyle.None;
        }
        
        if (confirmSale != null)
        {
            confirmSale.clicked += OnConfirmSaleClicked;
        }
    }
    
    private void SubscribeToEvents()
    {
        if (market != null)
        {
            market.OnMarketOpened += OnMarketOpened;
            market.OnMarketClosed += OnMarketClosed;
            market.OnMarketSlotsChanged += UpdateSellSlotsDisplay;
            market.OnTransactionCompleted += OnTransactionCompleted;
        }
        
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryItemClicked += OnInventoryItemClicked;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (market != null)
        {
            market.OnMarketOpened -= OnMarketOpened;
            market.OnMarketClosed -= OnMarketClosed;
            market.OnMarketSlotsChanged -= UpdateSellSlotsDisplay;
            market.OnTransactionCompleted -= OnTransactionCompleted;
        }
        
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryItemClicked -= OnInventoryItemClicked;
        }
    }
    
    private void OnMarketOpened()
    {
        if (marketContainer != null)
        {
            marketContainer.style.display = DisplayStyle.Flex;
        }
        UpdateAllDisplays();
    }
    
    private void OnMarketClosed()
    {
        if (marketContainer != null)
        {
            marketContainer.style.display = DisplayStyle.None;
        }
    }
    
    private void OnTransactionCompleted()
    {
        UpdateAllDisplays();
    }
    
    private void OnConfirmSaleClicked()
    {
        market?.ConfirmSale();
    }
    
    private void OnInventoryItemClicked(InventoryItem item)
    {
        if (market != null && marketContainer != null && marketContainer.style.display == DisplayStyle.Flex)
        {
            market.TryAddItemToSell(item, 1);
        }
    }
    
    private void UpdateAllDisplays()
    {
        UpdateSellSlotsDisplay();
        UpdateBuyItemsDisplay();
        UpdateMoneyDisplay();
    }
    
    private void UpdateSellSlotsDisplay()
    {
        if (sellSlotsContainer == null || market == null) return;
        
        sellSlotsContainer.Clear();
        
        for (int i = 0; i < market.sellSlots.Count; i++)
        {
            var slot = market.sellSlots[i];
            var slotElement = CreateSellSlotElement(slot, i);
            sellSlotsContainer.Add(slotElement);
        }
        
        if (totalValue != null)
        {
            int value = market.GetTotalSellValue();
            totalValue.text = $"Sell all for: {value} coins";
        }
        
        if (confirmSale != null)
        {
            confirmSale.SetEnabled(market.HasItemsToSell());
        }
    }
    
    private void UpdateBuyItemsDisplay()
    {
        if (buyItemsContainer == null || market == null) return;
        
        buyItemsContainer.Clear();
        
        var availableItems = market.GetAvailableItems();
        foreach (var item in availableItems)
        {
            var itemElement = CreateBuyItemElement(item);
            buyItemsContainer.Add(itemElement);
        }
    }
    
    private void UpdateMoneyDisplay()
    {
        if (playerMoney != null && market?.playerEconomy != null)
        {
            playerMoney.text = $"Money: {market.playerEconomy.CurrentMoney} coins";
        }
    }
    
    private VisualElement CreateSellSlotElement(InteractionMarket.MarketSellSlot slot, int index)
    {
        var slotElement = new VisualElement();
        slotElement.AddToClassList("sell-slot");
    
        if (!slot.IsEmpty)
        {
            var icon = new VisualElement();
            icon.AddToClassList("item-icon");
            icon.style.backgroundImage = new StyleBackground(slot.item.sprite);
            slotElement.Add(icon);
        
            var quantityLabel = new Label(slot.quantity.ToString());
            quantityLabel.AddToClassList("item-quantity");
            slotElement.Add(quantityLabel);
        
            slotElement.RegisterCallback<ClickEvent>(evt => market.TryRemoveItemFromSell(index, 1));
            slotElement.tooltip = $"Click to return 1x {slot.item.name} to inventory";
        }
        else
        {
            slotElement.AddToClassList("empty");
        }
    
        return slotElement;
    }
    
    private VisualElement CreateBuyItemElement(InventoryItem item)
    {
        var itemElement = new VisualElement();
        itemElement.AddToClassList("buy-slot");
        
        var icon = new VisualElement();
        icon.AddToClassList("item-icon");
        icon.style.backgroundImage = new StyleBackground(item.sprite);
        itemElement.Add(icon);
        
        itemElement.RegisterCallback<ClickEvent>(evt => market.TryBuyItem(item, 1));
        
        int buyPrice = market.playerEconomy.GetBuyPrice(item);
        itemElement.tooltip = $"Click to buy 1x {item.name} for {buyPrice} coins";
        
        return itemElement;
    }
}