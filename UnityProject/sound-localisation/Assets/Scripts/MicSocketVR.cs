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

        if (currentAudioSource == null || !currentAudioSource.isPlaying)
        {
            vad = 0;
            return;
        }

        vad = 1;
        angle = GetAngleToUser(currentAudioSource);

        /*
        AudioSource activeSource = GetActiveSource();
        if(activeSource == null)
        {
            vad = 0;
            return;
        }
        vad = 1;
        angle = GetAngleToUser(activeSource);
        */
    }

    /*
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
    */

    float GetAngleToUser(AudioSource src)
    {
        Vector3 localPosition = mainCamera.transform.InverseTransformPoint(src.transform.position);
        float angle = Mathf.Atan2(localPosition.x, localPosition.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        return angle;
    }

    public void NextSource(int audioIndex)
        {
            if (currentAudioSource != null)
            {
                currentAudioSource.Stop();
                currentAudioSource.gameObject.SetActive(false);
            }
            if (audioIndex < 0 || audioIndex >= audioSources.Count)
            {
                Debug.LogError("Invalid audio index");
                return;
            }
            currentAudioSource = audioSources[audioIndex];
            currentAudioSource.gameObject.SetActive(true);
            currentAudioSource.Play();

        Debug.Log($"Playing audio source {audioIndex}");
        }
       

}
