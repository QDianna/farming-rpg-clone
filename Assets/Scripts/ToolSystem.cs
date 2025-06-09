using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Available tool types for player actions.
/// </summary>
public enum ToolType
{
    None, Hoe, Axe, WaterCan
}

/// <summary>
/// Tool data container with type, icon, and keybind information.
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
/// Manages player tool selection and usage with context-sensitive actions.
/// Handles tilling, watering, and other tool-based interactions.
/// </summary>
public class ToolSystem : MonoBehaviour
{
    [Header("Tool Sprites")]
    public Sprite hoeSprite;
    public Sprite axeSprite;
    public Sprite waterCanSprite;
    
    [Header("Axe Settings")]
    public float axeRange = 2f;
    public LayerMask treeLayer = 1 << 8; // Set trees to layer 8


    [HideInInspector] public ToolType selectedTool = ToolType.None;
    private Dictionary<ToolType, Tool> tools = new();
    public event System.Action OnSelectedToolChange;
    
    private void Awake()
    {
        InitializeTools();
    }
    
    private void InitializeTools()
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
                OnSelectedToolChange?.Invoke();
                NotificationSystem.ShowNotification($"Selected: {selectedTool}");
                return;
            }
        }
    }
    
    public void UseTool(PlayerController player)
    {
        switch (selectedTool)
        {
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
            NotificationSystem.ShowNotification("Can't till here!");
        }
    }

    private void UseAxe(PlayerController player)
    {
        player.animator.SetTrigger("Use Axe");
        
        // Find trees in range
        Collider2D[] treesInRange = Physics2D.OverlapCircleAll(player.transform.position, axeRange, treeLayer);
    
        if (treesInRange.Length == 0)
        {
            NotificationSystem.ShowNotification("No trees in range!");
            return;
        }
    
        // Chop the closest tree
        TreeController closestTree = null;
        float closestDistance = float.MaxValue;
    
        foreach (Collider2D treeCollider in treesInRange)
        {
            TreeController tree = treeCollider.GetComponent<TreeController>();
            if (tree != null && !tree.isChopped)
            {
                float distance = Vector2.Distance(player.transform.position, tree.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTree = tree;
                }
            }
        }
    
        if (closestTree != null)
        {
            closestTree.TakeDamage(1);
        }
        else
        {
            NotificationSystem.ShowNotification("No trees to chop here!");
        }
    }

    private void UseWateringCan(PlayerController player)
    {
        if (!player.plotlandController.CanAttendPlot(player.transform.position))
        {
            NotificationSystem.ShowNotification("No plants here need watering");
            return;
        }
        
        if (!TimeSystem.Instance.isCurrentSeasonWarm())
        {
            NotificationSystem.ShowNotification("Plants don't need watering in cold season");
            return;
        }

        player.animator.SetTrigger("Use Water Can");
        player.plotlandController.AttendPlot(player.transform.position);
        NotificationSystem.ShowNotification("Watered plants");
    }
}