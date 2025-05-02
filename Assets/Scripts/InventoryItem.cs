using System;
using UnityEngine;

public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Sprite itemSpriteIcon;

    public virtual void Use(Vector3 position, PlayerController player)
    {
        Debug.Log("This item does nothing!");
    }
}
