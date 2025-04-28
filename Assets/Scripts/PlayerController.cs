using DefaultNamespace;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    // Variables related to player character movement
    public InputAction MoveAction;
    Rigidbody2D rigidbody2d;
    Vector2 move;
    float unitsPerSecond = 4.0f;
    
    // Variables related to player character interactions
    public InputAction InteractAction;
    private IInteractable currentInteractable;

    public IInteractable CurrentInteractable
    {
        get => currentInteractable;
        set => currentInteractable = value;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MoveAction.Enable();
        // TalkAction.Enable();
        
        InteractAction.Enable();
        
        rigidbody2d = GetComponent<Rigidbody2D>();
        // animator = GetComponent<Animator>();

        currentInteractable = null;
    }

    // Update is called once per frame
    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();
        
        if (currentInteractable != null)
        {
            // Debug.Log(("you can interact with: ", currentInteractable));
            if (InteractAction.triggered)
            {
                Debug.Log("player interacted!");
                currentInteractable.Interact(this);
            }
        }
    }
    
    // FixedUpdate has the same call rate as the physics system 
    void FixedUpdate()
    {
        Vector2 position = (Vector2)rigidbody2d.position +  unitsPerSecond * Time.deltaTime * move;
        rigidbody2d.MovePosition(position);
    }
    

}
