using UnityEngine;
using UnityEngine.UIElements;

public class InventorySystemHUD : MonoBehaviour
{
    [SerializeField] private InventorySystem inventorySystem;

    private VisualElement selectedItemIcon;
    private Label selectedItemCount;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        selectedItemIcon = root.Q<VisualElement>("SelectedItem");
        selectedItemCount = root.Q<Label>("SelectedItemCount");

        inventorySystem.OnSelectedItemChange += UpdateDisplay;
        UpdateDisplay();  // remove initial (test) values from ui builder
    }
    
    private void OnDisable()
    {
        inventorySystem.OnSelectedItemChange -= UpdateDisplay;
    }

    private void UpdateDisplay()
    {
        var selectedItem = inventorySystem.GetSelectedItem();
        if (selectedItem != null && selectedItem.itemSprite != null)
        {
            selectedItemIcon.style.backgroundImage = new StyleBackground(selectedItem.itemSprite);
            selectedItemCount.text = "x" + inventorySystem.GetSelectedItemQuantity();
        }
        else
        {
            selectedItemIcon.style.backgroundImage = null;
            selectedItemCount.text = "";
        }
    }
}
