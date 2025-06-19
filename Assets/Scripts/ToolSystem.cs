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
/// Tool data container with type and key-bind information.
/// </summary>
public class Tool
{
    public ToolType type;
    public int keyBind;

    public Tool(ToolType type, int keyBind)
    {
        this.type = type;
        this.keyBind = keyBind;
    }
}

/// <summary>
/// Player tool system managing selection and context-sensitive actions.
/// Handles tilling, tree chopping, watering, and other tool-based interactions with range detection and energy consumption.
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

    [Header("Energy Consumption")]
    [SerializeField] private float hoeEnergyCost ;
    [SerializeField] private float axeEnergyCost ;
    [SerializeField] private float waterCanEnergyCost;

    [HideInInspector] public ToolType selectedTool = ToolType.None;
    private List<Tool> toolList = new();

    public event System.Action OnSelectedToolChange;

    private void Awake()
    {
        InitializeTools();
    }

    private void InitializeTools()
    {
        toolList.Add(new Tool(ToolType.None, 0));
        toolList.Add(new Tool(ToolType.Hoe, 1));
        toolList.Add(new Tool(ToolType.Axe, 2));
        toolList.Add(new Tool(ToolType.WaterCan, 3));
    }

    public void SetTool(int key)
    {
        foreach (var tool in toolList)
        {
            if (tool.keyBind == key)
            {
                selectedTool = tool.type;
                OnSelectedToolChange?.Invoke();
                return;
            }
        }
    }

    public void UseTool(PlayerController player)
    {
        if (!HasEnoughEnergyForTool(player))
        {
            NotificationSystem.ShowHelp("Too tired to use tools./n" +
                                        "Get some rest or eat something.");
            return;
        }

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
            player.playerStats.ConsumeEnergy(hoeEnergyCost);
        }
    }

    private void UseAxe(PlayerController player)
    {
        player.animator.SetTrigger("Use Axe");

        var closestTree = FindClosestTree(player.transform.position);
        if (closestTree != null)
        {
            closestTree.TakeDamage();
            player.playerStats.ConsumeEnergy(axeEnergyCost);
        }
    }

    private void UseWateringCan(PlayerController player)
    {
        if (!CanWaterPlants(player)) return;

        player.animator.SetTrigger("Use Water Can");
        player.plotlandController.AttendPlot(player.transform.position);
        player.playerStats.ConsumeEnergy(waterCanEnergyCost);
    }

    private bool HasEnoughEnergyForTool(PlayerController player)
    {
        float requiredEnergy = selectedTool switch
        {
            ToolType.Hoe => hoeEnergyCost,
            ToolType.Axe => axeEnergyCost,
            ToolType.WaterCan => waterCanEnergyCost,
            _ => 0f
        };

        return player.playerStats.GetEnergy() >= requiredEnergy;
    }

    private TreeController FindClosestTree(Vector3 playerPosition)
    {
        Collider2D[] treesInRange = Physics2D.OverlapCircleAll(playerPosition, axeRange, treeLayer);
        TreeController closestTree = null;
        float closestDistance = float.MaxValue;

        foreach (var collider in treesInRange)
        {
            TreeController tree = collider.GetComponent<TreeController>();
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

    private bool CanWaterPlants(PlayerController player)
    {
        if (!player.plotlandController.CanAttendPlot(player.transform.position))
            return false;

        if (!TimeSystem.Instance.IsCurrentSeasonWarm())
        {
            NotificationSystem.ShowHelp("Plants don't need watering in cold season");
            return false;
        }

        return true;
    }
}
