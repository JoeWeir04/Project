using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using UnityEngine;

public class MicSocketVR : MonoBehaviour
{
    public Camera mainCamera;
    public List<AudioSource> audioSources;
    public float angle;
    public int vad;
    public string classification;

    public bool isConnected = true;

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
        Vector3 direction = src.transform.position - mainCamera.transform.position;
        direction.y = 0f;
        direction.Normalize();

        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f ;

        return angle;
    }
}
