using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central controller for player behavior, including movement, interaction, inventory handling,
/// tool usage, and animation updates. Uses Unity's Input System for modular input mapping.
/// 
/// The class also manages interaction logic with the environment through two mechanisms:
/// 1. Interface-based interaction (IInteractable) for objects like doors or buyable plots
/// 2. Direct interaction with the tilemap (e.g. harvesting crops) without using IInteractable
///    to optimize performance and avoid complex collider management.
///
/// The system supports modular inventory item usage (via ScriptableObjects), tool actions,
/// and a scalable interaction pipeline for extensibility.
/// </summary>

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private InputAction MoveAction;
    private Rigidbody2D rigidbody2d;
    private Vector2 move;
    private Vector2 moveDirection = new Vector2(0, -1);
    public float speed = 6.0f;  // units per second
    
    [Header("Animation")] 
    [HideInInspector] public Animator animator;

    [Header("Interaction")]
    [SerializeField] private InputAction InteractAction;
    [HideInInspector] public InteractionSystem interactionSystem;
    
    [Header("Tool")]
    [SerializeField] private InputAction ToolAction;
    private ToolSystem toolSystem;

    [Header("Inventory")]
    [SerializeField] private InputAction InventoryAction;
    [SerializeField] private InputAction UseItemAction;
    [HideInInspector] public InventorySystem inventorySystem;
    
    [Header("Farming")]
    public PlotlandController plotlandController;

    [Header("Stats")]
    [HideInInspector] public PlayerStats playerStats;
    
    // debug / starter items
    public InventoryItem seed1;
    public InventoryItem seed2;
    public InventoryItem seed3;
    
    
    #region Unity Methods
    
    private void Start()
    {
        MoveAction.Enable();
        InteractAction.Enable();
        ToolAction.Enable();
        InventoryAction.Enable();
        UseItemAction.Enable();
        
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        toolSystem = GetComponent<ToolSystem>();
        inventorySystem = GetComponent<InventorySystem>();
        interactionSystem = GetComponent<InteractionSystem>();
        playerStats = GetComponent<PlayerStats>();
        
        inventorySystem.AddItem(seed1, 5);
        inventorySystem.AddItem(seed2, 3);
        inventorySystem.AddItem(seed3, 3);
    }

    private void Update()
    {
        move = MoveAction.ReadValue<Vector2>();

        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y,0.0f))
        {
            moveDirection.Set(move.x, move.y);
            moveDirection.Normalize();
        }
        
        // update player animation and tools facing direction
        animator.SetFloat("Move X", moveDirection.x);
        animator.SetFloat("Move Y", moveDirection.y);
        animator.SetFloat("Speed", move.magnitude);
        
        HandleInteractInput();  // check if the player is interacting with IInteractables or Tilemaps
        HandleToolInput();  // check if the player is using tools
        HandleInventoryInput();  // check if the player is selecting items from inventory
        HandleUseItemInput();  // check if the player is using an inventory item
    }

    private void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position + speed * Time.deltaTime * move;
        rigidbody2d.MovePosition(position);
    }
    
    #endregion

    
    #region Input Handling

    private void HandleInteractInput()
    {
        if (InteractAction.triggered)
            interactionSystem.TryInteract(this);
    }

    private void HandleToolInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            toolSystem.SetTool(1);
        
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            toolSystem.SetTool(2);
        
        else if (Input.GetKeyDown(KeyCode.Alpha0))
            toolSystem.SetTool(0);
        
        if (ToolAction.triggered)
        {
            toolSystem.UseTool(this);
        }
    }
    
    private void HandleInventoryInput()
    {
        if (InventoryAction.triggered)
            inventorySystem.GetNextItem();
    }
    
    private void HandleUseItemInput()
    {
        if (UseItemAction.triggered)
            inventorySystem.UseCurrentItem(this);
    }
    
    #endregion
    
}
