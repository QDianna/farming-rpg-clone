using UnityEngine;
using UnityEngine.UIElements;
using System;

public class InformationHUD : MonoBehaviour
{
    [Header("FPS Display")]
    private VisualElement fpsContainer;
    private Label fpsLabel;
    private float deltaTime;

    private float displayInterval = 0.3f;
    private float timeSinceLastUpdate = 0f;

    private bool isMeasuring = false;
    private float totalFPS = 0f;
    private int frameCount = 0;

    [Header("Keybinds Help")]
    private VisualElement keybindsContainer;
    private Label keybindsLabel;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        fpsContainer = root.Q<VisualElement>("FPSContainer");
        fpsLabel = fpsContainer.Q<Label>("Text");

        keybindsContainer = root.Q<VisualElement>("KeybindsContainer");
        keybindsLabel = keybindsContainer.Q<Label>("Text");

        if (keybindsContainer != null && keybindsLabel != null)
        {
            keybindsContainer.RegisterCallback<MouseEnterEvent>(OnKeybindsMouseEnter);
            keybindsContainer.RegisterCallback<MouseLeaveEvent>(OnKeybindsMouseLeave);
            keybindsLabel.style.display = DisplayStyle.None;
        }
    }

    void Update()
    {
        float fps = 1.0f / Time.unscaledDeltaTime;

        if (isMeasuring)
        {
            totalFPS += fps;
            frameCount++;
        }

        if (Input.GetKeyDown(KeyCode.F5))
            StartMeasurement();

        if (Input.GetKeyDown(KeyCode.F6))
            StopMeasurement();

        timeSinceLastUpdate += Time.unscaledDeltaTime;
        if (timeSinceLastUpdate >= displayInterval)
        {
            timeSinceLastUpdate = 0f;
        }
    }

    private void OnKeybindsMouseEnter(MouseEnterEvent evt) => ShowKeybinds(true);
    private void OnKeybindsMouseLeave(MouseLeaveEvent evt) => ShowKeybinds(false);

    public void ShowKeybinds(bool show)
    {
        if (keybindsLabel != null)
            keybindsLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void StartMeasurement()
    {
        isMeasuring = true;
        totalFPS = 0f;
        frameCount = 0;

        fpsLabel.text = "";
        var message = "FPS TEST INFORMATION\n[{CurrentTime()}] test started.";
        Debug.Log(message);
        AppendToFPSLabel(message);
    }

    public void StopMeasurement()
    {
        isMeasuring = false;
        float averageFPS = frameCount > 0 ? totalFPS / frameCount : 0f;

        var message = $"[{{CurrentTime()}}] test stopped.\nTest result: {Mathf.RoundToInt(averageFPS)} AVG FPS.";
        Debug.Log(message);
        AppendToFPSLabel(message);
    }

    private void AppendToFPSLabel(string message)
    {
        if (fpsLabel != null)
            fpsLabel.text += message + "\n";
    }

    private string CurrentTime()
    {
        return DateTime.Now.ToString("HH:mm:ss");
    }
}
