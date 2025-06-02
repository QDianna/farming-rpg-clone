using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// A reusable interaction component that teleports the player between zones (e.g. house entrance/exit).
/// Integrates with Cinemachine to update camera bounds and positioning during the transition.
/// Implements the IInteractable interface to support modular interaction logic.
/// </summary>

public class InteractionTeleport : MonoBehaviour, IInteractable
{
    [SerializeField] private CinemachineConfiner2D cameraConfiner;
    [SerializeField] private Zone targetZone;
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("Press E to open the door!");
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("exited interaction");
            InteractionSystem.Instance.SetCurrentInteractable(null);
        }
    }

    /// <summary>
    /// Teleports the player to the specified target zone and updates the Cinemachine camera confiner accordingly.
    /// Ensures a seamless transition by adjusting the camera bounds and syncing the camera position after the move.
    /// </summary>
    
    public void Interact(PlayerController player)
    {
        if (targetZone == null || cameraConfiner == null)
        {
            Debug.LogWarning("Missing target zone or camera confiner.");
            return;
        }

        if (targetZone.defaultSpawnPoint == null || targetZone.cameraBounds == null)
        {
            Debug.LogWarning($"Zone '{targetZone.zoneName}' is missing spawn point or camera bounds.");
            return;
        }
        
        var playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null)
            return;

        Vector3 oldPos = playerRb.transform.position;
        Vector3 newPos = targetZone.defaultSpawnPoint.position;

        // Move player
        Debug.Log("teleporting player...");
        playerRb.position = newPos;
        
        // Update camera confiner
        cameraConfiner.BoundingShape2D = targetZone.cameraBounds;
        cameraConfiner.InvalidateBoundingShapeCache();

        // Notify Cinemachine about the warp
        var vcam = cameraConfiner.GetComponent<CinemachineCamera>();
        if (vcam != null)
        {
            Vector3 delta = newPos - oldPos;
            vcam.OnTargetObjectWarped(player.transform, delta);
        }

        Debug.Log($"Player teleported to zone: {targetZone.zoneName}");
        
        // since the player is teleporting the 'OnTriggerExit2D' method won't be able to reset this
        InteractionSystem.Instance.SetCurrentInteractable(null);
    }
}
