using DefaultNamespace;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public InputAction MoveAction;
    private Rigidbody2D rigidbody2d;
    private Vector2 move;
    private Vector2 moveDirection = new Vector2(0, -1);
    private float unitsPerSecond = 4.0f;

    [Header("Interaction Settings")]
    public InputAction InteractAction;
    private IInteractable currentInteractable = null;
    public IInteractable CurrentInteractable
    {
        get => currentInteractable;
        set => currentInteractable = value;
    }
    
    [Header("Tool Settings")]
    public InputAction ToolAction;
    [SerializeField] private ToolSystem tools;
    [SerializeField] private Transform toolPivot;  // for positioning and animation of tools

    [Header("Animation Settings")] 
    public Animator animator;

    [Header("Inventory Settings")]
    public InputAction InventoryAction;
    [SerializeField] private InventorySystem inventory;
    

    #region Unity Methods
    
    private void Start()
    {
        MoveAction.Enable();
        InteractAction.Enable();
        ToolAction.Enable();
        InventoryAction.Enable();
        
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        inventory = GetComponent<InventorySystem>();

        // debug hardcoding - to be removed
        inventory.AddItem("plant1_seeds", 3);
        inventory.AddItem("plant2_seeds", 3);
        inventory.AddItem("plant3_seeds", 3);
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
        
        HandleInteractInput();
        HandleToolInput();
        HandleInventoryInput();
    }

    private void FixedUpdate()
    {
        Vector2 position = (Vector2)rigidbody2d.position + unitsPerSecond * Time.deltaTime * move;
        rigidbody2d.MovePosition(position);
    }

    #endregion

    #region Input Handling

    private void HandleInteractInput()
    {
        if (currentInteractable != null && InteractAction.triggered)
        {
            Debug.Log("Player interacted!");
            currentInteractable.Interact(this);
        }
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
