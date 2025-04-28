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
    [SerializeField] private ToolSystem toolSystem;
    [SerializeField] private PlotlandController plotlandController;
    // [SerializeField] private Inventory playerInventory;
    // [SerializeField] private GameObject seedPrefab;

    #endregion

    #region Unity Methods

    private void Start()
    {
        MoveAction.Enable();
        InteractAction.Enable();
        ToolAction.Enable();
        
        rigidbody2d = GetComponent<Rigidbody2D>();
        currentInteractable = null;
    }

    private void Update()
    {
        move = MoveAction.ReadValue<Vector2>();

        HandleInteractInput();
        HandleToolInput();
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
                plotlandController.TillPlot(playerPosition);
                break;

            case ToolType.Axe:
                // TODO
                break;
        }
    }

    #endregion
}
