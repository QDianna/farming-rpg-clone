using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    private PlayerController target;
    private float speed = 2.5f;
    private float stopDistance = 0.1f;

    public void Initialize(PlayerController player)
    {
        if (player != null)
            target = player;
    }
    
    // make item move towards target and disappear once it reaches it
    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) <= stopDistance)
        {
            Debug.Log("Dropped item reached target");
            Destroy(gameObject);
        }
    }
}
