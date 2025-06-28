using UnityEngine;

/// <summary>
/// Wild plant system with collection and automatic regrowth mechanics.
/// Provides harvestable resources that regenerate after a specified time period with seasonal spawning.
/// </summary>
public class WildPlantController : MonoBehaviour, IInteractable
{
    [Header("References")]
    public InventoryItem collectedWildPlant;
    [SerializeField] private Sprite readyStageSprite;
    [SerializeField] private Sprite growingStageSprite;
    
    [Header("Settings")]
    [Range(0f, 1f)] public float dailySpawnChance;
    public float regrowthTime;
    public bool warmSeason;
    
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
        
        if (TimeSystem.Instance.IsCurrentSeasonWarm() == warmSeason)
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
    
    private void CheckDailySpawn()
    {
        if (TimeSystem.Instance == null) return;

        // Always reset at start of day
        SetPlantInactive();

        // Skip if wrong season
        if (TimeSystem.Instance.IsCurrentSeasonWarm() != warmSeason)
            return;

        // Roll spawn chance
        float randomValue = Random.Range(0f, 1f);
        if (randomValue <= dailySpawnChance)
        {
            SpawnPlant(); // begin the regrowth timer today
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
        return collectedWildPlant != null && isReadyForHarvest && isActive;
    }
    
    // Processes plant harvesting and gives rewards
    private void HarvestPlant(PlayerController player)
    {
        int collectAmount = Random.Range(1, 4);

        InventorySystem.Instance.AddItem(collectedWildPlant, collectAmount);
        NotificationSystem.ShowHelp($"Picked up {collectedWildPlant.newName} x{collectAmount}");
        collectedWildPlant.CollectItem(player);

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