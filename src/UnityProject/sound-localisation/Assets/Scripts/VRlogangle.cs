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
    [SerializeField] private bool isPractice = true;
    private List<Trial> experimentTrials = new List<Trial>();
    private List<Trial> practiceTrials = new List<Trial>();
    private int currentTrialIndex = 0;
    private int practiceTrialIndex = 0;
    public int currentPid = 0;
    public string pidFileName = "last_pid.txt";
    private string pidFilePath;

    public TMP_Text pidText;
    public TMP_Text pidTextClone;

    public TMP_Text errorText;
    public float pathLogInterval = 0.16f;
    private float nextPathLogTime = 0f;
    private string pathFilePath;
    private bool onBreak;


    public AudioSource feedbackAudioSource;
    public AudioClip experimentStart;
    public AudioClip experimentFinish;     
    public AudioClip conditionBegun;
    public AudioClip conditionComplete;
    public AudioClip firstPractice;
    public AudioClip conditionPracticeBegun;



    [System.Serializable]
    public struct Trial
    {
        public int spawnIndex;
        public int audioIndex;
        
    }

    private List<Trial> CurrentTrials => isPractice ? practiceTrials : experimentTrials;
    private int CurrentTrialIndex
    {
        get => isPractice ? practiceTrialIndex : currentTrialIndex;
        set { if (isPractice) practiceTrialIndex = value; else currentTrialIndex = value; }
    }


    List<Trial> GenerateTrials()
    {
        List<Trial> tempTrials = new List<Trial>();
        tempTrials.Clear();
        for (int s = 0; s < spawnPoints.Count; s++)
        {
            for (int a = 0; a < micSocket.audioSources.Count; a++)
            {
                tempTrials.Add(new Trial
                {
                    spawnIndex = s,
                    audioIndex = a
                });
            }
        }
        int removeSpawnIndex = 4;
        int removeAudioIndex = 1;
        tempTrials.RemoveAll(t => t.spawnIndex == removeSpawnIndex && t.audioIndex == removeAudioIndex);
        Shuffle(tempTrials);
        Debug.LogError($"Size of trials: {tempTrials.Count}");
        return tempTrials;
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
        experimentTrials = GenerateTrials();
        practiceTrials = GenerateTrials();
        nextPathLogTime = Time.time + pathLogInterval;
        filePath = Application.persistentDataPath + "/Experiment_log.csv";
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "PID,Time,TrialIndex,SpawnIndex,AudioIndex,AudioAngle,absError,DistanceFromSource,ResponseTime,Visualisation\n");
        }

        pathFilePath = Application.persistentDataPath + "/Experiment_paths.csv";
        if (!File.Exists(pathFilePath))
        {
            File.WriteAllText(pathFilePath, "PID,Time,TrialIndex,Visualisation,PosX,PosY,PosZ,RotY\n"
            );
        }
        CallNextSource();
        SetSpawnPointsInvisible();
    }


    void Update()
    {
        if(!isPractice && trialActive &&Time.time >= nextPathLogTime)
        {
            LogPathSample();
            nextPathLogTime = Time.time + pathLogInterval; 
        }
    }


    private void Awake()
    {
        aButton.action.Enable();
        aButton.action.performed += OnButtonPress;
        startExperimentButton.action.Enable();
        startExperimentButton.action.performed += changeVisualFalse;
        pidFilePath = Path.Combine(Application.persistentDataPath, pidFileName);
        currentPid = GetPid();
        changeVisual.allowChange = true;
        if (pidText != null)
        {
            pidText.text = pidTextClone.text = $"PID: {currentPid+1}";
        }
        
    }


    private int GetPid()
    {
        if (File.Exists(pidFilePath))
        {
            try
            {
                string content = File.ReadAllText(pidFilePath).Trim();
                int pid = int.Parse(content);
                return pid; 
            }
            catch(System.Exception e)
            {
                Debug.LogError($"Failed to read PID file: {e.Message}, will start from 0");
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }


    private void WritePid(int pid)
    {
        try
        {
            File.WriteAllText(pidFilePath,pid.ToString());
            Debug.Log($"Saved PID {pid} to file");
        }
        catch(System.Exception e){
            Debug.Log($"Failed to save PID to file: {e.Message}");
        }
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
        PlayClip(experimentStart);
        changeVisual.allowChange = false;
        nextPathLogTime = Time.time + pathLogInterval;
        currentPid += 1;
        WritePid(currentPid);
        if (pidText != null)
        {
            pidText.text = pidTextClone.text = $"PID: {currentPid}";
        }
        isPractice = false;
        currentTrialIndex = 0;
        trialActive = true;
        experimentTrials = GenerateTrials();   
        ExperimentText.text =  "Started";
        Debug.Log("Logging enabled");
        CallNextSource();
        startExperimentButton.action.performed -= StartExperiment;
    }

    private void changeVisualFalse(InputAction.CallbackContext ctx)
    {
        PlayClip(firstPractice);
        changeVisual.allowChange = false;
        startExperimentButton.action.performed -= changeVisualFalse          ;
        startExperimentButton.action.performed += StartExperiment;

    }

    private void NextConditionPractice(InputAction.CallbackContext ctx)
    {
        isPractice = true;
        changeVisual.allowChange = false;
        trialActive = false;
        practiceTrialIndex = 0;
        PlayClip(conditionPracticeBegun);
        ExperimentText.text  = "Practice";
        startExperimentButton.action.performed -= NextConditionPractice;
        startExperimentButton.action.performed += NextCondition;
    }

    private void NextCondition(InputAction.CallbackContext ctx)
    {
        isPractice = false;
        changeVisual.allowChange = false;
        trialActive = true;
        onBreak = true; 
        ExperimentText.text = "Press button to begin";
        ExperimentText.fontSize -= 20f;
        PlayClip(conditionBegun);
        startExperimentButton.action.performed -= NextCondition;
    }
        


    public void OnButtonPress(InputAction.CallbackContext context)
    {
        if (!trialActive && !isPractice)
        {
           return; 
        }   
        if(!isPractice && !onBreak)
        {
            float audioAngle = micSocket.realAngle; 
            float signedError = Mathf.DeltaAngle(0f, audioAngle);
            float absError = Mathf.Abs(signedError);
            float responseTime = Time.time - trialStartTime;
            AudioSource audioSource = micSocket.currentAudioSource;
            int visualisation = changeVisual.visualCounter+1;
            float distance = micSocket.realDistance;
            try
            {
                File.AppendAllText(filePath, $"{currentPid},{Time.time},{currentTrialIndex-1},{experimentTrials[currentTrialIndex-1].spawnIndex},{experimentTrials[currentTrialIndex-1].audioIndex},{audioAngle},{absError},{distance},{responseTime},{visualisation}\n");
            }
            catch(System.Exception)
            {
                if(errorText != null)
                {
                    errorText.text=$"Error: Cant write to log file. Restart application.";
                    trialActive = false;
                }
            }
            if (logText != null)
            {
                logText.text = 
                $"Audio angle: {audioAngle:F1}°\n" +
                $"Error: {absError:F1}°\n" +
                $"RT: {responseTime:F2}s";
            }
        }
        CallNextSource();
    }


    private void LogPathSample()
    {
        try
        {
            Vector3 pos = playerRig.Camera.transform.position;
            float rotY = playerRig.Camera.transform.eulerAngles.y;

            int trialIndexForLog = currentTrialIndex - 1;
            int visualisation = changeVisual.visualCounter + 1;

            string line =
                $"{currentPid},{Time.time},{trialIndexForLog},{visualisation},f{pos.x},{pos.y},{pos.z},{rotY}\n";
            File.AppendAllText(pathFilePath, line);
        }
        catch (System.Exception)
        {
            if (errorText != null)
            {
                errorText.text = "Error: Cant write to path log file. Restart application.";
                trialActive = false;
            }
        }
    }

    public void CallNextSource()
    {
        ExperimentText.fontSize = 45f;
        nextPathLogTime = Time.time + pathLogInterval;
        List<Trial> activeTrials = CurrentTrials;
        int idx = CurrentTrialIndex;
        
        if (idx >= activeTrials.Count)
        {
            if (isPractice)
            {
                idx = 0;
                CurrentTrialIndex = 0;
            }
            else
            {
                PlayClip(experimentFinish);
                ExperimentText.text = "Finished";
                trialActive = false;
                return;
            }
        }
        if (idx % 7 == 0 && idx > 0 && onBreak==false && !isPractice)
        {
            PlayClip(conditionComplete);
            ExperimentText.text = "Break";
            changeVisual.allowChange = true;
            onBreak = true;
            trialActive = false;
            startExperimentButton.action.performed += NextConditionPractice;
            return;
        }
        onBreak = false;
        trialActive = true;
        
        if (isPractice)
        {
            ExperimentText.text = $"Practice";
        }
        else{
            ExperimentText.text = $"Trial: {idx+1} / {experimentTrials.Count}";
        }
        Trial t = activeTrials[idx];
        trialStartTime = Time.time;
        playerRig.transform.SetPositionAndRotation(
            spawnPoints[t.spawnIndex].position,
            spawnPoints[t.spawnIndex].rotation
        );
        micSocket.NextSource(t.audioIndex);
        CurrentTrialIndex = idx + 1;
    }

    void PlayClip(AudioClip clip)
    {
        if (feedbackAudioSource != null && clip != null)
        {
            feedbackAudioSource.PlayOneShot(clip);
        }
    }

}
