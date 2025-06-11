using UnityEngine;

/// <summary>
/// Star particle effect that smoothly animates from spawn position with offset movement.
/// Self-destructs after specified lifetime. Used for positive feedback like harvesting or crafting.
/// </summary>
public class EffectStars : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float effectDuration = 1.2f;
    [SerializeField] private Vector3 movementOffset = new Vector3(0.2f, -0.2f, 0f);

    private float elapsedTime;
    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        UpdateAnimation();
        CheckForDestruction();
    }

    // Updates the smooth movement animation based on elapsed time
    private void UpdateAnimation()
    {
        elapsedTime += Time.deltaTime;
        
        float normalizedTime = elapsedTime / effectDuration;
        Vector3 currentOffset = movementOffset * Mathf.SmoothStep(0f, 1.2f, normalizedTime);
        
        transform.position = startPosition + currentOffset;
    }

    // Destroys the effect when lifetime expires
    private void CheckForDestruction()
    {
        if (elapsedTime >= effectDuration)
            Destroy(gameObject);
    }
}