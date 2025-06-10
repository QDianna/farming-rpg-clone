using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Teleports players between zones with seamless camera transitions.
/// Handles door/entrance interactions and updates Cinemachine camera bounds automatically.
/// Forces camera to snap to player position to avoid bounds collision issues.
/// </summary>
public class InteractionTeleport : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    [SerializeField] private CinemachineConfiner2D cameraConfiner;
    [SerializeField] private Zone targetZone;
    
    public void Interact(PlayerController player)
    {
        if (targetZone?.defaultSpawnPoint == null || targetZone?.cameraBounds == null || cameraConfiner == null)
        {
            Debug.Log("ERROR - spawn, camera bounds or camera confiner are null");
            return;
        }

        var playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        Vector3 oldPos = playerRb.transform.position;
        Vector3 newPos = targetZone.defaultSpawnPoint.position;

        // Move player to new zone
        playerRb.position = newPos;
        
        // Force camera to follow immediately (prevents bounds collision)
        var vcam = cameraConfiner.GetComponent<CinemachineCamera>();
        if (vcam != null)
        {
            Vector3 cameraPos = new Vector3(newPos.x, newPos.y, vcam.transform.position.z);
            vcam.transform.position = cameraPos;
        }
        
        // Update camera bounds for new zone
        cameraConfiner.BoundingShape2D = targetZone.cameraBounds;
        cameraConfiner.InvalidateBoundingShapeCache();

        // Tell Cinemachine this was a teleport, not normal movement
        if (vcam != null)
        {
            Vector3 delta = newPos - oldPos;
            vcam.OnTargetObjectWarped(player.transform, delta);
        }

        NotificationSystem.ShowNotification($"You entered {targetZone.zoneName}");
        InteractionSystem.Instance.SetCurrentInteractable(null);
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
            InteractionSystem.Instance.SetCurrentInteractable(this);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
            InteractionSystem.Instance.SetCurrentInteractable(null);
    }
}