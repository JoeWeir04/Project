using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompassRotate : MonoBehaviour
{
    [SerializeField] private MonoBehaviour micSocketBehaviour;
    private IMicSocket micSocket;
    public TMP_Text angleText;
    public Camera mainCamera;
    public float rotationspeed = 180f;
    public float visibleDuration = 1f;
    private float currentTimer = 0f;
    public float fadeSpeed = 3f;
    private Renderer[] renderers;
    private float currentAlpha = 0f;
    public bool isVR = true;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        SetAlpha(0f);

        micSocket = micSocketBehaviour as IMicSocket;
    }

    void Update()
    {
        if (!micSocket.isConnected) return;
        float angle;
        float distance = micSocket.distanceProxy; 
        if (isVR){
            angle = micSocket.angle + mainCamera.transform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

            transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationspeed*Time.deltaTime);
            distance = micSocket.distanceProxy;
            Debug.Log($"This is the distance being set {distance}");
            if(angleText != null)
            {
                angleText.text = $"Distance: {distance:F1}°";
                
            }
            SetAlpha(distance);
        } else
        {
            angle = micSocket.angle;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

            transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRotation,
            rotationspeed*Time.deltaTime);
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
