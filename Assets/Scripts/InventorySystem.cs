using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public List<InventoryItem> items;
    public int selected;

    public InventoryItem GetSelectedItem()
    {
        if (items.Count == 0 || selected < 0 || selected > items.Count - 1)
        {
            return null;
        }

        return items[selected];
    }

    public void GetNextItem()
    {
        if (selected == -1)
        {
            Debug.Log("Opening inventory");
            selected = 0;
            return;
        }
        if (items.Count == 0)
        {
            Debug.Log("Empty inventory");
            return;
        }

        Debug.Log("You've selected item: " + GetSelectedItem().itemName);
        selected = (selected + 1) % items.Count;
    }

    public void AddItem(string itemName, int amount)
    {
        InventoryItem newItem = new InventoryItem(itemName, amount);
        
        items.Add(newItem);
    }
    
    public void RemoveItem(string itemName, int amount) { /*...*/ }
    
    void Awake()
    {
        items = new List<InventoryItem>();
        selected = -1;
    }
}
