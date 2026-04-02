using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns corridor segments ahead of the player and destroys old ones behind.
/// Keeps a rolling queue of active segments to simulate an infinite corridor.
/// </summary>
public class SegmentSpawner : MonoBehaviour
{
    [Header("Segment Prefabs")]
    [Tooltip("Drag segment prefabs here. Index 0 = open/safe segment.")]
    public GameObject[] segmentPrefabs;

    [Header("Spawning Config")]
    [Tooltip("How many segments to keep ahead of the player at all times.")]
    public int segmentsAhead = 5;

    [Tooltip("Length of each segment in Unity units. Must match the prefab Z size.")]
    public float segmentLength = 30f;

    // Internal state
    private Queue<GameObject> activeSegments = new Queue<GameObject>();
    private float nextSpawnZ = 0f;
    private Transform player;

    void Start()
    {
        // Find the player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("SegmentSpawner: No GameObject with tag 'Player' found!");
            return;
        }
        player = playerObj.transform;

        // Pre-fill the corridor with segments
        for (int i = 0; i < segmentsAhead; i++)
            SpawnNextSegment();
    }

    void Update()
    {
        if (player == null) return;

        // Spawn a new segment when the player gets close enough to the end
        // The threshold: player Z + (look-ahead distance) > where we last spawned
        while (player.position.z + (segmentsAhead * segmentLength) > nextSpawnZ)
            SpawnNextSegment();

        // Destroy segments that are too far behind the player
        // Keep segmentsAhead + 2 as a buffer before destroying
        while (activeSegments.Count > segmentsAhead + 2)
        {
            GameObject old = activeSegments.Dequeue();
            Destroy(old);
        }
    }

    void SpawnNextSegment()
    {
        if (segmentPrefabs == null || segmentPrefabs.Length == 0)
        {
            Debug.LogError("SegmentSpawner: No segment prefabs assigned!");
            return;
        }

        // For Week 1-2: randomly pick from available prefabs
        // Week 3 will replace this with weighted ScriptableObject selection
        int index = Random.Range(0, segmentPrefabs.Length);
        Vector3 spawnPos = new Vector3(0f, 0f, nextSpawnZ);
        GameObject seg = Instantiate(segmentPrefabs[index], spawnPos, Quaternion.identity);
        activeSegments.Enqueue(seg);
        nextSpawnZ += segmentLength;
    }
}