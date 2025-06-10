using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Pure UI layer. Handles display, clicks, and visual updates.
/// </summary>
public class ResearchSystemHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InteractionResearchItem researchTable;
    
    // UI Elements - keeping all original names
    private VisualElement tableContainer;
    private VisualElement slotContainer;
    private VisualElement slotIcon;
    private Label slotQuantity;
    private Button researchButton;
    private Label progressLabel;
    private VisualElement resultsContainer; // Restored!
    
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
        
        // Find UI elements - keeping exact same names
        tableContainer = root.Q<VisualElement>("ResearchTableContainer");
        slotContainer = tableContainer?.Q<VisualElement>("ResearchSlotContainer");
        slotIcon = slotContainer?.Q<VisualElement>("ItemIcon");
        slotQuantity = slotContainer?.Q<Label>("ItemQuantity");
        researchButton = tableContainer?.Q<Button>("ConfirmResearch");
        progressLabel = tableContainer?.Q<Label>("ResearchProgress");
        resultsContainer = tableContainer?.Q<VisualElement>("ResearchResultsContainer"); // Restored!
        
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
            researchTable.OnResearchCompleted += ShowResults; // Restored!
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
            researchTable.OnResearchCompleted -= ShowResults; // Restored!
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
        
        if (resultsContainer != null) // Restored!
            resultsContainer.Clear();
    }
    
    private void OnInventoryItemClicked(InventoryItem item)
    {
        // Only handle if table is open
        if (researchTable == null || !researchTable.IsOpen()) return;
        
        // Try to add item to research slot
        if (researchTable.TryAddItem(item))
        {
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
    
    // Restored and adapted to work with simplified data
    private void ShowResults(string itemName)
    {
        if (resultsContainer == null || string.IsNullOrEmpty(itemName)) return;
        
        resultsContainer.Clear();
        
        // Title
        var title = new Label($"Research Complete: {itemName}");
        title.AddToClassList("research-title");
        resultsContainer.Add(title);
        
        // Status - we can check if it was already researched
        bool wasAlreadyResearched = ResearchSystem.Instance.IsResearched(itemName);
        var statusText = "This ingredient has been added to your research notes.";
        var status = new Label(statusText);
        status.AddToClassList("research-status-new");
        resultsContainer.Add(status);
        
        // Show recipes that use this ingredient
        ShowRecipesForIngredient(itemName);
        
        // Show tier progression hints
        var progress = ResearchSystem.Instance.GetProgress();
        if (progress.currentTier < progress.maxTier)
        {
            var hint = new Label($"Keep researching Tier {progress.currentTier} ingredients to unlock Tier {progress.currentTier + 1} seeds in the market!");
            hint.AddToClassList("research-status-known");
            resultsContainer.Add(hint);
        }
        
        UpdateAllDisplays();
    }
    
    // New simplified method to show recipes
    private void ShowRecipesForIngredient(string itemName)
    {
        if (CraftingSystem.Instance == null) return;
    
        var allRecipes = CraftingSystem.Instance.GetAllRecipes();
        var recipesUsingItem = new List<CraftingRecipe>();
    
        // Find recipes that use this ingredient
        foreach (var recipe in allRecipes)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.item.name == itemName)
                {
                    recipesUsingItem.Add(recipe);
                    break; // Found it, no need to check other ingredients in this recipe
                }
            }
        }
    
        if (recipesUsingItem.Count > 0)
        {
            var header = new Label($"This ingredient is used in {recipesUsingItem.Count} recipe(s):");
            header.AddToClassList("research-recipes-header");
            resultsContainer.Add(header);
        
            foreach (var recipe in recipesUsingItem)
            {
                CreateVisualRecipeDisplay(recipe);
            }
        }
        else
        {
            var noRecipes = new Label("This ingredient is not used in any known recipes yet.");
            noRecipes.AddToClassList("research-status-known");
            resultsContainer.Add(noRecipes);
        }
    }
    
    // Restored recipe display methods
    private void CreateVisualRecipeDisplay(CraftingRecipe recipe)
    {
        if (recipe == null) return;
        
        // Check if all ingredients are researched
        bool allIngredientsResearched = true;
        int researchedCount = 0;
        
        foreach (var ingredient in recipe.ingredients)
        {
            if (ResearchSystem.Instance.IsResearched(ingredient.item.name))
            {
                researchedCount++;
            }
            else
            {
                allIngredientsResearched = false;
            }
        }
        
        // Main recipe container
        var recipeContainer = new VisualElement();
        recipeContainer.AddToClassList("recipe-display");
        resultsContainer.Add(recipeContainer);
        
        // Recipe name
        var recipeName = new Label(recipe.recipeName);
        recipeName.AddToClassList("recipe-name");
        recipeContainer.Add(recipeName);
        
        // Ingredients and result layout
        var recipeLayout = new VisualElement();
        recipeLayout.AddToClassList("recipe-layout");
        recipeContainer.Add(recipeLayout);
        
        // Ingredients section
        var ingredientsContainer = new VisualElement();
        ingredientsContainer.AddToClassList("ingredients-container");
        recipeLayout.Add(ingredientsContainer);
        
        foreach (var ingredient in recipe.ingredients)
        {
            var ingredientSlot = CreateIngredientSlot(ingredient);
            if (ingredientSlot != null)
            {
                ingredientsContainer.Add(ingredientSlot);
            }
        }
        
        // Arrow
        var arrow = new Label("→");
        arrow.AddToClassList("recipe-arrow");
        recipeLayout.Add(arrow);
        
        // Result section
        var resultSlot = CreateResultSlot(recipe);
        if (resultSlot != null)
        {
            recipeLayout.Add(resultSlot);
        }
        
        // Status message
        string statusText = allIngredientsResearched ? 
            "All ingredients researched. You can try crafting this recipe!" :
            $"Research {recipe.ingredients.Count - researchedCount} more ingredient(s) to unlock this recipe.";
        
        var statusLabel = new Label(statusText);
        statusLabel.AddToClassList(allIngredientsResearched ? "recipe-unlocked" : "recipe-locked");
        recipeContainer.Add(statusLabel);
    }
    
    private VisualElement CreateIngredientSlot(CraftingRecipe.CraftingIngredient ingredient)
    {
        if (ingredient?.item == null) return null;
        
        var slot = new VisualElement();
        slot.AddToClassList("ingredient-slot");
        
        // Item icon
        var icon = new VisualElement();
        icon.AddToClassList("ingredient-icon");
        if (ingredient.item.sprite != null)
        {
            icon.style.backgroundImage = new StyleBackground(ingredient.item.sprite);
        }
        slot.Add(icon);
        
        // Quantity
        var quantity = new Label($"x{ingredient.quantity}");
        quantity.AddToClassList("ingredient-quantity");
        slot.Add(quantity);
        
        // Research status indicator
        bool isResearched = ResearchSystem.Instance.IsResearched(ingredient.item.name);
        if (isResearched)
        {
            slot.AddToClassList("ingredient-known");
            slot.tooltip = $"{ingredient.item.name} (researched)";
        }
        else
        {
            slot.AddToClassList("ingredient-unknown");
            slot.tooltip = $"{ingredient.item.name} (not researched yet)";
        }
        
        return slot;
    }
    
    private VisualElement CreateResultSlot(CraftingRecipe recipe)
    {
        if (recipe?.result == null) return null;
        
        var slot = new VisualElement();
        slot.AddToClassList("result-slot");
        
        // Result icon
        var icon = new VisualElement();
        icon.AddToClassList("result-icon");
        if (recipe.result.sprite != null)
        {
            icon.style.backgroundImage = new StyleBackground(recipe.result.sprite);
        }
        slot.Add(icon);
        
        // Result quantity
        var quantity = new Label($"x{recipe.resultQuantity}");
        quantity.AddToClassList("result-quantity");
        slot.Add(quantity);
        
        slot.tooltip = $"Crafts: {recipe.result.name}";
        
        return slot;
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
            slotContainer.tooltip = "Click an item in your inventory to place it here for research";
        }
        else
        {
            // Item in slot
            if (slotIcon != null) slotIcon.style.backgroundImage = new StyleBackground(item.sprite);
            if (slotQuantity != null) slotQuantity.text = "1";
            slotContainer.RemoveFromClassList("empty");
            slotContainer.tooltip = $"Researching: {item.name} (click to remove)";
        }
    }
    
    private void UpdateProgress()
    {
        if (progressLabel == null || ResearchSystem.Instance == null) return;
        
        var progress = ResearchSystem.Instance.GetProgress();
        progressLabel.text = $"Research Progress: {progress.researchedCount} items studied\n" +
                            $"Recipes unlocked: {progress.unlockedRecipes}/{progress.totalRecipes}\n" +
                            $"Current seeds tier: {progress.currentTier}/{progress.maxTier}";
    }
    
    private void UpdateButtonState()
    {
        if (researchButton == null || researchTable == null) return;
        
        bool hasItem = researchTable.HasItem();
        researchButton.SetEnabled(hasItem);
        researchButton.text = "go";
    }
    
    #endregion
}