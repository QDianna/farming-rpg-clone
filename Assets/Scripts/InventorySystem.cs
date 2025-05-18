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
    public event System.Action OnSelectedItemChange;
    private int selectedItem = -2;

    void Awake()
    {
        selectedItem = -2;
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
        if (items.Count == 0 || selectedItem < 0 || selectedItem > items.Count - 1)
            return null;
        
        return items[selectedItem].item;
    }

    public int GetSelectedItemQuantity()
    {
        if (items.Count == 0 || selectedItem < 0 || selectedItem > items.Count - 1)
            return 0;
        
        return items[selectedItem].quantity;
    }
    
    public void GetNextItem()
    {
        if (selectedItem == -2)
        {
            Debug.Log("Opening inventory");
            selectedItem = -1;
            return;
        }
        
        if (items.Count == 0)
        {
            Debug.Log("Empty inventory");
            return;
        }

        selectedItem = (selectedItem + 1) % items.Count;
        
        Debug.Log("You've selected item: " + items[selectedItem].item + " - count: " + items[selectedItem].quantity);
        OnSelectedItemChange?.Invoke();
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
        if (entry == null)
        {
            Debug.Log("Error - no item to remove");
            return;
        }
        
        entry.quantity -= amount;
        
        if (entry.quantity <= 0)                // no more quantity, whole entry needs removing
        {
            items.Remove(entry);

            if (items.Count == 0)               // no more items in the inventory, reset inventory
                selectedItem = -1;
                
            if (selectedItem >= items.Count)    // deleted item in last position, reset cursor
                selectedItem = 0;
        }
        
        OnSelectedItemChange?.Invoke();        // notify the HUD
    }
}
