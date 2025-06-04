using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Teleports players between zones with seamless camera transitions.
/// Handles door/entrance interactions with Cinemachine camera bound updates.
/// </summary>
public class InteractionTeleport : MonoBehaviour, IInteractable
{
    [SerializeField] private CinemachineConfiner2D cameraConfiner;
    [SerializeField] private Zone targetZone;
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
        }
    }

    public void Interact(PlayerController player)
    {
        if (targetZone?.defaultSpawnPoint == null || targetZone?.cameraBounds == null || cameraConfiner == null)
        {
            NotificationSystem.ShowNotification("This door is currently unavailable");
            return;
        }

        var playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        Vector3 oldPos = playerRb.transform.position;
        Vector3 newPos = targetZone.defaultSpawnPoint.position;

        // Move player
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

        NotificationSystem.ShowNotification($"Entered {targetZone.zoneName}");
        
        // Reset interaction since player teleported
        InteractionSystem.Instance.SetCurrentInteractable(null);
    }
}