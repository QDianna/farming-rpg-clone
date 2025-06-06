using UnityEngine;

/// <summary>
/// Bridge between HUD and ResearchSystem. Handles interaction, costs, and slot management.
/// NO complex logic - just coordination.
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
    
    // Events - clean and simple
    public event System.Action OnTableOpened;
    public event System.Action OnTableClosed;
    public event System.Action OnSlotChanged;
    public event System.Action<ResearchResult> OnResearchCompleted;
    
    #region Interaction
    
    public void Interact(PlayerController player)
    {
        if (!isUnlocked)
        {
            NotificationSystem.ShowNotification("Research Table is locked!");
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
        NotificationSystem.ShowNotification($"Research Table opened - Cost: {GetCost()} coins");
    }
    
    private void CloseTable()
    {
        isOpen = false;
        ReturnItemToInventory();
        OnTableClosed?.Invoke();
        NotificationSystem.ShowNotification("Research Table closed");
    }
    
    #endregion
    
    #region Slot Management
    
    public bool TryAddItem(InventoryItem item)
    {
        if (currentResearchItem != null)
        {
            NotificationSystem.ShowNotification("Research slot is occupied!");
            return false;
        }
        
        currentResearchItem = item;
        OnSlotChanged?.Invoke();
        NotificationSystem.ShowNotification($"Added {item.name} to research");
        return true;
    }
    
    public bool TryRemoveItem()
    {
        if (currentResearchItem == null) return false;
        
        InventorySystem.Instance.AddItem(currentResearchItem, 1);
        currentResearchItem = null;
        OnSlotChanged?.Invoke();
        NotificationSystem.ShowNotification("Item returned to inventory");
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
            NotificationSystem.ShowNotification("No item to research!");
            return false;
        }
        
        if (ResearchSystem.Instance.IsResearched(currentResearchItem.name))
        {
            NotificationSystem.ShowNotification($"Already researched {currentResearchItem.name}!");
            return false;
        }
        
        // Cost check
        int cost = GetCost();
        if (playerEconomy != null && !playerEconomy.CanAfford(cost))
        {
            NotificationSystem.ShowNotification($"Need {cost} coins!");
            return false;
        }
        
        // Pay and research
        if (playerEconomy != null)
            playerEconomy.SpendMoney(cost);
        
        var result = ResearchSystem.Instance.DoResearch(currentResearchItem);
        
        if (result != null)
        {
            currentResearchItem = null; // Item consumed
            OnSlotChanged?.Invoke();
            OnResearchCompleted?.Invoke(result);
            NotificationSystem.ShowNotification($"Research complete! (Cost: {cost} coins)");
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
        return baseResearchCost + (ResearchSystem.Instance?.GetProgress().researchedIngredients * 10 ?? 0);
    }
    
    #endregion
    
    #region IInteractable
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
            NotificationSystem.ShowNotification($"Press E to use Research Table! (Cost: {GetCost()} coins)");
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