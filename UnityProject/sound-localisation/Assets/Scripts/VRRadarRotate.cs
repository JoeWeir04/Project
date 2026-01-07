using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VRRRadarRotate : MonoBehaviour
{
    public float binSize = 30f;
    public TMP_Text angleText;
    public AudioSource audioSource;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource == null || !audioSource.isPlaying)
            return;
        Vector3 direction = (audioSource.transform.position-transform.position).normalized;
        direction.y = 0f;
        direction.Normalize();

        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f -90;
        float binAngle = Mathf.Floor(angle/binSize) * binSize;
        transform.localRotation = Quaternion.Euler(0,0,binAngle);
        
        if(angleText != null){
            angleText.text = $"Bin Angle: {binAngle:F1}°\n" + $"Mic Angle: {angle:F1}°\n";
        }
    }
}
