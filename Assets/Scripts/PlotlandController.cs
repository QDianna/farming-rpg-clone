using UnityEngine;
using UnityEngine.Tilemaps;

public class PlotlandController : MonoBehaviour
{
    public Tilemap plotTilemap;         // Tilemap-ul plotland-ului
    public TileBase soilNormalTile;     // tile pamant normal/nearat
    public TileBase soilTilledTile;     // tile pamand arat
    public TileBase soilPlantedTile;    // tile pamant cu samanta

    private void Awake()
    {
        if (plotTilemap == null)
        {
            plotTilemap = GetComponent<Tilemap>();
        }
    }

    public void TillPlot(Vector3 worldPosition)
    {
        Vector3Int tilePosition = plotTilemap.WorldToCell(worldPosition);
        TileBase currentTile = plotTilemap.GetTile(tilePosition);

        if (currentTile == soilNormalTile)
        {
            plotTilemap.SetTile(tilePosition, soilTilledTile);
        }
    }

    public bool CanPlant(Vector3 worldPosition)
    {
        Vector3Int tilePosition = plotTilemap.WorldToCell(worldPosition);
        TileBase currentTile = plotTilemap.GetTile(tilePosition);

        return currentTile == soilTilledTile;
    }
}
