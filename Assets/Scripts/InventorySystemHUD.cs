using UnityEngine;
using UnityEngine.UIElements;

public class InventorySystemHUD : MonoBehaviour
{
    [SerializeField] private InventorySystem inventorySystem;

    private VisualElement inventoryPanel;
    private ScrollView itemList;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        inventoryPanel = root.Q<VisualElement>("InventoryPanel");
        itemList = root.Q<ScrollView>("ItemList");

        inventoryPanel.style.display = DisplayStyle.None; // Hide by default

        // inventorySystem.OnInventoryChanged += RefreshInventoryDisplay;
    }

    private void OnDisable()
    {
        // inventorySystem.OnInventoryChanged -= RefreshInventoryDisplay;
    }

    public void ToggleInventoryDisplay()
    {
        bool isVisible = inventoryPanel.style.display == DisplayStyle.Flex;
        inventoryPanel.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
        if (!isVisible)
            RefreshInventoryDisplay();
    }

    private void RefreshInventoryDisplay()
    {
        itemList.Clear();
        /*foreach (var entry in inventorySystem.GetEntries())
        {
            var label = new Label($"{entry.item.name} x{entry.quantity}");
            itemList.Add(label);
        }*/
    }
}
