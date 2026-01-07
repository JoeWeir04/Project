using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RadarRotate : MonoBehaviour
{
    public MicSocket micSocket;
    public float binSize = 30f;
    public TMP_Text angleText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
    }
}
