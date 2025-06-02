using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolSystemHUD : MonoBehaviour
{
    [SerializeField] private ToolSystem toolSystem;

    private Dictionary<ToolType, VisualElement> toolContainers = new();
    private Dictionary<ToolType, VisualElement> toolIcons = new();

    private bool renderTools = true;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        toolContainers[ToolType.None]       = root.Q<VisualElement>("NoneContainer");
        toolContainers[ToolType.Hoe]        = root.Q<VisualElement>("HoeContainer");
        toolContainers[ToolType.Axe]        = root.Q<VisualElement>("AxeContainer");
        toolContainers[ToolType.WaterCan]   = root.Q<VisualElement>("WaterCanContainer");

        toolIcons[ToolType.None]     = root.Q<VisualElement>("None");
        toolIcons[ToolType.Hoe]      = root.Q<VisualElement>("Hoe");
        toolIcons[ToolType.Axe]      = root.Q<VisualElement>("Axe");
        toolIcons[ToolType.WaterCan] = root.Q<VisualElement>("WaterCan");
        
        toolSystem.OnSelectedToolChange += UpdateDisplay;
        UpdateDisplay();
    }
    
    private void OnDisable()
    {
        toolSystem.OnSelectedToolChange -= UpdateDisplay;
    }
    

    private void UpdateDisplay()
    {
        if (renderTools)
        {
            foreach (var kvp in toolIcons)
            {
                ToolType type = kvp.Key;
                VisualElement icon = kvp.Value;
                Sprite sprite = null;

                switch (type)
                {
                    case ToolType.Hoe:       sprite = toolSystem.hoeSprite; break;
                    case ToolType.Axe:       sprite = toolSystem.axeSprite; break;
                    case ToolType.WaterCan:  sprite = toolSystem.waterCanSprite; break;
                }

                if (sprite != null)
                    icon.style.backgroundImage = new StyleBackground(sprite);
            }

            renderTools = false;
        }

        
        // TODO - get selected tool type and mark the tool as selected visually somehow
        foreach (var container in toolContainers.Values)
            container.RemoveFromClassList("selected");

        ToolType selected = toolSystem.selectedTool;
        if (toolContainers.ContainsKey(selected))
            toolContainers[selected].AddToClassList("selected");

        
    }
}
