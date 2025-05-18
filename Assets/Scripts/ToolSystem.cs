using System.Collections.Generic;
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
    [SerializeField] private GameObject hoePrefab;
    // [SerializeField] private GameObject axePrefab;
    
    public event System.Action OnSelectedToolChange;
    public ToolType selectedTool = ToolType.None;
    private GameObject selectedToolPrefab;
    
    // keyboard key - ToolType mapping
    private Dictionary<int, ToolType> toolBindings = new()
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
        
        selectedTool = toolBindings[toolKey];
        Debug.Log("Tool changed to: " + selectedTool);

        if (selectedToolPrefab != null)
        {
            Destroy(selectedToolPrefab);
            selectedToolPrefab = null;
        }

        if (selectedTool == ToolType.Hoe && hoePrefab != null)
        {
            selectedToolPrefab = Instantiate(hoePrefab, player.toolPivot);
            selectedToolPrefab.transform.localPosition = Vector3.zero;
        }
        
        OnSelectedToolChange?.Invoke();  // notify HUD
    }
    
    public void UseTool(PlayerController player)
    {
        switch (selectedTool)
        {
            case ToolType.Hoe:
                
                player.plotlandController.TillPlot(player.transform.position);
                break;

            case ToolType.Axe:
                // TODO
                break;
        }
    }

    public Sprite GetSelectedToolSprite()
    {
        if (selectedTool == ToolType.None || selectedToolPrefab == null)
            return null;

        var spriteRenderer = selectedToolPrefab.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            return spriteRenderer.sprite;

        return null;

    }
    
    public ToolType GetSelectedToolType()
    {
        return selectedTool;

    }
}
