using UnityEngine;

/// <summary>
/// Base class for all inventory items. Inherits from ScriptableObject to allow easy creation and management
/// of item data through the Unity Editor. Includes a virtual Use() method that can be overridden to define
/// custom item behavior (e.g., planting seeds, consuming food).
/// </summary>

public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;  // image that represents the object
    
    public virtual void UseItem(PlayerController player)
    {
        Debug.Log("This item does nothing!");
    }
}
