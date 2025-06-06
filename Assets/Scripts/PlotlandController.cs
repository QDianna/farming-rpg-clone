using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Plot state enumeration for farming system.
/// </summary>
public enum PlotState
{
    Locked, Empty, Tilled, Planted, Grown
}

/// <summary>
/// Data container for individual plot information.
/// </summary>
public class PlotData
{
    public PlotState state;
    public ItemSeed seedData;
    public float growthTimer;
    public int currentGrowthStage;
    public bool canStartGrowing;
}

/// <summary>
/// Manages farming plot states, crop growth, weather integration, and land expansion.
/// Handles tilling, planting, harvesting, and dynamic crop progression.
/// </summary>
public class PlotlandController : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap cropTilemap;
    [SerializeField] private List<Tilemap> expansionTilemaps;
    
    [Header("Tile Assets")]
    public TileBase lockedTile;
    public TileBase emptyTile;
    public TileBase tilledTile;
    
    private Tilemap plotTilemap;
    private Dictionary<Vector3Int, PlotData> plotStates = new();
    
    private void Awake()
    {
        plotTilemap = GetComponent<Tilemap>();
        InitializePlotData();
    }
    
    private void Start()
    {
        SubscribeToWeatherEvents();
    }
    
    private void Update()
    {
        UpdateCropGrowth();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromWeatherEvents();
    }
    
    private void InitializePlotData()
    {
        // Initialize main plotland
        foreach (Vector3Int pos in plotTilemap.cellBounds.allPositionsWithin)
        {
            if (plotTilemap.GetTile(pos) == emptyTile)
                plotStates[pos] = new PlotData { state = PlotState.Empty };
        }
        
        // Initialize expansion areas
        foreach (var expansionTilemap in expansionTilemaps)
        {
            if (expansionTilemap == null) continue;

            foreach (Vector3Int pos in expansionTilemap.cellBounds.allPositionsWithin)
            {
                if (expansionTilemap.GetTile(pos) == lockedTile)
                    plotStates[pos] = new PlotData { state = PlotState.Locked };
            }
        }
    }
    
    private void SubscribeToWeatherEvents()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.OnBeneficialRain += HandleBeneficialRain;
            WeatherSystem.Instance.OnStorm += HandleStorm;
        }
    }
    
    private void UnsubscribeFromWeatherEvents()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.OnBeneficialRain -= HandleBeneficialRain;
            WeatherSystem.Instance.OnStorm -= HandleStorm;
        }
    }
    
    public bool CanTill(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) && plotStates[tilePos].state == PlotState.Empty;
    }
    
    public bool CanPlant(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) && plotStates[tilePos].state == PlotState.Tilled;
    }
    
    public bool CanHarvest(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) && plotStates[tilePos].state == PlotState.Grown;
    }
    
    public bool CanAttendPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) &&
               plotStates[tilePos].state == PlotState.Planted &&
               plotStates[tilePos].canStartGrowing == false;
    }
    
    public void TillPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        
        plotStates[tilePos].state = PlotState.Tilled;
        plotTilemap.SetTile(tilePos, tilledTile);
    }
    
    public void PlantPlot(InventoryItem seed, Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        var seedItem = seed as ItemSeed;
        
        if (seedItem == null) return;
        
        var plotData = plotStates[tilePos];
        plotData.state = PlotState.Planted;
        plotData.seedData = seedItem;
        plotData.currentGrowthStage = 0;
        plotData.canStartGrowing = false;

        cropTilemap.SetTile(tilePos, seedItem.growthStageTiles[0]);
    }
    
    public void AttendPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        var plotData = plotStates[tilePos];
        
        plotData.growthTimer = 0;
        plotData.currentGrowthStage = 1;
        plotData.canStartGrowing = true;
        
        cropTilemap.SetTile(tilePos, plotData.seedData.growthStageTiles[1]);
    }
    
    public void HarvestPlot(Vector3 worldPos, PlayerController player)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        var data = plotStates[tilePos];
        var cropItem = data.seedData.resultedCrop;

        // Give harvest rewards
        player.inventorySystem.AddItem(data.seedData, Random.Range(1, 3));
        player.inventorySystem.AddItem(cropItem, Random.Range(1, 3));
        
        // Show harvest effect
        cropItem.DisplayCrop(worldPos, player);
        
        // Reset plot
        ResetPlotToEmpty(tilePos, data);
    }
    
    private void ResetPlotToEmpty(Vector3Int tilePos, PlotData data)
    {
        cropTilemap.SetTile(tilePos, null);
        plotTilemap.SetTile(tilePos, emptyTile);
        
        data.state = PlotState.Empty;
        data.seedData = null;
        data.growthTimer = 0;
        data.currentGrowthStage = 0;
        data.canStartGrowing = false;
    }
    
    private void UpdateCropGrowth()
    {
        float growthModifier = WeatherSystem.Instance?.GetGrowthModifier() ?? 1f;
        
        foreach (var kvp in plotStates)
        {
            var data = kvp.Value;

            if (data.state != PlotState.Planted || data.seedData == null || !data.canStartGrowing)
                continue;

            data.growthTimer += Time.deltaTime * growthModifier;

            var growthPercent = Mathf.Clamp01(data.growthTimer / data.seedData.growthTime);
            var maxStage = data.seedData.growthStageTiles.Count - 1;
            var newStage = Mathf.FloorToInt(growthPercent * maxStage);

            if (newStage <= data.currentGrowthStage) continue;

            var currentTile = data.seedData.GetStageTile(newStage);
            if (currentTile == null) continue;

            data.currentGrowthStage = newStage;
            cropTilemap.SetTile(kvp.Key, currentTile);

            if (newStage == maxStage)
            {
                data.state = PlotState.Grown;
                NotificationSystem.ShowNotification($"{data.seedData.name} is ready to harvest!");
            }
        }
    }
    
    private void HandleBeneficialRain()
    {
        foreach (var kvp in plotStates)
        {
            var data = kvp.Value;
        
            if (data.state == PlotState.Planted && data.canStartGrowing && data.seedData != null)
            {
                int maxStage = data.seedData.growthStageTiles.Count - 1;
                int newStage = Mathf.Min(data.currentGrowthStage + 1, maxStage);
            
                if (newStage > data.currentGrowthStage)
                {
                    data.currentGrowthStage = newStage;
                    cropTilemap.SetTile(kvp.Key, data.seedData.GetStageTile(newStage));
                
                    if (newStage == maxStage)
                        data.state = PlotState.Grown;
                }
            }
        }
        
        NotificationSystem.ShowNotification("Rain helped your crops grow!");
    }

    private void HandleStorm()
    {
        var plantedPlots = new List<Vector3Int>();
    
        foreach (var kvp in plotStates)
        {
            if (kvp.Value.state == PlotState.Planted || kvp.Value.state == PlotState.Grown)
                plantedPlots.Add(kvp.Key);
        }
    
        if (plantedPlots.Count == 0) return;
    
        int plantsToDestroy = Random.Range(0, Mathf.Min(2, plantedPlots.Count + 1));
    
        for (int i = 0; i < plantsToDestroy; i++)
        {
            int randomIndex = Random.Range(0, plantedPlots.Count);
            Vector3Int plotPos = plantedPlots[randomIndex];
            plantedPlots.RemoveAt(randomIndex);
        
            var plotData = plotStates[plotPos];
            plotData.state = PlotState.Tilled;
            plotData.seedData = null;
            plotData.growthTimer = 0;
            plotData.currentGrowthStage = 0;
            plotData.canStartGrowing = false;
        
            cropTilemap.SetTile(plotPos, null);
        }
        
        if (plantsToDestroy > 0)
            NotificationSystem.ShowNotification($"Storm destroyed {plantsToDestroy} crops!");
    }
    
    public bool UnlockPlot(Tilemap expansionTilemap)
    {
        bool unlockedAny = false;
        
        foreach (Vector3Int pos in expansionTilemap.cellBounds.allPositionsWithin)
        {
            var tile = expansionTilemap.GetTile(pos);
            
            if (tile == lockedTile && plotStates.ContainsKey(pos) && plotStates[pos].state == PlotState.Locked)
            {
                plotStates[pos].state = PlotState.Empty;
                plotTilemap.SetTile(pos, emptyTile);
                expansionTilemap.SetTile(pos, null);
                unlockedAny = true;
            }
        }
        
        return unlockedAny;
    }
}