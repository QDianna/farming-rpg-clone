using UnityEngine;

public class InventoryItem
{
    public string itemName;
    public Sprite itemSpriteIcon;
    public int quantity;

    public InventoryItem(string itemName, int quantity)
    {
        this.itemName = itemName;
        this.quantity = quantity;
    }
}
