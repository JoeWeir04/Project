using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassRotate : MonoBehaviour
{
    public MicSocket micSocket;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!micSocket.isConnected) return;
        float angle = micSocket.angle;
        transform.localRotation = Quaternion.Euler(0,angle,0);
        Debug.Log("Angle from the mic socket" + micSocket.angle);
    }
}
