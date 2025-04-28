using UnityEngine;

/// <summary>
/// Represents a game zone with a spawn point and camera bounds.
/// </summary>
/// 
public class Zone : MonoBehaviour
{
    public string zoneName;
    public Collider2D cameraBounds;
    public Transform defaultSpawnPoint;
}
