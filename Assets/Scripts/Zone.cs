using UnityEngine;

/// <summary>
/// Zone data container for area transitions and camera management.
/// Defines spawn points and camera bounds for teleportation systems.
/// </summary>
public class Zone : MonoBehaviour
{
    [Header("Zone Settings")]
    public string zoneName;
    public Collider2D cameraBounds;
    public Transform defaultSpawnPoint;
}