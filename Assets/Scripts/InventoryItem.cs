using UnityEngine;

/// <summary>
/// Base ScriptableObject for all inventory items with economic and usage properties.
/// Override UseItem() to define custom item behavior like planting seeds or consuming food.
/// </summary>
public abstract class InventoryItem : ScriptableObject
{
    [Header("General Information")]
    public string name;
    public string description;
    public Sprite sprite;
    public GameObject droppedItemPrefab;
    
    [Header("Economic Properties")]
    public int basePrice = 10;
    public bool canBeSold = true;
    public bool canBeBought = false;
    
    [Header("Inventory Properties")]
    public int maxStackSize = 99;
    
    public virtual void UseItem(PlayerController player)
    {
        // Override in derived classes for custom behavior
    }

    public void CollectItem(PlayerController player)
    {
        if (droppedItemPrefab == null) return;

        Vector3 playerPosition = player.transform.position;
    
        // Get random direction and distance between 1 and 1.5 units
        Vector2 direction = Random.insideUnitCircle.normalized; // Random direction
        float distance = Random.Range(0.8f, 1.2f); // Random distance between 1 and 1.5
        Vector2 offset = direction * distance;
    
        Vector3 spawnPosition = playerPosition + new Vector3(offset.x, offset.y, 0);

        var obj = Instantiate(droppedItemPrefab, spawnPosition, Quaternion.identity);
    
        obj.transform.localScale = Vector3.one * 0.85f;
    
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && sprite != null)
            spriteRenderer.sprite = sprite;

        var droppedEffect = obj.GetComponent<EffectDroppedItem>();
        droppedEffect?.Initialize(player);
    }
}