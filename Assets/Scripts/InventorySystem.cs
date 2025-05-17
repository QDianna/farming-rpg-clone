using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single entry in the player's inventory, storing a reference to an InventoryItem
/// and the quantity owned. Used internally by the InventorySystem to track item stacks.
/// </summary>

public class InventoryEntry
{
    public InventoryItem item;
    public int quantity;

    public InventoryEntry(InventoryItem item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

/// <summary>
/// Manages the player's inventory, including adding, removing, and selecting items.
/// Designed to be lightweight, stack-based, and scalable for future features like UI or item categories.
/// Items are stored as InventoryEntry instances containing item data and quantity.
/// </summary>

public class InventorySystem : MonoBehaviour
{
    private List<InventoryEntry> items = new();
    private int selected = -2;

    void Awake()
    {
        selected = -2;
    }

    public void UseCurrentItem(PlayerController player)
    {
        var item = GetSelectedItem();
        if (item == null)
        {
            Debug.Log("No item to use.");
            return;
        }

        item.UseItem(player);
    }
    public InventoryItem GetSelectedItem()
    {
        if (items.Count == 0 || selected < 0 || selected > items.Count - 1)
            return null;
        
        return items[selected].item;
    }
    
    public void GetNextItem()
    {
        if (selected == -2)
        {
            Debug.Log("Opening inventory");
            selected = -1;
            return;
        }
        
        if (items.Count == 0)
        {
            Debug.Log("Empty inventory");
            return;
        }

        selected = (selected + 1) % items.Count;
        Debug.Log("You've selected item: " + items[selected].item + " - count: " + items[selected].quantity);
    }

    public void AddItem(InventoryItem item, int amount)
    {
        var entry = items.Find(i => i.item == item);
        
        if (entry != null)
            entry.quantity += amount;
        else
            items.Add(new InventoryEntry(item, amount));
    }

    public void RemoveItem(InventoryItem item, int amount)
    {
        var entry = items.Find(i => i.item == item);
        if (entry != null)
        {
            entry.quantity -= amount;
            
            if (entry.quantity <= 0)
            {
                items.Remove(entry);

                if (items.Count == 0)
                {
                    // deleted all inventory
                    selected = -1;
                    return;
                }
                
                if (selected >= items.Count)
                {
                    // deleted last position item of inventory
                    selected = 0;  // reset to first item
                    Debug.Log("You've selected item: " + items[selected].item + " - count: " + items[selected].quantity);
                }
            }

            else
            {
                Debug.Log("You've used item: " + items[selected].item + " - remaining: " + items[selected].quantity);
            }
        }
    }
}
