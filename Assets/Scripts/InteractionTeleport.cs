using System;
using UnityEngine;
using DefaultNamespace;
using Unity.Cinemachine;

/// <summary>
/// Handles teleporting the player from one area to another (inside - outside the house)
/// when interacting with a door trigger.
/// Updates the Cinemachine camera confiner to match the new area.
/// </summary>

public class InteractionTeleport : MonoBehaviour, IInteractable
{
    public CinemachineConfiner2D cameraConfiner;
    public Zone targetZone;
    
    // public CinemachineConfiner2D currentConfiner;
    // public Collider2D newConfiner;
    // public Transform teleportTarget;  // object that acts like a spawn point for each region
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null)
        {
            Debug.Log("player entered the trigger!");
            controller.CurrentInteractable = this;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null)
        {
            Debug.Log("player exited the trigger!");
            controller.CurrentInteractable = null;
        }
    }

    // Called when the player interacts with this door (e.g. presses "E")
    // Teleports the player to a new region and updates the camera confiner
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
        if (playerRb == null) return;

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
        
        // since the player is teleporting the 'OnTriggerExit' method won't be able to reset this
        player.CurrentInteractable = null;
    }
}
