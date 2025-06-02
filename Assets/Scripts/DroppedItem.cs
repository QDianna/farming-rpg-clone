using UnityEngine;

/// <summary>
/// Handles the visual behavior of a dropped item in the world.
/// 
/// When instantiated, the item moves toward the assigned PlayerController and disappears when close enough,
/// mimicking a "pickup" or "collection" effect.
/// 
/// The dropped item should be initialized via <see cref="Initialize"/> with a target player reference,
/// and the item's appearance should be configured via a SpriteRenderer before or during instantiation.
/// </summary>

public class DroppedItem : MonoBehaviour
{
    private PlayerController target;
    private float speed = 4.0f;
    private float stopDistance = 0.1f;
    
    public void Initialize(PlayerController player)
    {
        if (player != null)
            target = player;
    }
    
    void FixedUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPos = target.transform.position;
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPos, 
            speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) <= stopDistance)
        {
            // Debug.Log("Dropped item reached target");
            Destroy(gameObject);
        }
    }
}
