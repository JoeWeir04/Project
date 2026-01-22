using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompassRotate : MonoBehaviour
{
    public MicSocket micSocket;
    public TMP_Text angleText;
    public float rotationspeed = 180f;
    public float visibleDuration = 1f;
    private float currentTimer = 0f;
    public float fadeSpeed = 3f;
    private Renderer[] renderers;
    private float currentAlpha = 0f;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        SetAlpha(0f);
    }

    void Update()
    {
        if (!micSocket.isConnected) return;
        float angle = micSocket.angle;
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRotation,
            rotationspeed*Time.deltaTime);

        if(angleText != null)
        {
            angleText.text = $"Angle: {angle:F1}°";
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
        foreach (Renderer r in renderers)
        {
            foreach(Material mat in r.materials)
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
        }
    }
}
