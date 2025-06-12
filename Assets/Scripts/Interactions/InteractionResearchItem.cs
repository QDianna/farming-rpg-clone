using UnityEngine;

/// <summary>
/// Interactive research table for unlocking new content with items and coins.
/// Manages single-slot research interface with escalating costs based on progress.
/// </summary>
public class InteractionResearchItem : MonoBehaviour, IInteractable
{
    [Header("Research Settings")]
    [SerializeField] private int baseResearchCost = 50;
    [SerializeField] private bool isUnlocked = true;
    
    [Header("References")]
    [SerializeField] private PlayerEconomy playerEconomy;
    
    public InventoryItem currentResearchItem;
    private bool isTableOpen;
    
    public event System.Action OnTableOpened;
    public event System.Action OnTableClosed;
    public event System.Action OnSlotChanged;
    public event System.Action<string, bool> OnResearchCompleted; // item name, was already researched
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
            // NotificationSystem.ShowNotification("Press E to use the Research Table!");
        }
    }
    
    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
            if (isTableOpen) 
                CloseTable();
        }
    }
    
    public void Interact(PlayerController player)
    {
        if (!isUnlocked)
            return;
        
        if (playerEconomy == null)
            playerEconomy = player.GetComponent<PlayerEconomy>();
        
        if (isTableOpen)
            CloseTable();
        else
            OpenTable();
    }
    
    public bool TryAddItem(InventoryItem item)
    {
        if (currentResearchItem != null)
            return false;
        
        currentResearchItem = item;
        OnSlotChanged?.Invoke();
        return true;
    }
    
    public bool TryRemoveItem()
    {
        if (currentResearchItem == null) 
            return false;
        
        InventorySystem.Instance.AddItem(currentResearchItem, 1);
        currentResearchItem = null;
        OnSlotChanged?.Invoke();
        return true;
    }
    
    public bool TryResearch()
    {
        if (!CanResearch())
            return false;
        
        string itemName = currentResearchItem.name;
        bool wasAlreadyResearched = ResearchSystem.Instance.IsResearched(itemName);
    
        // Only charge for new research
        if (!wasAlreadyResearched)
        {
            int cost = GetResearchCost();
            playerEconomy?.SpendMoney(cost);
        }
    
        bool success = ResearchSystem.Instance.DoResearch(currentResearchItem);
    
        if (success)
        {
            // Only consume item for new research
            if (!wasAlreadyResearched)
            {
                currentResearchItem = null;
            }
            OnSlotChanged?.Invoke();
            OnResearchCompleted?.Invoke(itemName, wasAlreadyResearched); // Pass both parameters!
        }
    
        return success;
    }
    
    // State getters
    public bool IsOpen() => isTableOpen;
    public bool HasItem() => currentResearchItem != null;
    public InventoryItem GetCurrentItem() => currentResearchItem;
    
    // Opens the research table interface
    private void OpenTable()
    {
        isTableOpen = true;
        OnTableOpened?.Invoke();
    }
    
    // Closes table and returns any item to inventory
    private void CloseTable()
    {
        isTableOpen = false;
        ReturnItemToInventory();
        OnTableClosed?.Invoke();
    }
    
    // Returns current research item to player inventory
    private void ReturnItemToInventory()
    {
        if (currentResearchItem != null)
        {
            InventorySystem.Instance.AddItem(currentResearchItem, 1);
            currentResearchItem = null;
            OnSlotChanged?.Invoke();
        }
    }
    
    // Validates if research can be performed
    private bool CanResearch()
    {
        if (currentResearchItem == null)
            return false;
        
        // Allow research of already researched items (for recipe viewing)
        if (ResearchSystem.Instance.IsResearched(currentResearchItem.name))
        {
            return true; // No cost for already researched items
        }
        
        int cost = GetResearchCost();
        if (playerEconomy != null && !playerEconomy.CanAfford(cost))
        {
            NotificationSystem.ShowNotification($"You need {cost} coins!");
            return false;
        }
        
        return true;
    }
    
    // Calculates research cost based on progress
    private int GetResearchCost()
    {
        int researchedCount = ResearchSystem.Instance?.GetProgress().researchedCount ?? 0;
        return baseResearchCost + (researchedCount * 10);
    }
}