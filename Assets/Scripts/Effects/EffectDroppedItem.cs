using UnityEngine;

/// <summary>
/// Handles visual pickup animation for dropped items.
/// Items move toward the target player and self-destruct upon reaching them.
/// </summary>
public class EffectDroppedItem : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float initialSpeed = 1f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float destroyDistance = 0.1f;

    private float currentSpeed;
    private PlayerController targetPlayer;
    private bool isInitialized;

    public void Initialize(PlayerController player)
    {
        targetPlayer = player;
        isInitialized = true;
        currentSpeed = initialSpeed;
    }

    private void FixedUpdate()
    {
        if (!isInitialized || targetPlayer == null)
            return;

        Accelerate();
        MoveTowardTarget();
        CheckForDestruction();
    }

    private void Accelerate()
    {
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);
    }

    private void MoveTowardTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPlayer.transform.position,
            currentSpeed * Time.fixedDeltaTime);
    }

    private void CheckForDestruction()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.transform.position);

        if (distanceToTarget <= destroyDistance)
        {
            Destroy(gameObject);
        }
    }
}