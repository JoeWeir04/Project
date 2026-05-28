using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ArrowRotate : MonoBehaviour
{
    [SerializeField] private MonoBehaviour micSocketBehaviour;
    private IMicSocket micSocket;
    public TMP_Text angleText;
    public Camera mainCamera;
    private float rotationspeed = 400f;
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
    
            SetAlpha(distance);
        } else
        {
            float socketAngle = micSocket.angle;
            float cameraYaw = mainCamera.transform.eulerAngles.y;
            float combinedAngle = socketAngle + cameraYaw;

            Vector3 pos = transform.position;
            pos.x = mainCamera.transform.position.x;
            pos.z = mainCamera.transform.position.z;
            transform.position = pos;



            angle = combinedAngle;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

            transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationspeed*Time.deltaTime);

            float arrowWorldY = transform.rotation.eulerAngles.y;

            if(angleText != null)
            {
                angleText.text =  $"Socket Angle: {socketAngle:F1}\n" + $"Camera Yaw: {cameraYaw:F1}\n" + $"Combined:{combinedAngle:F1}\n" + $"Arrow World Y:{arrowWorldY:F1}\n";   
            }

        }   
        Fade(distance);
    }


    void Fade(float distance)
    {
        bool soundReceived = micSocket.vad ==1;
        if (soundReceived)
        {
            currentAlpha = distance;
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
