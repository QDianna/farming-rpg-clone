using UnityEngine;

/// <summary>
/// Represents a modular gameplay area (zone) within the world.
/// 
/// Contains metadata necessary for transitioning into and confining the player and camera inside a specific region.
/// 
/// Fields:
/// - zoneName: identifier used for saving, loading, or referencing the zone
/// - cameraBounds: defines the Cinemachine confiner area
/// - defaultSpawnPoint: where the player appears upon entering the zone
/// 
/// Design Considerations:
/// - Acts as a self-contained data object for teleportation, camera control, and future scene transitions
/// - Enables zone-based world architecture with flexible setup and low coupling
/// 
/// Used by InteractionTeleport and other systems to manage clean and scalable area transitions.
/// </summary>

public class Zone : MonoBehaviour
{
    public string zoneName;
    public Collider2D cameraBounds;
    public Transform defaultSpawnPoint;
}
