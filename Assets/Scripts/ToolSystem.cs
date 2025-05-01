using UnityEngine;

public enum ToolType
{
    None,
    Hoe,
    Axe
}

public class ToolSystem : MonoBehaviour
{
    [SerializeField] private PlotlandController plotlandController;
    [SerializeField] private GameObject hoeObject;
    // [SerializeField] private GameObject axeObject;
    
    public ToolType currentTool = ToolType.None;

    void Start()
    {
        UpdateToolVisibility(); // Hide all tools at start
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetTool(ToolType.Hoe);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetTool(ToolType.Axe);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetTool(ToolType.None);
        }
    }

    private void SetTool(ToolType newTool)
    {
        currentTool = newTool;
        Debug.Log("Tool changed to: " + currentTool);
        
        UpdateToolVisibility();
    }

    private void UpdateToolVisibility()
    {
        if (hoeObject != null)
        {
            hoeObject.SetActive(currentTool == ToolType.Hoe);
        }
        
        // if (axeObject != null) {
        //    axeObject.SetActive(currentTool == ToolType.Axe);
        // }
    }

    public void ToolAction(PlayerController player)
    {
        switch (currentTool)
        {
            case ToolType.Hoe:
                
                plotlandController.TillPlot(player.transform.position);
                break;

            case ToolType.Axe:
                // TODO
                break;
        }
    }
    
}