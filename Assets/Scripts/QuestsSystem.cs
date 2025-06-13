using UnityEngine;

/// <summary>
/// Simple quest system to track player progression and unlocked features.
/// Manages key story milestones, feature unlocks, and initial story introduction.
/// </summary>
public class QuestsSystem : MonoBehaviour
{
    public static QuestsSystem Instance { get; private set; }
    
    [Header("Quest Progress")]
    [SerializeField] private bool hasMetWitch;
    [SerializeField] private bool hasCompletedWitchQuest;
    [SerializeField] private bool hasStartedMainQuest;
    
    public event System.Action OnWitchFirstMet;
    public event System.Action OnWitchQuestCompleted;
    public event System.Action OnMainQuestStarted;
    
    // Public getters for quest states
    public bool HasMetWitch => hasMetWitch;
    public bool HasCompletedWitchQuest => hasCompletedWitchQuest;
    public bool HasStartedMainQuest => hasStartedMainQuest;
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        // Start the main quest when the game begins
        if (!hasStartedMainQuest)
        {
            StartMainQuest();
        }
    }
    
    // Starts the main quest and shows the story introduction
    public void StartMainQuest()
    {
        if (!hasStartedMainQuest)
        {
            hasStartedMainQuest = true;
            OnMainQuestStarted?.Invoke();
            ShowStoryIntroduction();
            Debug.Log("Quest: Main quest 'Find a Cure' has started!");
        }
    }
    
    // Shows the initial story and quest objective
    private void ShowStoryIntroduction()
    {
        NotificationSystem.ShowDialogue("My beloved has fallen gravely ill... I've never seen anything like this.", 1f);
        NotificationSystem.ShowDialogue("It came without warning. Each day, their strength fades... their skin pale, their breath weak.", 1f);
        NotificationSystem.ShowDialogue("No healer in town can explain it. I’m running out of time.", 1f);
        NotificationSystem.ShowDialogue("I must find a cure... whatever it takes.", 1f);
        NotificationSystem.ShowDialogue("I’ll search the land, gather what herbs I can, and seek help from anyone who might know more.", 1f);
    }
    
    // Marks that the player has met the witch for the first time
    public void SetWitchMet()
    {
        if (!hasMetWitch)
        {
            hasMetWitch = true;
            OnWitchFirstMet?.Invoke();
            Debug.Log("Quest: Player has met the witch!");
        }
    }
    
    // Marks that the player has completed the witch's quest
    public void SetWitchQuestCompleted()
    {
        if (!hasCompletedWitchQuest)
        {
            hasCompletedWitchQuest = true;
            OnWitchQuestCompleted?.Invoke();
            Debug.Log("Quest: Witch quest completed!");
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