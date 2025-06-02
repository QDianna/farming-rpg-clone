using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public string recipeName;
    public InventoryItem result;
    public int resultQuantity = 1;
    public List<CraftingIngredient> ingredients;
    
    [Header("Unlock Requirements")]
    public List<CraftingRecipe> prerequisiteRecipes; // What recipes must be crafted first
    public bool isUnlocked = false; // Starts false, becomes true when crafted
    public bool startsUnlocked = false; // For basic recipes like fertilizer
    
    [System.Serializable]
    public class CraftingIngredient
    {
        public InventoryItem item;
        public int quantity;
    }
}