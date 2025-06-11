using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Zone teleporting with seamless camera transitions and automatic bounds updating.
/// Handles player positioning and prevents camera collision issues through forced snapping.
/// </summary>
public class InteractionTeleport : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    [SerializeField] private CinemachineConfiner2D cameraConfiner;
    [SerializeField] private Zone targetZone;
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
            InteractionSystem.Instance.SetCurrentInteractable(this);
        NotificationSystem.ShowNotification("Press E to use go through!");
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
            InteractionSystem.Instance.SetCurrentInteractable(null);
    }
    
    public void Interact(PlayerController player)
    {
        if (!CanTeleport())
            return;

        var playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null) 
            return;

        PerformTeleport(playerRb, player.transform);
    }
    
    // Validates teleport requirements
    private bool CanTeleport()
    {
        return targetZone?.defaultSpawnPoint != null && 
               targetZone?.cameraBounds != null && 
               cameraConfiner != null;
    }
    
    // Executes teleport with camera handling
    private void PerformTeleport(Rigidbody2D playerRb, Transform playerTransform)
    {
        Vector3 oldPosition = playerRb.transform.position;
        Vector3 newPosition = targetZone.defaultSpawnPoint.position;

        // Move player to new zone
        playerRb.position = newPosition;
        
        UpdateCameraForTeleport(oldPosition, newPosition, playerTransform);
        ShowTeleportNotification();
        
        InteractionSystem.Instance.SetCurrentInteractable(null);
    }
    
    // Handles camera positioning and bounds updates
    private void UpdateCameraForTeleport(Vector3 oldPosition, Vector3 newPosition, Transform playerTransform)
    {
        var virtualCamera = cameraConfiner.GetComponent<CinemachineCamera>();
        if (virtualCamera == null) 
            return;

        // Force camera to snap to new position to prevent bounds collision
        Vector3 cameraPosition = new Vector3(newPosition.x, newPosition.y, virtualCamera.transform.position.z);
        virtualCamera.transform.position = cameraPosition;
        
        // Update camera bounds for new zone
        cameraConfiner.BoundingShape2D = targetZone.cameraBounds;
        cameraConfiner.InvalidateBoundingShapeCache();

        // Notify Cinemachine about the teleport to prevent interpolation
        Vector3 teleportDelta = newPosition - oldPosition;
        virtualCamera.OnTargetObjectWarped(playerTransform, teleportDelta);
    }
    
    // Shows zone entry notification
    private void ShowTeleportNotification()
    {
        NotificationSystem.ShowNotification($"You entered {targetZone.zoneName}");
    }
}