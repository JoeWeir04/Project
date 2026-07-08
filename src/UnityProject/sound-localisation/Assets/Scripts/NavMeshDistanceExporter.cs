using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshDistanceExporter : MonoBehaviour
{
    public List<Transform> spawnPoints;
    public List<Transform> audioSources;
    public string outputFileName = "navmeshDistances.csv";

    [ContextMenu("Export NavMesh Distances")]

    
    void Start()
    {
        Export();
    }

    public void Export()
    {
        var sb = new StringBuilder();
        sb.AppendLine("SpawnName,AudioName,NavMeshDistance");

        foreach (var spawn in spawnPoints)
        {
            foreach (var audio in audioSources)
            {
                Debug.Log($"For spawn: {spawn} and audio: {audio}");
                Vector3 audioPos = audio.position;
                audioPos.y -= 0.95f;
                float dist = NavMeshDistanceCalculator.GetPathLength(spawn.position, audioPos);
                sb.AppendLine($"{spawn.name},{audio.name},{dist}");
            }
        }

        string path = Path.Combine("C:/Users/JoeWe/Uni/IndProject/Project/data/raw", outputFileName);
        File.WriteAllText(path, sb.ToString());
        Debug.Log($"NavMesh distances exported to: {path}");
    }
}