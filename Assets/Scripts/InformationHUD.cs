using UnityEngine;
using UnityEngine.UIElements;

public class InformationHUD : MonoBehaviour
{
    [Header("FPS Display")]
    private VisualElement fpsContainer;
    private Label fpsLabel;
    private float deltaTime;
    
    private float displayInterval = 1f; // cât de des actualizezi afișajul
    private float timeSinceLastUpdate = 0f;

    private bool isMeasuring = false;
    private float totalFPS = 0f;
    private float minFPS = float.MaxValue;
    private int frameCount = 0;

    [Header("Keybinds Help")]
    private VisualElement keybindsContainer;
    private Label keybindsLabel;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        // FPS elements
        fpsContainer = root.Q<VisualElement>("FPSContainer");
        fpsLabel = fpsContainer.Q<Label>("Text");
        
        // Keybinds elements
        keybindsContainer = root.Q<VisualElement>("KeybindsContainer");
        keybindsLabel = keybindsContainer.Q<Label>("Text");
        
        // Set up keybinds hover events
        if (keybindsContainer != null && keybindsLabel != null)
        {
            keybindsContainer.RegisterCallback<MouseEnterEvent>(OnKeybindsMouseEnter);
            keybindsContainer.RegisterCallback<MouseLeaveEvent>(OnKeybindsMouseLeave);
            
            // Hide keybinds label initially
            keybindsLabel.style.display = DisplayStyle.None;
        }
    }

    void Update()
    {
        // compute fps
        float fps = 1.0f / Time.unscaledDeltaTime;
        
        if (isMeasuring)
        {
            totalFPS += fps;
            frameCount++;
            if (fps < minFPS)
                minFPS = fps;
        }
        
        // detect start & stop measuring commands
        if (Input.GetKeyDown(KeyCode.F5))
            StartMeasurement();

        if (Input.GetKeyDown(KeyCode.F6))
            StopMeasurement();
        
        // HUD
        // update UI mai rar
        timeSinceLastUpdate += Time.unscaledDeltaTime;
        if (timeSinceLastUpdate >= displayInterval)
        {
            if (fpsLabel != null)
                fpsLabel.text = $"{Mathf.RoundToInt(fps)} FPS";
            timeSinceLastUpdate = 0f;
        }
    }

    private void OnKeybindsMouseEnter(MouseEnterEvent evt)
    {
        ShowKeybinds(true);
    }

    private void OnKeybindsMouseLeave(MouseLeaveEvent evt)
    {
        ShowKeybinds(false);
    }

    public void ShowKeybinds(bool show)
    {
        if (keybindsLabel != null)
        {
            keybindsLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public void StartMeasurement()
    {
        isMeasuring = true;
        totalFPS = 0f;
        minFPS = float.MaxValue;
        frameCount = 0;
        Debug.Log("FPS measurement started.");
    }

    public void StopMeasurement()
    {
        isMeasuring = false;
        float averageFPS = frameCount > 0 ? totalFPS / frameCount : 0f;
        Debug.Log($"FPS measurement stopped.\n" +
                  $"Average FPS: {Mathf.Round(averageFPS)}\n" +
                  $"Min FPS: {Mathf.Round(minFPS)}");
    }
}