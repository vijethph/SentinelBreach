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
				int runningTotal = PlayerPrefs.GetInt("TotalShards", 0) + 10;
				PlayerPrefs.SetInt("TotalShards", runningTotal);
				PlayerPrefs.Save();

                OnShardCollected?.Invoke(TotalShards);
				AudioManager.Instance?.PlayShard();    
                break;
            case CollectibleType.ShieldCell:
                
				ShieldEffect shieldEffect = player.GetComponent<ShieldEffect>();
				if (shieldEffect != null)
					shieldEffect.ActivateShield();
				else
					ph?.SetInvincible(true, 5f);  

				OnShieldCollected?.Invoke();
				Debug.Log("[Collectible] Shield Cell collected — shield activated.");
				break;
            case CollectibleType.SurgeToken:
                
				GadgetManager.Instance?.StartSurgeRefill(3f);
				OnSurgeCollected?.Invoke();
				Debug.Log("[Collectible] Surge Token collected — gadget cooldowns refilling.");
				break;
            case CollectibleType.GhostChip:
				GhostChipEffect.Instance?.ActivateGhostMode();
				OnGhostChipCollected?.Invoke();
				Debug.Log("[Collectible] Ghost Chip collected — ghost mode active.");
				break;
				
			case CollectibleType.InvertChip:
				GravityInversion.Instance?.Activate();
				Debug.Log("[Collectible] Invert Chip collected — gravity reversing.");
				break;
        }
    }
	
	
	public void NotifyBonusShards(int amount)
	{
		TotalShards += amount;
		OnShardCollected?.Invoke(TotalShards);
	}
}