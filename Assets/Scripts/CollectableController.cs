using UnityEngine;

public class CollectableController : MonoBehaviour, IInteractable
{
    [Header("Collectable Settings")]
    public InventoryItem item;
    public int collectAmount = 1;
    
    [Header("Growth Settings")]
    [SerializeField] private Sprite readyStageSprite;
    [SerializeField] private Sprite growingStageSprite;
    public float regrowthTime = 60f;
    
    private SpriteRenderer spriteRenderer;
    private bool isReady = true;
    private float growthTimer = 0f;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = readyStageSprite;
    }
    
    void Update()
    {
        if (!isReady)
        {
            growthTimer += Time.deltaTime;
            
            if (growthTimer >= regrowthTime)
            {
                isReady = true;
                spriteRenderer.sprite = readyStageSprite;
            }
        }
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (isReady && other.TryGetComponent<PlayerController>(out var player))
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
        }
    }

    public void Interact(PlayerController player)
    {
        if (item == null || !isReady) return;
        
        InventorySystem.Instance.AddItem(item, collectAmount);
        NotificationSystem.ShowNotification($"Picked up {item.name} x{collectAmount}");
        item.CollectItem(player);
        
        InteractionSystem.Instance.SetCurrentInteractable(null);
        
        // Start regrowth
        isReady = false;
        growthTimer = 0f;
        spriteRenderer.sprite = growingStageSprite;
    }
}