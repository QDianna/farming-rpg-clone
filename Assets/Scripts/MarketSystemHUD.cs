using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Market UI manager handling buy/sell interface with seed progression and crafting bench upgrades.
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
        buyItemsContainer = root.Q<VisualElement>("BuySlotsContainer");
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
            market.OnSellSlotsChanged += UpdateSellSlotsDisplay; // Updated event name
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
            market.OnSellSlotsChanged -= UpdateSellSlotsDisplay; // Updated event name
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
            totalValue.text = $"Sell for {value}";
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
        
        // Add seeds section
        var seedTitle = new Label("Seeds Available Today");
        seedTitle.AddToClassList("market-section-title");
        buyItemsContainer.Add(seedTitle);
        
        // Add current tier info
        if (ResearchSystem.Instance != null)
        {
            var tierInfo = new Label($"Current Seeds Tier {ResearchSystem.Instance.currentSeedsTier}");
            tierInfo.AddToClassList("market-tier-info");
            buyItemsContainer.Add(tierInfo);
        }
        
        // Add available seeds - Updated to use new API
        var availableItems = market.GetAvailableItems();
        if (availableItems.Count > 0)
        {
            var seedsContainer = new VisualElement();
            seedsContainer.AddToClassList("seeds-grid");
            
            foreach (var item in availableItems)
            {
                var itemElement = CreateBuyItemElement(item);
                seedsContainer.Add(itemElement);
            }
            
            buyItemsContainer.Add(seedsContainer);
        }
        else
        {
            var noSeedsLabel = new Label("No seeds available for current tier/season");
            noSeedsLabel.AddToClassList("market-no-items");
            buyItemsContainer.Add(noSeedsLabel);
        }

        // Add crafting bench upgrade - Updated to use new API
        if (market.IsCraftingBenchUpgradeAvailable())
        {
            var upgradeElement = CreateCraftingBenchUpgradeElement();
            buyItemsContainer.Add(upgradeElement);
        }
        else
        {
            var upgradedLabel = new Label("No more upgrades available");
            upgradedLabel.AddToClassList("market-upgraded");
            buyItemsContainer.Add(upgradedLabel);
        }
    }
    
    private void UpdateMoneyDisplay()
    {
        if (playerMoney != null && market?.playerEconomy != null)
        {
            playerMoney.text = $"You have {market.playerEconomy.CurrentMoney} coins";
        }
    }
    
    private VisualElement CreateSellSlotElement(MarketSellSlot slot, int index) // Updated class name
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
        
        // Add price
        int buyPrice = market.playerEconomy.GetBuyPrice(item);
        var priceLabel = new Label($"{buyPrice}");
        priceLabel.AddToClassList("item-price");
        itemElement.Add(priceLabel);
        
        // Click to buy
        itemElement.RegisterCallback<ClickEvent>(evt => market.TryBuyItem(item, 1));
        itemElement.tooltip = $"{item.name} - {buyPrice} coins";
        
        return itemElement;
    }
    
    private VisualElement CreateCraftingBenchUpgradeElement()
    {
        var upgradeElement = new VisualElement();
        upgradeElement.AddToClassList("upgrade-slot");
        
        // Add upgrade name
        var nameLabel = new Label("Crafting Bench Upgrade");
        nameLabel.AddToClassList("upgrade-name");
        upgradeElement.Add(nameLabel);
        
        // Add upgrade description
        var descLabel = new Label("Unlocks 2 additional crafting slots");
        descLabel.AddToClassList("upgrade-description");
        upgradeElement.Add(descLabel);
        
        // Add price and buy button - Updated to use new API
        int upgradeCost = market.GetCraftingBenchUpgradeCost();
        var priceLabel = new Label($"{upgradeCost} coins");
        priceLabel.AddToClassList("upgrade-price");
        upgradeElement.Add(priceLabel);
        
        var buyButton = new Button(() => market.TryBuyUpgrade()); // Updated method call
        buyButton.text = "buy";
        buyButton.AddToClassList("upgrade-button");
        
        // Disable button if can't afford
        bool canAfford = market.playerEconomy.CanAfford(upgradeCost);
        buyButton.SetEnabled(canAfford);
        if (!canAfford)
        {
            buyButton.AddToClassList("disabled");
        }
        
        upgradeElement.Add(buyButton);
        upgradeElement.tooltip = $"Crafting Bench Upgrade for {upgradeCost} coins";
        
        return upgradeElement;
    }
}