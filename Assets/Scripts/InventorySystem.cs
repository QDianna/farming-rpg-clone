using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private List<InventoryEntry> items = new();
    private int selected = -2;

    void Awake()
    {
        selected = -2;
    }
    public InventoryItem GetSelectedItem()
    {
        if (items.Count == 0 || selected < 0 || selected > items.Count - 1)
        {
            return null;
        }

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
        {
            entry.quantity += amount;
        }
        else
        {
            items.Add(new InventoryEntry(item, amount));
        }
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
                    selected = -2;
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
