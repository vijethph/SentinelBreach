using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the persistent skill tree.
/// Reads SkillData assets, saves/loads upgrade levels via PlayerPrefs,
/// and exposes modifier methods so PlayerController and GadgetManager
/// can query the current bonus at run start.
/// </summary>
public class SkillTree : MonoBehaviour
{
    // ─── Singleton ───────────────────────────────────────────────
    public static SkillTree Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // ─── Inspector ───────────────────────────────────────────────
    [Header("Skill Data Assets (assign all 5)")]
    public SkillData[] skills;   // drag SD_Speed, SD_Shield, etc. here in the Inspector

    // ─── Public API ──────────────────────────────────────────────

    /// <summary>Returns current upgrade level (0 = not purchased) for a node.</summary>
    public int GetLevel(string nodeId)
    {
        return PlayerPrefs.GetInt("Skill_" + nodeId, 0);
    }

    /// <summary>
    /// Attempts to purchase the next level of a skill.
    /// Returns true if successful, false if max level or insufficient shards.
    /// </summary>
    public bool TryUpgrade(string nodeId, int currentShards, out int newShards)
    {
        newShards = currentShards;
        SkillData data = GetSkillData(nodeId);
        if (data == null) return false;

        int currentLevel = GetLevel(nodeId);
        if (currentLevel >= data.MaxLevel) return false;

        int cost = data.costs[currentLevel];
        if (currentShards < cost) return false;

        // Deduct cost, save new level
        newShards = currentShards - cost;
        PlayerPrefs.SetInt("Skill_" + nodeId, currentLevel + 1);
        PlayerPrefs.SetInt("TotalShards", newShards);
        PlayerPrefs.Save();
        return true;
    }

    // ─── Modifier Getters ─────────────────────────────────────────
    // Called at run start by PlayerController and GadgetManager.

    /// <summary>Speed multiplier from Neural Speed skill. Base = 1.0.</summary>
    public float GetSpeedMultiplier()
    {
        int level = GetLevel("speed");
        float[] multipliers = { 1.0f, 1.05f, 1.12f, 1.20f };
        return multipliers[Mathf.Clamp(level, 0, 3)];
    }

    /// <summary>Bonus starting HP from Nano Shield skill.</summary>
    public int GetBonusHP()
    {
        int level = GetLevel("shield");
        int[] bonuses = { 0, 10, 25, 40 };
        return bonuses[Mathf.Clamp(level, 0, 3)];
    }

    /// <summary>Extra shard collection radius in world units.</summary>
    public float GetHackRangeBonus()
    {
        int level = GetLevel("hackrange");
        float[] bonuses = { 0f, 1f, 2f, 3f };
        return bonuses[Mathf.Clamp(level, 0, 3)];
    }

    /// <summary>Cooldown multiplier for all gadgets. Base = 1.0 (lower = faster).</summary>
    public float GetGadgetCooldownMultiplier()
    {
        int level = GetLevel("gadgeteff");
        float[] multipliers = { 1.0f, 0.9f, 0.8f, 0.7f };
        return multipliers[Mathf.Clamp(level, 0, 3)];
    }

    /// <summary>Knockback force multiplier. Base = 1.0 (lower = less knockback).</summary>
    public float GetKnockbackResistance()
    {
        int level = GetLevel("knockbackres");
        float[] multipliers = { 1.0f, 0.8f, 0.6f, 0f }; // 0 = immune
        return multipliers[Mathf.Clamp(level, 0, 3)];
    }

    // ─── Helpers ─────────────────────────────────────────────────

    public SkillData GetSkillData(string nodeId)
    {
        foreach (var s in skills)
            if (s != null && s.nodeId == nodeId) return s;
        return null;
    }

    /// <summary>Reads total shards from PlayerPrefs (persisted across scenes).</summary>
    public int GetTotalShards() => PlayerPrefs.GetInt("TotalShards", 0);

    /// <summary>Adds shards and saves immediately.</summary>
    public void AddShards(int amount)
    {
        int total = GetTotalShards() + amount;
        PlayerPrefs.SetInt("TotalShards", total);
        PlayerPrefs.Save();
    }
}
