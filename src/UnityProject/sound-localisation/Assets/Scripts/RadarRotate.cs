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

    public Color normalColor = Color.green;
    public Color warningColor = Color.red;
    private float rotationspeed = 350f;
    private Color originalColor;

    public Sprite normalSprite;   
    public float colorTransitionSpeed = 3f;
    private float colorT = 0f;


    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        micSocket = micSocketBehaviour as IMicSocket;
        spriteRenderer.sprite = normalSprite;
        SetAlpha(0f);

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
        Fade();
    }


    void Fade()
    {
        bool soundReceived = micSocket.vad == 1;
        float targetAlpha;

        if (soundReceived)
        {
            targetAlpha = micSocket.distanceProxy;
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
        Color c = Color.Lerp(normalColor, warningColor, colorT);
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
