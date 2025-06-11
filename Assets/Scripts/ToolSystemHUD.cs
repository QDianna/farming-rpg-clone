using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Tool selection UI displaying available tools with visual selection feedback.
/// Shows tool icons and highlights currently selected tool through CSS class management.
/// </summary>
public class ToolSystemHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private ToolSystem toolSystem;

    private Dictionary<ToolType, VisualElement> toolContainers = new();
    private Dictionary<ToolType, VisualElement> toolIcons = new();

    private void Awake()
    {
        InitializeUI();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    // Sets up UI element references and initial state
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        CacheUIElements(root);
        InitializeToolSprites();
        UpdateDisplay();
    }
    
    // Caches tool container and icon references
    private void CacheUIElements(VisualElement root)
    {
        // Cache containers
        toolContainers[ToolType.None] = root.Q<VisualElement>("NoneContainer");
        toolContainers[ToolType.Hoe] = root.Q<VisualElement>("HoeContainer");
        toolContainers[ToolType.Axe] = root.Q<VisualElement>("AxeContainer");
        toolContainers[ToolType.WaterCan] = root.Q<VisualElement>("WaterContainer");

        // Cache icons from containers
        foreach (var kvp in toolContainers)
        {
            toolIcons[kvp.Key] = kvp.Value?.Q<VisualElement>("ToolIcon");
        }
    }
    
    // Subscribes to tool system change events
    private void SubscribeToEvents()
    {
        if (toolSystem != null)
        {
            toolSystem.OnSelectedToolChange += UpdateDisplay;
        }
    }
    
    // Unsubscribes from tool system change events
    private void UnsubscribeFromEvents()
    {
        if (toolSystem != null)
        {
            toolSystem.OnSelectedToolChange -= UpdateDisplay;
        }
    }

    // Sets up tool icon sprites from tool system
    private void InitializeToolSprites()
    {
        if (toolSystem == null) 
            return;
        
        SetToolSprite(ToolType.Hoe, toolSystem.hoeSprite);
        SetToolSprite(ToolType.Axe, toolSystem.axeSprite);
        SetToolSprite(ToolType.WaterCan, toolSystem.waterCanSprite);
    }
    
    // Sets sprite for specific tool icon
    private void SetToolSprite(ToolType toolType, Sprite sprite)
    {
        if (HasValidToolIcon(toolType) && sprite != null)
        {
            toolIcons[toolType].style.backgroundImage = new StyleBackground(sprite);
        }
    }
    
    // Checks if tool has valid icon element
    private bool HasValidToolIcon(ToolType toolType)
    {
        return toolIcons.ContainsKey(toolType) && toolIcons[toolType] != null;
    }

    // Updates visual selection state for all tools
    private void UpdateDisplay()
    {
        if (toolSystem == null) 
            return;
        
        ClearAllSelectionStates();
        HighlightSelectedTool();
    }
    
    // Removes selected class from all tool containers
    private void ClearAllSelectionStates()
    {
        foreach (var container in toolContainers.Values)
        {
            container?.RemoveFromClassList("selected");
        }
    }
    
    // Adds selected class to currently selected tool
    private void HighlightSelectedTool()
    {
        ToolType selectedTool = toolSystem.selectedTool;
        if (HasValidToolContainer(selectedTool))
        {
            toolContainers[selectedTool].AddToClassList("selected");
        }
    }
    
    // Checks if tool has valid container element
    private bool HasValidToolContainer(ToolType toolType)
    {
        return toolContainers.ContainsKey(toolType) && toolContainers[toolType] != null;
    }
}