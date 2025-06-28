using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Market UI system managing buy/sell interface display only.
/// Pure UI layer - no business logic, just displays data from MarketSystem and InteractionMarket.
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
    private Label tooltipsLabel;
    
    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        tooltipsLabel = root.Q<Label>("Tooltip");
        tooltipsLabel.style.display = DisplayStyle.None;
        
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
    
    private void SetupUIReferences()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        marketContainer = root.Q<VisualElement>("MarketContainer");
        sellSlotsContainer = root.Q<VisualElement>("SellSlotsContainer");
        buyItemsContainer = root.Q<VisualElement>("BuySlotsContainer");
        confirmSale = root.Q<Button>("ConfirmSale");
        totalValue = root.Q<Label>("TotalValue");
        playerMoney = root.Q<Label>("PlayerMoney");
        tooltipsLabel = root.Q<Label>("Tooltip");
        
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
            market.OnSellSlotsChanged += UpdateSellSlotsDisplay;
            market.OnTransactionCompleted += OnTransactionCompleted;
        }
        
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryItemClicked += OnInventoryItemClicked;
        }
        
        if (MarketSystem.Instance != null)
        {
            MarketSystem.Instance.OnMarketDataChanged += UpdateBuyItemsDisplay;
        }
    }
    
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
        
        if (MarketSystem.Instance != null)
        {
            MarketSystem.Instance.OnMarketDataChanged -= UpdateBuyItemsDisplay;
        }
    }
    
    // EVENT HANDLERS
    
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
    
    // Handle inventory clicks when market is open
    private void OnInventoryItemClicked(InventoryItem item, bool shiftHeld)
    {
        if (IsMarketOpen())
        {
            int amountToSell = shiftHeld ? 10 : 1;
            market.TryAddItemToSell(item, amountToSell);
        }
    }
    
    // UTILITY METHODS
    
    private bool IsMarketOpen()
    {
        return market != null && marketContainer != null && marketContainer.style.display == DisplayStyle.Flex;
    }
    
    private void UpdateAllDisplays()
    {
        UpdateSellSlotsDisplay();
        UpdateBuyItemsDisplay();
        UpdateMoneyDisplay();
    }
    
    // SELL SECTION
    
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
    
    // BUY SECTION
    
    private void UpdateBuyItemsDisplay()
    {
        if (buyItemsContainer == null || market == null) 
            return;
        
        buyItemsContainer.Clear();
        
        AddItemsSection();
        AddStructuresSection();
    }
    
    private void RegisterTooltip(VisualElement target, string text)
    {
        target.RegisterCallback<MouseEnterEvent>(evt =>
        {
            tooltipsLabel.text = text;
            tooltipsLabel.style.display = DisplayStyle.Flex;
        });

        target.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            tooltipsLabel.style.display = DisplayStyle.None;
        });
        
        target.RegisterCallback<DetachFromPanelEvent>(evt =>
        {
            tooltipsLabel.style.display = DisplayStyle.None;
        });


        target.RegisterCallback<MouseMoveEvent>(evt =>
        {
            tooltipsLabel.style.left = evt.mousePosition.x + 10;
            tooltipsLabel.style.top = evt.mousePosition.y + 10;
        });
    }

    
    private void AddItemsSection()
    {
        // Create title with tier info
        var itemsTitleWithTier = "";
        if (ResearchSystem.Instance != null)
        {
            itemsTitleWithTier = $"Items Available Today (Tier {ResearchSystem.Instance.currentSeedsTier})";
        }
        else
        {
            itemsTitleWithTier = "Items Available Today";
        }
        
        var itemsTitle = new Label(itemsTitleWithTier);
        itemsTitle.AddToClassList("market-section-title");
        buyItemsContainer.Add(itemsTitle);
        
        AddAvailableItems();
    }
    
    private void AddAvailableItems()
    {
        var availableSeeds = MarketSystem.Instance?.GetAvailableSeeds() ?? new System.Collections.Generic.List<InventoryItem>();
        var availableCrops = MarketSystem.Instance?.GetAvailableCrops() ?? new System.Collections.Generic.List<InventoryItem>();
        
        if (availableSeeds.Count > 0 || availableCrops.Count > 0)
        {
            // Add seeds section using existing classes
            if (availableSeeds.Count > 0)
            {
                var seedsContainer = new VisualElement();
                seedsContainer.AddToClassList("seeds-container");
                
                foreach (var item in availableSeeds)
                {
                    var itemElement = CreateBuyItemElement(item);
                    seedsContainer.Add(itemElement);
                    
                    RegisterTooltip(seedsContainer, item.newName);
                }
                buyItemsContainer.Add(seedsContainer);
            }
            
            // Add crops section using same classes as seeds
            if (availableCrops.Count > 0)
            {
                var cropsContainer = new VisualElement();
                cropsContainer.AddToClassList("seeds-container"); // Same class as seeds
                
                foreach (var item in availableCrops)
                {
                    var itemElement = CreateBuyItemElement(item);
                    cropsContainer.Add(itemElement);
                }
                buyItemsContainer.Add(cropsContainer);
            }
        }
        else
        {
            var noItemsLabel = new Label("No items available for current tier/season");
            noItemsLabel.AddToClassList("market-no-items");
            buyItemsContainer.Add(noItemsLabel);
        }
    }
    
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
        RegisterTooltip(itemElement, item.newName);
        
        // Just trigger the action - let InteractionMarket handle the logic
        itemElement.RegisterCallback<ClickEvent>(_ => market.TryBuyItem(item, 1));
        return itemElement;
    }
    
    // STRUCTURES SECTION
    
    // Only show structures if player has met the witch
    private void AddStructuresSection()
    {
        if (QuestsSystem.Instance == null || !QuestsSystem.Instance.hasMetWitch)
        {
            return;
        }
        
        var structuresTitle = new Label("Structures Available");
        structuresTitle.AddToClassList("market-section-title");
        buyItemsContainer.Add(structuresTitle);
        
        var structuresContainer = new VisualElement();
        structuresContainer.AddToClassList("structures-container");
        
        bool hasStructures = false;
        
        // Use MarketSystem directly for availability checks
        if (MarketSystem.Instance != null)
        {
            if (MarketSystem.Instance.IsCraftingBenchAvailable())
            {
                var craftingBenchElement = CreateCraftingBenchElement();
                structuresContainer.Add(craftingBenchElement);
                hasStructures = true;
            }
            
            if (MarketSystem.Instance.IsCraftingBenchUpgradeAvailable())
            {
                var upgradeElement = CreateCraftingBenchUpgradeElement();
                structuresContainer.Add(upgradeElement);
                hasStructures = true;
            }
            
            if (MarketSystem.Instance.IsResearchTableAvailable())
            {
                var researchTableElement = CreateResearchTableElement();
                structuresContainer.Add(researchTableElement);
                hasStructures = true;
            }
        }
        
        if (hasStructures)
        {
            buyItemsContainer.Add(structuresContainer);
        }
        else
        {
            var noStructuresLabel = new Label("No structures available");
            noStructuresLabel.AddToClassList("market-section-title");
            buyItemsContainer.Add(noStructuresLabel);
        }
    }
    
    // STRUCTURE ELEMENT CREATION
    
    private VisualElement CreateCraftingBenchElement()
    {
        var benchElement = new VisualElement();
        benchElement.AddToClassList("upgrade-slot");
        
        AddCraftingBenchLabels(benchElement);
        AddCraftingBenchPurchaseButton(benchElement);
        
        return benchElement;
    }
    
    private VisualElement CreateCraftingBenchUpgradeElement()
    {
        var upgradeElement = new VisualElement();
        upgradeElement.AddToClassList("upgrade-slot");
        
        AddCraftingBenchUpgradeLabels(upgradeElement);
        AddCraftingBenchUpgradePurchaseButton(upgradeElement);
        
        return upgradeElement;
    }
    
    private VisualElement CreateResearchTableElement()
    {
        var tableElement = new VisualElement();
        tableElement.AddToClassList("upgrade-slot");
        
        AddResearchTableLabels(tableElement);
        AddResearchTablePurchaseButton(tableElement);
        
        return tableElement;
    }
    
    // STRUCTURE LABELS - Pure display, no logic
    
    private void AddCraftingBenchLabels(VisualElement benchElement)
    {
        var nameLabel = new Label("Crafting Bench Structure");
        nameLabel.AddToClassList("upgrade-name");
        benchElement.Add(nameLabel);
        
        var descLabel = new Label("Unlocks crafting potion recipes");
        descLabel.AddToClassList("upgrade-description");
        benchElement.Add(descLabel);
        
        // Just display prices from MarketSystem
        int benchCost = MarketSystem.Instance.GetCraftingBenchCost();
        int woodCost = MarketSystem.Instance.GetCraftingBenchWoodCost();
        var priceLabel = new Label($"{benchCost} coins\n{woodCost} wood");
        priceLabel.AddToClassList("upgrade-price");
        benchElement.Add(priceLabel);
    }
    
    private void AddCraftingBenchUpgradeLabels(VisualElement upgradeElement)
    {
        var nameLabel = new Label("Crafting Bench Upgrade");
        nameLabel.AddToClassList("upgrade-name");
        upgradeElement.Add(nameLabel);
        
        var descLabel = new Label("Unlocks 2 additional crafting slots");
        descLabel.AddToClassList("upgrade-description");
        upgradeElement.Add(descLabel);
        
        // Just display price from MarketSystem
        int upgradeCost = MarketSystem.Instance.GetCraftingBenchUpgradeCost();
        var priceLabel = new Label($"{upgradeCost} coins");
        priceLabel.AddToClassList("upgrade-price");
        upgradeElement.Add(priceLabel);
    }
    
    private void AddResearchTableLabels(VisualElement tableElement)
    {
        var nameLabel = new Label("Research Table Structure");
        nameLabel.AddToClassList("upgrade-name");
        tableElement.Add(nameLabel);
        
        var descLabel = new Label("Unlocks research and seed upgrades");
        descLabel.AddToClassList("upgrade-description");
        tableElement.Add(descLabel);
        
        // Just display prices from MarketSystem
        int tableCost = MarketSystem.Instance.GetResearchTableCost();
        int woodCost = MarketSystem.Instance.GetResearchTableWoodCost();
        var priceLabel = new Label($"{tableCost} coins\n{woodCost} wood");
        priceLabel.AddToClassList("upgrade-price");
        tableElement.Add(priceLabel);
    }
    
    // STRUCTURE BUTTONS - Pure UI, no business logic
    
    private void AddCraftingBenchPurchaseButton(VisualElement benchElement)
    {
        var buyButton = new Button(() => market.TryBuyCraftingBench())
        {
            text = "buy"
        };
        buyButton.AddToClassList("upgrade-button");
        benchElement.Add(buyButton);
    }
    
    private void AddCraftingBenchUpgradePurchaseButton(VisualElement upgradeElement)
    {
        var buyButton = new Button(() => market.TryBuyUpgrade())
        {
            text = "buy"
        };
        buyButton.AddToClassList("upgrade-button");
        upgradeElement.Add(buyButton);
    }
    
    private void AddResearchTablePurchaseButton(VisualElement tableElement)
    {
        var buyButton = new Button(() => market.TryBuyResearchTable())
        {
            text = "buy"
        };
        buyButton.AddToClassList("upgrade-button");
        tableElement.Add(buyButton);
    }
    
    // MONEY DISPLAY
    
    private void UpdateMoneyDisplay()
    {
        if (playerMoney != null && market?.playerEconomy != null)
        {
            playerMoney.text = $"You have {market.playerEconomy.CurrentMoney} gold";
        }
    }
}
