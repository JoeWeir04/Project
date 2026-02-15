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
    Vector3 leftBaseScale;
    Vector3 rightBaseScale;
    private float distanceFromCenter = 1f;

    public float facingThreshold = 30f;
    void Awake()
    {
        leftBaseScale = leftLight.rectTransform.localScale;
        rightBaseScale = rightLight.rectTransform.localScale;
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

        float distance = micSocket.distanceProxy;
        float cameraYaw = mainCamera.transform.eulerAngles.y;
        float angle = micSocket.angle;
        float distanceScale = Mathf.Clamp(micSocket.distanceProxy, 0.2f, 1f);

        Vector3 newScale = leftBaseScale;
        newScale.y *= distanceScale;
        leftLight.rectTransform.localScale = newScale;

        Vector3 rightScale = rightBaseScale;
        rightScale.y *= distanceScale;
        rightLight.rectTransform.localScale = rightScale;


        if(angle <= facingThreshold || angle >= (360f - facingThreshold))
        {
            SetAlpha(leftLight, 1f);
            SetAlpha(rightLight, 1f);
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

        float degreesFromCentre = Mathf.Abs(Mathf.DeltaAngle(angle, 0f));
        distanceFromCenter = 1f - Mathf.Clamp(degreesFromCentre / 180f, 0f, 0.9f);

        if (showRight)
        {
            SetAlpha(rightLight, distanceFromCenter);
            SetAlpha(leftLight, 0f);
        }
        else
        {
            SetAlpha(leftLight, distanceFromCenter);
            SetAlpha(rightLight, 0f);
        }        
    }


    void Fade(bool soundReceived)
    {
        if (soundReceived)
        {
            currentAlpha = distanceFromCenter;
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
