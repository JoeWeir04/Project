using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using TMPro;
using Unity.XR.CoreUtils;

public class VRlogAngle : MonoBehaviour
{
    public InputActionReference aButton; 
    public TMP_Text logText;
    public TMP_Text ExperimentText;
    public XROrigin playerRig;
    private string filePath;
    public MicSocketVR micSocket;
    public List<Transform> spawnPoints;
    private current spawnPointCounter;
    private float trialStartTime;
    private bool trialActive = false;
    public ChangeVisual changeVisual;
    public float TrialsPerVisualCount = 6;

    private List<Trial> trials = new List<Trial>();
    private int currentTrialIndex = 0;

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


    void Start()
    {
        filePath = Application.persistentDataPath + "/VR_log.csv";
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Time,AudioSource,AudioAngle,Error,ResponseTime,Visualisation\n");
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
        float audioAngle = micSocket.angle; 
        float error = Mathf.DeltaAngle(0f, audioAngle);
        float responseTime = Time.time - trialStartTime;
        AudioSource audioSource = micSocket.currentAudioSource;

        File.AppendAllText(filePath, $"{Time.time},{audioSource},{audioAngle},{error},{responseTime},{Visualisation}\n");

        if (logText != null)
        {
            $"Audio angle: {audioAngle:F1}°\n" +
            $"Error: {error:F1}°\n" +
            $"RT: {responseTime:F2}s";
        }
        callNextSource();
    }

    public void callNextSource()
    {
        Trial t = trials[currentTrialIndex]; 
        trialStartTime = Time.time;
        playerRig.transform.SetPositionAndRotation(
            spawnPoints[t.spawnIndex].position,
            spawnPoints[t.spawnIndex].rotation
        );
        spawnPointCounter = (spawnPointCounter + 1) % spawnPoints.Count;
        micSocket.NextSource(t.audioIndex);
        trialActive = false;
    }

}
