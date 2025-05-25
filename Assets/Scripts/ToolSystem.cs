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
    Axe,
    WateringCan
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
    [SerializeField] private Sprite wateringCanSprite;
    public static int toolCount = 3;
    
    public event System.Action OnSelectedToolChange;

    private Tool[] tools = new Tool[toolCount + 1];
    private int selected = 0;
    
    private void Awake()
    {
        tools[0] = new Tool(ToolType.None, null, 0);
        tools[1] = new Tool(ToolType.Hoe, hoeSprite, 1);
        tools[2] = new Tool(ToolType.Axe, axeSprite, 2);
        tools[3] = new Tool(ToolType.WateringCan, wateringCanSprite, 3);
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
    
    public void UseTool(PlayerController player)
    {
        switch (selected)
        {
            case 0:
                break;
            
            case 1:
                UseHoe(player);
                break;

            case 2:
                UseAxe(player);
                break;
            
            case 3:
                UseWateringCan(player);
                break;
        }
    }

    private void UseHoe(PlayerController player)
    {
        if (player.plotlandController.CanTill(player.transform.position))
        {
            player.animator.SetTrigger("Use Hoe");
            player.plotlandController.TillPlot(player.transform.position);
        }

        else
        {
            Debug.Log("You can only till the plotland!");
        }
    }

    private void UseAxe(PlayerController player)
    {
        player.animator.SetTrigger("Use Axe");
        // TODO
    }

    private void UseWateringCan(PlayerController player)
    {
        if (player.plotlandController.CanAttendPlot(player.transform.position) == false)
            return;
        
        if (TimeSystem.Instance.isWarmSeason() == false)
        {
            Debug.Log("Plant doesnt need watering in cold season");
            return;
        }

        player.animator.SetTrigger("Use Watering Can");
        player.plotlandController.AttendPlot(player.transform.position);
    }
    
}
