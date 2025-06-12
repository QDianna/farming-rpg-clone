using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Crop item that can be consumed to restore hunger and spawns visual drops when harvested.
/// </summary>
[CreateAssetMenu(menuName = "Items/TheCureBeneath")]
public class ItemTheCureBeneath : InventoryItem
{
    [Header("Cure Settings")]
    [SerializeField] private float useRange = 3f;
    [SerializeField] private Sprite emptyBedSprite;
    private GameObject healthySpouse, sickSpouse;
    
    public override void UseItem(PlayerController player)
    {
        // Check if player is near the finish object
        sickSpouse = GameObject.FindGameObjectWithTag("SickSpouse");
        healthySpouse = GameObject.FindGameObjectWithTag("HealthySpouse");
        
        if (sickSpouse&& healthySpouse)
        {
            float distance = Vector3.Distance(player.transform.position, sickSpouse.transform.position);
            
            if (distance <= useRange)
            {
                // Player is close enough - cure worked!
                NotificationSystem.ShowNotification("You have been cured! Congratulations!");
                player.inventorySystem.RemoveItem(this, 1);
                
                // Change bed sprite to empty bed
                SpriteRenderer bedRenderer = sickSpouse.GetComponent<SpriteRenderer>();
                if (bedRenderer != null && emptyBedSprite != null)
                {
                    bedRenderer.sprite = emptyBedSprite;
                }

                var sr = healthySpouse.GetComponent<SpriteRenderer>();
                if (sr)
                    sr.enabled = true;
                
                // Trigger end game
                Debug.Log("GAME WON! THE CURE WORKED!");
                // TODO: Add your end game logic here
            }
            else
            {
                NotificationSystem.ShowNotification("You need to be closer to the sick spouse to use the cure!");
            }
        }
        else
        {
            Debug.LogError("Finish object not found!");
            NotificationSystem.ShowNotification("Something went wrong...");
        }
    }
}