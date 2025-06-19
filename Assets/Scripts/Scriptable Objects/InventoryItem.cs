using UnityEngine;

/// <summary>
/// Base class for all inventory items with economic properties and usage behavior.
/// Provides virtual UseItem() method for custom item functionality and visual drop effects.
/// </summary>
public abstract class InventoryItem : ScriptableObject
{
    [Header("Item Information")]
    public string newName;
    public string description;
    public Sprite sprite;
    public GameObject droppedItemPrefab;
    
    [Header("Economic Properties")]
    public int basePrice;
    public bool canBeSold = true;
    public bool canBeBought;
    
    [Header("Inventory Properties")]
    public int maxStackSize = 99;
    
    public virtual void UseItem(PlayerController player)
    {
        // Override in derived classes for custom behavior
    }

    // Spawns visual drop effect at random position around player
    public void CollectItem(PlayerController player)
    {
        if (droppedItemPrefab == null) 
            return;

        Vector3 spawnPosition = CalculateDropPosition(player.transform.position);
        CreateDropEffect(spawnPosition, player);
    }
    
    // Calculates random spawn position around player
    private Vector3 CalculateDropPosition(Vector3 playerPosition)
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(0.8f, 1.2f);
        Vector2 offset = randomDirection * randomDistance;
        
        return playerPosition + new Vector3(offset.x, offset.y, 0);
    }
    
    // Creates and configures the visual drop effect
    private void CreateDropEffect(Vector3 spawnPosition, PlayerController player)
    {
        var dropObject = Instantiate(droppedItemPrefab, spawnPosition, Quaternion.identity);
        
        ConfigureDropAppearance(dropObject);
        InitializeDropEffect(dropObject, player);
    }
    
    // Sets up visual appearance of dropped item
    private void ConfigureDropAppearance(GameObject dropObject)
    {
        dropObject.transform.localScale = Vector3.one * 0.85f;
        
        var spriteRenderer = dropObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && sprite != null)
            spriteRenderer.sprite = sprite;
    }
    
    // Initializes drop effect behavior
    private void InitializeDropEffect(GameObject dropObject, PlayerController player)
    {
        var droppedEffect = dropObject.GetComponent<EffectDroppedItem>();
        droppedEffect?.Initialize(player);
    }
}