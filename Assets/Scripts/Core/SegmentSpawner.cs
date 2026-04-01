using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns corridor segments ahead of the player.
/// Recycles old segments by moving them to the front.
/// Keeps 5 segments active at all times.
/// </summary>
public class SegmentSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag CIPHER (the Player GameObject) here.")]
    public Transform player;

    // Replace the single segmentPrefab field with an array:
	[Tooltip("Drag PF_Seg_Open, PF_Seg_Laser, PF_Seg_Turret, PF_Seg_Drone here.")]
	public GameObject[] segmentPrefabs;

    [Header("Settings")]
    public int initialSegments = 5;
    public float segmentLength = 30f;

    private readonly Queue<GameObject> activeSegments = new Queue<GameObject>();

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("SegmentSpawner: Player not assigned!");
            return;
        }

        float zPos = 0f;
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnSegment(zPos);
            zPos += segmentLength;
        }
    }

    void Update()
    {
        if (activeSegments.Count == 0 || player == null) return;

        GameObject first = activeSegments.Peek();
        Transform exit = first.transform.Find("Exit");

        if (exit == null)
        {
            Debug.LogWarning("SegmentSpawner: Segment has no 'Exit' child.");
            return;
        }

        // When player passes the Exit marker, recycle the oldest segment
        if (player.position.z > exit.position.z)
            RecycleSegment();
    }

    // Replace SpawnSegment():
	void SpawnSegment(float zPos)
	{
		if (segmentPrefabs == null || segmentPrefabs.Length == 0) return;
		// Use open segment for first 2, then random after that
		int idx = (activeSegments.Count < 2) ? 0 : Random.Range(0, segmentPrefabs.Length);
		GameObject seg = Instantiate(
			segmentPrefabs[idx],
			new Vector3(0f, 0f, zPos),
			Quaternion.identity
		);
		activeSegments.Enqueue(seg);
	}

    // Replace RecycleSegment():
	void RecycleSegment()
	{
		GameObject first = activeSegments.Dequeue();
		float newZ = activeSegments.ToArray()[activeSegments.Count - 1].transform.position.z + segmentLength;
		first.transform.position = new Vector3(0f, 0f, newZ);
		activeSegments.Enqueue(first);
	}
}