using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Main player controller handling movement, interactions, tools, and inventory.
/// Uses Unity's Input System and integrates with modular game systems.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private float speed = 6.0f;
    
    [Header("Input Actions")]
    [SerializeField] private InputAction interactAction;
    [SerializeField] private InputAction toolAction;
    [SerializeField] private InputAction inventoryAction;
    [SerializeField] private InputAction useItemAction;
    
    [Header("System References")]
    public PlotlandController plotlandController;
    
    // Components (assigned in Awake)
    [HideInInspector] public Animator animator;
    [HideInInspector] public InventorySystem inventorySystem;
    [HideInInspector] public InteractionSystem interactionSystem;
    [HideInInspector] public PlayerStats playerStats;
    [HideInInspector] public PlayerEconomy playerEconomy;
    
    private Rigidbody2D rb2d;
    private ToolSystem toolSystem;
    private Vector2 move;
    private Vector2 moveDirection = Vector2.down;
    
    // Debug starter items
    [Header("Debug Items")]
    public InventoryItem seed1, seed2, seed3, strengthSolution;
    
    private void Awake()
    {
        // Get components
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        toolSystem = GetComponent<ToolSystem>();
        inventorySystem = GetComponent<InventorySystem>();
        interactionSystem = GetComponent<InteractionSystem>();
        playerStats = GetComponent<PlayerStats>();
        playerEconomy = GetComponent<PlayerEconomy>();
        
        // Enable input actions
        moveAction.Enable();
        interactAction.Enable();
        toolAction.Enable();
        inventoryAction.Enable();
        useItemAction.Enable();
    }

    private void Start()
    {
        // Add debug starter items
        if (seed1 != null) inventorySystem.AddItem(seed1, 5);
        if (seed2 != null) inventorySystem.AddItem(seed2, 3);
        if (seed3 != null) inventorySystem.AddItem(seed3, 3);
        if (strengthSolution != null) inventorySystem.AddItem(strengthSolution, 3);
    }

    private void Update()
    {
        HandleMovement();
        HandleInputs();
    }

    private void FixedUpdate()
    {
        Vector2 position = rb2d.position + speed * Time.deltaTime * move;
        rb2d.MovePosition(position);
    }
    
    private void HandleMovement()
    {
        move = moveAction.ReadValue<Vector2>();

        if (move.magnitude > 0.1f)
        {
            moveDirection = move.normalized;
        }
        
        // Update animation
        animator.SetFloat("Move X", moveDirection.x);
        animator.SetFloat("Move Y", moveDirection.y);
        animator.SetFloat("Speed", move.magnitude);
    }
    
    private void HandleInputs()
    {
        if (interactAction.triggered)
            interactionSystem.TryInteract(this);
            
        if (toolAction.triggered)
            toolSystem.UseTool(this);
            
        if (inventoryAction.triggered)
            inventorySystem.GetNextItem();
            
        if (useItemAction.triggered)
            inventorySystem.UseCurrentItem(this);
            
        HandleToolSelection();
    }
    
    private void HandleToolSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) toolSystem.SetTool(0);
        else if (Input.GetKeyDown(KeyCode.Alpha1)) toolSystem.SetTool(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) toolSystem.SetTool(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) toolSystem.SetTool(3);
    }
}