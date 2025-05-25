using UnityEngine;
using UnityEngine.UIElements;

public class ToolSystemHUD : MonoBehaviour
{
    [SerializeField] private ToolSystem toolSystem;

    private VisualElement selectedToolIcon;
    private Label selectedToolName;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        selectedToolIcon = root.Q<VisualElement>("SelectedTool");
        selectedToolName = root.Q<Label>("SelectedToolName");

        toolSystem.OnSelectedToolChange += UpdateDisplay;
        UpdateDisplay();  // remove initial (test) values from ui builder
    }
    
    private void OnDisable()
    {
        toolSystem.OnSelectedToolChange -= UpdateDisplay;
    }
    

    private void UpdateDisplay()
    {
        // Clear if no prefab
        Sprite toolSprite = toolSystem.GetSelectedToolSprite();
        if (toolSprite == null)
        {
            selectedToolIcon.style.backgroundImage = null;
            selectedToolName.text = "";
            return;
        }
        
        selectedToolIcon.style.backgroundImage = new StyleBackground(toolSprite);
        selectedToolName.text = toolSystem.GetSelectedToolType().ToString();
    }
}
