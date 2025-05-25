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

public class Tool
{
    public ToolType type;
    public Sprite icon;
    public int keybind;
    
    public Tool(ToolType type, Sprite icon, int keybind)
    {
        this.type = type;
        this.icon = icon;
        this.keybind = keybind;
    }
}

public class ToolSystem : MonoBehaviour
{
    [SerializeField] private Sprite hoeSprite;
    [SerializeField] private Sprite axeSprite;
    
    public event System.Action OnSelectedToolChange;

    private Tool[] tools = new Tool[3];
    private int selected = 0;
    
    private void Awake()
    {
        tools[0] = new Tool(ToolType.None, null, 0);
        tools[1] = new Tool(ToolType.Hoe, hoeSprite, 1);
        tools[2] = new Tool(ToolType.Axe, axeSprite, 2);
    }

    public void SetTool(int toolKey)
    {
        if (toolKey >= tools.Length)
        {
            Debug.Log("Tool not in dictonary: " + toolKey);
            return;
        }
        
        selected = toolKey;
        Debug.Log("Tool changed to: " + tools[selected]);
        
        OnSelectedToolChange?.Invoke();  // notify HUD
    }
    
    public void UseTool(PlayerController player)
    {
        
        switch (selected)
        {
            case 0:
                break;
            
            case 1:
                player.animator.SetTrigger("Use Hoe");
                player.plotlandController.TillPlot(player.transform.position);
                break;

            case 2:
                player.animator.SetTrigger("Use Axe");
                // TODO
                break;
        }
    }

    public Sprite GetSelectedToolSprite()
    {
        if (selected > 0 && selected < tools.Length)
            return tools[selected].icon;

        return null;
    }
    
    public ToolType GetSelectedToolType()
    {
        if (selected > 0 && selected < tools.Length)
            return tools[selected].type;

        return ToolType.None;
    }
    
}
