using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI handler for displaying notifications with fade in/out animations.
/// Just displays what NotificationSystem tells it to display.
/// </summary>
public class NotificationSystemHUD : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeInDuration = 0.1f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    
    private VisualElement infoContainer;
    private Label infoTextField;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        infoContainer = root.Q<VisualElement>("InfoContainer");
        infoTextField = root.Q<Label>("Info");
        
        if (infoContainer != null)
        {
            infoContainer.style.display = DisplayStyle.None;
        }
        
        NotificationSystem.OnShowNotification += ShowNotification;
    }



    private void OnDisable()
    {
        NotificationSystem.OnShowNotification -= ShowNotification;
    }

    private void ShowNotification(string message)
    {
        Debug.Log("Entered show notification");
        StartCoroutine(DisplayNotificationCoroutine(message));
    }
    
    private IEnumerator DisplayNotificationCoroutine(string message)
    {
        if (infoTextField != null)
        {
            infoTextField.text = message;
        }
        
        if (infoContainer != null)
        {
            infoContainer.style.display = DisplayStyle.Flex;
            
            yield return FadeCoroutine(0, 1, fadeInDuration);
            yield return new WaitForSeconds(displayDuration);
            yield return FadeCoroutine(1, 0, fadeOutDuration);
            
            infoContainer.style.display = DisplayStyle.None;
        }
        
        // When done, tell system to show next
        Debug.Log("notif done, call finished");
        NotificationSystem.Instance?.NotificationFinished();
    }

    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration)
    {
        if (infoContainer == null) yield break;
        
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