using UnityEngine;

/// <summary>
/// Star effect that smoothly moves from spawn position with an offset before self-destructing.
/// Useful for harvest, crafting, or other positive action feedback.
/// </summary>
public class EffectStars : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.2f;
    [SerializeField] private Vector3 offset = new Vector3(0.2f, -0.2f, 0f);

    private float timer;
    private Vector3 startPos;

    private void Awake()
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