using UnityEngine;

/// <summary>
/// Bridge between HUD and ResearchSystem. Handles interaction, costs, and slot management.
/// </summary>
public class InteractionResearchItem : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private int baseResearchCost = 50;
    [SerializeField] private bool isUnlocked = true;
    
    [Header("References")]
    [SerializeField] private PlayerEconomy playerEconomy;
    
    // Current state
    public InventoryItem currentResearchItem;
    private bool isOpen;
    
    // Simple events
    public event System.Action OnTableOpened;
    public event System.Action OnTableClosed;
    public event System.Action OnSlotChanged;
    public event System.Action<string> OnResearchCompleted; // Just pass item name
    
    #region Interaction
    
    public void Interact(PlayerController player)
    {
        if (!isUnlocked)
        {
            Debug.Log("Research Table is locked!");
            return;
        }
        
        if (playerEconomy == null)
            playerEconomy = player.GetComponent<PlayerEconomy>();
        
        if (isOpen)
            CloseTable();
        else
            OpenTable();
    }
    
    private void OpenTable()
    {
        isOpen = true;
        OnTableOpened?.Invoke();
        Debug.Log($"Research Table opened");
    }
    
    private void CloseTable()
    {
        isOpen = false;
        ReturnItemToInventory();
        OnTableClosed?.Invoke();
        Debug.Log("Research Table closed");
    }
    
    #endregion
    
    #region Slot Management
    
    public bool TryAddItem(InventoryItem item)
    {
        if (currentResearchItem != null)
        {
            Debug.Log("Research slot occupied");
            return false;
        }
        
        currentResearchItem = item;
        OnSlotChanged?.Invoke();
        Debug.Log($"Added {item.name} to research");
        return true;
    }
    
    public bool TryRemoveItem()
    {
        if (currentResearchItem == null) return false;
        
        InventorySystem.Instance.AddItem(currentResearchItem, 1);
        currentResearchItem = null;
        OnSlotChanged?.Invoke();
        Debug.Log("Item returned to inventory");
        return true;
    }
    
    private void ReturnItemToInventory()
    {
        if (currentResearchItem != null)
        {
            InventorySystem.Instance.AddItem(currentResearchItem, 1);
            currentResearchItem = null;
            OnSlotChanged?.Invoke();
        }
    }
    
    #endregion
    
    #region Research Action
    
    public bool TryResearch()
    {
        // Basic checks
        if (currentResearchItem == null)
        {
            Debug.Log("No item to research!");
            return false;
        }
        
        if (ResearchSystem.Instance.IsResearched(currentResearchItem.name))
        {
            NotificationSystem.ShowNotification($"You already researched {currentResearchItem.name}!");
            return false;
        }
        
        // Cost check
        int cost = GetCost();
        if (playerEconomy != null && !playerEconomy.CanAfford(cost))
        {
            NotificationSystem.ShowNotification($"You need {cost} coins!");
            return false;
        }
        
        // Pay and research
        if (playerEconomy != null)
            playerEconomy.SpendMoney(cost);
        
        string itemName = currentResearchItem.name;
        bool success = ResearchSystem.Instance.DoResearch(currentResearchItem);
        
        if (success)
        {
            currentResearchItem = null; // Item consumed
            OnSlotChanged?.Invoke();
            OnResearchCompleted?.Invoke(itemName);
            Debug.Log($"Research complete! (Cost: {cost} coins)");
            return true;
        }
        
        return false;
    }
    
    #endregion
    
    #region Simple Getters
    
    public bool IsOpen() => isOpen;
    public bool HasItem() => currentResearchItem != null;
    public InventoryItem GetCurrentItem() => currentResearchItem;
    
    private int GetCost()
    {
        return baseResearchCost + (ResearchSystem.Instance?.GetProgress().researchedCount * 10 ?? 0);
    }
    
    #endregion
    
    #region IInteractable
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
            NotificationSystem.ShowNotification($"Press E to use Research Table!");
        }
    }
    
    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
            if (isOpen) CloseTable();
        }
    }
    
    #endregion
}