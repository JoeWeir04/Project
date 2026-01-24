using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using UnityEngine;

public class MicSocketVR : MonoBehaviour, IMicSocket
{
    public Camera mainCamera;
    public List<AudioSource> audioSources;
    public float angle{ get; private set; }
    public int vad{ get; private set; }
    public string classification { get; private set; } = "speech";
    public bool isConnected { get; private set; } = true;

    // Start is called before the first frame update
    void Update()
    {
        AudioSource activeSource = GetActiveSource();
        if(activeSource == null)
        {
            vad = 0;
            return;
        }
        vad = 1;
        angle = GetAngleToUser(activeSource);
    }

    AudioSource GetActiveSource()
    {
        foreach (var src in audioSources)
        {
            if (src != null && src.isPlaying)
            {
                return src;
            }
        }
        return null;
    }

    float GetAngleToUser(AudioSource src)
{
    // 1. Convert the audio source's world position to the Camera's local space
    // This accounts for the player's head rotation and position automatically.
    Vector3 localPosition = mainCamera.transform.InverseTransformPoint(src.transform.position);

    // 2. Calculate the angle on the local XZ plane
    // Atan2(x, z) gives 0 degrees when Z is positive (Forward)
    float angle = Mathf.Atan2(localPosition.x, localPosition.z) * Mathf.Rad2Deg;
    
    // Result: 
    // 0 = Straight Ahead
    // 90 = Right
    // -90 = Left
    // 180 = Behind
    
    // Optional: Normalize to 0-360 range if your receiving socket expects positive integers
    if (angle < 0) angle += 360f;

    return angle;
}
}
