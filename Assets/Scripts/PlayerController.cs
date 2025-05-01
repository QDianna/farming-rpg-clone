using DefaultNamespace;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Movement

    [Header("Movement Settings")]
    public InputAction MoveAction;
    private Rigidbody2D rigidbody2d;
    private Vector2 move;
    private Vector2 moveDirection = new Vector2(0, -1);
    private float unitsPerSecond = 4.0f;

    #endregion

    #region Interaction

    [Header("Interaction Settings")]
    public InputAction InteractAction;
    private IInteractable currentInteractable;

    public IInteractable CurrentInteractable
    {
        get => currentInteractable;
        set => currentInteractable = value;
    }

    #endregion

    #region Tool Interaction

    [Header("Tool Settings")]
    public InputAction ToolAction;
    [SerializeField] private Transform toolPivot;
    [SerializeField] private ToolSystem toolSystem;
    [SerializeField] private PlotlandController plotlandController;
    // [SerializeField] private Inventory playerInventory;
    // [SerializeField] private GameObject seedPrefab;

    #endregion
    
    #region Animations

    [Header("Animation Settings")] 
    public Animator animator;
    
    #endregion

    #region Unity Methods

    private void Start()
    {
        MoveAction.Enable();
        InteractAction.Enable();
        ToolAction.Enable();
        
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        currentInteractable = null;
    }

    private void Update()
    {
        move = MoveAction.ReadValue<Vector2>();

        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y,0.0f))
        {
            moveDirection.Set(move.x, move.y);
            moveDirection.Normalize();
        }
        
        animator.SetFloat("Move X", moveDirection.x);
        animator.SetFloat("Move Y", moveDirection.y);
        animator.SetFloat("Speed", move.magnitude);
        
        HandleInteractInput();
        HandleToolInput();
        
        UpdateToolDirection(moveDirection);
    }

    private void FixedUpdate()
    {
        Vector2 position = (Vector2)rigidbody2d.position + unitsPerSecond * Time.deltaTime * move;
        rigidbody2d.MovePosition(position);
    }

    #endregion

    #region Interaction Methods

    private void HandleInteractInput()
    {
        if (currentInteractable != null && InteractAction.triggered)
        {
            Debug.Log("Player interacted!");
            currentInteractable.Interact(this);
        }
    }

    #endregion

    #region Tool Methods

    private void HandleToolInput()
    {
        if (ToolAction.triggered)
        {
            HandleToolAction();
        }
    }

    private void HandleToolAction()
    {
        Vector3 playerPosition = transform.position;

        switch (toolSystem.currentTool)
        {
            case ToolType.Hoe:
                animator.SetTrigger("UseTool");
                plotlandController.TillPlot(playerPosition);
                break;

            case ToolType.Axe:
                // TODO
                break;
        }
    }

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
