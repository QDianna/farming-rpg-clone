using UnityEngine;

/// <summary>
/// Interaction for building bridges over rivers.
/// Requires resources and unlocks bridge passage.
/// </summary>
public class InteractionBuildBridge : MonoBehaviour, IInteractable
{
    [Header("Requirements")]
    [SerializeField] private int energyRequired;

    [SerializeField] private int woodRequired;
    
    [Header("References")]
    [SerializeField] private GameObject bridgeSprite; // river_bridge
    [SerializeField] private GameObject blockerCollider; // river_bridge_blocker
    
    private bool bridgeBuilt;
    
    private void Start()
    {
        // Hide bridge initially
        if (bridgeSprite != null)
            bridgeSprite.SetActive(false);
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var _) && !bridgeBuilt)
        {
            NotificationSystem.ShowDialogue("You could build a bridge to go across! " +
                                                $"Gather {woodRequired} pieces of wood by chopping down trees. " +
                                                "Press E when you are ready.", 6f);
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var _))
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
        }
    }

    public void Interact(PlayerController player)
    {
        if (bridgeBuilt)
        {
            return;
        }
        
        if (!CanBuildBridge(player))
            return;
            
        BuildBridge(player);
    }
    
    // Checks if player has required resources
    private bool CanBuildBridge(PlayerController player)
    {
        // Check energy
        if (player.playerStats.GetEnergy() < energyRequired)
        {
            NotificationSystem.ShowHelp($"Need {energyRequired} energy to build bridge!");
            return false;
        }
        
        // Check wood - find by name
        if (!InventorySystem.Instance.HasItemByName("wood", woodRequired))
        {
            NotificationSystem.ShowHelp($"Need {woodRequired} wood to build bridge!");
            return false;
        }
        
        return true;
    }
    
    // Builds the bridge and consumes resources
    private void BuildBridge(PlayerController player)
    {
        // Consume resources
        player.playerStats.SetEnergy(player.playerStats.GetEnergy() - energyRequired);
        InventorySystem.Instance.RemoveItemByName("wood", woodRequired);
        
        // Show bridge
        if (bridgeSprite != null)
            bridgeSprite.SetActive(true);
            
        // Remove blocker
        if (blockerCollider != null)
            Destroy(blockerCollider);
            
        bridgeBuilt = true;
        NotificationSystem.ShowHelp("Bridge built! Now you can cross the river.");
        
        // Remove interaction
        InteractionSystem.Instance.SetCurrentInteractable(null);
    }
}