using UnityEngine;

/// <summary>
/// Wild plant system with collection and automatic regrowth mechanics.
/// Provides harvestable resources that regenerate after a specified time period with seasonal spawning.
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
    
    [Header("Seasonal Spawning")]
    public Season season;
    [Range(0f, 1f)] public float dailySpawnChance = 0.5f;
    
    private SpriteRenderer spriteRenderer;
    private bool isReadyForHarvest = false;
    private bool isActive = false;
    private float growthTimer;
    
    private void Start()
    {
        InitializePlant();
        SubscribeToTimeEvents();
        CheckSeasonalSpawn();
    }
    
    private void Update()
    {
        if (isActive && !isReadyForHarvest)
        {
            UpdateRegrowth();
        }
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromTimeEvents();
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
        SetPlantInactive();
    }
    
    // Subscribes to time system events for daily spawning checks
    private void SubscribeToTimeEvents()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange += CheckDailySpawn;
        }
    }
    
    // Unsubscribes from time system events
    private void UnsubscribeFromTimeEvents()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange -= CheckDailySpawn;
        }
    }
    
    // Checks if plant should spawn based on current season
    private void CheckSeasonalSpawn()
    {
        if (TimeSystem.Instance == null) return;
        
        Season currentSeason = TimeSystem.Instance.GetSeason();
        if (currentSeason == season)
        {
            // Plant can potentially spawn in its season
            CheckDailySpawn();
        }
        else
        {
            // Wrong season, make sure plant is inactive
            SetPlantInactive();
        }
    }
    
    // Daily spawn chance check
    private void CheckDailySpawn()
    {
        if (TimeSystem.Instance == null) return;
        
        Season currentSeason = TimeSystem.Instance.GetSeason();
        
        // Only check spawn in correct season
        if (currentSeason != season)
        {
            SetPlantInactive();
            return;
        }
        
        // If plant is already active and regrowing, don't interrupt
        if (isActive && !isReadyForHarvest) return;
        
        // Random spawn chance
        float randomValue = Random.Range(0f, 1f);
        if (randomValue <= dailySpawnChance)
        {
            SpawnPlant();
        }
        else if (!isActive || (isActive && isReadyForHarvest))
        {
            // Plant doesn't spawn today or expires if ready
            SetPlantInactive();
        }
    }
    
    // Spawns the plant and makes it ready for harvest
    private void SpawnPlant()
    {
        isActive = true;
        isReadyForHarvest = true;
        spriteRenderer.sprite = readyStageSprite;
        spriteRenderer.color = Color.white;
    }
    
    // Makes plant inactive and invisible
    private void SetPlantInactive()
    {
        isActive = false;
        isReadyForHarvest = false;
        growthTimer = 0f;
        spriteRenderer.color = Color.clear; // Make invisible
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
        return harvestableItem != null && isReadyForHarvest && isActive;
    }
    
    // Processes plant harvesting and gives rewards
    private void HarvestPlant(PlayerController player)
    {
        InventorySystem.Instance.AddItem(harvestableItem, collectAmount);
        NotificationSystem.ShowNotification($"Picked up {harvestableItem.name} x{collectAmount}");
        harvestableItem.CollectItem(player);
        
        InteractionSystem.Instance.SetCurrentInteractable(null);
    }
    
    // Begins the regrowth cycle after harvesting
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