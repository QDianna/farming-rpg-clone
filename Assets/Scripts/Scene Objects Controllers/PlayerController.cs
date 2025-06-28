using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Main player controller managing movement, interactions, tools, and inventory systems.
/// Integrates Unity Input System with modular game components for comprehensive player control.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseMovementSpeed = 6.0f;
    [SerializeField] private float speedMultiplier = 2f;
    
    [Header("Input Actions")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction interactAction;
    [SerializeField] private InputAction toolAction;
    [SerializeField] private InputAction inventoryAction;
    [SerializeField] private InputAction useItemAction;
    
    [Header("References")]
    public PlotlandController plotlandController;
    
    // components of player object
    [HideInInspector] public Animator animator;
    [HideInInspector] public Rigidbody2D playerRigidbody;
    [HideInInspector] public InteractionSystem interactionSystem;
    [HideInInspector] public ToolSystem toolSystem;
    [HideInInspector] public InventorySystem inventorySystem;
    [HideInInspector] public PlayerEconomy playerEconomy;
    [HideInInspector] public PlayerStats playerStats;
    
    // movement parameters
    [HideInInspector] public bool hasSpeedBuff;
    [HideInInspector] public float movementSpeed;
    private Vector2 currentMovement;
    private Vector2 lastMoveDirection = Vector2.down;
    
    private void Awake()
    {
        InitializeComponents();
        EnableInputActions();
    }
    
    private void Start()
    {
        movementSpeed = baseMovementSpeed;
    
        // Subscribe to day change event
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange += RemoveSpeedBuff;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange -= RemoveSpeedBuff;
        }
    }

    private void Update()
    {
        HandleMovementInput();
        HandleActionInputs();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }
    
    // Caches all required component references
    private void InitializeComponents()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        toolSystem = GetComponent<ToolSystem>();
        inventorySystem = GetComponent<InventorySystem>();
        interactionSystem = GetComponent<InteractionSystem>();
        playerStats = GetComponent<PlayerStats>();
        playerEconomy = GetComponent<PlayerEconomy>();
    }
    
    // Enables all input action maps
    private void EnableInputActions()
    {
        moveAction.Enable();
        interactAction.Enable();
        toolAction.Enable();
        inventoryAction.Enable();
        useItemAction.Enable();
    }
    
    // Processes movement input and updates animation parameters
    private void HandleMovementInput()
    {
        currentMovement = moveAction.ReadValue<Vector2>();

        if (currentMovement.magnitude > 0.1f)
        {
            lastMoveDirection = currentMovement.normalized;
        }
        
        UpdateMovementAnimation();
    }
    
    // Updates animator with current movement values
    private void UpdateMovementAnimation()
    {
        animator.SetFloat("Move X", lastMoveDirection.x);
        animator.SetFloat("Move Y", lastMoveDirection.y);
        animator.SetFloat("Speed", currentMovement.magnitude);
    }
    
    // Applies movement to rigidbody
    private void ApplyMovement()
    {
        Vector2 newPosition = playerRigidbody.position + movementSpeed * Time.fixedDeltaTime * currentMovement;
        playerRigidbody.MovePosition(newPosition);
    }
    
    // Method to apply speed buff
    public void ApplySpeedBuff(float multiplier)
    {
        speedMultiplier = multiplier;
        movementSpeed = baseMovementSpeed * speedMultiplier;
        hasSpeedBuff = true;
    
        NotificationSystem.ShowDialogue($"Speed increased by {(multiplier - 1f) * 100f:F0}% " +
                                        $"for the rest of the day!", 3f);
    }

    // Method to remove speed buff (called on day change)
    private void RemoveSpeedBuff()
    {
        if (hasSpeedBuff)
        {
            speedMultiplier = 1f;
            movementSpeed = baseMovementSpeed;
            hasSpeedBuff = false;
            NotificationSystem.ShowHelp("Speed boost expired.");
        }
    }
    
    // Processes all action inputs (interact, tool use, inventory)
    private void HandleActionInputs()
    {
        if (interactAction.triggered)
            interactionSystem.TryInteract(this);
        
        if (toolAction.triggered)
            toolSystem.UseTool(this);
        
        if (useItemAction.triggered)
            inventorySystem.UseCurrentItem(this);

        
            
        HandleInventoryNavigation();
        HandleToolSelection();
    }
    
    // Handles inventory scrolling with input action
    private void HandleInventoryNavigation()
    {
        /*if (inventoryOpenAction.triggered)
        {
            inventorySystem.ShowAllItems();
            return;
        }*/

        if (inventoryAction.triggered)
        {
            float navigationInput = inventoryAction.ReadValue<float>();
            if (navigationInput > 0.5f)
                inventorySystem.GetNextItem();
            else if (navigationInput < -0.5f)
                inventorySystem.GetPreviousItem();
        }
    }
    
    // Handles numeric key tool selection
    private void HandleToolSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) toolSystem.SetTool(0);
        else if (Input.GetKeyDown(KeyCode.Alpha1)) toolSystem.SetTool(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) toolSystem.SetTool(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) toolSystem.SetTool(3);
    }
}