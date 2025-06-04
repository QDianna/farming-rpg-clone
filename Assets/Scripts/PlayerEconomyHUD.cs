using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Displays player money in the UI with visual feedback for transactions.
/// Shows green flash for income and red flash for expenses.
/// </summary>
public class PlayerEconomyHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private PlayerEconomy playerEconomy;
    
    private Label moneyLabel;
    
    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        moneyLabel = root.Q<Label>("Money");
        
        if (playerEconomy != null)
        {
            UpdateMoneyDisplay(playerEconomy.CurrentMoney);
        }
    }
    
    private void OnEnable()
    {
        if (playerEconomy != null)
        {
            playerEconomy.OnMoneyChanged += UpdateMoneyDisplay;
            playerEconomy.OnItemSold += OnItemSold;
            playerEconomy.OnItemBought += OnItemBought;
        }
    }
    
    private void OnDisable()
    {
        if (playerEconomy != null)
        {
            playerEconomy.OnMoneyChanged -= UpdateMoneyDisplay;
            playerEconomy.OnItemSold -= OnItemSold;
            playerEconomy.OnItemBought -= OnItemBought;
        }
    }
    
    private void UpdateMoneyDisplay(int amount)
    {
        if (moneyLabel != null)
        {
            moneyLabel.text = $"{amount}g";
        }
    }
    
    private void OnItemSold(InventoryItem item, int quantity, int totalPrice)
    {
        StartCoroutine(FlashMoney(Color.green));
    }
    
    private void OnItemBought(InventoryItem item, int quantity, int totalPrice)
    {
        StartCoroutine(FlashMoney(Color.red));
    }
    
    private IEnumerator FlashMoney(Color flashColor)
    {
        if (moneyLabel != null)
        {
            var originalColor = moneyLabel.style.color;
            moneyLabel.style.color = flashColor;
            yield return new WaitForSeconds(0.5f);
            moneyLabel.style.color = originalColor;
        }
    }
}