using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the different types of tools available in the game.
/// These correspond to player abilities like tilling, chopping, or watering.
/// </summary>
public enum ToolType
{
    None,
    Hoe,
    Axe,
    WaterCan
}

/// <summary>
/// Represents a tool the player can equip and use.
/// Holds metadata such as its type, icon for HUD display, and assigned hotkey.
/// </summary>
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

/// <summary>
/// Controls the player's equipped tool and handles tool-based interactions.
/// 
/// Responsibilities:
/// - Stores and switches between available tools (by keybind)
/// - Executes context-sensitive tool actions (e.g., tilling a tile or watering a plant)
/// - Triggers animation and updates the plotland state through PlotlandController
/// 
/// Tools are accessed via number keys and applied to the tile the player is facing.
/// The system is extendable with new ToolTypes and custom actions.
/// </summary>
public class ToolSystem : MonoBehaviour
{
    public Sprite hoeSprite;
    public Sprite axeSprite;
    public Sprite waterCanSprite;

    public Dictionary<ToolType, Tool> tools = new();
    public ToolType selectedTool = ToolType.None;
    public event System.Action OnSelectedToolChange;
    
    private void Awake()
    {
        tools[ToolType.None] = new Tool(ToolType.None, null, 0);
        tools[ToolType.Hoe] = new Tool(ToolType.Hoe, hoeSprite, 1);
        tools[ToolType.Axe] = new Tool(ToolType.Axe, axeSprite, 2);
        tools[ToolType.WaterCan] = new Tool(ToolType.WaterCan, waterCanSprite, 3);
    }

    public void SetTool(int toolKey)
    {
        foreach (var tool in tools.Values)
        {
            if (tool.keybind == toolKey)
            {
                selectedTool = tool.type;
                Debug.Log("Tool changed to: " + selectedTool);
                OnSelectedToolChange?.Invoke();  // notify HUD
                return;
            }
        }
        Debug.Log("Error - no tool configured for this key.");
    }
    
    public void UseTool(PlayerController player)
    {
        switch (selectedTool)
        {
            case ToolType.None:
                break;
            
            case ToolType.Hoe:
                UseHoe(player);
                break;

            case ToolType.Axe:
                UseAxe(player);
                break;
            
            case ToolType.WaterCan:
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

        player.animator.SetTrigger("Use Water Can");
        player.plotlandController.AttendPlot(player.transform.position);
    }
    
}
