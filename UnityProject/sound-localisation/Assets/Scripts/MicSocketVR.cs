using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using UnityEngine;

public class MicSocketVR : MonoBehaviour, IMicSocket
{
    public Camera mainCamera;
    public List<AudioSource> audioSources;
    public AudioSource currentAudioSource;
    public float angle{ get; private set; }
    public int vad{ get; private set; }
    public string classification { get; private set; } = "NA";
    public bool isConnected { get; private set; } = true;


    [System.Serializable]
    public struct Trial
    {
        public int spawnIndex;
        public int audioIndex;
    }

    
    void Start()
    {   
        foreach (var src in audioSources)
        {
            if (src != null)
            {
                src.Stop();
                src.gameObject.SetActive(false);
            }
        }

    }
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
        Vector3 localPosition = mainCamera.transform.InverseTransformPoint(src.transform.position);
        float angle = Mathf.Atan2(localPosition.x, localPosition.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        return angle;
    }

    public void NextSource(int audioIndex)
        {
            AudioSource current = audioSources[audioIndex];
            if (current != null)
            {
                current.Stop();
                current.gameObject.SetActive(false);
            }
        Debug.Log($"Playing audio source {currentSourceIndex}");
        }
       

}
