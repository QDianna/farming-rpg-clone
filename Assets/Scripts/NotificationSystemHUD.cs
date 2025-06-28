using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Notification UI handler with separate containers for dialogue and help messages.
/// Handles both dialogue (custom duration) and help (2s duration) with fade animations.
/// </summary>
public class NotificationSystemHUD : MonoBehaviour
{
    [Header("Animation Settings")]
    private float fadeInDuration = 0.2f;
    private float fadeOutDuration = 0.3f;
    
    // Dialogue UI elements
    private VisualElement dialogueContainer;
    private Label dialogueTextField;
    
    // Help UI elements
    private VisualElement helpContainer;
    private Label helpTextField;

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
        
        // Dialogue elements
        dialogueContainer = root.Q<VisualElement>("DialogueContainer");
        dialogueTextField = dialogueContainer.Q<Label>("Text"); // Inside DialogueContainer
        
        // Help elements
        helpContainer = root.Q<VisualElement>("HelpContainer");
        helpTextField = helpContainer.Q<Label>("Text"); // Inside HelpContainer
        
        if (dialogueContainer != null)
        {
            dialogueContainer.style.display = DisplayStyle.None;
        }
        
        if (helpContainer != null)
        {
            helpContainer.style.display = DisplayStyle.None;
        }
    }
    
    // Subscribes to notification system events
    private void SubscribeToEvents()
    {
        NotificationSystem.OnShowDialogue += ShowDialogue;
        NotificationSystem.OnShowHelp += ShowHelp;
    }
    
    // Unsubscribes from notification system events
    private void UnsubscribeFromEvents()
    {
        NotificationSystem.OnShowDialogue -= ShowDialogue;
        NotificationSystem.OnShowHelp -= ShowHelp;
    }

    // DIALOGUE METHODS
    
    // Starts dialogue display sequence with custom duration
    private void ShowDialogue(string message, float duration)
    {
        StartCoroutine(DisplayDialogueSequence(message, duration));
    }
    
    // Handles complete dialogue display cycle with animations and custom duration
    private IEnumerator DisplayDialogueSequence(string message, float duration)
    {
        SetDialogueText(message);
        ShowDialogueContainer();
        
        yield return FadeDialogueAnimation(0, 1, fadeInDuration);
        yield return new WaitForSeconds(duration);
        yield return FadeDialogueAnimation(1, 0, fadeOutDuration);
        
        HideDialogueContainer();
        NotifyDialogueComplete();
    }
    
    // Sets dialogue text content
    private void SetDialogueText(string message)
    {
        if (dialogueTextField != null)
        {
            dialogueTextField.text = message;
        }
    }
    
    // Shows dialogue container
    private void ShowDialogueContainer()
    {
        if (dialogueContainer != null)
        {
            dialogueContainer.style.display = DisplayStyle.Flex;
        }
    }
    
    // Hides dialogue container
    private void HideDialogueContainer()
    {
        if (dialogueContainer != null)
        {
            dialogueContainer.style.display = DisplayStyle.None;
        }
    }
    
    // Notifies system that dialogue display is complete
    private void NotifyDialogueComplete()
    {
        NotificationSystem.Instance?.DialogueFinished();
    }

    // Handles opacity fade animation for dialogue container
    private IEnumerator FadeDialogueAnimation(float startAlpha, float endAlpha, float duration)
    {
        if (dialogueContainer == null) 
            yield break;
        
        float elapsedTime = 0;
        
        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            dialogueContainer.style.opacity = alpha;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        dialogueContainer.style.opacity = endAlpha;
    }
    
    // HELP METHODS
    
    // Starts help display sequence
    private void ShowHelp(string message, float duration)
    {
        StartCoroutine(DisplayHelpSequence(message, duration));
    }
    
    // Handles complete help display cycle with animations
    private IEnumerator DisplayHelpSequence(string message, float duration)
    {
        SetHelpText(message);
        ShowHelpContainer();
        
        yield return FadeHelpAnimation(0, 1, fadeInDuration);
        yield return new WaitForSeconds(duration);
        yield return FadeHelpAnimation(1, 0, fadeOutDuration);
        
        HideHelpContainer();
        NotifyHelpComplete();
    }
    
    // Sets help text content
    private void SetHelpText(string message)
    {
        if (helpTextField != null)
        {
            helpTextField.text = message;
        }
    }
    
    // Shows help container
    private void ShowHelpContainer()
    {
        if (helpContainer != null)
        {
            helpContainer.style.display = DisplayStyle.Flex;
        }
    }
    
    // Hides help container
    private void HideHelpContainer()
    {
        if (helpContainer != null)
        {
            helpContainer.style.display = DisplayStyle.None;
        }
    }
    
    // Notifies system that help display is complete
    private void NotifyHelpComplete()
    {
        NotificationSystem.Instance?.HelpFinished();
    }

    // Handles opacity fade animation for help container
    private IEnumerator FadeHelpAnimation(float startAlpha, float endAlpha, float duration)
    {
        if (helpContainer == null) 
            yield break;
        
        float elapsedTime = 0;
        
        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            helpContainer.style.opacity = alpha;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        helpContainer.style.opacity = endAlpha;
    }
}