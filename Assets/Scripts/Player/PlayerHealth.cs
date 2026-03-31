using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages CIPHER's health. Handles damage intake, invincibility frames,
/// knockback delegation to PlayerController, and death event broadcasting.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 100;

    [Tooltip("How long after taking damage CIPHER is invincible (i-frames).")]
    public float invincibilityDuration = 1.5f;

    // ─────────────────────────────────────────────
    // PUBLIC STATE
    // ─────────────────────────────────────────────

    public int CurrentHP { get; private set; }
    public bool IsInvincible { get; private set; }

    // Events — other systems (GameManager, UI) subscribe to these
    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged; // (currentHP, maxHP)

    // ─────────────────────────────────────────────
    // PRIVATE
    // ─────────────────────────────────────────────

    private PlayerController playerController;
    private float invincibilityTimer = 0f;

    void Start()
    {
        CurrentHP = maxHP;
        playerController = GetComponent<PlayerController>();

        // Notify UI of starting health
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    void Update()
    {
        // Count down invincibility frames
        if (IsInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
                IsInvincible = false;
        }
    }

    /// <summary>
    /// Deals damage to CIPHER. Applies knockback and checks for death.
    /// </summary>
    /// <param name="amount">HP to subtract.</param>
    /// <param name="sourcePosition">World position of the damage source (for knockback direction).</param>
    public void TakeDamage(int amount, Vector3 sourcePosition)
    {
        if (IsInvincible) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);

        // Delegate knockback to custom physics in PlayerController
        playerController?.ApplyKnockback(sourcePosition);

        // Start invincibility frames to prevent multiple rapid hits
        IsInvincible = true;
        invincibilityTimer = invincibilityDuration;

        Debug.Log($"[PlayerHealth] Took {amount} damage from {sourcePosition}. HP: {CurrentHP}/{maxHP}");

        if (CurrentHP <= 0)
        {
            Debug.Log("[PlayerHealth] CIPHER captured — firing OnDeath event.");
            OnDeath?.Invoke();
        }
    }

    /// <summary>
    /// Restores HP. Called by Shield Cell collectible.
    /// </summary>
    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    /// <summary>
    /// Manually sets invincibility state. Called by Shield Cell collectible.
    /// </summary>
    public void SetInvincible(bool value, float duration = 0f)
    {
        IsInvincible = value;
        if (value && duration > 0f)
        {
            invincibilityTimer = duration;
        }
    }

    /// <summary>
    /// Adds bonus HP to max (called by Skill Tree upgrades in Week 3).
    /// </summary>
    public void AddMaxHP(int bonus)
    {
        maxHP += bonus;
        CurrentHP += bonus;
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }
}