using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class SmoothRadarRotate : MonoBehaviour
{
    [SerializeField] private MonoBehaviour micSocketBehaviour;
    private IMicSocket micSocket;
    public TMP_Text angleText;
    private SpriteRenderer spriteRenderer;

    public float fadeSpeed = 3f;
    public float visibleDuration = 1f;
    public float currentTimer = 0f;
    public float currentAlpha = 0f;

    private float rotationspeed = 350f;

    public Sprite normalSprite;   

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
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        micSocket = micSocketBehaviour as IMicSocket;
        spriteRenderer.sprite = normalSprite;
        currentColor = farColor;
        SetColor(currentColor, 0f);

    }


    void Update()
    {
        if (!micSocket.isConnected) return;
        float angle = micSocket.angle;
        

        Quaternion targetRotation = Quaternion.Euler(0,0,angle-15f);
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRotation,
            rotationspeed*Time.deltaTime);
        
        if(angleText != null){
            angleText.text = $"Mic Angle: {angle:F1}°\n";
        }
        float distance = micSocket.distanceProxy;
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
        Color c = color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
