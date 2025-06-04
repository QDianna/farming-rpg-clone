using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple data container for inventory items with quantity tracking.
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
/// Singleton inventory system managing player items with selection, stacking, and UI integration.
/// Supports item cycling, usage, and events for UI synchronization.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }
    
    private List<InventoryEntry> items = new();
    private int selectedItem = -2;

    public event System.Action OnSelectedItemChange;
    public event System.Action OnInventoryChanged;
    public event System.Action<InventoryItem> OnInventoryItemClicked;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            selectedItem = -2;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool HasItem(InventoryItem item, int quantity)
    {
        foreach (var entry in items)
            if (entry.item == item && entry.quantity >= quantity)
                return true;
        return false;
    }

    public void UseCurrentItem(PlayerController player)
    {
        var item = GetSelectedItem();
        if (item != null)
        {
            item.UseItem(player);
        }
    }
    
    public InventoryItem GetSelectedItem()
    {
        if (items.Count == 0 || selectedItem < 0 || selectedItem >= items.Count)
            return null;
        return items[selectedItem].item;
    }

    public int GetSelectedItemQuantity()
    {
        if (items.Count == 0 || selectedItem < 0 || selectedItem >= items.Count)
            return 0;
        return items[selectedItem].quantity;
    }
    
    public void GetNextItem()
    {
        if (selectedItem == -2)
        {
            selectedItem = -1;
            return;
        }
        
        if (items.Count == 0) return;

        selectedItem = (selectedItem + 1) % items.Count;
        OnSelectedItemChange?.Invoke();
    }

    public void AddItem(InventoryItem item, int amount)
    {
        var entry = items.Find(i => i.item == item);
        
        if (entry != null)
            entry.quantity += amount;
        else
            items.Add(new InventoryEntry(item, amount));
        
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(InventoryItem item, int amount)
    {
        var entry = items.Find(i => i.item == item);
        if (entry == null) return;
        
        entry.quantity -= amount;
        
        if (entry.quantity <= 0)
        {
            items.Remove(entry);

            if (items.Count == 0)
                selectedItem = -1;
            else if (selectedItem >= items.Count)
                selectedItem = 0;
        }
        
        OnSelectedItemChange?.Invoke();
        OnInventoryChanged?.Invoke();
    }
    
    public List<InventoryEntry> GetAllItems()
    {
        return new List<InventoryEntry>(items);
    }
    
    public void TriggerItemClick(InventoryItem item)
    {
        OnInventoryItemClicked?.Invoke(item);
    }
    
    public int GetItemQuantity(InventoryItem item)
    {
        if (item == null) return 0;
    
        int totalQuantity = 0;
        foreach (var entry in items)
        {
            if (entry.item == item)
                totalQuantity += entry.quantity;
        }
        return totalQuantity;
    }

    public List<InventoryItem> GetSellableItems()
    {
        var sellableItems = new List<InventoryItem>();
    
        foreach (var entry in items)
        {
            if (entry.item?.canBeSold == true && entry.quantity > 0 && !sellableItems.Contains(entry.item))
            {
                sellableItems.Add(entry.item);
            }
        }
        return sellableItems;
    }
}