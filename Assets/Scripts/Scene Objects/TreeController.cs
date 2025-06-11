using UnityEngine;
using System.Collections;

/// <summary>
/// Tree system with growth stages, health management, and resource harvesting.
/// Handles small/big tree progression, chopping mechanics, and automatic regrowth cycles.
/// </summary>
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
    [SerializeField] private Sprite smallTreeSprite;
    [SerializeField] private Sprite bigTreeSprite;
    [SerializeField] private Sprite trunkSprite;
    
    [HideInInspector] public int currentHealth;
    [HideInInspector] public bool isChopped;
    [HideInInspector] public int treeStage; // 0 = small, 1 = big, 2 = trunk
    
    private SpriteRenderer spriteRenderer;
    private PlayerController player;
    private float growthTimer;
    private bool isGrowing;
    
    private void Start()
    {
        InitializeTree();
    }
    
    private void Update()
    {
        if (isGrowing)
        {
            UpdateGrowth();
        }
    }
    
    public void TakeDamage(int damage = 1)
    {
        if (isChopped) 
            return;
        
        currentHealth -= damage;
        StartCoroutine(ShakeEffect());
        
        if (currentHealth <= 0)
        {
            ChopDownTree();
        }
    }
    
    // Sets up initial tree state and finds player reference
    private void InitializeTree()
    {
        FindPlayerReference();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        currentHealth = maxHealth;
        treeStage = Random.Range(0, 2); // Spawn as small or big tree
        UpdateTreeSprite();
        
        if (treeStage == 0)
        {
            StartGrowthCycle();
        }
    }
    
    // Finds and caches player reference
    private void FindPlayerReference()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.GetComponent<PlayerController>();
    }
    
    // Updates growth progression based on current stage
    private void UpdateGrowth()
    {
        growthTimer += Time.deltaTime;
        
        if (treeStage == 2 && growthTimer >= trunkToSmallTime)
        {
            GrowToStage(0); // Trunk -> Small tree
        }
        else if (treeStage == 0 && growthTimer >= smallToBigTime)
        {
            GrowToStage(1); // Small -> Big tree
            StopGrowthCycle(); // Big tree is final stage
        }
    }
    
    // Handles tree chopping and resource distribution
    private void ChopDownTree()
    {
        isChopped = true;
        
        int woodReward = (treeStage == 0) ? smallTreeWood : bigTreeWood;
        GiveWoodReward(woodReward);
        
        treeStage = 2; // Become trunk
        UpdateTreeSprite();
        StartGrowthCycle(); // Begin regrowth
    }
    
    // Gives wood reward to player inventory
    private void GiveWoodReward(int amount)
    {
        if (wood != null && player != null)
        {
            InventorySystem.Instance.AddItem(wood, amount);
            wood.CollectItem(player);
        }
    }
    
    // Starts the growth timer and process
    private void StartGrowthCycle()
    {
        isGrowing = true;
        growthTimer = 0f;
        isChopped = false;
    }
    
    // Stops growth progression
    private void StopGrowthCycle()
    {
        isGrowing = false;
        growthTimer = 0f;
    }
    
    // Advances tree to specified growth stage
    private void GrowToStage(int newStage)
    {
        treeStage = newStage;
        UpdateTreeSprite();
        growthTimer = 0f;
        currentHealth = maxHealth;
        
        StartCoroutine(GrowthScaleEffect());
    }
    
    // Updates sprite based on current tree stage
    private void UpdateTreeSprite()
    {
        switch (treeStage)
        {
            case 0:
                spriteRenderer.sprite = smallTreeSprite;
                break;
            case 1:
                spriteRenderer.sprite = bigTreeSprite;
                break;
            case 2:
                spriteRenderer.sprite = trunkSprite;
                break;
        }
    }
    
    // Visual shake effect when tree takes damage
    private IEnumerator ShakeEffect()
    {
        Vector3 originalPosition = transform.position;
        float shakeIntensity = 0.1f;
        float shakeDuration = 0.2f;
        
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = new Vector3(originalPosition.x + offsetX, originalPosition.y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPosition;
    }
    
    // Visual scale-up effect when tree grows to new stage
    private IEnumerator GrowthScaleEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 startScale = originalScale * 0.8f;
        transform.localScale = startScale;
        
        float effectDuration = 0.8f;
        float elapsed = 0f;
        
        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / effectDuration;
            transform.localScale = Vector3.Lerp(startScale, originalScale, progress);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
}