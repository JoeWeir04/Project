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
    public float realAngle {get; private set;}
    public int vad{ get; private set; }
    public string classification { get; private set; } = "NA";
    public bool isConnected { get; private set; } = true;
    public float distanceProxy{get; private set; }
    public float realDistance {get; private set; }

    [Header("Distance Settings")]
    public float maxDistance = 10f; 
    public float minDistance = 0.5f;
    [Header("Noise Settings")]
    private float maxAngleError = 8f;     
    private float angleSD = 2f;
    public float distanceNoise = 0.03f; 
    public float angleUpdateInterval = 0.05f; 
    private float nextAngleUpdateTime = 0f;



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

        realAngle = GetAngleToUser(currentAudioSource);
        realDistance = GetRealDistanceToUser(currentAudioSource);
        distanceProxy = GetProxyDistanceToUser(currentAudioSource);

        if (Time.time >= nextAngleUpdateTime)
        {
            float noisyAngle = GetNoisyAngleToUser();
            angle = noisyAngle;
            nextAngleUpdateTime = Time.time + angleUpdateInterval;
        }
    }



    float GetAngleToUser(AudioSource src)
    {
        Vector3 localPosition = mainCamera.transform.InverseTransformPoint(src.transform.position);
        float angle = Mathf.Atan2(localPosition.x, localPosition.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        return angle;
    }

    float GetNoisyAngleToUser()
    {
        //float noisy = GetAngleToUser(currentAudioSource) + Random.Range(-angleNoiseDeg, angleNoiseDeg);
        float real = GetAngleToUser(currentAudioSource);
        float noisy = real + GaussianRandom(0f, angleSD);  
        noisy = Mathf.Clamp(noisy, real - maxAngleError, real + maxAngleError);
        noisy = noisy % 360f;          
        
        if (noisy < 0f) noisy += 360f;
        return noisy;
    }

    float GaussianRandom(float mean = 0f, float stdDev = 1f)
    {
        float u1 = 1.0f - Random.value; 
        float u2 = 1.0f - Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                            Mathf.Sin(2.0f * Mathf.PI * u2); 
        return mean + stdDev * randStdNormal;
    }


    float GetRealDistanceToUser(AudioSource src)
    {
        Vector3 toSource = currentAudioSource.transform.position - mainCamera.transform.position;
        float distance = toSource.magnitude;
        return distance;
    }


    float GetProxyDistanceToUser(AudioSource src)
    {
        float distance = GetRealDistanceToUser(src);
        float normalized = Mathf.InverseLerp(maxDistance, minDistance, distance);
        normalized += Random.Range(-distanceNoise, distanceNoise);

        if (normalized < 0.4f)
        return 0.2f;   
        else if (normalized < 0.7f)
            return 0.5f;   
        else
            return 1f; 
        //return Mathf.Lerp(0.1f,1f,normalized);
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
