using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data class for individual recipe ingredients with item and quantity requirements.
/// </summary>
[System.Serializable]
public class CraftingIngredient
{
    public InventoryItem item;
    public int quantity;
}

/// <summary>
/// Crafting recipe definition with ingredients, results, and unlock progression.
/// Supports dynamic unlocking system for recipe advancement and complexity scaling.
/// </summary>
[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Recipe Definition")]
    public string recipeName;
    public InventoryItem result;
    public int resultQuantity = 1;
    
    [Header("Requirements")]
    public List<CraftingIngredient> ingredients;
    
    [System.NonSerialized]
    public bool isUnlocked;
}