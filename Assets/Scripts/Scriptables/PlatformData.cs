using System.Collections.Generic;
using Platform;
using UnityEngine;

[CreateAssetMenu(fileName = "PlatformData", menuName = "Scriptable Objects/PlatfromDataSO")]
public class PlatformData : ScriptableObject
{
    public string id;
    public PlatformPieceData[] pieces;
    
    [Header("Height Variation")] public float minHeightOffset = -1f;
    public float maxHeightOffset = 1f;

    [Header("Generation")] public float spawnWeight = 1f;
    public bool allowEarlyGame = true;
    public bool allowMidGame = true;
    public bool allowLateGame = true;

    [Header("Content")] public bool canSpawnFish = true;
    public bool canSpawnObstacle = true;
}