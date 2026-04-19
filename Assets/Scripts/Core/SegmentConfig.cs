using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[CreateAssetMenu(fileName = "SegmentConfig", menuName = "CipherGame/Segment Config")]
public class SegmentConfig : ScriptableObject
{
    [Tooltip("The segment prefab to spawn.")]
    public GameObject segmentPrefab;

    [Tooltip("Relative spawn weight. Higher = spawns more often.")]
    [Range(0, 100)]
    public int spawnWeight = 50;

    [Tooltip("Minimum distance before this segment can start appearing.")]
    public float minDistance = 0f;

    [Tooltip("Minimum SENTINEL tier required to spawn this segment.")]
    [Range(1, 5)]
    public int minSentinelTier = 1;
}
