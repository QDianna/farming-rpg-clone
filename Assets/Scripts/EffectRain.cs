using UnityEngine;

/// <summary>
/// Rain effect that follows the camera with a fixed offset to create seamless weather coverage.
/// </summary>
public class EffectRain : MonoBehaviour
{ 
    [SerializeField] private Vector2 offset = new Vector2(3, 10);
    
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }
    
    private void Update()
    {
        if (cam != null)
        {
            transform.position = new Vector3(
                cam.transform.position.x + offset.x,
                cam.transform.position.y + offset.y,
                transform.position.z
            );
        }
    }
}