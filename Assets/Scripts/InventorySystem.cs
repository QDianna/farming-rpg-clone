using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data container for inventory items with quantity tracking.
/// </summary>
[System.Serializable]
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
/// Handles item cycling, usage, quantity tracking, and provides events for UI synchronization.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [SerializeField] private List<InventoryEntry> items = new List<InventoryEntry>();
    [HideInInspector] public int selectedItemIndex = -1;

    public event System.Action OnSelectedItemChange;
    public event System.Action OnInventoryChanged;
    // public event System.Action<InventoryItem> OnInventoryItemClicked;

    public event System.Action<InventoryItem, bool> OnInventoryItemClicked;

    
    private void Awake()
    {
        InitializeSingleton();
    }

    public bool HasItem(InventoryItem item, int quantity)
    {
        foreach (var entry in items)
            if (entry.item == item && entry.quantity >= quantity)
                return true;
        return false;
    }
    
    public bool HasItemByName(string itemName, int quantity)
    {
        foreach (var entry in items)
        {
            if (entry.item != null && entry.item.newName == itemName && entry.quantity >= quantity)
                return true;
        }
        return false;
    }

    public void UseCurrentItem(PlayerController player)
    {
        var item = GetSelectedItem();
        if (item == null) return;

        var quantity = GetSelectedItemQuantity();
        if (quantity <= 0)
        {
            NotificationSystem.ShowHelp("You don't have any more of this item!");
            return;
        }

        item.UseItem(player);
    }
    
    public InventoryItem GetSelectedItem()
    {
        if (IsValidSelection())
            return items[selectedItemIndex].item;
        return null;
    }

    public int GetSelectedItemQuantity()
    {
        if (IsValidSelection())
            return items[selectedItemIndex].quantity;
        return 0;
    }
    
    public void GetNextItem()
    {
        if (items.Count == 0) 
            return;

        selectedItemIndex = (selectedItemIndex + 1) % items.Count;
        OnSelectedItemChange?.Invoke();
    }
    
    public void GetPreviousItem()
    {
        if (items.Count == 0) 
            return;

        selectedItemIndex = (selectedItemIndex - 1 + items.Count) % items.Count;
        OnSelectedItemChange?.Invoke();
    }
    
    public void AddItem(InventoryItem item, int amount)
    {
        var existingEntry = FindItemEntry(item);
        
        if (existingEntry != null)
            existingEntry.quantity += amount;
        else
            items.Add(new InventoryEntry(item, amount));
        
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(InventoryItem item, int amount)
    {
        var entry = FindItemEntry(item);
        if (entry == null) 
            return;
        
        entry.quantity -= amount;
        TriggerInventoryEvents();
    }
    
    public void RemoveItemByName(string itemName, int amount)
    {
        foreach (var entry in items)
        {
            if (entry.item != null && entry.item.newName == itemName && entry.quantity >= amount)
            {
                entry.quantity -= amount;
                return;
            }
        }
    }
    
    public InventoryItem FindItemByName(string itemName)
    {
        foreach (var entry in items)
        {
            if (entry.item != null && string.Equals(entry.item.newName, itemName))
                return entry.item;
        }
        return null;
    }
    
    public List<InventoryEntry> GetAllItems()
    {
        return new List<InventoryEntry>(items);
    }
    
    /*public void TriggerItemClick(InventoryItem item)
    {
        var entry = FindItemEntry(item);
        if (entry == null || entry.quantity <= 0)
            return; // Do nothing if item doesn't exist or is empty

        OnInventoryItemClicked?.Invoke(item);
    }*/
    
    
    public void TriggerItemClick(InventoryItem item, bool shiftHeld)
    {
        var entry = FindItemEntry(item);
        if (entry == null || entry.quantity <= 0)
            return;

        OnInventoryItemClicked?.Invoke(item, shiftHeld);
    }
    
    // Sets up singleton instance
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            selectedItemIndex = -1;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Checks if current selection index is valid
    private bool IsValidSelection()
    {
        return items.Count > 0 && selectedItemIndex >= 0 && selectedItemIndex < items.Count;
    }
    
    // Finds inventory entry for specific item
    private InventoryEntry FindItemEntry(InventoryItem item)
    {
        return items.Find(entry => entry.item == item);
    }
    
    // Triggers both selection and inventory change events
    private void TriggerInventoryEvents()
    {
        OnSelectedItemChange?.Invoke();
        OnInventoryChanged?.Invoke();
    }
}