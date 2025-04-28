using UnityEngine;

public enum ToolType
{
    None,
    Hoe,
    Axe
}

public class ToolSystem : MonoBehaviour
{
    public ToolType currentTool = ToolType.None;

    void Update()
    {
        // Schimbă uneltele apăsând tastele 1, 2, 3, 4
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

    public void SetTool(ToolType newTool)
    {
        currentTool = newTool;
        Debug.Log("Tool changed to: " + currentTool);
    }
}