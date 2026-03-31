using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that processes collectible pickups and notifies other systems.
/// Uses C# events so any system can subscribe without tight coupling.
/// </summary>
public class CollectibleManager : MonoBehaviour
{
    // ─── Singleton ───────────────────────────────────────────────
    public static CollectibleManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    // ─── Events ──────────────────────────────────────────────────
    public event Action<int> OnShardCollected;    // passes updated total shard count
    public event Action     OnShieldCollected;
    public event Action     OnSurgeCollected;
    public event Action     OnGhostChipCollected;

    // ─── State ───────────────────────────────────────────────────
    [Header("Shard Value")]
    public int shardValue = 10;

    public int TotalShards { get; private set; } = 0;

    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player")
                                 ?.GetComponent<PlayerHealth>();
    }

    /// <summary>
    /// Called by Collectible.cs when the player picks up a collectible.
    /// </summary>
    public void Collect(CollectibleType type)
    {
        switch (type)
        {
            case CollectibleType.DataShard:
                TotalShards += shardValue;
                OnShardCollected?.Invoke(TotalShards);
                break;

            case CollectibleType.ShieldCell:
                // Grant 3 seconds of invincibility
                playerHealth?.SetInvincible(true, 3f);
                OnShieldCollected?.Invoke();
                break;

            case CollectibleType.SurgeToken:
                // GadgetManager will handle charge refill in Week 3
                OnSurgeCollected?.Invoke();
                break;

            case CollectibleType.GhostChip:
                // QuestManager + obstacle disabling in Week 3
                OnGhostChipCollected?.Invoke();
                break;
        }
    }
}