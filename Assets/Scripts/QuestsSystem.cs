using UnityEngine;
using System.Collections;

/// <summary>
/// Simple quest system to track player progression and unlocked features.
/// Manages story introduction, tutorial, and witch quest progression.
/// </summary>
public class QuestsSystem : MonoBehaviour
{
    public static QuestsSystem Instance { get; private set; }
    
    [Header("Quest Progress")]
    [SerializeField] private bool hasShownIntroduction;
    [SerializeField] private bool hasShownTutorial;
    [SerializeField] private bool hasMetWitch;
    [SerializeField] private bool hasStartedWitchQuest;
    [SerializeField] private bool hasCompletedWitchQuest;
    
    [Header("Tutorial Settings")]
    [SerializeField] private int tutorialDelay = 8 * 4; // Delay after introduction before showing tutorial
    
    public event System.Action OnWitchFirstMet;
    public event System.Action OnWitchQuestCompleted;
    
    // Public getters for quest states
    public bool HasMetWitch => hasMetWitch;
    public bool HasCompletedWitchQuest => hasCompletedWitchQuest;
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        // Start the introduction when the game begins
        if (!hasShownIntroduction)
            ShowIntroduction();
        
    }
    
    // Shows the player's opening monologue
    private void ShowIntroduction()
    {
        if (!hasShownIntroduction)
        {
            hasShownIntroduction = true;
            
            NotificationSystem.ShowDialogue("My beloved has fallen gravely ill... " +
                                            "I have never seen anything like this.", 3.4f);
            NotificationSystem.ShowDialogue("It came without warning. Each day, their strength fades, " +
                                            "their skin pale, their breath weak.", 3.4f);
            NotificationSystem.ShowDialogue("No healer in town can explain it. I am running out of time.", 3.4f);
            NotificationSystem.ShowDialogue("But it is not just them... " +
                                            "Something is wrong with the world itself.", 3.4f);
            NotificationSystem.ShowDialogue("The weather has turned strange... sudden storms, " +
                                            "harsh winters, crops refusing to grow...", 3.4f);
            NotificationSystem.ShowDialogue("Even the plants seem sick. " +
                                            "I have seen seeds rot before they sprout.", 3.4f);
            NotificationSystem.ShowDialogue("I must find a cure... maybe there is somebody out there who knows more...", 3.4f);
        }
    }
    
    // Marks that the player has met the witch and starts the witch quest
    public void SetWitchMet()
    {
        if (!hasMetWitch)
        {
            hasMetWitch = true;
            OnWitchFirstMet?.Invoke();
            StartWitchQuest();
        }
    }
    
    // Starts the witch quest when first meeting the witch
    private void StartWitchQuest()
    {
        if (!hasStartedWitchQuest)
        {
            hasStartedWitchQuest = true;
        }
    }
    
    // Marks that the player has completed the witch's quest
    public void SetWitchQuestCompleted()
    {
        if (!hasCompletedWitchQuest)
        {
            hasCompletedWitchQuest = true;
            OnWitchQuestCompleted?.Invoke();
        }
    }
    
    // Sets up singleton instance with persistence
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}