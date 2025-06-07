using UnityEngine;

public class TreeController : MonoBehaviour
{
    [Header("Tree Settings")]
    public int smallTreeWood = 3;
    public int bigTreeWood = 5;
    public int maxHealth = 3;
    public float trunkToSmallTime;
    public float smallToBigTime;

    [Header("References")]
    [SerializeField] private ItemResource wood;
    [SerializeField] private Sprite smallTree;
    [SerializeField] private Sprite bigTree;
    [SerializeField] private Sprite trunk;
    
    [HideInInspector] public int currentHealth;
    [HideInInspector] public bool isChopped = false;
    [HideInInspector] public int stage; // 0 = small, 1 = big, 2 = trunk
    
    private SpriteRenderer currentStageSprite;
    private PlayerController player;
    private float growthTimer = 0f;
    private bool isGrowing = false;
    
    void Start()
    {
        // Find player in scene
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.GetComponent<PlayerController>();

        currentStageSprite = GetComponent<SpriteRenderer>();
        
        currentHealth = maxHealth;
        
        // Random initial spawn: small or big tree
        stage = Random.Range(0, 2);
        UpdateSprite();
        
        // If spawned as small tree, start growing to big
        if (stage == 0)
        {
            StartGrowth();
        }
    }
    
    void Update()
    {
        if (isGrowing)
        {
            growthTimer += Time.deltaTime;
            
            // Trunk -> Small Tree
            if (stage == 2 && growthTimer >= trunkToSmallTime)
            {
                GrowToStage(0); // Grow to small tree
            }
            // Small Tree -> Big Tree
            else if (stage == 0 && growthTimer >= smallToBigTime)
            {
                GrowToStage(1); // Grow to big tree
                StopGrowth(); // Big tree is final stage
            }
        }
    }
    
    public void TakeDamage(int damage = 1)
    {
        if (isChopped) return;
        
        currentHealth -= damage;
        StartCoroutine(ShakeTree());
        
        if (currentHealth <= 0)
        {
            ChopDown();
        }
    }
    
    void ChopDown()
    {
        isChopped = true;
        
        // Give wood based on current tree size
        int woodToGive = (stage == 0) ? smallTreeWood : bigTreeWood;
        
        if (wood && player)
        {
            InventorySystem.Instance.AddItem(wood, woodToGive);
            wood.CollectItem(player);
        }

        // Always become trunk after chopping
        stage = 2;
        UpdateSprite();
        
        // Start regrowth cycle: trunk -> small -> big
        StartGrowth();
    }
    
    void StartGrowth()
    {
        isGrowing = true;
        growthTimer = 0f;
        isChopped = false; // Can be chopped again once it starts growing
    }
    
    void StopGrowth()
    {
        isGrowing = false;
        growthTimer = 0f;
    }
    
    void GrowToStage(int newStage)
    {
        stage = newStage;
        UpdateSprite();
        growthTimer = 0f; // Reset timer for next growth stage
        currentHealth = maxHealth; // Restore health when growing
        
        // Optional: Play growth effect
        StartCoroutine(GrowthEffect());
    }
    
    void UpdateSprite()
    {
        switch (stage)
        {
            case 0: // Small tree
                currentStageSprite.sprite = smallTree;
                break;
            case 1: // Big tree
                currentStageSprite.sprite = bigTree;
                break;
            case 2: // Trunk
                currentStageSprite.sprite = trunk;
                break;
        }
    }
    
    System.Collections.IEnumerator ShakeTree()
    {
        Vector3 originalPos = transform.position;
        float shakeAmount = 0.1f;
        float shakeTime = 0.2f;
        
        float elapsed = 0f;
        while (elapsed < shakeTime)
        {
            float x = originalPos.x + Random.Range(-shakeAmount, shakeAmount);
            transform.position = new Vector3(x, originalPos.y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPos;
    }
    
    System.Collections.IEnumerator GrowthEffect()
    {
        // Scale-up effect from 50% to 100% when growing
        Vector3 originalScale = transform.localScale;
        Vector3 startScale = originalScale * 0.8f;
        transform.localScale = startScale;
        
        float growTime = 0.8f;
        float elapsed = 0f;
        
        while (elapsed < growTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / growTime;
            transform.localScale = Vector3.Lerp(startScale, originalScale, progress);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
}