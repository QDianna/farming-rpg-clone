using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Plot state enumeration for farming system progression.
/// </summary>
public enum PlotState
{
    Locked, Empty, Tilled, Planted, Grown
}

/// <summary>
/// Data container for individual plot information, growth tracking, and potion effects.
/// Stores all plot-specific states including infection, protection, and nourishment.
/// </summary>
public class PlotData
{
    public PlotState state;
    public ItemSeed seedData;
    public float growthTimer;
    public int currentGrowthStage;
    public bool canStartGrowing;
    public bool isNourished;
    public bool isInfected;
    public bool isProtected;
    public float nourishMultiplier = 1f;
}

/// <summary>
/// Comprehensive farming system managing plot states, crop growth, weather integration, and potion effects.
/// Handles tilling, planting, harvesting, land expansion, weather damage, disease infection, and farm protection.
/// </summary>
public class PlotlandController : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private List<Tilemap> plotlandTierTilemaps;
    [SerializeField] private Tilemap cropTilemap;
    
    [Header("Tile Assets")]
    [SerializeField] private TileBase lockedTile;
    [SerializeField] private TileBase emptyTile;
    [SerializeField] private TileBase tilledTile;
    
    private readonly Dictionary<Vector3Int, PlotData> plotStates = new();
    private bool farmProtected;
    
    private void Awake()
    {
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
    
    // Validates if player can till at position (empty plots or infected plants)
    public bool CanTill(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        return IsValidPlotAction(tilePos, PlotState.Empty) || 
               (plotStates.ContainsKey(tilePos) && plotStates[tilePos].isInfected);
    }
    
    public bool CanPlant(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        return IsValidPlotAction(tilePos, PlotState.Tilled);
    }
    
    // Validates harvest action (grown and not infected)
    public bool CanHarvest(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        return IsValidPlotAction(tilePos, PlotState.Grown) && 
               !plotStates[tilePos].isInfected;
    }
    
    public bool CanAttendPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        return plotStates.ContainsKey(tilePos) &&
               plotStates[tilePos].state == PlotState.Planted &&
               !plotStates[tilePos].canStartGrowing;
    }
    
    // Tills plot or destroys infected plants
    public void TillPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        
        if (!plotStates.TryGetValue(tilePos, out var plotData)) 
            return;
        
        // Handle infected plants - player loses them
        if (plotData.isInfected)
        {
            ClearPlotAndSetTilled(tilePos, plotData);
            return;
        }
        
        // Normal tilling for empty plots
        if (plotData.state == PlotState.Empty)
        {
            plotData.state = PlotState.Tilled;
            UpdateTilemapTile(tilePos, tilledTile);
        }
    }
    
    public void PlantPlot(InventoryItem seed, Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        var seedItem = seed as ItemSeed;
        
        if (seedItem == null || !plotStates.ContainsKey(tilePos)) 
            return;
        
        SetupPlantedPlot(tilePos, seedItem);
        DisplayCropStage(tilePos, seedItem, 0);
        
        // Apply protection to new plants if farm is currently protected
        if (farmProtected)
        {
            plotStates[tilePos].isProtected = true;
        }
    }
    
    // Applies strength potion effect to planted crop
    public void AttendPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        
        if (!plotStates.TryGetValue(tilePos, out var plotData)) 
            return;

        plotData.growthTimer = 0;
        plotData.currentGrowthStage = 1;
        plotData.canStartGrowing = true;
        
        DisplayCropStage(tilePos, plotData.seedData, 1);
    }
    
    public void HarvestPlot(Vector3 worldPos, PlayerController player)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        
        if (!plotStates.TryGetValue(tilePos, out var plotData)) 
            return;

        GiveHarvestRewards(plotData, player);
        ResetPlotToEmpty(tilePos, plotData);
    }
    
    public void UnlockPlotland(Tilemap tilemap)
    {
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(pos);
            
            if (tile == lockedTile && IsValidPlotAction(pos, PlotState.Locked))
            {
                plotStates[pos].state = PlotState.Empty;
                tilemap.SetTile(pos, emptyTile);
            }
        }
    }
    
    // Applies a nourish potion effect to specific plot
    public void ApplyNourishEffect(Vector3 worldPos, float multiplier)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
    
        if (!plotStates.TryGetValue(tilePos, out var plotData)) 
            return;
    
        plotData.isNourished = true;
        plotData.nourishMultiplier = multiplier;
    }
    
    // Applies power potion protection to entire farm until next weather event
    public void ApplyFarmProtection()
    {
        farmProtected = true;
        
        foreach (var kvp in plotStates)
        {
            if (kvp.Value.state == PlotState.Planted || kvp.Value.state == PlotState.Grown)
            {
                kvp.Value.isProtected = true;
            }
        }
        
        NotificationSystem.ShowDialogue("Farm-wide protection active! " +
                                        "The land is shielded against the next corruption event.", 2f);
        
        Debug.Log("[PlotlandController] Farm protection applied");
    }
    
    // Heals all infected plants across the farm with heal potion
    public void HealAllInfectedPlants()
    {
        int healedCount = 0;
        
        foreach (var kvp in plotStates)
        {
            var plotData = kvp.Value;
            if (plotData.isInfected)
            {
                plotData.isInfected = false;
                healedCount++;
                
                if (plotData.seedData != null && plotData.currentGrowthStage >= 0)
                {
                    DisplayCropStage(kvp.Key, plotData.seedData, plotData.currentGrowthStage);
                }
            }
        }

        if (healedCount > 0)
            NotificationSystem.ShowDialogue($"Healed {healedCount} infected plants! " +
                                            $"The corruption has been cleansed.", 2f);
        else
            NotificationSystem.ShowHelp("No infected plants found to heal.");
    }
    
    // Removes farm protection after it has been used
    private void RemoveFarmProtection()
    {
        farmProtected = false;
        
        foreach (var plotData in plotStates.Values)
        {
            plotData.isProtected = false;
        }
        
        NotificationSystem.ShowHelp("Farm protection has been consumed.");
        Debug.Log("[PlotlandController] Farm protection consumed by weather event");
    }
    
    // Scans all tilemaps and creates plot data entries
    private void InitializePlotData()
    {
        plotStates.Clear();
        
        foreach (var tilemap in plotlandTierTilemaps)
        {
            ScanTilemapForPlots(tilemap);
        }
    }
    
    private void ScanTilemapForPlots(Tilemap tilemap)
    {
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(pos);
            if (tile != null)
            {
                var plotData = CreatePlotDataFromTile(tile);
                plotStates[pos] = plotData;
            }
        }
    }
    
    private PlotData CreatePlotDataFromTile(TileBase tile)
    {
        var plotData = new PlotData();
        
        if (tile == lockedTile)
            plotData.state = PlotState.Locked;
        else if (tile == emptyTile)
            plotData.state = PlotState.Empty;
        else if (tile == tilledTile)
            plotData.state = PlotState.Tilled;
        
        return plotData;
    }
    
    private void SubscribeToWeatherEvents()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.OnStorm += HandleStorm;
            WeatherSystem.Instance.OnFreeze += HandleFreeze;
            WeatherSystem.Instance.OnDisease += HandleDisease;
        }
    }
    
    private void UnsubscribeFromWeatherEvents()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.OnStorm -= HandleStorm;
            WeatherSystem.Instance.OnFreeze -= HandleFreeze;
            WeatherSystem.Instance.OnDisease -= HandleDisease;
        }
    }
    
    // Handles storm weather event with protection checks
    private void HandleStorm()
    {
        if (farmProtected)
        {
            NotificationSystem.ShowHelp("Power Potion shields the land from the storm's corruption!");
            RemoveFarmProtection(); // Consume the protection
            return;
        }
    
        var plantedPlots = GetPlantedPlots();
        if (plantedPlots.Count == 0) return;

        int plantsToDestroy = Random.Range(1, Mathf.Min(3, plantedPlots.Count + 1));
    
        for (int i = 0; i < plantsToDestroy; i++)
        {
            DestroyRandomPlot(plantedPlots);
        }
    
        if (plantsToDestroy > 0)
            NotificationSystem.ShowDialogue($"Corrupted storm destroyed {plantsToDestroy} crops!", 2f);
    }

    // Handles freeze weather event with protection checks  
    private void HandleFreeze()
    {
        if (farmProtected)
        {
            NotificationSystem.ShowHelp("Power Potion shields the crops from the unnatural cold!");
            RemoveFarmProtection(); // Consume the protection
            return;
        }
    
        var plantedPlots = GetPlantedPlots();
        if (plantedPlots.Count == 0) return;

        int plantsToFreeze = Random.Range(1, Mathf.Min(2, plantedPlots.Count + 1));
    
        for (int i = 0; i < plantsToFreeze; i++)
        {
            FreezeRandomPlot(plantedPlots);
        }
    
        if (plantsToFreeze > 0)
            NotificationSystem.ShowDialogue($"Corrupted frost damaged {plantsToFreeze} crops!", 2f);
    }

    // Handles disease outbreak, infecting 30-60% of planted crops
    private void HandleDisease()
    {
        var plantedPlots = GetPlantedPlots();
    
        if (plantedPlots.Count == 0) 
            return;

        int plantsToInfect = Random.Range(
            Mathf.RoundToInt(plantedPlots.Count * 0.3f), 
            Mathf.RoundToInt(plantedPlots.Count * 0.6f) + 1
        );
    
        plantsToInfect = Mathf.Max(1, plantsToInfect);
    
        for (int i = 0; i < plantsToInfect && plantedPlots.Count > 0; i++)
        {
            InfectRandomPlot(plantedPlots);
        }
    
        NotificationSystem.ShowDialogue($"Mysterious blight infected {plantsToInfect} crops!", 2f);
    }
    
    // Freezes random plot, reducing growth stage or destroying early crops
    private void FreezeRandomPlot(List<Vector3Int> plantedPlots)
    {
        if (plantedPlots.Count == 0) return;
        
        int randomIndex = Random.Range(0, plantedPlots.Count);
        Vector3Int plotPos = plantedPlots[randomIndex];
        plantedPlots.RemoveAt(randomIndex);
        
        var plotData = plotStates[plotPos];
        
        if (plotData.currentGrowthStage > 1)
        {
            plotData.currentGrowthStage = Mathf.Max(1, plotData.currentGrowthStage - 1);
            plotData.growthTimer *= 0.5f;
            DisplayCropStage(plotPos, plotData.seedData, plotData.currentGrowthStage);
        }
        else
        {
            ResetPlotFromStorm(plotPos, plotData);
        }
    }
    
    // Infects random plot and displays sick sprite
    private void InfectRandomPlot(List<Vector3Int> plantedPlots)
    {
        if (plantedPlots.Count == 0) return;
        
        int randomIndex = Random.Range(0, plantedPlots.Count);
        Vector3Int plotPos = plantedPlots[randomIndex];
        plantedPlots.RemoveAt(randomIndex);
        
        var plotData = plotStates[plotPos];
        plotData.isInfected = true;
        
        if (plotData.seedData?.sickStageTile != null && cropTilemap != null)
        {
            cropTilemap.SetTile(plotPos, plotData.seedData.sickStageTile);
        }
    }
    
    // Clears infected plant and resets to tilled state
    private void ClearPlotAndSetTilled(Vector3Int tilePos, PlotData plotData)
    {
        if (cropTilemap != null)
            cropTilemap.SetTile(tilePos, null);
        
        plotData.state = PlotState.Tilled;
        plotData.seedData = null;
        plotData.growthTimer = 0;
        plotData.currentGrowthStage = 0;
        plotData.canStartGrowing = false;
        plotData.isInfected = false;
        plotData.isNourished = false;
        plotData.nourishMultiplier = 1f;
        
        UpdateTilemapTile(tilePos, tilledTile);
    }
    
    private Vector3Int GetTilePosition(Vector3 worldPos)
    {
        return plotlandTierTilemaps[0].WorldToCell(worldPos);
    }
    
    private bool IsValidPlotAction(Vector3Int tilePos, PlotState requiredState)
    {
        return plotStates.ContainsKey(tilePos) && plotStates[tilePos].state == requiredState;
    }
    
    private void UpdateTilemapTile(Vector3Int tilePos, TileBase newTile)
    {
        foreach (var tilemap in plotlandTierTilemaps)
        {
            if (tilemap.GetTile(tilePos) != null)
            {
                tilemap.SetTile(tilePos, newTile);
                break;
            }
        }
    }
    
    private void SetupPlantedPlot(Vector3Int tilePos, ItemSeed seedItem)
    {
        var plotData = plotStates[tilePos];
        plotData.state = PlotState.Planted;
        plotData.seedData = seedItem;
        plotData.currentGrowthStage = 0;
        plotData.canStartGrowing = false;
    }
    
    private void DisplayCropStage(Vector3Int tilePos, ItemSeed seedData, int stage)
    {
        if (cropTilemap != null && seedData != null && stage < seedData.growthStageTiles.Count)
        {
            cropTilemap.SetTile(tilePos, seedData.growthStageTiles[stage]);
        }
    }
    
    private void UpdateCropGrowth()
    {
        float growthModifier = WeatherSystem.Instance?.GetGrowthModifier() ?? 1f;
        
        foreach (var kvp in plotStates)
        {
            UpdateIndividualCropGrowth(kvp.Key, kvp.Value, growthModifier);
        }
    }
    
    // Updates individual crop growth, infected plants don't grow
    private void UpdateIndividualCropGrowth(Vector3Int tilePos, PlotData plotData, float growthModifier)
    {
        if (plotData.state != PlotState.Planted || plotData.seedData == null || !plotData.canStartGrowing)
            return;
        
        if (plotData.isInfected) return;

        plotData.growthTimer += Time.deltaTime * growthModifier;
        
        int newStage = CalculateGrowthStage(plotData);
        if (newStage > plotData.currentGrowthStage)
        {
            AdvanceCropToStage(tilePos, plotData, newStage);
        }
    }
    
    private int CalculateGrowthStage(PlotData plotData)
    {
        float growthPercent = Mathf.Clamp01(plotData.growthTimer / plotData.seedData.growthTime);
        int maxStage = plotData.seedData.growthStageTiles.Count - 1;
        return Mathf.FloorToInt(growthPercent * maxStage);
    }
    
    private void AdvanceCropToStage(Vector3Int tilePos, PlotData plotData, int newStage)
    {
        var stageTile = plotData.seedData.GetStageTile(newStage);
        if (stageTile == null) return;

        plotData.currentGrowthStage = newStage;
        
        if (cropTilemap != null)
            cropTilemap.SetTile(tilePos, stageTile);

        int maxStage = plotData.seedData.growthStageTiles.Count - 1;
        if (newStage == maxStage)
            plotData.state = PlotState.Grown;
    }
    
    // Simulates plant growth for the time period when player sleeps
    public void SimulateGrowthDuringSleep(float timeInSeconds)
    {
        float growthModifier = WeatherSystem.Instance?.GetGrowthModifier() ?? 1f;
    
        foreach (var kvp in plotStates)
        {
            var plotData = kvp.Value;
        
            if (plotData.state != PlotState.Planted || plotData.seedData == null || !plotData.canStartGrowing)
                continue;
        
            if (plotData.isInfected) continue; // Infected plants don't grow
        
            // Apply the time skip to growth timer
            plotData.growthTimer += timeInSeconds * growthModifier;
        
            // Update to appropriate growth stage
            int newStage = CalculateGrowthStage(plotData);
            if (newStage > plotData.currentGrowthStage)
            {
                AdvanceCropToStage(kvp.Key, plotData, newStage);
            }
        }
    
        Debug.Log($"[PlotlandController] Simulated {timeInSeconds/3600f:F1} hours of plant growth during sleep");
    }
    
    private List<Vector3Int> GetPlantedPlots()
    {
        var plantedPlots = new List<Vector3Int>();
        
        foreach (var kvp in plotStates)
        {
            if (kvp.Value.state == PlotState.Planted || kvp.Value.state == PlotState.Grown)
                plantedPlots.Add(kvp.Key);
        }
        
        return plantedPlots;
    }
    
    private void DestroyRandomPlot(List<Vector3Int> plantedPlots)
    {
        int randomIndex = Random.Range(0, plantedPlots.Count);
        Vector3Int plotPos = plantedPlots[randomIndex];
        plantedPlots.RemoveAt(randomIndex);
        
        var plotData = plotStates[plotPos];
        ResetPlotFromStorm(plotPos, plotData);
    }
    
    private void ResetPlotFromStorm(Vector3Int plotPos, PlotData plotData)
    {
        plotData.state = PlotState.Tilled;
        plotData.seedData = null;
        plotData.growthTimer = 0;
        plotData.currentGrowthStage = 0;
        plotData.canStartGrowing = false;
        
        if (cropTilemap != null)
            cropTilemap.SetTile(plotPos, null);
        
        UpdateTilemapTile(plotPos, tilledTile);
    }
    
    // Gives harvest rewards with nourish potion bonuses
    private void GiveHarvestRewards(PlotData plotData, PlayerController player)
    {
        var cropItem = plotData.seedData.resultedCrop;
        int seedAmount = Random.Range(1, 3);
        int cropAmount = Random.Range(1, 3);
        
        if (plotData.isNourished)
        {
            seedAmount = Mathf.RoundToInt(seedAmount * plotData.nourishMultiplier);
            cropAmount = Mathf.RoundToInt(cropAmount * plotData.nourishMultiplier);
            NotificationSystem.ShowDialogue($"Nourish potion bonus: +{(plotData.nourishMultiplier - 1f) * 100f:F0}% " +
                                            $"yield!", 1f);
        }
        
        player.inventorySystem.AddItem(plotData.seedData, seedAmount);
        player.inventorySystem.AddItem(cropItem, cropAmount);
        cropItem.CollectItem(player);
    }
    
    // Resets plot to empty state and clears all potion effects
    private void ResetPlotToEmpty(Vector3Int tilePos, PlotData plotData)
    {
        if (cropTilemap != null)
            cropTilemap.SetTile(tilePos, null);

        UpdateTilemapTile(tilePos, emptyTile);

        plotData.state = PlotState.Empty;
        plotData.seedData = null;
        plotData.growthTimer = 0;
        plotData.currentGrowthStage = 0;
        plotData.canStartGrowing = false;
        plotData.isNourished = false;
        plotData.nourishMultiplier = 1f;
        plotData.isInfected = false;
        plotData.isProtected = false;
    }
}