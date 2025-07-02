using System.Collections;
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
        NotificationSystem.ShowHelp("Press E to use go through!");
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

        player.LockInputForFrames(3); // lock player so it doesnt fck the camera
        
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

    private void UpdateCameraForTeleport(Vector3 oldPosition, Vector3 newPosition, Transform playerTransform)
    {
        StartCoroutine(DeferredCameraUpdate(oldPosition, newPosition, playerTransform));
    }

    private IEnumerator DeferredCameraUpdate(Vector3 oldPos, Vector3 newPos, Transform playerTransform)
    {
        var virtualCamera = cameraConfiner.GetComponent<CinemachineCamera>();
        if (virtualCamera == null)
            yield break;

        cameraConfiner.BoundingShape2D = targetZone.cameraBounds;
        cameraConfiner.InvalidateBoundingShapeCache();

        yield return null; // confiner apply

        Vector3 teleportDelta = newPos - oldPos;
        virtualCamera.OnTargetObjectWarped(playerTransform, teleportDelta);

        yield return null; // așteaptă ca CinemachineBrain să aplice transformul

        var vcamPos = virtualCamera.transform.position;
        var mainCam = Camera.main;

        if (mainCam != null)
        {
            var mainCamPos = mainCam.transform.position;

            if (Vector3.Distance(mainCamPos, vcamPos) > 0.1f)
            {
                Debug.Log("Main Camera out of sync with Virtual Camera — forcing position");
                mainCam.transform.position = vcamPos;
            }
        }
        else
        {
            Debug.Log("ERROR Camera.main is NULL — could not verify sync");
            // yield break;
        }

    }

    
    // Shows zone entry notification
    private void ShowTeleportNotification()
    {
        NotificationSystem.ShowHelp($"You entered {targetZone.zoneName}");
    }
}