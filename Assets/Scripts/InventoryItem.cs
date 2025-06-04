using UnityEngine;

/// <summary>
/// Base ScriptableObject for all inventory items with economic and usage properties.
/// Override UseItem() to define custom item behavior like planting seeds or consuming food.
/// </summary>
public abstract class InventoryItem : ScriptableObject
{
    [Header("Basic Information")]
    public string itemName;
    public string description;
    public Sprite itemSprite;
    
    [Header("Economic Properties")]
    public int basePrice = 10;
    public bool canBeSold = true;
    public bool canBeBought = false;
    
    [Header("Inventory Properties")]
    public int maxStackSize = 99;
    
    public virtual void UseItem(PlayerController player)
    {
        // Override in derived classes for custom behavior
    }
}