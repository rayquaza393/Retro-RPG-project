using Unity.Netcode;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private Transform[] spawnPoints;

    void Awake()
    {
        spawnPoints = GetComponentsInChildren<Transform>();
    }

    public Vector3 GetSpawnPosition()
    {
        // Skip index 0 if it's the parent object
        return spawnPoints[Random.Range(1, spawnPoints.Length)].position;
    }
}
