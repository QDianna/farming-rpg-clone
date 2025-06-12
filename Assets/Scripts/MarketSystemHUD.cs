using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Market UI system managing buy/sell interface with inventory integration.
/// Handles daily seed display, sell slot management, crafting bench upgrade, and research table purchases.
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
    
    // SETUP AND EVENT HANDLING
    
    /// <summary>
    /// Sets up UI element references and initial state
    /// </summary>
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
    
    /// <summary>
    /// Sets up event subscriptions for market and inventory systems
    /// </summary>
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
    
    /// <summary>
    /// Removes event subscriptions
    /// </summary>
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
    
    // EVENT HANDLERS
    
    /// <summary>
    /// Shows market interface and updates all displays
    /// </summary>
    private void OnMarketOpened()
    {
        if (marketContainer != null)
        {
            marketContainer.style.display = DisplayStyle.Flex;
        }
        UpdateAllDisplays();
    }
    
    /// <summary>
    /// Hides market interface
    /// </summary>
    private void OnMarketClosed()
    {
        if (marketContainer != null)
        {
            marketContainer.style.display = DisplayStyle.None;
        }
    }
    
    /// <summary>
    /// Updates displays after transaction completion
    /// </summary>
    private void OnTransactionCompleted()
    {
        UpdateAllDisplays();
    }
    
    /// <summary>
    /// Handles confirm sale button clicks
    /// </summary>
    private void OnConfirmSaleClicked()
    {
        market?.ConfirmSale();
    }
    
    /// <summary>
    /// Handles inventory item clicks when market is open
    /// </summary>
    private void OnInventoryItemClicked(InventoryItem item)
    {
        if (IsMarketOpen())
        {
            market.TryAddItemToSell(item, 1);
        }
    }
    
    // UTILITY METHODS
    
    /// <summary>
    /// Checks if market interface is currently open
    /// </summary>
    private bool IsMarketOpen()
    {
        return market != null && marketContainer != null && marketContainer.style.display == DisplayStyle.Flex;
    }
    
    /// <summary>
    /// Updates all market display components
    /// </summary>
    private void UpdateAllDisplays()
    {
        UpdateSellSlotsDisplay();
        UpdateBuyItemsDisplay();
        UpdateMoneyDisplay();
    }
    
    // SELL SECTION METHODS
    
    /// <summary>
    /// Updates sell slots with current items and total value
    /// </summary>
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
    
    /// <summary>
    /// Updates total sell value and confirm button state
    /// </summary>
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
    
    /// <summary>
    /// Creates UI element for sell slot
    /// </summary>
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
    
    /// <summary>
    /// Adds content to sell slot element
    /// </summary>
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
    
    // BUY SECTION METHODS
    
    /// <summary>
    /// Rebuilds buy items display with seeds and upgrades
    /// </summary>
    private void UpdateBuyItemsDisplay()
    {
        if (buyItemsContainer == null || market == null) 
            return;
        
        buyItemsContainer.Clear();
        
        AddSeedsSection();
        AddStructuresSection();
    }
    
    /// <summary>
    /// Adds seeds section to buy display
    /// </summary>
    private void AddSeedsSection()
    {
        // Create combined title with tier info
        var seedTitleWithTier = "";
        if (ResearchSystem.Instance != null)
        {
            seedTitleWithTier = $"Seeds Available Today (Tier {ResearchSystem.Instance.currentSeedsTier})";
        }
        else
        {
            seedTitleWithTier = "Seeds Available Today";
        }
        
        var seedTitle = new Label(seedTitleWithTier);
        seedTitle.AddToClassList("market-section-title");
        buyItemsContainer.Add(seedTitle);
        
        AddAvailableSeeds();
    }
    
    /// <summary>
    /// Adds available seeds or no seeds message
    /// </summary>
    private void AddAvailableSeeds()
    {
        var availableItems = market.GetAvailableItems();
        if (availableItems.Count > 0)
        {
            var seedsContainer = new VisualElement();
            seedsContainer.AddToClassList("seeds-container");
            
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
    
    /// <summary>
    /// Creates UI element for buy-able item
    /// </summary>
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
    
    // STRUCTURES SECTION METHODS
    
    /// <summary>
    /// Adds structures section to buy display
    /// </summary>
    private void AddStructuresSection()
    {
        // Only show structures if player has met the witch
        if (QuestsSystem.Instance == null || !QuestsSystem.Instance.HasMetWitch)
        {
            return; // Don't show any structures if witch hasn't been met
        }
        
        // Create structures section header
        var structuresTitle = new Label("Structures Available");
        structuresTitle.AddToClassList("market-section-title");
        buyItemsContainer.Add(structuresTitle);
        
        // Create container for structures
        var structuresContainer = new VisualElement();
        structuresContainer.AddToClassList("structures-container");
        
        bool hasStructures = false;
        
        // Add crafting bench purchase if available
        if (market.IsCraftingBenchAvailable())
        {
            var craftingBenchElement = CreateCraftingBenchElement();
            structuresContainer.Add(craftingBenchElement);
            hasStructures = true;
        }
        
        // Add crafting bench upgrade if available (only after bench is purchased)
        if (market.IsCraftingBenchUpgradeAvailable())
        {
            var upgradeElement = CreateCraftingBenchUpgradeElement();
            structuresContainer.Add(upgradeElement);
            hasStructures = true;
        }
        
        // Add research table if available
        if (market.IsResearchTableAvailable())
        {
            var researchTableElement = CreateResearchTableElement();
            structuresContainer.Add(researchTableElement);
            hasStructures = true;
        }
        
        if (hasStructures)
        {
            buyItemsContainer.Add(structuresContainer);
        }
        else
        {
            // Show message if no structures available (but only if witch has been met)
            var noStructuresLabel = new Label("No structures available");
            noStructuresLabel.AddToClassList("market-upgraded");
            buyItemsContainer.Add(noStructuresLabel);
        }
    }
    
    // STRUCTURE ELEMENT CREATION METHODS
    
    /// <summary>
    /// Creates UI element for crafting bench purchase
    /// </summary>
    private VisualElement CreateCraftingBenchElement()
    {
        var benchElement = new VisualElement();
        benchElement.AddToClassList("upgrade-slot");
        
        AddCraftingBenchLabels(benchElement);
        AddCraftingBenchPurchaseButton(benchElement);
        
        return benchElement;
    }
    
    /// <summary>
    /// Creates UI element for crafting bench upgrade
    /// </summary>
    private VisualElement CreateCraftingBenchUpgradeElement()
    {
        var upgradeElement = new VisualElement();
        upgradeElement.AddToClassList("upgrade-slot");
        
        AddCraftingBenchUpgradeLabels(upgradeElement);
        AddCraftingBenchUpgradePurchaseButton(upgradeElement);
        
        return upgradeElement;
    }
    
    /// <summary>
    /// Creates UI element for research table purchase
    /// </summary>
    private VisualElement CreateResearchTableElement()
    {
        var tableElement = new VisualElement();
        tableElement.AddToClassList("upgrade-slot");
        
        AddResearchTableLabels(tableElement);
        AddResearchTablePurchaseButton(tableElement);
        
        return tableElement;
    }
    
    // STRUCTURE LABEL METHODS
    
    /// <summary>
    /// Adds labels to crafting bench purchase element
    /// </summary>
    private void AddCraftingBenchLabels(VisualElement benchElement)
    {
        var nameLabel = new Label("Crafting Bench Structure");
        nameLabel.AddToClassList("upgrade-name");
        benchElement.Add(nameLabel);
        
        var descLabel = new Label("Essential crafting station");
        descLabel.AddToClassList("upgrade-description");
        benchElement.Add(descLabel);
        
        int benchCost = market.GetCraftingBenchCost();
        var priceLabel = new Label($"{benchCost} coins");
        priceLabel.AddToClassList("upgrade-price");
        benchElement.Add(priceLabel);
    }
    
    /// <summary>
    /// Adds labels to crafting bench upgrade element
    /// </summary>
    private void AddCraftingBenchUpgradeLabels(VisualElement upgradeElement)
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
    
    /// <summary>
    /// Adds labels to research table element
    /// </summary>
    private void AddResearchTableLabels(VisualElement tableElement)
    {
        var nameLabel = new Label("Research Table Structure");
        nameLabel.AddToClassList("upgrade-name");
        tableElement.Add(nameLabel);
        
        var descLabel = new Label("Unlocks research and seed upgrades");
        descLabel.AddToClassList("upgrade-description");
        tableElement.Add(descLabel);
        
        int tableCost = market.GetResearchTableCost();
        var priceLabel = new Label($"{tableCost} coins");
        priceLabel.AddToClassList("upgrade-price");
        tableElement.Add(priceLabel);
    }
    
    // STRUCTURE BUTTON METHODS
    
    /// <summary>
    /// Adds purchase button to crafting bench element
    /// </summary>
    private void AddCraftingBenchPurchaseButton(VisualElement benchElement)
    {
        int benchCost = market.GetCraftingBenchCost();
        var buyButton = new Button(() => market.TryBuyCraftingBench())
        {
            text = "buy"
        };
        buyButton.AddToClassList("upgrade-button");
        
        bool canAfford = market.playerEconomy.CanAfford(benchCost);
        buyButton.SetEnabled(canAfford);
        if (!canAfford)
        {
            buyButton.AddToClassList("disabled");
        }
        
        benchElement.Add(buyButton);
    }
    
    /// <summary>
    /// Adds purchase button to crafting bench upgrade element
    /// </summary>
    private void AddCraftingBenchUpgradePurchaseButton(VisualElement upgradeElement)
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
    
    /// <summary>
    /// Adds purchase button to research table element
    /// </summary>
    private void AddResearchTablePurchaseButton(VisualElement tableElement)
    {
        int tableCost = market.GetResearchTableCost();
        var buyButton = new Button(() => market.TryBuyResearchTable())
        {
            text = "buy"
        };
        buyButton.AddToClassList("upgrade-button");
        
        bool canAfford = market.playerEconomy.CanAfford(tableCost);
        buyButton.SetEnabled(canAfford);
        if (!canAfford)
        {
            buyButton.AddToClassList("disabled");
        }
        
        tableElement.Add(buyButton);
    }
    
    // MONEY DISPLAY METHOD
    
    /// <summary>
    /// Updates player money display
    /// </summary>
    private void UpdateMoneyDisplay()
    {
        if (playerMoney != null && market?.playerEconomy != null)
        {
            playerMoney.text = $"You have {market.playerEconomy.CurrentMoney} gold";
        }
    }
}