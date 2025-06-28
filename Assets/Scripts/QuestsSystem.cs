using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

/// <summary>
/// Simple quest system to track player progression and unlocked features.
/// Manages story introduction, tutorial, and witch quest progression.
/// </summary>
public class QuestsSystem : MonoBehaviour
{
    public static QuestsSystem Instance { get; private set; }

    [Header("Quest Progress")]
    public bool hasShownIntroduction;
    public bool hasStartedWitchQuest;
    public bool hasCompletedWitchQuest;
    public bool hasMetWitch;

    [Header("Tutorial Settings")]
    private float introDelay = 3f;

    
    public event System.Action OnWitchFirstMet;
    public event System.Action OnWitchQuestCompleted;

    [SerializeField] private UIDocument uiDocument;
    private Label missionText;
    private VisualElement missionsContainer;

    private string currentMissionText = "";

    private void Awake()
    {
        InitializeSingleton();
        
        // Setup UI references
        var root = uiDocument?.rootVisualElement;
        if (root != null)
        {
            missionsContainer = root.Q<VisualElement>("MissionsContainer");
            missionText = missionsContainer?.Q<Label>("Text");

            if (missionsContainer != null && missionText != null)
            {
                missionsContainer.RegisterCallback<MouseEnterEvent>(OnMissionsMouseEnter);
                missionsContainer.RegisterCallback<MouseLeaveEvent>(OnMissionsMouseLeave);
                missionText.style.display = DisplayStyle.None;
            }
        }
    }

    private void Start()
    {
        StartCoroutine(DelayedIntroduction());
    }

    private IEnumerator DelayedIntroduction()
    {
        yield return new WaitForSeconds(introDelay);
        if (!hasShownIntroduction)
            ShowIntroduction();
    }

    private void OnMissionsMouseEnter(MouseEnterEvent evt) => ShowMissions(true);
    private void OnMissionsMouseLeave(MouseLeaveEvent evt) => ShowMissions(false);

    private void ShowMissions(bool show)
    {
        if (missionText != null)
            missionText.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void AddMissionLine(string line)
    {
        if (missionText == null) return;

        if (!string.IsNullOrEmpty(currentMissionText))
            currentMissionText += "\n";

        currentMissionText += "Task: " + line;
        missionText.text = currentMissionText;
    }

    // Shows the player's opening monologue
    private void ShowIntroduction()
    {
        NotificationSystem.ShowDialogue("My beloved has fallen gravely ill... " +
                                        "I have never seen anything like this.", 4f);
        NotificationSystem.ShowDialogue("It came without warning. No healer in town can explain it " +
                                        "and he's running out of time.", 4f);
        NotificationSystem.ShowDialogue("But it is not just them... " +
                                        "Something is wrong with the world itself.", 4f);
        NotificationSystem.ShowDialogue("The weather has turned strange... sudden storms, " +
                                        "harsh winters, crops refusing to grow...", 4f);
        NotificationSystem.ShowDialogue("Even the plants seem sick. " +
                                        "I have seen seeds rot before they sprout.", 4f);
        NotificationSystem.ShowDialogue("I must find a cure... " +
                                        "there must be somebody who knows more...", 4f);

        AddMissionLine("Use the hoe and seeds from your inventory to grow crops. " +
                       "Don't forget to water them in warm seasons!");
        
        hasShownIntroduction = true;
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
            AddMissionLine("Buy Research Table and Crafting Bench from the Market.");
            AddMissionLine("Learn how to craft Strength Potion by researching plants. " +
                           "You need to prepare for cold seasons, crops won't grow without them!");
            AddMissionLine("Try to research the seeds also, they might give you more insight!");
        }
    }


    // Marks that the player has completed the witch's quest
    public void SetWitchQuestCompleted()
    {
        if (!hasCompletedWitchQuest)
        {
            hasCompletedWitchQuest = true;
            OnWitchQuestCompleted?.Invoke();
            AddMissionLine("Craft The Cure Beneath and give it to your spouse!");
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
