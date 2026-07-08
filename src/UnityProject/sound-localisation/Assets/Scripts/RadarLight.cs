using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadarLight : MonoBehaviour
{
    public Image leftLight;
    public Image rightLight;
    public Sprite leftSprite; 
    public Sprite rightSprite; 
    public Sprite redLeftLight;  
    public Sprite redRightLight;
    public Camera mainCamera;
    public TMP_Text logText;
    public GameObject arrow;
    [SerializeField] private MonoBehaviour micSocketBehaviour;
    public IMicSocket micSocket;
    float currentAlpha = 0f;
    private float currentLeftAlpha = 0f;
    private float currentRightAlpha = 0f;
    public float sideFadeSpeed = 5f;
    float currentTimer = 0f;
    public float visibleDuration = 1f;
    public float fadeSpeed = 3f;
    Vector3 leftBaseScale;
    Vector3 rightBaseScale;
    private Vector3 leftTargetScale;
    private Vector3 rightTargetScale;
    public float scaleTransitionSpeed = 5f;
    private float distanceFromCenter = 1f;

    public float facingThreshold = 30f;

    public float colorTransitionSpeed = 3f;
    private float colorT = 0f;
    public Color normalColor = Color.green;
    public Color warningColor = Color.red;

    public float pushOffset = 50f;
    public float positionTransitionSpeed = 5f;
    private Vector2 leftBasePos;
    private Vector2 rightBasePos;
    private Vector2 leftCurrentPos;
    private Vector2 rightCurrentPos;
    
    
    
    void Awake()
    {
        micSocket = micSocketBehaviour as IMicSocket;
        leftBaseScale = leftLight.rectTransform.localScale;
        rightBaseScale = rightLight.rectTransform.localScale;
        leftTargetScale = leftBaseScale;
        rightTargetScale = rightBaseScale;

        leftBasePos = leftLight.rectTransform.anchoredPosition;
        rightBasePos = rightLight.rectTransform.anchoredPosition;
        leftCurrentPos = leftBasePos;
        rightCurrentPos = rightBasePos;

        SetAlpha(leftLight,0f);
        SetAlpha(rightLight,0f);
        
    }


    void Update()
    {
        UpdateScales();
        UpdatePositions();
         if (!micSocket.isConnected) return;

        bool soundReceived = micSocket.vad == 1;
        float distance = micSocket.distanceProxy;
        float cameraYaw = mainCamera.transform.eulerAngles.y;
        float angle = micSocket.angle;
        float distanceScale = Mathf.Clamp(micSocket.distanceProxy, 0.2f, 1f);

        Vector3 newScale = leftBaseScale;
        newScale.y *= distanceScale;
        leftTargetScale = newScale;
        Vector3 rightScale = rightBaseScale;
        rightScale.y *= distanceScale;
        rightTargetScale = rightScale;

        if (logText != null)
            {
                logText.text = $"Angle: {angle} \n Facing threshold: {facingThreshold} \n bool: {angle <= facingThreshold || angle >= (360f - facingThreshold)}";
            }

        Fade(soundReceived);
        float targetLeftAlpha;
        float targetRightAlpha;
        
        if(angle <= facingThreshold || angle >= (360f - facingThreshold))
        {
            targetLeftAlpha = currentAlpha;
            targetRightAlpha = currentAlpha;
        }
        else
        {
            bool showRight = angle < 180f;

            float degreesFromCentre = Mathf.Abs(Mathf.DeltaAngle(angle, 0f));
            distanceFromCenter = 1f - Mathf.Clamp(degreesFromCentre / 180f, 0f, 0.9f);

            if (showRight)
            {
                targetRightAlpha = distanceFromCenter * currentAlpha;
                targetLeftAlpha = 0f;
            }
            else
            {
                targetLeftAlpha = distanceFromCenter * currentAlpha;
                targetRightAlpha = 0f;
            }     
        }
        
        currentLeftAlpha = Mathf.MoveTowards(currentLeftAlpha, targetLeftAlpha, sideFadeSpeed * Time.deltaTime);
        currentRightAlpha = Mathf.MoveTowards(currentRightAlpha, targetRightAlpha, sideFadeSpeed * Time.deltaTime);
        SetAlpha(leftLight, currentLeftAlpha);
        SetAlpha(rightLight, currentRightAlpha);
    }

    void UpdateScales()
    {
        leftLight.rectTransform.localScale = Vector3.MoveTowards(
            leftLight.rectTransform.localScale,
            leftTargetScale,
            scaleTransitionSpeed * Time.deltaTime
        );
        rightLight.rectTransform.localScale = Vector3.MoveTowards(
            rightLight.rectTransform.localScale,
            rightTargetScale,
            scaleTransitionSpeed * Time.deltaTime
        );
    }


     void UpdatePositions()
    {
        bool isClose = micSocket.isConnected && micSocket.isClose;

        Vector2 leftTargetPos = isClose ? leftBasePos + new Vector2(-pushOffset, 0f) : leftBasePos;
        Vector2 rightTargetPos = isClose ? rightBasePos + new Vector2(pushOffset, 0f) : rightBasePos;

        leftCurrentPos = Vector2.MoveTowards(leftCurrentPos, leftTargetPos, positionTransitionSpeed * Time.deltaTime);
        rightCurrentPos = Vector2.MoveTowards(rightCurrentPos, rightTargetPos, positionTransitionSpeed * Time.deltaTime);

        leftLight.rectTransform.anchoredPosition = leftCurrentPos;
        rightLight.rectTransform.anchoredPosition = rightCurrentPos;
    }


    void Fade(bool soundReceived)
    {
        if (soundReceived)
        {
            
            currentAlpha = 1f;
            currentTimer = visibleDuration;
        }
        else
        {
            currentTimer -= Time.deltaTime;
            if (currentTimer <= 0f)
            {
                currentAlpha = Mathf.MoveTowards(
                    currentAlpha,
                    0f,
                    fadeSpeed * Time.deltaTime
                );
            }
        }
        float targetT = micSocket.isClose ? 1f : 0f;
        colorT = Mathf.MoveTowards(colorT, targetT, colorTransitionSpeed * Time.deltaTime);
    }


void SetAlpha(Image image,float alpha)
    {
        {
            Color c = Color.Lerp(normalColor, warningColor, colorT);
            c.a = alpha;
            image.color = c;
        }
    }
}
