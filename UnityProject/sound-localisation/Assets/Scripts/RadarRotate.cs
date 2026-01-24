using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RadarRotate : MonoBehaviour
{
    [SerializeField] private MonoBehaviour micSocketBehaviour;
    public IMicSocket micSocket;
    public float binSize = 30f;
    public TMP_Text angleText;
    private SpriteRenderer spriteRenderer;
    public float fadeSpeed = 3f;
    public float visibleDuration = 1f;
    public float currentTimer = 0f;
    public float currentAlpha = 0f;


    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        SetAlpha(0f);

        micSocket = micSocketBehaviour as IMicSocket;
    }

        void Update()
    {
        if (!micSocket.isConnected) return;
        float angle = micSocket.angle;

        float binAngle = Mathf.Floor(angle/binSize) * binSize;
        binAngle = (binAngle + 360f) % 360f;
        transform.localRotation = Quaternion.Euler(0,0,binAngle);
        string classification = micSocket.classification;
        
        if(angleText != null){
        
            angleText.text = $"Bin Angle: {binAngle:F1}°\n" + $"Mic Angle: {angle:F1}°\n" + $"Classification:{classification}\n";
        }
        fade();
    }

    void fade()
    {
        bool soundReceived = micSocket.vad ==1;
        if (soundReceived)
        {
            currentAlpha = 1f;
            currentTimer = visibleDuration;
        }
        else
        {
            currentTimer -= Time.deltaTime;
            if(currentTimer <= 0)
            {
                currentAlpha = Mathf.MoveTowards(
                    currentAlpha,0f,fadeSpeed * Time.deltaTime
                );
            }
        }
        SetAlpha(currentAlpha);
    }

    void SetAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
