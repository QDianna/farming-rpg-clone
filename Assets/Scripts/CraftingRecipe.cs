using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that defines a crafting recipe with ingredients, result, and unlock requirements.
/// Handles recipe progression where advanced recipes require crafting prerequisite recipes first.
/// </summary>
[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Recipe Definition")]
    public string recipeName;
    public InventoryItem result;
    public int resultQuantity = 1;
    public List<CraftingIngredient> ingredients;
    
    [Header("Unlock System")]
    public List<CraftingRecipe> prerequisiteRecipes;
    public bool startsUnlocked = false;
    
    [System.NonSerialized]
    public bool isUnlocked = false;
    
    [System.Serializable]
    public class CraftingIngredient
    {
        public InventoryItem item;
        public int quantity;
    }
}