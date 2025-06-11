using UnityEngine;

/// <summary>
/// Rain particle effect that follows the camera with configurable offset.
/// Maintains seamless weather coverage by tracking camera movement.
/// </summary>
public class EffectRain : MonoBehaviour
{ 
    [Header("Position Settings")]
    [SerializeField] private Vector2 cameraOffset = new Vector2(3f, 10f);
    
    private Camera targetCamera;
    private float originalZ;

    private void Awake()
    {
        targetCamera = Camera.main;
        originalZ = transform.position.z;
        
        if (targetCamera == null)
            Debug.LogWarning("EffectRain: No main camera found. Rain effect will not follow camera.");
    }
    
    private void LateUpdate()
    {
        if (targetCamera == null) 
            return;

        FollowCamera();
    }
    
    // Updates position to follow the camera with the specified offset
    private void FollowCamera()
    {
        Vector3 cameraPosition = targetCamera.transform.position;
        
        transform.position = new Vector3(
            cameraPosition.x + cameraOffset.x,
            cameraPosition.y + cameraOffset.y,
            originalZ
        );
    }
}