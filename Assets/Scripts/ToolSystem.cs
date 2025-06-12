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
/// Tool data container with type, icon, and key-bind information.
/// </summary>
public class Tool
{
    public readonly ToolType type;
    public readonly int keyBind;
    
    public Tool(ToolType type, int keyBind)
    {
        this.type = type;
        this.keyBind = keyBind;
    }
}

/// <summary>
/// Player tool system managing selection and context-sensitive actions.
/// Handles tilling, tree chopping, watering, and other tool-based interactions with range detection.
/// </summary>
public class ToolSystem : MonoBehaviour
{
    [Header("Tool Sprites")]
    public Sprite hoeSprite;
    public Sprite axeSprite;
    public Sprite waterCanSprite;
    
    [Header("Axe Configuration")]
    public float axeRange = 2f;
    public LayerMask treeLayer = 1 << 8;

    [HideInInspector] public ToolType selectedTool = ToolType.None;
    private Dictionary<ToolType, Tool> tools = new();
    
    public event System.Action OnSelectedToolChange;
    
    private void Awake()
    {
        InitializeTools();
    }
    
    public void SetTool(int toolKey)
    {
        foreach (var tool in tools.Values)
        {
            if (tool.keyBind == toolKey)
            {
                selectedTool = tool.type;
                OnSelectedToolChange?.Invoke();
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
    
    // Creates tool dictionary with sprites and key-binds
    private void InitializeTools()
    {
        tools[ToolType.None] = new Tool(ToolType.None, 0);
        tools[ToolType.Hoe] = new Tool(ToolType.Hoe, 1);
        tools[ToolType.Axe] = new Tool(ToolType.Axe, 2);
        tools[ToolType.WaterCan] = new Tool(ToolType.WaterCan, 3);
    }

    // Uses hoe to till empty plot land
    private void UseHoe(PlayerController player)
    {
        if (player.plotlandController.CanTill(player.transform.position))
        {
            player.animator.SetTrigger("Use Hoe");
            player.plotlandController.TillPlot(player.transform.position);
        }
    }

    // Uses axe to chop trees within range
    private void UseAxe(PlayerController player)
    {
        player.animator.SetTrigger("Use Axe");
        
        var closestTree = FindClosestTree(player.transform.position);
        
        if (closestTree != null)
        {
            closestTree.TakeDamage();
        }
    }

    // Uses watering can to help planted crops grow
    private void UseWateringCan(PlayerController player)
    {
        if (!CanWaterPlants(player))
            return;

        player.animator.SetTrigger("Use Water Can");
        player.plotlandController.AttendPlot(player.transform.position);
    }
    
    // Finds closest tree within axe range that can be chopped
    private TreeController FindClosestTree(Vector3 playerPosition)
    {
        Collider2D[] treesInRange = Physics2D.OverlapCircleAll(playerPosition, axeRange, treeLayer);
    
        if (treesInRange.Length == 0)
            return null;
    
        TreeController closestTree = null;
        float closestDistance = float.MaxValue;
    
        foreach (Collider2D treeCollider in treesInRange)
        {
            TreeController tree = treeCollider.GetComponent<TreeController>();
            if (tree != null && !tree.isChopped)
            {
                float distance = Vector2.Distance(playerPosition, tree.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTree = tree;
                }
            }
        }
        
        return closestTree;
    }
    
    // Validates watering conditions and shows appropriate messages
    private bool CanWaterPlants(PlayerController player)
    {
        if (!player.plotlandController.CanAttendPlot(player.transform.position))
        {
           // NotificationSystem.ShowNotification("You can water plants in the warm season to help them sprout");
            return false;
        }
        
        if (!TimeSystem.Instance.IsCurrentSeasonWarm())
        {
            NotificationSystem.ShowNotification("Plants don't need watering in cold season");
            return false;
        }
        
        return true;
    }
}