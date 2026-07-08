using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ArrowRotate : MonoBehaviour
{
    [SerializeField] private MonoBehaviour micSocketBehaviour;
    private IMicSocket micSocket;
    public TMP_Text angleText;
    public Camera mainCamera;
    private float rotationspeed = 350f;
    public float visibleDuration = 1f;
    private float currentTimer = 0f;
    public float fadeSpeed = 3f;
    private Renderer[] renderers;
    private float currentAlpha = 0f;
    public bool isVR = true;

    private Color[][] originalColors;
    private bool[] isArrowPart;
    public float colorTransitionSpeed = 3f;
    private float colorT = 0f;
    public Color normalColor = Color.green;
    public Color warningColor = Color.red;



    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        micSocket = micSocketBehaviour as IMicSocket;
        originalColors = new Color[renderers.Length][];
        isArrowPart = new bool[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            
            Material[] mats = renderers[i].materials;
            isArrowPart[i] = renderers[i].CompareTag("arrowPart");
            originalColors[i] = new Color[mats.Length];
            for (int j = 0; j < mats.Length; j++)
            {
                originalColors[i][j] = mats[j].color;
            }
        }

        SetAlpha(0f);  
    }


    void Update()
    {
        if (!micSocket.isConnected) return;
        float angle;
        float distance = micSocket.distanceProxy;
        
        if (isVR){
            angle = micSocket.angle + mainCamera.transform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

            transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationspeed*Time.deltaTime);
            distance = micSocket.distanceProxy;
    
        } else
        {
            float socketAngle = micSocket.angle;
            float cameraYaw = mainCamera.transform.eulerAngles.y;
            float combinedAngle = socketAngle + cameraYaw;

            Vector3 pos = transform.position;
            pos.x = mainCamera.transform.position.x;
            pos.z = mainCamera.transform.position.z;
            transform.position = pos;



            angle = combinedAngle;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

            transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationspeed*Time.deltaTime);

            float arrowWorldY = transform.rotation.eulerAngles.y;

            if(angleText != null)
            {
                angleText.text =  $"Socket Angle: {socketAngle:F1}\n" + $"Camera Yaw: {cameraYaw:F1}\n" + $"Combined:{combinedAngle:F1}\n" + $"Arrow World Y:{arrowWorldY:F1}\n";   
            }

        }   
        Fade(distance);
    }


    void Fade(float distance)
    {
        bool soundReceived = micSocket.vad == 1;
        float targetAlpha;

        if (soundReceived)
        {
            targetAlpha = distance;
            currentTimer = visibleDuration;
        }
        else
        {
            currentTimer -= Time.deltaTime;
            targetAlpha = currentTimer > 0 ? currentAlpha : 0f;
        }

        currentAlpha = Mathf.MoveTowards(
            currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime
        );
        float targetT = micSocket.isClose ? 1f : 0f;
        colorT = Mathf.MoveTowards(colorT, targetT, colorTransitionSpeed * Time.deltaTime);
        SetAlpha(currentAlpha);
    }
    

    void SetAlpha(float alpha)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            bool shouldTurnRed = micSocket.isClose && isArrowPart[i];
            for (int j = 0; j < mats.Length; j++)
            {   
                Color c = Color.Lerp(normalColor, warningColor, colorT);
                if (isArrowPart[i])
                {
                    c.a = alpha;
                }
                else
                {
                    c = originalColors[i][j];
                    if (alpha > 0.69f)
                    {
                        c.a = Mathf.Max(alpha - 0.5f, 0.10f);
                    }
                    else
                    {
                        c.a = Mathf.Max(alpha - 0.2f, 0.10f);
                    }
                    
                }
                mats[j].color = c;
            }
            renderers[i].materials = mats;
        }
    }
}
