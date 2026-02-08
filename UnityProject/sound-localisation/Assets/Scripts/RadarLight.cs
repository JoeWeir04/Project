using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadarLight : MonoBehaviour
{
    public Image leftLight;
    public Image rightLight;
    public Camera mainCamera;
    public TMP_Text logText;
    public GameObject arrow;
    [SerializeField] private MonoBehaviour micSocketBehaviour;
    public IMicSocket micSocket;
    float currentAlpha = 0f;
    float currentTimer = 0f;
    public float visibleDuration = 1f;
    public float fadeSpeed = 3f;

    public float facingThreshold = 30f;
    void Awake()
    {
        SetAlpha(leftLight,0f);
        SetAlpha(rightLight,0f);
        micSocket = micSocketBehaviour as IMicSocket;
    }

    
    void OnEnable()
    {
        //arrow.SetActive(false);
    }

    void OnDisable()
    {
        //arrow.SetActive(true);
    }
    

    void Update()
    {
         if (!micSocket.isConnected) return;

        float cameraYaw = mainCamera.transform.eulerAngles.y;
        float angle = micSocket.angle;
        float alpha = micSocket.distanceProxy;

        if(angle <= facingThreshold || angle >= (360f - facingThreshold))
        {
            SetAlpha(leftLight, alpha);
            SetAlpha(rightLight, alpha);
            return;
        }
        
        if (logText != null)
        {
            logText.text = $"Angle: {angle}";
        }

        bool showRight = angle < 180f;
        bool showLeft  = !showRight;

        bool soundReceived = micSocket.vad == 1;
        Fade(soundReceived);

        if (showRight)
        {
            SetAlpha(rightLight, alpha);
            SetAlpha(leftLight,0f);
        }
        else
        {
            SetAlpha(leftLight, alpha);
            SetAlpha(rightLight, 0f);
        }        
    }


    void Fade(bool soundReceived)
    {
        if (soundReceived)
        {
            currentAlpha = micSocket.distanceProxy;
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
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }
}
