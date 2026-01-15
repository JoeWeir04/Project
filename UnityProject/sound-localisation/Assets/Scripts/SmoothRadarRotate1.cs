using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SmoothRadarRotate : MonoBehaviour
{
    public MicSocket micSocket;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (!micSocket.isConnected) return;
        float angle = micSocket.angle - 7.5f;

        transform.localRotation = Quaternion.Euler(0,0,angle);
        string classification = micSocket.classification;
        
        if(angleText != null){
        
            angleText.text = $"Mic Angle: {angle:F1}°\n" + $"Classification:{classification}\n";
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
