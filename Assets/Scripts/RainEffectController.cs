using UnityEngine;

public class RainEffectController : MonoBehaviour
{ 
    private int offsetX, offsetY;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        offsetX = 3;
        offsetY = 10;
    }
    
    void Update()
    {
        if (cam != null)
        {
            transform.position = new Vector3(
                cam.transform.position.x + offsetX,
                cam.transform.position.y + offsetY,
                transform.position.z  // Keep particle system's Z position
            );
        }
    }
}

