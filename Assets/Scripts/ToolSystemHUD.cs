using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Displays tool selection UI with visual feedback for the currently selected tool.
/// </summary>
public class ToolSystemHUD : MonoBehaviour
{
    [SerializeField] private ToolSystem toolSystem;

    private Dictionary<ToolType, VisualElement> toolContainers = new();
    private Dictionary<ToolType, VisualElement> toolIcons = new();

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Initialize containers
        toolContainers[ToolType.None] = root.Q<VisualElement>("NoneContainer");
        toolContainers[ToolType.Hoe] = root.Q<VisualElement>("HoeContainer");
        toolContainers[ToolType.Axe] = root.Q<VisualElement>("AxeContainer");
        toolContainers[ToolType.WaterCan] = root.Q<VisualElement>("WaterContainer");

        // Initialize icons
        foreach (var kvp in toolContainers)
        {
            toolIcons[kvp.Key] = kvp.Value?.Q<VisualElement>("ToolIcon");
        }
        
        InitializeToolSprites();
        UpdateDisplay();
    }

    private void OnEnable()
    {
        if (toolSystem != null)
        {
            toolSystem.OnSelectedToolChange += UpdateDisplay;
        }
    }
    
    private void OnDisable()
    {
        if (toolSystem != null)
        {
            toolSystem.OnSelectedToolChange -= UpdateDisplay;
        }
    }

    private void InitializeToolSprites()
    {
        if (toolSystem == null) return;
        
        SetToolSprite(ToolType.Hoe, toolSystem.hoeSprite);
        SetToolSprite(ToolType.Axe, toolSystem.axeSprite);
        SetToolSprite(ToolType.WaterCan, toolSystem.waterCanSprite);
    }
    
    private void SetToolSprite(ToolType toolType, Sprite sprite)
    {
        if (toolIcons.ContainsKey(toolType) && toolIcons[toolType] != null && sprite != null)
        {
            toolIcons[toolType].style.backgroundImage = new StyleBackground(sprite);
        }
    }

    private void UpdateDisplay()
    {
        if (toolSystem == null) return;
        
        // Clear all selected states
        foreach (var container in toolContainers.Values)
        {
            container?.RemoveFromClassList("selected");
        }

        // Highlight selected tool
        ToolType selectedTool = toolSystem.selectedTool;
        if (toolContainers.ContainsKey(selectedTool) && toolContainers[selectedTool] != null)
        {
            toolContainers[selectedTool].AddToClassList("selected");
        }
    }
}