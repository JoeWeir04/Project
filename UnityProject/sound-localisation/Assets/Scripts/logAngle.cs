using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using TMPro;

public class logAngle : MonoBehaviour
{
    public InputActionReference aButton; // Bind to A button
    public TMP_Text logText;
    public TMP_Text ExperimentText;
    private string filePath;
    float[] calibrationAngles = new float[9];
    int calibrationCount = 9;
    int calibrationIndex = 0;
    int[][] trialOrders = new int[][]
    {
        new int[] {2,5,0,8,1,6,3,4,7},
        new int[] {7,1,4,0,6,2,8,5,3},
        new int[] {0,3,5,7,2,8,6,1,4}
    };
    int totalRounds = 3;
    int experimentRound = 0;
    int step = 0;

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
        aButton.action.performed += OnButtonPress;
    }

    private void OnDestroy()
    {
        aButton.action.performed -= OnButtonPress;
        aButton.action.Disable();
    }

    public void OnButtonPress(InputAction.CallbackContext context)
    {
        float azimuth = Camera.main.transform.eulerAngles.y;
        float time = Time.time;

        if (calibrationIndex < calibrationCount)
        {
            calibrationAngles[calibrationIndex] = azimuth;
            if(logText != null){
                logText.text = $"Calibrating {calibrationIndex + 1}/{calibrationCount}\n" +
                    $"Azimuth: {azimuth:F1}°";
                 calibrationIndex++;}
            }
        else{
        if(experimentRound < totalRounds)
        {   
            ExperimentText.text =  $"Round: {experimentRound+1}\n" + $"Step: {step+1}"; 
            float error = Mathf.DeltaAngle(azimuth, calibrationAngles[trialOrders[experimentRound][step]]);  
            File.AppendAllText(filePath, $"{time},{azimuth}\n");
            if(logText != null)
            {
                logText.text = $"logged: {error:F1}°";
            }
            step++;
            if(step >= calibrationCount)
                {
                    experimentRound++;
                    step = 0;
                }
        }
        }
        
    }
}
