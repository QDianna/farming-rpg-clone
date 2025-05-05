using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName = "Items/CropItem")]
public class CropItem : InventoryItem
{
    
    public Sprite itemSprite;
    [SerializeField] private GameObject droppedItemPrefab;  // visually drop item after harvest
    
    public override void Use(Vector3 position, PlayerController player)
    {
        Debug.Log("Do you want to eat " + this.itemName + "?");
        
        // TODO - eating crop, feeding crop to husband

        player.inventory.RemoveItem(this, 1);
    }

    public void DisplayCrop(Vector3 playerPosition, PlayerController player)
    {
        Vector2 offset = UnityEngine.Random.insideUnitCircle;
        Vector3 cropSpawnPosition = playerPosition + new Vector3(offset.x, offset.y, 0);

        if (droppedItemPrefab != null)
        {
            GameObject obj = Instantiate(droppedItemPrefab, cropSpawnPosition, Quaternion.identity);
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            
            if (itemSprite != null && sr != null)
            {
                sr.sprite = itemSprite;
            }
            else {
                Debug.Log("Error - cant find sprite");
            }

            if (obj.TryGetComponent(out DroppedItem dropped))
            {
                dropped.Initialize(player);
            }

        }
        else
        {
            Debug.Log("Error displaying harvested crop sprite");
        }
    }
    
}