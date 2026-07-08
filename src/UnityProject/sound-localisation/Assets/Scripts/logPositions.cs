using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class logPositions : MonoBehaviour
{
    public List<Transform> audioSources;
    public List<Transform> spawnPoints;

    void Start()
    {
        WritePositionsToCsv();
    }

    private void WritePositionsToCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Type,Name,PosX,PosY,PosZ");

        for (int i = 0; i < audioSources.Count; i++)
        {
            Transform t = audioSources[i];
            if (t == null) continue;
            Vector3 p = t.position;
            sb.AppendLine($"AudioSource,{t.name},{p.x},{p.y},{p.z}");
        }

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            Transform t = spawnPoints[i];
            if (t == null) continue;
            Vector3 p = t.position;
            sb.AppendLine($"SpawnPoint,{t.name},{p.x},{p.y},{p.z}");
        }

        string fileName = $"positions_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string fullPath = Path.Combine("C:/Users/JoeWe/Uni/IndProject/Project/data/raw", fileName);

        try
        {
            File.WriteAllText(fullPath, sb.ToString());
            Debug.Log($"Positions written to: {fullPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write positions CSV: {e.Message}");
        }
    }
}