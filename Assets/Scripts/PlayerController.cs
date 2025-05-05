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
    [Header("Movement Settings")]
    [SerializeField] private InputAction MoveAction;
    private Rigidbody2D rigidbody2d;
    private Vector2 move;
    private Vector2 moveDirection = new Vector2(0, -1);
    private float unitsPerSecond = 4.0f;

    [Header("Interaction Settings")]
    [SerializeField] private InputAction InteractAction;
    private IInteractable currentInteractable = null;   // for the IInteractable interface
    public IInteractable CurrentInteractable
    {
        get => currentInteractable;
        set => currentInteractable = value;
    }
    
    [Header("Tool Settings")]
    [SerializeField] private InputAction ToolAction;
    [SerializeField] private Transform toolPivot;       // for positioning and animation of tools
    private ToolSystem tools;

    [Header("Animation Settings")] 
    private Animator animator;

    [Header("Inventory Settings")]
    [SerializeField] private InputAction InventoryAction;
    [SerializeField] private InputAction UseItemAction;
    [HideInInspector] public InventorySystem inventory;
    
    public PlotlandController plotlandController;
    
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
        tools = GetComponent<ToolSystem>();
        inventory = GetComponent<InventorySystem>();
        
        inventory.AddItem(seed1, 5);
        inventory.AddItem(seed2, 3);
        inventory.AddItem(seed3, 3);
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
        UpdateToolDirection(moveDirection);
        
        HandleToolInput();
        HandleInventoryInput();
        
        // using the same keybinding
        // prioritise interaction handling over use of inventory items
        if (HandleInteractInput() == false)
        {
            HandleUseItemInput();
        }
    }

    private void FixedUpdate()
    {
        Vector2 position = (Vector2)rigidbody2d.position + unitsPerSecond * Time.deltaTime * move;
        rigidbody2d.MovePosition(position);
    }

    #endregion

    #region Input Handling

    private bool HandleInteractInput()
    {
        if (InteractAction.triggered)
        {
            // check if player can interact with an IInteractable object
            if (currentInteractable != null)
            {
                Debug.Log("Player interacted!");
                currentInteractable.Interact(this);
                return true;
            }
            
            // check if player can interact with the crops and harvest them
            if (plotlandController.CanHarvest(transform.position))
            {
                Debug.Log("Player harvested!");
                plotlandController.HarvestPlot(transform.position, this);
                return true;
            }
        }

        return false;
    }

    private void HandleToolInput()
    {
        if (ToolAction.triggered)
        {
            tools.ToolAction(this);
            animator.SetTrigger("UseTool");
        }
    }
    
    private void HandleInventoryInput()
    {
        if (InventoryAction.triggered)
        {
            inventory.GetNextItem();
        }
    }
    
    private void HandleUseItemInput()
    {
        if (UseItemAction.triggered)
        {
            var item = inventory.GetSelectedItem();
            if (item == null)
            {
                Debug.Log("No item to use.");
                return;
            }

            item.Use(transform.position, this);
        }
        
    }

    
    #endregion

    #region Update-related methods
    private void UpdateToolDirection(Vector2 direction)
    {
        Vector3 positionOffset = new Vector3(-0.45f, 0.6f, 0);
        // float rotationAngle = 13.25f;  // bug? rotation doesn't work
        
        if (direction.y < 0 && Mathf.Abs(direction.y) > Mathf.Abs(direction.x))          // down
        {
            toolPivot.localPosition = positionOffset;
            // toolPivot.localRotation = Quaternion.Euler(0, 0, rotationAngle);
        }
        else if (direction.y > 0 && Mathf.Abs(direction.y) > Mathf.Abs(direction.x))     // up
        {
            toolPivot.localPosition = new Vector3(
                -positionOffset.x,
                positionOffset.y,
                -positionOffset.z
            );
            // toolPivot.localRotation = Quaternion.Euler(0, 0, -rotationAngle);;
        }
        else if (direction.x < 0)                                                          // left
        {
            toolPivot.localPosition = new Vector3(
                0.0f * positionOffset.x,
                positionOffset.y,
                positionOffset.z
            );
            // toolPivot.localRotation = Quaternion.Euler(0, 0, -rotationAngle);
        }
        else if (direction.x > 0)                                                         // right
        {
            toolPivot.localPosition = new Vector3(
                0.0f * positionOffset.x,
                positionOffset.y,
                positionOffset.z
            );
            // toolPivot.localRotation = Quaternion.Euler(0, 0, rotationAngle);
        }
    }
    
    #endregion

    
    
    
    
}
