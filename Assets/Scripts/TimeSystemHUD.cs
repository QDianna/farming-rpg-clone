using UnityEngine;
using UnityEngine.UIElements;

public class TimeSystemHUD : MonoBehaviour
{
    private Label time;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        time = root.Q<Label>("Time");

        TimeSystem.Instance.OnDayChange += UpdateDisplay;
        TimeSystem.Instance.OnHourChange += UpdateDisplay;
        TimeSystem.Instance.OnMinuteChange += UpdateDisplay;
        
        UpdateDisplay();  // remove initial (test) values from ui builder
    }
    
    private void OnDisable()
    {
        TimeSystem.Instance.OnDayChange -= UpdateDisplay;
        TimeSystem.Instance.OnHourChange -= UpdateDisplay;
        TimeSystem.Instance.OnMinuteChange -= UpdateDisplay;
    }

    private void UpdateDisplay()
    { 
        time.text = "Day " + TimeSystem.Instance.currentDay + " - " 
                  + TimeSystem.Instance.GetHour() + ":" 
                  + TimeSystem.Instance.GetMinute();
    }
}
