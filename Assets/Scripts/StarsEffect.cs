using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    private float lifetime = 1.2f;
    private Vector3 offset = new (0.2f, -0.2f, 0f);

    private float timer;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        float t = timer / lifetime;
        transform.position = startPos + offset * Mathf.SmoothStep(0f, 1.2f, t);

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}