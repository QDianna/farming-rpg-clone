using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Research table UI system managing item slots, progress display, and recipe visualization.
/// Handles research interface interactions and displays detailed recipe unlocking information.
/// </summary>
public class ResearchSystemHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private InteractionResearchItem researchTable;
    
    private VisualElement tableContainer;
    private VisualElement slotContainer;
    private VisualElement slotIcon;
    private Label slotQuantity;
    private Button researchButton;
    private Label progressLabel;
    private VisualElement resultsContainer;
    
    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    // Sets up UI element references and initial state
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        CacheUIElements(root);
        SetupEventHandlers();
        HideTableInitially();
    }
    
    // Caches all UI element references
    private void CacheUIElements(VisualElement root)
    {
        tableContainer = root.Q<VisualElement>("ResearchTableContainer");
        slotContainer = tableContainer?.Q<VisualElement>("ResearchSlotContainer");
        slotIcon = slotContainer?.Q<VisualElement>("ItemIcon");
        slotQuantity = slotContainer?.Q<Label>("ItemQuantity");
        researchButton = tableContainer?.Q<Button>("ConfirmResearch");
        progressLabel = tableContainer?.Q<Label>("ResearchProgress");
        resultsContainer = tableContainer?.Q<VisualElement>("ResearchResultsContainer");
    }
    
    // Sets up UI event handlers
    private void SetupEventHandlers()
    {
        if (researchButton != null)
            researchButton.clicked += OnResearchButtonClicked;
        
        if (slotContainer != null)
        {
            slotContainer.RegisterCallback<ClickEvent>(OnSlotClicked);
        }
    }
    
    // Hides table container initially
    private void HideTableInitially()
    {
        if (tableContainer != null)
            tableContainer.style.display = DisplayStyle.None;
    }
    
    // Sets up all event subscriptions
    private void SubscribeToEvents()
    {
        SubscribeToResearchTableEvents();
        SubscribeToInventoryEvents();
        SubscribeToResearchSystemEvents();
    }
    
    // Subscribes to research table events
    private void SubscribeToResearchTableEvents()
    {
        if (researchTable != null)
        {
            researchTable.OnTableOpened += ShowTable;
            researchTable.OnTableClosed += HideTable;
            researchTable.OnSlotChanged += UpdateAllDisplays;
            researchTable.OnResearchCompleted += ShowResults;
        }
    }
    
    // Subscribes to inventory events
    private void SubscribeToInventoryEvents()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryItemClicked += OnInventoryItemClicked;
    }
    
    // Subscribes to research system events
    private void SubscribeToResearchSystemEvents()
    {
        if (ResearchSystem.Instance != null)
            ResearchSystem.Instance.OnResearchDataChanged += UpdateProgress;
    }
    
    // Removes all event subscriptions
    private void UnsubscribeFromEvents()
    {
        UnsubscribeFromResearchTableEvents();
        UnsubscribeFromInventoryEvents();
        UnsubscribeFromResearchSystemEvents();
    }
    
    // Unsubscribes from research table events
    private void UnsubscribeFromResearchTableEvents()
    {
        if (researchTable != null)
        {
            researchTable.OnTableOpened -= ShowTable;
            researchTable.OnTableClosed -= HideTable;
            researchTable.OnSlotChanged -= UpdateAllDisplays;
            researchTable.OnResearchCompleted -= ShowResults;
        }
    }
    
    // Unsubscribes from inventory events
    private void UnsubscribeFromInventoryEvents()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryItemClicked -= OnInventoryItemClicked;
    }
    
    // Unsubscribes from research system events
    private void UnsubscribeFromResearchSystemEvents()
    {
        if (ResearchSystem.Instance != null)
            ResearchSystem.Instance.OnResearchDataChanged -= UpdateProgress;
    }
    
    // Shows research table interface
    private void ShowTable()
    {
        if (tableContainer != null)
            tableContainer.style.display = DisplayStyle.Flex;
        
        UpdateAllDisplays();
    }
    
    // Hides research table interface
    private void HideTable()
    {
        if (tableContainer != null)
            tableContainer.style.display = DisplayStyle.None;
        
        if (resultsContainer != null)
            resultsContainer.Clear();
    }
    
    // Handles inventory item clicks when research table is open
    private void OnInventoryItemClicked(InventoryItem item)
    {
        if (researchTable == null || !researchTable.IsOpen()) 
            return;
        
        if (researchTable.TryAddItem(item))
        {
            InventorySystem.Instance.RemoveItem(item, 1);
        }
    }
    
    // Handles research slot clicks to remove items
    private void OnSlotClicked(ClickEvent evt)
    {
        if (evt.button == 0 && researchTable != null)
        {
            researchTable.TryRemoveItem();
        }
    }
    
    // Handles research button clicks
    private void OnResearchButtonClicked()
    {
        if (researchTable != null)
        {
            researchTable.TryResearch();
        }
    }
    
    // Shows research results with recipe information
    private void ShowResults(string itemName, bool wasAlreadyResearched)
    {
        if (resultsContainer == null || string.IsNullOrEmpty(itemName)) 
            return;
        
        resultsContainer.Clear();
        
        CreateResultsHeader(itemName, wasAlreadyResearched);
        ShowRecipesForIngredient(itemName);
        
        UpdateAllDisplays();
    }
    
    // Creates header for research results
    private void CreateResultsHeader(string itemName, bool wasAlreadyResearched)
    {
        var title = new Label($"Research: {itemName}");
        title.AddToClassList("research-title");
        resultsContainer.Add(title);
        
        string statusText = wasAlreadyResearched ? 
            "Already researched this plant! These are the notes from you research book." : 
            "Research complete! This ingredient has been added to your notes.";
            
        var status = new Label(statusText);
        status.AddToClassList(wasAlreadyResearched ? "research-status-known" : "research-status-new");
        resultsContainer.Add(status);
    }
    
    // Shows recipes that use the researched ingredient
    private void ShowRecipesForIngredient(string itemName)
    {
        if (CraftingSystem.Instance == null) 
            return;
    
        var recipesUsingItem = FindRecipesUsingIngredient(itemName);
    
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
    
    // Finds all recipes that use the specified ingredient
    private List<CraftingRecipe> FindRecipesUsingIngredient(string itemName)
    {
        var allRecipes = CraftingSystem.Instance.GetAllRecipes();
        var recipesUsingItem = new List<CraftingRecipe>();
    
        foreach (var recipe in allRecipes)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.item.name == itemName)
                {
                    recipesUsingItem.Add(recipe);
                    break;
                }
            }
        }
        
        return recipesUsingItem;
    }
    
    // Creates visual display for a recipe
    private void CreateVisualRecipeDisplay(CraftingRecipe recipe)
    {
        if (recipe == null) 
            return;
        
        var (allIngredientsResearched, researchedCount) = CheckRecipeResearchStatus(recipe);
        
        var recipeContainer = CreateRecipeContainer(recipe, allIngredientsResearched, researchedCount);
        resultsContainer.Add(recipeContainer);
    }
    
    // Checks research status of recipe ingredients
    private (bool allResearched, int researchedCount) CheckRecipeResearchStatus(CraftingRecipe recipe)
    {
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
        
        return (allIngredientsResearched, researchedCount);
    }
    
    // Creates complete recipe container with all elements
    private VisualElement CreateRecipeContainer(CraftingRecipe recipe, bool allIngredientsResearched, int researchedCount)
    {
        var recipeContainer = new VisualElement();
        recipeContainer.AddToClassList("recipe-display");
        
        // Recipe name
        var recipeName = new Label(recipe.recipeName);
        recipeName.AddToClassList("recipe-name");
        recipeContainer.Add(recipeName);
        
        // Recipe layout (ingredients + result)
        var recipeLayout = CreateRecipeLayout(recipe);
        recipeContainer.Add(recipeLayout);
        
        // Status message
        var statusLabel = CreateRecipeStatusLabel(recipe, allIngredientsResearched, researchedCount);
        recipeContainer.Add(statusLabel);
        
        return recipeContainer;
    }
    
    // Creates recipe layout with ingredients and result
    private VisualElement CreateRecipeLayout(CraftingRecipe recipe)
    {
        var recipeLayout = new VisualElement();
        recipeLayout.AddToClassList("recipe-layout");
        
        // Ingredients section
        var ingredientsContainer = CreateIngredientsContainer(recipe);
        recipeLayout.Add(ingredientsContainer);
        
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
        
        return recipeLayout;
    }
    
    // Creates ingredients container
    private VisualElement CreateIngredientsContainer(CraftingRecipe recipe)
    {
        var ingredientsContainer = new VisualElement();
        ingredientsContainer.AddToClassList("ingredients-container");
        
        foreach (var ingredient in recipe.ingredients)
        {
            var ingredientSlot = CreateIngredientSlot(ingredient);
            if (ingredientSlot != null)
            {
                ingredientsContainer.Add(ingredientSlot);
            }
        }
        
        return ingredientsContainer;
    }
    
    // Creates status label for recipe
    private Label CreateRecipeStatusLabel(CraftingRecipe recipe, bool allIngredientsResearched, int researchedCount)
    {
        string statusText = allIngredientsResearched ? 
            "All ingredients researched.\nYou can now craft this recipe!" :
            $"Research {recipe.ingredients.Count - researchedCount} more ingredient(s) to unlock this recipe.";
        
        var statusLabel = new Label(statusText);
        statusLabel.AddToClassList(allIngredientsResearched ? "recipe-unlocked" : "recipe-locked");
        
        return statusLabel;
    }
    
    // Creates ingredient slot display
    private VisualElement CreateIngredientSlot(CraftingIngredient ingredient)
    {
        if (ingredient?.item == null) 
            return null;
        
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
        slot.AddToClassList(isResearched ? "ingredient-known" : "ingredient-unknown");

        return slot;
    }
    
    // Creates result slot display
    private VisualElement CreateResultSlot(CraftingRecipe recipe)
    {
        if (recipe?.result == null) 
            return null;
        
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
        var quantity = new Label("x" + recipe.resultQuantity);
        quantity.AddToClassList("result-quantity");
        slot.Add(quantity);
        
        return slot;
    }
    
    // Updates all display components
    private void UpdateAllDisplays()
    {
        UpdateSlotDisplay();
        UpdateProgress();
        UpdateButtonState();
    }
    
    // Updates research slot display
    private void UpdateSlotDisplay()
    {
        if (slotContainer == null || researchTable == null) 
            return;
        
        var item = researchTable.GetCurrentItem();
        
        if (item == null)
        {
            ShowEmptySlot();
        }
        else
        {
            ShowItemInSlot(item);
        }

        if (slotQuantity != null)
            slotQuantity.text = "";
    }
    
    // Shows empty slot state
    private void ShowEmptySlot()
    {
        if (slotIcon != null) 
            slotIcon.style.backgroundImage = null;
        slotContainer.AddToClassList("empty");
    }
    
    // Shows item in slot state
    private void ShowItemInSlot(InventoryItem item)
    {
        if (slotIcon != null) 
            slotIcon.style.backgroundImage = new StyleBackground(item.sprite);
        slotContainer.RemoveFromClassList("empty");
    }
    
    // Updates research progress display
    private void UpdateProgress()
    {
        if (progressLabel == null || ResearchSystem.Instance == null) 
            return;
        
        var progress = ResearchSystem.Instance.GetProgress();
        progressLabel.text = $"Research Progress: {progress.researchedCount} items studied\n" +
                            $"Recipes unlocked: {progress.unlockedRecipes}/{progress.totalRecipes}\n" +
                            $"Current seeds tier: {progress.currentTier}/{progress.maxTier}";
    }
    
    // Updates research button state and text
    private void UpdateButtonState()
    {
        if (researchButton == null || researchTable == null) 
            return;
        
        bool hasItem = researchTable.HasItem();
        researchButton.SetEnabled(hasItem);
        researchButton.text = "go";
    }
}