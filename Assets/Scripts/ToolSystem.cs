using System.Collections.Generic;
using UnityEditor;
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
    private GameObject currentToolObject = null;
    public ToolType currentTool = ToolType.None;
    [SerializeField] private GameObject hoePrefab;
    // [SerializeField] private GameObject axePrefab;
    
    // keyboard key - ToolType mapping
    private Dictionary<int, ToolType> toolBindings = new Dictionary<int, ToolType>
    {
        { 0, ToolType.None },
        { 1, ToolType.Hoe },
        { 2, ToolType.Axe }
    };

    public void SetTool(int toolKey, PlayerController player)
    {
        if (!toolBindings.ContainsKey(toolKey))
        {
            Debug.Log("Tool not in dictonary: " + toolKey);
            return;
        }
        
        currentTool = toolBindings[toolKey];
        Debug.Log("Tool changed to: " + currentTool);

        if (currentToolObject != null)
        {
            Destroy(currentToolObject);
            currentToolObject = null;
        }

        if (currentTool == ToolType.Hoe && hoePrefab != null)
        {
            currentToolObject = Instantiate(hoePrefab, player.toolPivot);
            currentToolObject.transform.localPosition = Vector3.zero;
        }
    }
    
    public void UseTool(PlayerController player)
    {
        switch (currentTool)
        {
            case ToolType.Hoe:
                
                player.plotlandController.TillPlot(player.transform.position);
                break;

            case ToolType.Axe:
                // TODO
                break;
        }
    }
}
