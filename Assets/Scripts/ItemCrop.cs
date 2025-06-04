using UnityEngine;

/// <summary>
/// Crop item that can be consumed to restore hunger and spawns visual drops when harvested.
/// </summary>
[CreateAssetMenu(menuName = "Items/ItemCrop")]
public class ItemCrop : InventoryItem
{
    [SerializeField] private GameObject droppedItemPrefab;
    [SerializeField] private float hungerRestoreValue;
    
    public override void UseItem(PlayerController player)
    {
        player.playerStats.RestoreHunger(hungerRestoreValue);
        player.inventorySystem.RemoveItem(this, 1);
        NotificationSystem.ShowNotification($"Ate {itemName} (+{hungerRestoreValue} hunger)");
    }

    public void DisplayCrop(Vector3 playerPosition, PlayerController player)
    {
        if (droppedItemPrefab == null) return;

        Vector2 offset = 1.4f * Random.insideUnitCircle;
        Vector3 cropSpawnPosition = playerPosition + new Vector3(offset.x, offset.y, 0);

        GameObject obj = Instantiate(droppedItemPrefab, cropSpawnPosition, Quaternion.identity);
        
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && itemSprite != null)
        {
            spriteRenderer.sprite = itemSprite;
        }

        var droppedEffect = obj.GetComponent<EffectDroppedItem>();
        droppedEffect?.Initialize(player);
    }
}