using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public int TotalShards { get; private set; } = 0;

    public event Action<int> OnShardCollected;
    public event Action      OnShieldCollected;
    public event Action      OnSurgeCollected;
    public event Action      OnGhostChipCollected;

    public void Collect(CollectibleType type, GameObject player)
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();

        switch (type)
        {
            case CollectibleType.DataShard:
                TotalShards += 10;
                OnShardCollected?.Invoke(TotalShards);
				AudioManager.Instance?.PlayShard();    // ← ADD THIS
                break;
            case CollectibleType.ShieldCell:
                ph?.SetInvincible(true, 3f);
                OnShieldCollected?.Invoke();
                break;
            case CollectibleType.SurgeToken:
                OnSurgeCollected?.Invoke();
                break;
            case CollectibleType.GhostChip:
                OnGhostChipCollected?.Invoke();
                break;
        }
    }
}