using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Player money display UI with real-time updates.
/// Shows current money amount and responds to economy changes through event system.
/// </summary>
public class PlayerEconomyHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private PlayerEconomy playerEconomy;
    
    private Label moneyLabel;
    
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
    
    // Sets up UI element references and initial display
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        moneyLabel = root.Q<Label>("Money");
        
        if (playerEconomy != null)
        {
            UpdateMoneyDisplay(playerEconomy.CurrentMoney);
        }
    }
    
    // Subscribes to economy change events
    private void SubscribeToEvents()
    {
        if (playerEconomy != null)
            playerEconomy.OnMoneyChanged += UpdateMoneyDisplay;
    }
    
    // Unsubscribes from economy change events
    private void UnsubscribeFromEvents()
    {
        if (playerEconomy != null)
            playerEconomy.OnMoneyChanged -= UpdateMoneyDisplay;
    }
    
    // Updates money display with current amount
    private void UpdateMoneyDisplay(int amount)
    {
        if (moneyLabel != null)
        {
            moneyLabel.text = $"{amount}g";
        }
    }
}