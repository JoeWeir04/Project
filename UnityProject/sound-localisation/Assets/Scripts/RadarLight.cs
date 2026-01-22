using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarLight : MonoBehaviour
{
    // Start is called before the first frame update
    public Image leftLight;
    public Image rightLight;
    public Camera mainCamera;
    public MicSocket micSocket;
    float currentAlpha = 0f;
    float currentTimer = 0f;
    public float visibleDuration = 1f;
    public float fadeSpeed = 3f;

    public float facingThreshold = 60f;
    void Awake()
    {
        SetAlpha(leftLight,0f);
        SetAlpha(rightLight,0f);
    }

    void Update()
    {
         if (!micSocket.isConnected) return;

        float cameraYaw = mainCamera.transform.eulerAngles.y;
        float angle = micSocket.angle;

        float angleDiff = Mathf.Abs(
            Mathf.DeltaAngle(cameraYaw, angle)
        );

        if(angleDiff <= facingThreshold)
        {
            SetAlpha(leftLight, 0f);
            SetAlpha(rightLight, 0f);
            return;
        }

        bool showRight = angle < 180f;
        bool showLeft  = !showRight;

        bool soundReceived = micSocket.vad == 1;

        fade(soundReceived);

       


        if (showRight)
        {
            SetAlpha(rightLight, currentAlpha);
            SetAlpha(leftLight,0f);
        }
        else
        {
            SetAlpha(leftLight, currentAlpha);
            SetAlpha(rightLight, 0f);
        }        

        
    }

    void fade(bool soundReceived)
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
    }


        void SetAlpha(Image image,float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}
