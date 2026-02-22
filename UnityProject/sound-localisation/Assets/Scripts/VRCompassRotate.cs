using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VRCompassRotate : MonoBehaviour
{
    public TMP_Text angleText;
    public float rotationspeed = 180f;
    public AudioSource audioSource;


    void Update()
    {
        if (audioSource == null || !audioSource.isPlaying)
            return;
        
        Vector3 direction = (audioSource.transform.position-transform.position).normalized;
        direction.y = 0f;
        direction.Normalize();

        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f -90;

        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRotation,
            rotationspeed*Time.deltaTime);

        if(angleText != null)
        {
            angleText.text = $"Arrow Angle: {angle:F1}°";
        }
        
    }
}
