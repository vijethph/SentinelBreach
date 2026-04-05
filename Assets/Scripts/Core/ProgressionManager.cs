using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages CIPHER's permanent level progression across all runs.
/// XP is earned from: distance run, mini-quests completed, exfil checkpoints.
/// Persisted via PlayerPrefs. Unlocks gadget slots and other perks at specific levels.
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─── XP Thresholds per level (level 1→2 needs 100 XP, etc.) ─
    private static readonly int[] xpThresholds = { 0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700 };

    public event Action<int> OnLevelUp;  // passes new level

    private int xpEarnedThisRun = 0;

    void Start()
    {
        // Subscribe to quest completions for XP
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestCompleted += OnQuestXP;
    }

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>() is PlayerController pc && pc.IsRunning)
        {
            // 1 XP per 10m run
            xpEarnedThisRun += Mathf.FloorToInt(pc.runSpeed * Time.deltaTime / 10f);
        }
    }

    void OnQuestXP(int xpReward)
    {
        xpEarnedThisRun += xpReward;
    }

    /// <summary>Called by QuestManager when exfil checkpoint is reached.</summary>
    public void OnExfilReached()
    {
        xpEarnedThisRun += 100;
    }

    /// <summary>
    /// Called at run end (GameManager.HandleDeath) to commit XP to persistent storage.
    /// </summary>
    public int CommitXP()
    {
        int totalXP = PlayerPrefs.GetInt("CipherXP", 0) + xpEarnedThisRun;
        PlayerPrefs.SetInt("CipherXP", totalXP);

        int oldLevel = GetCurrentLevel();
        int newLevel = CalculateLevel(totalXP);

        if (newLevel > oldLevel)
        {
            PlayerPrefs.SetInt("CipherLevel", newLevel);
            ApplyLevelPerks(oldLevel, newLevel);
            OnLevelUp?.Invoke(newLevel);
        }

        PlayerPrefs.Save();
        xpEarnedThisRun = 0;
        return xpEarnedThisRun;
    }

    // ─── Getters ─────────────────────────────────────────────────

    public int GetCurrentLevel()     => PlayerPrefs.GetInt("CipherLevel", 1);
    public int GetCurrentXP()        => PlayerPrefs.GetInt("CipherXP", 0);
    public int GetXPForNextLevel()   => GetCurrentLevel() < xpThresholds.Length
                                        ? xpThresholds[GetCurrentLevel()] : 99999;
    public int GetXPEarnedThisRun()  => xpEarnedThisRun;

    // ─── Internal ────────────────────────────────────────────────

    static int CalculateLevel(int totalXP)
    {
        int level = 1;
        for (int i = 1; i < xpThresholds.Length; i++)
        {
            if (totalXP >= xpThresholds[i]) level = i + 1;
            else break;
        }
        return Mathf.Min(level, 10);
    }

    void ApplyLevelPerks(int oldLevel, int newLevel)
    {
        for (int lvl = oldLevel + 1; lvl <= newLevel; lvl++)
        {
            switch (lvl)
            {
                case 2:
                    GadgetManager.Instance?.UnlockSlot(1);
                    Debug.Log("[Progression] Level 2: Gadget slot 2 (EMP) unlocked.");
                    break;
                case 5:
                    GadgetManager.Instance?.UnlockSlot(2);
                    Debug.Log("[Progression] Level 5: Gadget slot 3 (Time-Slow) unlocked.");
                    break;
                case 7:
                    Debug.Log("[Progression] Level 7: Ghost Chips now spawn in segments.");
                    PlayerPrefs.SetInt("GhostChipsEnabled", 1);
                    break;
                case 10:
                    Debug.Log("[Progression] Level 10: Ghost Mode skin unlocked.");
                    PlayerPrefs.SetInt("GhostSkinUnlocked", 1);
                    break;
            }
        }
    }
}
