using UnityEngine;

/// <summary>
/// Wild plant system with collection and automatic regrowth mechanics.
/// Provides harvestable resources that regenerate after a specified time period.
/// </summary>
public class WildPlantController : MonoBehaviour, IInteractable
{
    [Header("Collection Settings")]
    public InventoryItem harvestableItem;
    public int collectAmount = 1;
    
    [Header("Growth Settings")]
    [SerializeField] private Sprite readyStageSprite;
    [SerializeField] private Sprite growingStageSprite;
    public float regrowthTime = 60f;
    
    private SpriteRenderer spriteRenderer;
    private bool isReadyForHarvest = true;
    private float growthTimer;
    
    private void Start()
    {
        InitializePlant();
    }
    
    private void Update()
    {
        if (!isReadyForHarvest)
        {
            UpdateRegrowth();
        }
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (isReadyForHarvest && other.TryGetComponent<PlayerController>(out _))
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
        }
    }

    public void Interact(PlayerController player)
    {
        if (!CanHarvest())
            return;
        
        HarvestPlant(player);
        StartRegrowthCycle();
    }
    
    // Sets up initial plant state and sprite
    private void InitializePlant()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = readyStageSprite;
    }
    
    // Updates regrowth timer and checks for completion
    private void UpdateRegrowth()
    {
        growthTimer += Time.deltaTime;
        
        if (growthTimer >= regrowthTime)
        {
            CompleteRegrowth();
        }
    }
    
    // Validates if plant can be harvested
    private bool CanHarvest()
    {
        return harvestableItem != null && isReadyForHarvest;
    }
    
    // Processes plant harvesting and gives rewards
    private void HarvestPlant(PlayerController player)
    {
        InventorySystem.Instance.AddItem(harvestableItem, collectAmount);
        NotificationSystem.ShowNotification($"Picked up {harvestableItem.name} x{collectAmount}");
        harvestableItem.CollectItem(player);
        
        InteractionSystem.Instance.SetCurrentInteractable(null);
    }
    
    // Begins the regrowth cycle
    private void StartRegrowthCycle()
    {
        isReadyForHarvest = false;
        growthTimer = 0f;
        spriteRenderer.sprite = growingStageSprite;
    }
    
    // Completes regrowth and makes plant harvestable again
    private void CompleteRegrowth()
    {
        isReadyForHarvest = true;
        spriteRenderer.sprite = readyStageSprite;
    }
}