using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using TMPro;

public class logAngle : MonoBehaviour
{
    public InputActionReference aButton; // Bind to A button

    public TMP_Text logText;

    private string filePath;

    void Start()
    {
        filePath = Application.persistentDataPath + "/azimuth_log.csv";

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Time,Azimuth\n");
        }
    }

    private void Awake()
    {
        aButton.action.Enable();
    }
    void OnEnable()
    {
        aButton.action.Enable();
        aButton.action.performed += LogAzimuth;
    }

    void OnDisable()
    {
        aButton.action.performed -= LogAzimuth;
        aButton.action.Disable();
    }

    private void onDestory()
    {
        aButton.action.Disable();
    }

    public void LogAzimuth(InputAction.CallbackContext context)
    {
        float azimuth = Camera.main.transform.eulerAngles.y;
        float time = Time.time;

        File.AppendAllText(filePath, $"{time},{azimuth}\n");

        Debug.Log($"Azimuth logged: {azimuth}°");
        if(logText != null)
        {
            logText.text = $"logged: {azimuth:F1}°";
        }
    }
}
