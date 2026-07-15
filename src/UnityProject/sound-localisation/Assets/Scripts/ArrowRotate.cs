using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ArrowRotate : MonoBehaviour
{
    [SerializeField] private MonoBehaviour micSocketBehaviour;
    private IMicSocket micSocket;
    public TMP_Text angleText;
    public bool isVR = true;
    public Camera mainCamera;
    private float rotationspeed = 350f;

    private Renderer[] renderers;
    private bool[] isArrowPart;
    private Color[][] originalColors;


    [Header("Sound-Triggered Fade")]
    public float visibleDuration = 1f;
    private float currentTimer = 0f;
    public float fadeSpeed = 3f;
    private float currentAlpha = 0f;


    [Header("Distance Color Encoding")]
    private Color nearColor = new Color(253f / 255f, 231f / 255f, 37f / 255f);
    private Color mediumColor = new Color(33f / 255f, 145f / 255f, 140f / 255f);
    private Color farColor = new Color(68f / 255f, 1f / 255f, 84f / 255f);
    private float colorTransitionSpeed = 3f;
    private Color currentColor;
    private const float farDistance = 0.2f;
    private const float mediumDistance = 0.5f;
    private const float nearDistance = 1f;





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
        currentColor = farColor;
        SetColor(currentColor, 0f);
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
        UpdateColor(distance);
        UpdateFade();
        SetColor(currentColor,currentAlpha);

    }

    void UpdateColor(float distance)
    {
        Color targetColor = GetColorForDistance(distance);
 
        currentColor = new Color(
            Mathf.MoveTowards(currentColor.r, targetColor.r, colorTransitionSpeed * Time.deltaTime),
            Mathf.MoveTowards(currentColor.g, targetColor.g, colorTransitionSpeed * Time.deltaTime),
            Mathf.MoveTowards(currentColor.b, targetColor.b, colorTransitionSpeed * Time.deltaTime)
        );

    }


    Color GetColorForDistance(float distance)
    {
        if (distance <= farDistance)
        {
            return farColor;
        }
        else if (distance <= mediumDistance)
        {
            return mediumColor;
        }
        else
        {
            return nearColor;
        }
    }


    void UpdateFade()
    {
        bool soundReceived = micSocket.vad == 1;
        float targetAlpha;
 
        if (soundReceived)
        {
            targetAlpha = 1f;
            currentTimer = visibleDuration;
        }
        else
        {
            currentTimer -= Time.deltaTime;
            targetAlpha = currentTimer > 0 ? currentAlpha : 0f;
        }
 
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }


    void SetColor(Color color, float alpha)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                Color c;
                if (isArrowPart[i])
                {
                    c = color;
                    c.a = alpha;
                }
                else
                {
                    c = originalColors[i][j];
                    c.a = 0.4f;
                }
                mats[j].color = c;
            }
            renderers[i].materials = mats;
        }
    }


}
