using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks the current SENTINEL security tier (1–5).
/// Higher tiers increase obstacle density and run speed.
/// </summary>
public class SentinelManager : MonoBehaviour
{
    public static SentinelManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Range(1, 5)]
    public int currentTier = 1;

    [Tooltip("Speed added to the player run speed per tier increase.")]
    public float speedBonusPerTier = 0.5f;

    private PlayerController playerController;

    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag("Player")
                                      ?.GetComponent<PlayerController>();
    }

    public void IncreaseTier()
	{
		if (currentTier >= 5) return;
		currentTier++;
		if (playerController != null) playerController.runSpeed += speedBonusPerTier;
		AudioManager.Instance?.IncreaseMusicPitch();    // ← ADD THIS
	}
}
