using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompassRotate : MonoBehaviour
{
    public MicSocket micSocket;
    public TMP_Text angleText;
    public float rotationspeed = 180f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
        
    }
}
