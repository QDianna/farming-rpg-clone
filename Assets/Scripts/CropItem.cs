using UnityEngine;

/// <summary>
/// A specialized InventoryItem representing a harvestable crop.
/// 
/// CropItems can be used by the player (e.g., to eat or feed others) and visually displayed
/// in the world upon harvesting through a collectible drop effect.
/// </summary>

[CreateAssetMenu(menuName = "Items/CropItem")]
public class CropItem : InventoryItem
{
    // image that represents the crop (ex: carrot image)
    public Sprite itemSprite;
    // prefab used to visually represent the dropped item after harvesting
    [SerializeField] private GameObject droppedItemPrefab;
    // hunger restore value
    [SerializeField] private float hungerRestoreValue;
    
    public override void UseItem(PlayerController player)
    {
        Debug.Log("Eating " + this);
        player.playerStats.RestoreHunger(hungerRestoreValue);
        player.inventorySystem.RemoveItem(this, 1);
    }

    /// <summary>
    /// Spawns a visual drop of the crop in the world at a random nearby position.
    /// The drop moves toward the player and disappears when collected.
    /// Uses DroppedItem to render and move the itemSprite.
    /// </summary>
    public void DisplayCrop(Vector3 playerPosition, PlayerController player)
    {
        Vector2 offset = Random.insideUnitCircle;
        Vector3 cropSpawnPosition = playerPosition + new Vector3(offset.x, offset.y, 0);

        if (droppedItemPrefab != null)
        {
            GameObject obj = Instantiate(droppedItemPrefab, cropSpawnPosition, Quaternion.identity);
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            
            if (itemSprite != null && sr != null)
                sr.sprite = itemSprite;
            else
                Debug.Log("Error - cant find sprite");

            if (obj.TryGetComponent(out DroppedItem dropped))
                dropped.Initialize(player);
            else
                Debug.Log("Error - cant find scriptable object");
        }
        
        else
        {
            Debug.Log("Error displaying harvested crop sprite");
        }
    }
}
