using UnityEngine;

/// <summary>
/// Manages the player's equipped tool and tool-based interactions such as tilling.
/// 
/// Responsibilities:
/// - Tracks the currently selected tool (Hoe, Axe, etc.)
/// - Handles visual activation of tool GameObjects
/// - Executes tool-specific actions (e.g., tilling the soil when using a hoe)
/// 
/// Usage:
/// - Tools are switched using number key shortcuts (1 = Hoe, 2 = Axe, 0 = None)
/// - The ToolAction() method is called by the player controller when the tool input is triggered
/// 
/// Design Considerations:
/// - Tool switching and action logic are encapsulated in one place for modularity
/// - Easily extendable: new tools and behaviors can be added without modifying other systems
/// - References external systems (e.g., PlotlandController) to perform context-sensitive actions
/// 
/// Part of the modular gameplay system enabling flexible player actions through tool types.
/// </summary>

public enum ToolType
{
    None,
    Hoe,
    Axe
}

public class ToolSystem : MonoBehaviour
{
    [SerializeField] private PlotlandController plotlandController;
    [SerializeField] private GameObject hoeObject;
    // [SerializeField] private GameObject axeObject;
    
    public ToolType currentTool = ToolType.None;

    void Start()
    {
        UpdateToolVisibility(); // Hide all tools at start
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetTool(ToolType.Hoe);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetTool(ToolType.Axe);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetTool(ToolType.None);
        }
    }

    private void SetTool(ToolType newTool)
    {
        currentTool = newTool;
        Debug.Log("Tool changed to: " + currentTool);
        
        UpdateToolVisibility();
    }

    private void UpdateToolVisibility()
    {
        if (hoeObject != null)
        {
            hoeObject.SetActive(currentTool == ToolType.Hoe);
        }
        
        // if (axeObject != null) {
        //    axeObject.SetActive(currentTool == ToolType.Axe);
        // }
    }

    public void ToolAction(PlayerController player)
    {
        switch (currentTool)
        {
            case ToolType.Hoe:
                
                plotlandController.TillPlot(player.transform.position);
                break;

            case ToolType.Axe:
                // TODO
                break;
        }
    }
    
}