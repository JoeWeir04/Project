using UnityEngine;
using UnityEngine.AI;

public static class NavMeshDistanceCalculator
{
    public static float GetPathLength(Vector3 start, Vector3 end)
    {
        NavMeshPath path = new NavMeshPath();
        bool success = NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);

        if (!success || path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning($"No complete path found from {start} to {end} (status: {path.status})");
            return -1f;
        }

        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
    }
}