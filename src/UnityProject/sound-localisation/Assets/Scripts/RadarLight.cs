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
    public Camera mainCamera;
    public TMP_Text logText;
    [SerializeField] private MonoBehaviour micSocketBehaviour;
    public IMicSocket micSocket;
    float currentAlpha = 0f;
    private float currentLeftAlpha = 0f;
    private float currentRightAlpha = 0f;
    public float sideFadeSpeed = 3f;
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


    [Header("Distance Color Encoding")]
    private Color nearColor = new Color(253f / 255f, 231f / 255f, 37f / 255f);
    private Color mediumColor = new Color(85f / 255f, 198f / 255f, 104f / 255f);
    private Color farColor = new Color(35f / 255f, 137f / 255f, 141f / 255f);
    private float colorTransitionSpeed = 3f;
    private Color currentColor;

    private const float farDistance = 0.2f;
    private const float mediumDistance = 0.5f;
    private const float nearDistance = 1f;


    /*
    public float pushOffset = 50f;
    public float positionTransitionSpeed = 5f;
    private Vector2 leftBasePos;
    private Vector2 rightBasePos;
    private Vector2 leftCurrentPos;
    private Vector2 rightCurrentPos;

    private const float farDistance = 0.2f;
    private const float mediumDistance = 0.5f;
    private const float nearDistance = 1f;
    */
    
    
    
    void Awake()
    {
        micSocket = micSocketBehaviour as IMicSocket;
        currentColor = farColor;
        leftBaseScale = leftLight.rectTransform.localScale;
        rightBaseScale = rightLight.rectTransform.localScale;

        SetColor(leftLight, currentColor, 0f);
        SetColor(rightLight, currentColor, 0f);

    }


    void Update()
    {
        if (micSocket == null || !micSocket.isConnected)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, fadeSpeed * Time.deltaTime);
            SetColor(leftLight,currentColor, currentAlpha);
            SetColor(rightLight,currentColor, currentAlpha);
            return;
        } 
        UpdateScales();
        if (!micSocket.isConnected) return;

        bool soundReceived = micSocket.vad == 1;
        float distance = micSocket.distanceProxy;
        float cameraYaw = mainCamera.transform.eulerAngles.y;
        float angle = micSocket.angle;

        if (logText != null)
            {
                logText.text = $"Angle: {angle} \n Facing threshold: {facingThreshold} \n bool: {angle <= facingThreshold || angle >= (360f - facingThreshold)}";
            }

        Fade(soundReceived);
        UpdateColor(distance);
        
        float targetLeftAlpha;
        float targetRightAlpha;
        
        if(angle <= facingThreshold/2 || angle >= (360f - facingThreshold/2))
        {
            targetLeftAlpha = currentAlpha;
            targetRightAlpha = currentAlpha;

            Vector3 rScale = rightBaseScale;
            rScale.y *= 1f;
            rightTargetScale = rScale;

            Vector3 lScale = leftBaseScale;
            lScale.y *= 1f;
            leftTargetScale = lScale;
        }
        else
        {
            bool showRight = angle > 0f && angle < 180f;
            float degreesFromCentre = Mathf.Abs(Mathf.DeltaAngle(angle, 0f));
            distanceFromCenter = 1f - Mathf.Clamp(degreesFromCentre / 180f, 0f, 0.8f);

            if (showRight)
            {
                Vector3 rScale = rightBaseScale;
                rScale.y *= distanceFromCenter;
                rightTargetScale = rScale;

                leftTargetScale = leftBaseScale;
                leftTargetScale.y = 0f;

                targetLeftAlpha = 0f;
                targetRightAlpha = currentAlpha;
            }
            else
            {
                Vector3 lScale = leftBaseScale;
                lScale.y *= distanceFromCenter;
                leftTargetScale = lScale;

                rightTargetScale = rightBaseScale;
                rightTargetScale.y = 0f;

                targetLeftAlpha = currentAlpha;
                targetRightAlpha = 0f;
            }   
        }
        
        currentLeftAlpha = Mathf.MoveTowards(currentLeftAlpha, targetLeftAlpha, sideFadeSpeed * Time.deltaTime);
        currentRightAlpha = Mathf.MoveTowards(currentRightAlpha, targetRightAlpha, sideFadeSpeed * Time.deltaTime);
        
        SetColor(leftLight, currentColor, currentLeftAlpha);
        SetColor(rightLight, currentColor, currentRightAlpha);
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


    /*
     void UpdatePositions()
    {

        Vector2 leftTargetPos = isClose ? leftBasePos + new Vector2(-pushOffset, 0f) : leftBasePos;
        Vector2 rightTargetPos = isClose ? rightBasePos + new Vector2(pushOffset, 0f) : rightBasePos;

        leftCurrentPos = Vector2.MoveTowards(leftCurrentPos, leftTargetPos, positionTransitionSpeed * Time.deltaTime);
        rightCurrentPos = Vector2.MoveTowards(rightCurrentPos, rightTargetPos, positionTransitionSpeed * Time.deltaTime);

        leftLight.rectTransform.anchoredPosition = leftCurrentPos;
        rightLight.rectTransform.anchoredPosition = rightCurrentPos;
    }
    */


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
                currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, fadeSpeed * Time.deltaTime);
            }
        }
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


    void SetColor(Image image, Color color, float alpha)
    {
        Color c = color;
        c.a = alpha;
        image.color = c;
    }

}
