using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    
    private static readonly int[] xpThresholds = { 0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700 };

    public event Action<int> OnLevelUp;  

    
    private float xpAccumulator    = 0f;
    private int   xpEarnedThisRun  = 0;
    private bool  committed        = false;   
	
	private PlayerController playerControllerCache;

    void Start()
    {
		xpAccumulator   = 0f;
        xpEarnedThisRun = 0;
        committed       = false;
		
        
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestCompleted += OnQuestXP;
    }

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>() is PlayerController pc && pc.IsRunning)
        {
            
            xpEarnedThisRun += Mathf.FloorToInt(pc.runSpeed * Time.deltaTime / 10f);
        }
    }

    void OnQuestXP(int xpReward)
    {
        xpEarnedThisRun += xpReward;
    }

    
    public void OnExfilReached()
    {
        xpEarnedThisRun += 100;
    }

    
    
    
    public int CommitXP()
    {
		if (committed)
        {
            Debug.LogWarning("[Progression] CommitXP called twice — ignoring second call.");
            return 0;
        }
        committed = true;
		
		
        if (xpAccumulator >= 1f)
        {
            xpEarnedThisRun += Mathf.FloorToInt(xpAccumulator);
            xpAccumulator = 0f;
        }
		
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

    

    public int GetCurrentLevel()     => PlayerPrefs.GetInt("CipherLevel", 1);
    public int GetCurrentXP()        => PlayerPrefs.GetInt("CipherXP", 0);
    public int GetXPForNextLevel()   => GetCurrentLevel() < xpThresholds.Length
                                        ? xpThresholds[GetCurrentLevel()] : 99999;
    public int GetXPEarnedThisRun()  => xpEarnedThisRun;

    

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
