using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using System.Linq;





public class SegmentSpawner : MonoBehaviour
{
    [Header("Segment Configs (assign all SegmentConfig SOs)")]
    public SegmentConfig[] segmentConfigs;

    [Header("Spawning Config")]
    public int   segmentsAhead  = 5;
    public float segmentLength  = 30f;

    private Queue<GameObject> activeSegments = new Queue<GameObject>();
    private float  nextSpawnZ = 0f;
    private Transform player;
	private Dictionary<GameObject, ObjectPool<GameObject>> pools
    = new Dictionary<GameObject, ObjectPool<GameObject>>();

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) { Debug.LogError("SegmentSpawner: No Player found!"); return; }

        LoadJSONOverrides();

        for (int i = 0; i < segmentsAhead; i++)
            SpawnNextSegment();
    }

    void Update()
    {
        if (player == null) return;

        while (player.position.z + (segmentsAhead * segmentLength) > nextSpawnZ)
            SpawnNextSegment();

        while (activeSegments.Count > segmentsAhead + 2)
        {
            GameObject old = activeSegments.Dequeue();
            
			ReleaseSegment(old);
        }
    }

    void SpawnNextSegment()
	{
		SegmentConfig chosen = PickWeightedSegment();
		if (chosen == null || chosen.segmentPrefab == null) return;

		GameObject prefab = chosen.segmentPrefab;

		
		if (!pools.ContainsKey(prefab))
		{
			pools[prefab] = new ObjectPool<GameObject>(
				createFunc:  () => Instantiate(prefab),
				actionOnGet: obj => obj.SetActive(true),
				actionOnRelease: obj => obj.SetActive(false),
				actionOnDestroy: obj => Destroy(obj),
				defaultCapacity: 3, maxSize: 10
			);
		}

		GameObject seg = pools[prefab].Get();
		seg.transform.position = new Vector3(0f, 0f, nextSpawnZ);
		seg.transform.rotation = Quaternion.identity;
		activeSegments.Enqueue(seg);
		nextSpawnZ += segmentLength;
	}
	
	void ReleaseSegment(GameObject seg)
	{
		
		
		foreach (var kvp in pools)
		{
			if (seg.name.StartsWith(kvp.Key.name))
			{
				kvp.Value.Release(seg);
				return;
			}
		}
		Destroy(seg); 
	}

    SegmentConfig PickWeightedSegment()
    {
        float dist = player != null ? player.position.z : 0f;
        int   tier = SentinelManager.Instance != null ? SentinelManager.Instance.currentTier : 1;

        
        var eligible = segmentConfigs
            .Where(c => c != null && c.segmentPrefab != null
                        && dist >= c.minDistance
                        && tier >= c.minSentinelTier)
            .ToList();

        if (eligible.Count == 0) return segmentConfigs[0]; 

        int totalWeight = eligible.Sum(c => c.spawnWeight);
        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var config in eligible)
        {
            cumulative += config.spawnWeight;
            if (roll < cumulative) return config;
        }
        return eligible[eligible.Count - 1];
    }

    

    void LoadJSONOverrides()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "segment_config.json");
        if (!System.IO.File.Exists(path)) return;

        try
        {
            string json = System.IO.File.ReadAllText(path);
            SegmentConfigJSON data = JsonUtility.FromJson<SegmentConfigJSON>(json);
            if (data == null || data.overrides == null) return;

            foreach (var ov in data.overrides)
            {
                foreach (var config in segmentConfigs)
                {
                    if (config != null && config.name == ov.configName)
                    {
                        config.spawnWeight = ov.spawnWeight;
                        config.minDistance = ov.minDistance;
                        Debug.Log($"[SegmentSpawner] JSON override applied: {ov.configName}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SegmentSpawner] Failed to load JSON config: {e.Message}");
        }
    }

    [System.Serializable] class SegmentConfigJSON { public SegmentOverride[] overrides; }
    [System.Serializable] class SegmentOverride   { public string configName; public int spawnWeight; public float minDistance; }
}