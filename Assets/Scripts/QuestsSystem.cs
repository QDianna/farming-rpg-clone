using UnityEngine;

/// <summary>
/// Simple quest system to track player progression and unlocked features.
/// Manages key story milestones and feature unlocks.
/// </summary>
public class QuestsSystem : MonoBehaviour
{
    public static QuestsSystem Instance { get; private set; }
    
    [Header("Quest Progress")]
    [SerializeField] private bool hasMetWitch ;
    [SerializeField] private bool hasCompletedWitchQuest ;
    
    public event System.Action OnWitchFirstMet;
    public event System.Action OnWitchQuestCompleted;
    
    // Public getters for quest states
    public bool HasMetWitch => hasMetWitch;
    public bool HasCompletedWitchQuest => hasCompletedWitchQuest;
    
    private void Awake()
    {
        InitializeSingleton();
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