using UnityEngine;

/// <summary>
/// Zone configuration for area transitions and camera management.
/// Defines teleportation spawn points and camera boundary constraints.
/// </summary>
public class Zone : MonoBehaviour
{
    [Header("Zone Configuration")]
    public string zoneName;
    public Collider2D cameraBounds;
    public Transform defaultSpawnPoint;
}