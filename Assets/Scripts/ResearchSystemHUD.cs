using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Pure UI layer. Handles display, clicks, and visual updates.
/// Delegates all logic to InteractionResearchItem.
/// </summary>
public class ResearchSystemHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InteractionResearchItem researchTable;
    
    // UI Elements
    private VisualElement tableContainer;
    private VisualElement slotContainer;
    private VisualElement slotIcon;
    private Label slotQuantity;
    private Button researchButton;
    private Label progressLabel;
    private VisualElement resultsContainer;
    
    #region Unity Lifecycle
    
    private void Start()
    {
        SetupUI();
        SubscribeEvents();
    }
    
    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    
    #endregion
    
    #region UI Setup
    
    private void SetupUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        // Find UI elements
        tableContainer = root.Q<VisualElement>("ResearchTableContainer");
        slotContainer = tableContainer?.Q<VisualElement>("ResearchSlotContainer");
        slotIcon = slotContainer?.Q<VisualElement>("ItemIcon");
        slotQuantity = slotContainer?.Q<Label>("ItemQuantity");
        researchButton = tableContainer?.Q<Button>("ConfirmResearch");
        progressLabel = tableContainer?.Q<Label>("ResearchProgress");
        resultsContainer = tableContainer?.Q<VisualElement>("ResearchResultsContainer");
        
        // Setup button click
        if (researchButton != null)
            researchButton.clicked += OnResearchButtonClicked;
        
        // Setup slot click (for removing items)
        if (slotContainer != null)
        {
            slotContainer.RegisterCallback<ClickEvent>(OnSlotClicked);
        }
        
        // Hide initially
        if (tableContainer != null)
            tableContainer.style.display = DisplayStyle.None;
    }
    
    #endregion
    
    #region Event Management
    
    private void SubscribeEvents()
    {
        // Research table events
        if (researchTable != null)
        {
            researchTable.OnTableOpened += ShowTable;
            researchTable.OnTableClosed += HideTable;
            researchTable.OnSlotChanged += UpdateAllDisplays;
            researchTable.OnResearchCompleted += ShowResults;
        }
        
        // Inventory events
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryItemClicked += OnInventoryItemClicked;
        
        // Research system events
        if (ResearchSystem.Instance != null)
            ResearchSystem.Instance.OnResearchDataChanged += UpdateProgress;
    }
    
    private void UnsubscribeEvents()
    {
        if (researchTable != null)
        {
            researchTable.OnTableOpened -= ShowTable;
            researchTable.OnTableClosed -= HideTable;
            researchTable.OnSlotChanged -= UpdateAllDisplays;
            researchTable.OnResearchCompleted -= ShowResults;
        }
        
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryItemClicked -= OnInventoryItemClicked;
        
        if (ResearchSystem.Instance != null)
            ResearchSystem.Instance.OnResearchDataChanged -= UpdateProgress;
    }
    
    #endregion
    
    #region Event Handlers
    
    private void ShowTable()
    {
        if (tableContainer != null)
            tableContainer.style.display = DisplayStyle.Flex;
        
        UpdateAllDisplays();
    }
    
    private void HideTable()
    {
        if (tableContainer != null)
            tableContainer.style.display = DisplayStyle.None;
        
        if (resultsContainer != null)
            resultsContainer.Clear();
    }
    
    private void OnInventoryItemClicked(InventoryItem item)
    {
        // Only handle if table is open
        if (researchTable == null || !researchTable.IsOpen()) return;
        
        // Try to add item to research slot
        if (researchTable.TryAddItem(item))
        {
            Debug.Log("Added item to research table!");
            InventorySystem.Instance.RemoveItem(item, 1);
        }
    }
    
    private void OnSlotClicked(ClickEvent evt)
    {
        // Left click to remove item
        if (evt.button == 0 && researchTable != null)
        {
            researchTable.TryRemoveItem();
        }
    }
    
    private void OnResearchButtonClicked()
    {
        if (researchTable != null)
        {
            researchTable.TryResearch();
        }
    }
    
    #endregion
    
    #region Display Updates
    
    private void UpdateAllDisplays()
    {
        UpdateSlotDisplay();
        UpdateProgress();
        UpdateButtonState();
    }
    
    private void UpdateSlotDisplay()
    {
        if (slotContainer == null || researchTable == null) return;
        
        var item = researchTable.GetCurrentItem();
        
        if (item == null)
        {
            // Empty slot
            if (slotIcon != null) slotIcon.style.backgroundImage = null;
            if (slotQuantity != null) slotQuantity.text = "";
            slotContainer.AddToClassList("empty");
            slotContainer.tooltip = "Click inventory items to research them";
        }
        else
        {
            // Item in slot
            if (slotIcon != null) slotIcon.style.backgroundImage = new StyleBackground(item.itemSprite);
            if (slotQuantity != null) slotQuantity.text = "1";
            slotContainer.RemoveFromClassList("empty");
            slotContainer.tooltip = $"Research {item.itemName} (click to remove)";
        }
    }
    
    private void UpdateProgress()
    {
        if (progressLabel == null || ResearchSystem.Instance == null) return;
        
        var progress = ResearchSystem.Instance.GetProgress();
        progressLabel.text = $"Progress: {progress.researchedIngredients}/{progress.totalIngredients} ingredients | " +
                            $"{progress.unlockedRecipes}/{progress.totalRecipes} recipes";
    }
    
    private void UpdateButtonState()
    {
        if (researchButton == null || researchTable == null) return;
        
        bool hasItem = researchTable.HasItem();
        researchButton.SetEnabled(hasItem);
        researchButton.text = hasItem ? "Research" : "Add Item";
    }
    
    private void ShowResults(ResearchResult result)
    {
        if (resultsContainer == null || result == null) return;
        
        resultsContainer.Clear();
        
        // Title
        var title = new Label($"Research: {result.ingredientName}");
        title.AddToClassList("research-title");
        resultsContainer.Add(title);
        
        // Status
        var status = new Label(result.wasNewResearch ? "🔬 New discovery!" : "Already known");
        status.AddToClassList(result.wasNewResearch ? "research-status-new" : "research-status-known");
        resultsContainer.Add(status);
        
        // Recipes
        if (result.availableRecipes.Count > 0)
        {
            var header = new Label($"Used in {result.availableRecipes.Count} recipes:");
            header.AddToClassList("research-recipes-header");
            resultsContainer.Add(header);
            
            foreach (var recipe in result.availableRecipes)
            {
                var recipeLabel = new Label($"• {recipe.recipeName} {(recipe.canCraftNow ? "✓" : "?")}");
                recipeLabel.AddToClassList(recipe.canCraftNow ? "recipe-available" : "recipe-locked");
                resultsContainer.Add(recipeLabel);
            }
        }
        
        UpdateAllDisplays();
    }
    
    #endregion
}