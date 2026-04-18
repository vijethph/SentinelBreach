using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages CIPHER's HP. Handles damage, i-frames, knockback, and death.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    public int   maxHP               = 100;
    public float invincibilityDuration = 1.5f;

    // Public state
    public int  CurrentHP    { get; private set; }
    public bool IsInvincible { get; private set; }

    // Events for GameManager and UI
    public event Action       OnDeath;
    public event Action<int, int> OnHealthChanged;  // (current, max)

    private PlayerController playerController;
    private float            invincibleTimer;
	
	[Header("Hit Effects")]
	[Tooltip("Drag HitParticles prefab here.")]
	public GameObject hitParticlePrefab;

	[Tooltip("Knockback force applied on every hit.")]
	public float knockbackForce = 10f;

    void Start()
	{
		CurrentHP = maxHP;
		playerController = GetComponent<PlayerController>();

		// ── Apply Shield bonus HP ─────────────────────────────────
		if (SkillTree.Instance != null)
		{
			int bonus = SkillTree.Instance.GetBonusHP();
			maxHP += bonus;
			CurrentHP = maxHP;
		}

		OnHealthChanged?.Invoke(CurrentHP, maxHP);
	}

    void Update()
    {
        if (IsInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f) IsInvincible = false;
        }
    }

    public void TakeDamage(int amount, Vector3 sourcePosition)
    {
        if (IsInvincible || CurrentHP <= 0) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
		// In TakeDamage(), after currentHP is reduced:
		QuestManager.Instance?.NotifyDamageTaken();
        OnHealthChanged?.Invoke(CurrentHP, maxHP);

        // Delegate knockback to custom physics
        if (sourcePosition != default)
			playerController?.ApplyKnockback(sourcePosition, knockbackForce);
		
		GetComponentInChildren<Animator>()
        ?.SetTrigger(Animator.StringToHash("hitTrigger"));
		
		CameraShake.Instance?.Shake();
		DamageFlash.Instance?.TriggerFlash();
		GetComponent<HitFlash>()?.Flash();
		
		if (hitParticlePrefab != null)
		{
			Vector3 spawnPos = sourcePosition != default
				? sourcePosition
				: transform.position;
			GameObject burst = Instantiate(hitParticlePrefab, spawnPos, Quaternion.identity);
			Destroy(burst, 1f);
		}
	
		AudioManager.Instance?.PlayHit();

        IsInvincible    = true;
        invincibleTimer = invincibilityDuration;

        if (CurrentHP <= 0) OnDeath?.Invoke();
    }

    public void SetInvincible(bool value, float duration = 0f)
    {
        IsInvincible = value;
        if (value && duration > 0f) invincibleTimer = duration;
    }

    public float GetHealthPercent() => (float)CurrentHP / maxHP;
}