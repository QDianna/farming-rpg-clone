using UnityEngine;

/// <summary>
/// Handles visual pickup animation for dropped items.
/// Items move toward the target player and self-destruct upon reaching them.
/// </summary>
public class EffectDroppedItem : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.8f;
    [SerializeField] private float destroyDistance = 0.1f;
    
    private PlayerController targetPlayer;
    private bool isInitialized;
    
    // Sets up the pickup effect to move toward the specified player
    public void Initialize(PlayerController player)
    {
        targetPlayer = player;
        isInitialized = true;
    }
    
    private void FixedUpdate()
    {
        if (!isInitialized || targetPlayer == null) 
            return;

        MoveTowardTarget();
        CheckForDestruction();
    }
    
    // Moves the item toward the target player
    private void MoveTowardTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPlayer.transform.position, 
            moveSpeed * Time.fixedDeltaTime);
    }
    
    // Destroys the item when close enough to the target
    private void CheckForDestruction()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.transform.position);
        
        if (distanceToTarget <= destroyDistance)
        {
            Destroy(gameObject);
        }
    }
}