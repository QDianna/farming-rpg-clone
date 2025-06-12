using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Notification UI handler with fade animations and sequential display management.
/// Receives messages from NotificationSystem and handles visual presentation with timing control.
/// </summary>
public class NotificationSystemHUD : MonoBehaviour
{
    [Header("Animation Settings")]
    
    [SerializeField] private float fadeInDuration = 0.1f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    
    private VisualElement infoContainer;
    private Label infoTextField;

    private void Awake()
    {
        InitializeUI();
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
        infoContainer = root.Q<VisualElement>("InfoContainer");
        infoTextField = root.Q<Label>("Info");
        
        if (infoContainer != null)
        {
            infoContainer.style.display = DisplayStyle.None;
        }
    }
    
    // Subscribes to notification system events
    private void SubscribeToEvents()
    {
        NotificationSystem.OnShowNotification += ShowNotification;
    }
    
    // Unsubscribes from notification system events
    private void UnsubscribeFromEvents()
    {
        NotificationSystem.OnShowNotification -= ShowNotification;
    }

    // Starts notification display sequence
    private void ShowNotification(string message)
    {
        StartCoroutine(DisplayNotificationSequence(message));
    }
    
    // Handles complete notification display cycle with animations
    private IEnumerator DisplayNotificationSequence(string message)
    {
        SetNotificationText(message);
        ShowNotificationContainer();
        
        yield return FadeAnimation(0, 1, fadeInDuration);
        yield return new WaitForSeconds(NotificationSystem.Instance.displayDuration);
        yield return FadeAnimation(1, 0, fadeOutDuration);
        
        HideNotificationContainer();
        NotifyDisplayComplete();
    }
    
    // Sets notification text content
    private void SetNotificationText(string message)
    {
        if (infoTextField != null)
        {
            infoTextField.text = message;
        }
    }
    
    // Shows notification container
    private void ShowNotificationContainer()
    {
        if (infoContainer != null)
        {
            infoContainer.style.display = DisplayStyle.Flex;
        }
    }
    
    // Hides notification container
    private void HideNotificationContainer()
    {
        if (infoContainer != null)
        {
            infoContainer.style.display = DisplayStyle.None;
        }
    }
    
    // Notifies system that display is complete
    private void NotifyDisplayComplete()
    {
        NotificationSystem.Instance?.NotificationFinished();
    }

    // Handles opacity fade animation between start and end values
    private IEnumerator FadeAnimation(float startAlpha, float endAlpha, float duration)
    {
        if (infoContainer == null) 
            yield break;
        
        float elapsedTime = 0;
        
        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            infoContainer.style.opacity = alpha;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        infoContainer.style.opacity = endAlpha;
    }
}