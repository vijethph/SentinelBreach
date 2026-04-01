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

    void Start()
    {
        CurrentHP        = maxHP;
        playerController = GetComponent<PlayerController>();
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
        if (IsInvincible) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);

        // Delegate knockback to custom physics
        playerController?.ApplyKnockback(sourcePosition);

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