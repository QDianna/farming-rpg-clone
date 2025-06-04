using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI handler for displaying notifications with fade in/out animations.
/// Manages notification display timing and visual transitions.
/// </summary>
public class NotificationSystemHUD : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    
    private VisualElement infoContainer;
    private Label infoTextField;
    private Coroutine currentNotificationCoroutine;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        infoContainer = root.Q<VisualElement>("InfoContainer");
        infoTextField = root.Q<Label>("Info");
        
        if (infoContainer != null)
        {
            infoContainer.style.display = DisplayStyle.None;
        }
    }

    private void Start()
    {
        NotificationSystem.OnNotificationRequested += ShowNotification;
    }

    private void OnDisable()
    {
        NotificationSystem.OnNotificationRequested -= ShowNotification;
        
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
        }
    }

    private void ShowNotification(string message)
    {
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
        }
        
        currentNotificationCoroutine = StartCoroutine(DisplayNotificationCoroutine(message));
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
        
        currentNotificationCoroutine = null;
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