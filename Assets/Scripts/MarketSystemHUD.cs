using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Market UI system managing buy/sell interface with inventory integration.
/// Handles daily seed display, sell slot management, and crafting bench upgrade purchases.
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
    
    // Sets up UI element references and initial state
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
    
    // Sets up event subscriptions for market and inventory systems
    private void SubscribeToEvents()
    {
        if (market != null)
        {
            market.OnMarketOpened += OnMarketOpened;
            market.OnMarketClosed += OnMarketClosed;
            market.OnSellSlotsChanged += UpdateSellSlotsDisplay;
            market.OnTransactionCompleted += OnTransactionCompleted;
        }
        
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryItemClicked += OnInventoryItemClicked;
        }
    }
    
    // Removes event subscriptions
    private void UnsubscribeFromEvents()
    {
        if (market != null)
        {
            market.OnMarketOpened -= OnMarketOpened;
            market.OnMarketClosed -= OnMarketClosed;
            market.OnSellSlotsChanged -= UpdateSellSlotsDisplay;
            market.OnTransactionCompleted -= OnTransactionCompleted;
        }
        
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryItemClicked -= OnInventoryItemClicked;
        }
    }
    
    // Shows market interface and updates all displays
    private void OnMarketOpened()
    {
        if (marketContainer != null)
        {
            marketContainer.style.display = DisplayStyle.Flex;
        }
        UpdateAllDisplays();
    }
    
    // Hides market interface
    private void OnMarketClosed()
    {
        if (marketContainer != null)
        {
            marketContainer.style.display = DisplayStyle.None;
        }
    }
    
    // Updates displays after transaction completion
    private void OnTransactionCompleted()
    {
        UpdateAllDisplays();
    }
    
    // Handles confirm sale button clicks
    private void OnConfirmSaleClicked()
    {
        market?.ConfirmSale();
    }
    
    // Handles inventory item clicks when market is open
    private void OnInventoryItemClicked(InventoryItem item)
    {
        if (IsMarketOpen())
        {
            market.TryAddItemToSell(item, 1);
        }
    }
    
    // Checks if market interface is currently open
    private bool IsMarketOpen()
    {
        return market != null && marketContainer != null && marketContainer.style.display == DisplayStyle.Flex;
    }
    
    // Updates all market display components
    private void UpdateAllDisplays()
    {
        UpdateSellSlotsDisplay();
        UpdateBuyItemsDisplay();
        UpdateMoneyDisplay();
    }
    
    // Updates sell slots with current items and total value
    private void UpdateSellSlotsDisplay()
    {
        if (sellSlotsContainer == null || market == null) 
            return;
        
        sellSlotsContainer.Clear();
        
        for (int i = 0; i < market.sellSlots.Count; i++)
        {
            var slot = market.sellSlots[i];
            var slotElement = CreateSellSlotElement(slot, i);
            sellSlotsContainer.Add(slotElement);
        }
        
        UpdateSellTotalAndButton();
    }
    
    // Updates total sell value and confirm button state
    private void UpdateSellTotalAndButton()
    {
        if (totalValue != null)
        {
            int value = market.GetTotalSellValue();
            totalValue.text = $"{value}g";
        }
        
        if (confirmSale != null)
        {
            confirmSale.SetEnabled(market.HasItemsToSell());
        }
    }
    
    // Rebuilds buy items display with seeds and upgrades
    private void UpdateBuyItemsDisplay()
    {
        if (buyItemsContainer == null || market == null) 
            return;
        
        buyItemsContainer.Clear();
        
        AddSeedsSection();
        AddUpgradeSection();
    }
    
    // Adds seeds section to buy display
    private void AddSeedsSection()
    {
        var seedTitle = new Label("Seeds Available Today");
        seedTitle.AddToClassList("market-section-title");
        buyItemsContainer.Add(seedTitle);
        
        AddTierInfo();
        AddAvailableSeeds();
    }
    
    // Adds current tier information
    private void AddTierInfo()
    {
        if (ResearchSystem.Instance != null)
        {
            var tierInfo = new Label($"Current Seeds Tier {ResearchSystem.Instance.currentSeedsTier}");
            tierInfo.AddToClassList("market-tier-info");
            buyItemsContainer.Add(tierInfo);
        }
    }
    
    // Adds available seeds or no seeds message
    private void AddAvailableSeeds()
    {
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
    }
    
    // Adds upgrade section to buy display
    private void AddUpgradeSection()
    {
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
    
    // Updates player money display
    private void UpdateMoneyDisplay()
    {
        if (playerMoney != null && market?.playerEconomy != null)
        {
            playerMoney.text = $"You have {market.playerEconomy.CurrentMoney} gold";
        }
    }
    
    // Creates UI element for sell slot
    private VisualElement CreateSellSlotElement(MarketSellSlot slot, int index)
    {
        var slotElement = new VisualElement();
        slotElement.AddToClassList("sell-slot");
    
        if (!slot.IsEmpty)
        {
            AddSellSlotContent(slotElement, slot, index);
        }
        else
        {
            slotElement.AddToClassList("empty");
        }
    
        return slotElement;
    }
    
    // Adds content to sell slot element
    private void AddSellSlotContent(VisualElement slotElement, MarketSellSlot slot, int index)
    {
        var icon = new VisualElement();
        icon.AddToClassList("item-icon");
        icon.style.backgroundImage = new StyleBackground(slot.item.sprite);
        slotElement.Add(icon);
    
        var quantityLabel = new Label("x" + slot.quantity);
        quantityLabel.AddToClassList("item-quantity");
        slotElement.Add(quantityLabel);
    
        slotElement.RegisterCallback<ClickEvent>(_ => market.TryRemoveItemFromSell(index, 1));
    }
    
    // Creates UI element for buy-able item
    private VisualElement CreateBuyItemElement(InventoryItem item)
    {
        var itemElement = new VisualElement();
        itemElement.AddToClassList("buy-slot");
        
        var icon = new VisualElement();
        icon.AddToClassList("item-icon");
        icon.style.backgroundImage = new StyleBackground(item.sprite);
        itemElement.Add(icon);
        
        int buyPrice = market.playerEconomy.GetBuyPrice(item);
        var priceLabel = new Label($"{buyPrice}g");
        priceLabel.AddToClassList("item-price");
        itemElement.Add(priceLabel);
        
        itemElement.RegisterCallback<ClickEvent>(_ => market.TryBuyItem(item, 1));
        return itemElement;
    }
    
    // Creates UI element for crafting bench upgrade
    private VisualElement CreateCraftingBenchUpgradeElement()
    {
        var upgradeElement = new VisualElement();
        upgradeElement.AddToClassList("upgrade-slot");
        
        AddUpgradeLabels(upgradeElement);
        AddUpgradePurchaseButton(upgradeElement);
        
        return upgradeElement;
    }
    
    // Adds labels to upgrade element
    private void AddUpgradeLabels(VisualElement upgradeElement)
    {
        var nameLabel = new Label("Crafting Bench Upgrade");
        nameLabel.AddToClassList("upgrade-name");
        upgradeElement.Add(nameLabel);
        
        var descLabel = new Label("Unlocks 2 additional crafting slots");
        descLabel.AddToClassList("upgrade-description");
        upgradeElement.Add(descLabel);
        
        int upgradeCost = market.GetCraftingBenchUpgradeCost();
        var priceLabel = new Label($"{upgradeCost} coins");
        priceLabel.AddToClassList("upgrade-price");
        upgradeElement.Add(priceLabel);
    }
    
    // Adds purchase button to upgrade element
    private void AddUpgradePurchaseButton(VisualElement upgradeElement)
    {
        int upgradeCost = market.GetCraftingBenchUpgradeCost();
        var buyButton = new Button(() => market.TryBuyUpgrade())
        {
            text = "buy"
        };
        buyButton.AddToClassList("upgrade-button");
        
        bool canAfford = market.playerEconomy.CanAfford(upgradeCost);
        buyButton.SetEnabled(canAfford);
        if (!canAfford)
        {
            buyButton.AddToClassList("disabled");
        }
        
        upgradeElement.Add(buyButton);
    }
}