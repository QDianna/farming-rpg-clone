using UnityEngine;

/// <summary>
/// Crop item that can be consumed to restore hunger and spawns visual drops when harvested.
/// </summary>
[CreateAssetMenu(menuName = "Items/HealPotion")]
public class ItemHealPotion : InventoryItem
{
    [SerializeField] private GameObject starsEffectPrefab;
    
    public override void UseItem(PlayerController player)
    {
        player.animator.SetTrigger("Plant"); // Application animation
        
        if (starsEffectPrefab != null)
        {
            Instantiate(starsEffectPrefab, player.transform.position, Quaternion.identity);
        }
        
        // Heal all infected plants farm-wide
        player.plotlandController.HealAllInfectedPlants();
        player.inventorySystem.RemoveItem(this, 1);
    }
    
}