using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarRotate : MonoBehaviour
{
    public MicSocket micSocket;
    public float binSize = 30f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!micSocket.isConnected) return;
        float angle = micSocket.angle;

        float binAngle = Mathf.Round(angle/binSize) * binSize;
        binAngle -= 195f;
        binAngle = (binAngle + 360f) % 360f;
        transform.localRotation = Quaternion.Euler(0,0,binAngle);
        Debug.Log("Angle from the mic socket for radar" + micSocket.angle);
    }
}
