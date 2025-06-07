using UnityEngine;

/// <summary>
/// Visual effect for dropped items that move toward the player and disappear when collected.
/// Initialize with a target player to activate the pickup animation.
/// </summary>
public class EffectDroppedItem : MonoBehaviour
{
    [SerializeField] private float speed = 3.8f;
    [SerializeField] private float stopDistance = 0.1f;
    
    private PlayerController target;
    
    public void Initialize(PlayerController player)
    {
        Debug.Log("dropped item init method");
        target = player;
    }
    
    private void FixedUpdate()
    {
        if (target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position, 
            target.transform.position, 
            speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.transform.position) <= stopDistance)
        {
            Destroy(gameObject);
        }
    }
}