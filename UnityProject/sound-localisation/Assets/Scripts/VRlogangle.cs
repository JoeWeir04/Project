using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using TMPro;
using Unity.XR.CoreUtils;
using System.Collections.Generic;

public class VRlogAngle : MonoBehaviour
{
    public InputActionReference aButton; 
    public InputActionReference startExperimentButton;
    public TMP_Text logText;
    public TMP_Text ExperimentText;
    public XROrigin playerRig;
    private string filePath;
    public MicSocketVR micSocket;
    public List<Transform> spawnPoints;
    private float trialStartTime;
    private bool trialActive = false;
    public ChangeVisual changeVisual;
    public float TrialsPerVisualCount = 6;
    [SerializeField] private bool isPractice = true;
    private List<Trial> trials = new List<Trial>();
    private int currentTrialIndex = 0;

    [System.Serializable]
    public struct Trial
{
    public int spawnIndex;
    public int audioIndex;
}

    void GenerateTrials()
    {
        trials.Clear();
        for (int s = 0; s < spawnPoints.Count; s++)
        {
            for (int a = 0; a < micSocket.audioSources.Count; a++)
            {
                trials.Add(new Trial
                {
                    spawnIndex = s,
                    audioIndex = a
                });
            }
        }
        Shuffle(trials);
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    void SetSpawnPointsInvisible(){
        foreach (Transform spawn in spawnPoints)
        {
            MeshRenderer mr = spawn.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.enabled = false;
            }
        }
    }

    void Start()
    {
        filePath = Application.persistentDataPath + "/VR_log.csv";
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Time,SpawnIndex,AudioIndex,AudioAngle,absError,DistanceFromSource,ResponseTime,Visualisation\n");
        }
        GenerateTrials();
        callNextSource();
        SetSpawnPointsInvisible();
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
        startExperimentButton.action.Enable();
        startExperimentButton.action.performed += StartExperiment;
    }

    private void OnDestroy()
    {
        aButton.action.performed -= OnButtonPress;
        aButton.action.Disable();
        startExperimentButton.action.performed -= StartExperiment;
        startExperimentButton.action.Disable();
    }

    private void StartExperiment(InputAction.CallbackContext ctx)
    {
        isPractice = false;

        currentTrialIndex = 0;
        trialActive = false;
        GenerateTrials();   
        ExperimentText.text = "Started";
        Debug.Log("Logging enabled");
        callNextSource();
    }

    public void OnButtonPress(InputAction.CallbackContext context)
    {
        if (!trialActive)
            return;
        if(!isPractice)
        {
            float audioAngle = micSocket.angle; 
            float signedError = Mathf.DeltaAngle(0f, audioAngle);
            float absError = Mathf.Abs(signedError);
            float responseTime = Time.time - trialStartTime;
            AudioSource audioSource = micSocket.currentAudioSource;
            int visualisation = changeVisual.visualCounter+1;
            float distance = micSocket.realDistance;

            File.AppendAllText(filePath, $"{Time.time},{trials[currentTrialIndex].spawnIndex},{trials[currentTrialIndex].audioIndex},{audioAngle},{absError},{distance},{responseTime},{visualisation}\n");

            if (logText != null)
            {
                logText.text = 
                $"Audio angle: {audioAngle:F1}°\n" +
                $"Error: {absError:F1}°\n" +
                $"RT: {responseTime:F2}s";
            }
        }
        callNextSource();
    }

    public void callNextSource()
    {
        if (currentTrialIndex >= trials.Count)
        {
            ExperimentText.text = "Finished";
            return;
        }
        if (isPractice)
        {
            ExperimentText.text = $"Practice";
        }
        else{
            ExperimentText.text = $"Trial: {currentTrialIndex+1} / {trials.Count}";
        }
        Trial t = trials[currentTrialIndex]; 
        trialStartTime = Time.time;
        playerRig.transform.SetPositionAndRotation(
            spawnPoints[t.spawnIndex].position,
            spawnPoints[t.spawnIndex].rotation
        );
        micSocket.NextSource(t.audioIndex);
        trialActive = false;
        currentTrialIndex++;
    }

}
