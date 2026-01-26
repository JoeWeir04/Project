using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using TMPro;

public class VRlogAngle : MonoBehaviour
{
    public InputActionReference aButton; // Bind to A button
    public TMP_Text logText;
    public TMP_Text ExperimentText;
    private string filePath;
    public MicSocketVR micSocket;
    private float trialStartTime;
    private bool trialActive = false;

    void Start()
    {
        filePath = Application.persistentDataPath + "/VR_log.csv";
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Time,AudioAngle,Error,ResponseTime\n");
        }
    }

    void Update()
{
    if (!trialActive && micSocket.vad == 1)
        {
            trialStartTime = Time.time;
            trialActive = true;
        }
    }
    private void Awake()
    {
        aButton.action.Enable();
        aButton.action.performed += OnButtonPress;
    }

    private void OnDestroy()
    {
        aButton.action.performed -= OnButtonPress;
        aButton.action.Disable();
    }

    public void OnButtonPress(InputAction.CallbackContext context)
    {
        if (!trialActive)
            return;
        float audioAngle = micSocket.angle; // relative to head
        float error = Mathf.DeltaAngle(0f, audioAngle);
        float responseTime = Time.time - trialStartTime;

        File.AppendAllText(filePath, $"{Time.time},{audioAngle},{error},{responseTime}\n");

        if (logText != null)
        {
            logText.text =
            $"Audio angle: {audioAngle:F1}°\n" +
            $"Error: {error:F1}°\n" +
            $"RT: {responseTime:F2}s";
        }

        trialActive = false; // ready for next sound
 
    }
}
