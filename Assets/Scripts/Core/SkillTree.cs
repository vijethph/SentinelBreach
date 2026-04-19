using System.Collections;
using System.Collections.Generic;
using UnityEngine;







public class SkillTree : MonoBehaviour
{
    
    public static SkillTree Instance { get; private set; }

    void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject); 
		}
		else
		{
			Destroy(gameObject);  
			return;
		}
	}

    
    [Header("Skill Data Assets (assign all 5)")]
    public SkillData[] skills;   

    

    
    public int GetLevel(string nodeId)
    {
        return PlayerPrefs.GetInt("Skill_" + nodeId, 0);
    }

    
    
    
    
    public bool TryUpgrade(string nodeId, int currentShards, out int newShards)
    {
        newShards = currentShards;
        SkillData data = GetSkillData(nodeId);
        if (data == null) return false;

        int currentLevel = GetLevel(nodeId);
        if (currentLevel >= data.MaxLevel) return false;

        int cost = data.costs[currentLevel];
        if (currentShards < cost) return false;

        
        newShards = currentShards - cost;
        PlayerPrefs.SetInt("Skill_" + nodeId, currentLevel + 1);
        PlayerPrefs.SetInt("TotalShards", newShards);
        PlayerPrefs.Save();
        return true;
    }

    
    

    
    public float GetSpeedMultiplier()
    {
        int level = GetLevel("speed");
        float[] multipliers = { 1.0f, 1.05f, 1.12f, 1.20f };
        return multipliers[Mathf.Clamp(level, 0, 3)];
    }

    
    public int GetBonusHP()
    {
        int level = GetLevel("shield");
        int[] bonuses = { 0, 10, 25, 40 };
        return bonuses[Mathf.Clamp(level, 0, 3)];
    }

    
    public float GetHackRangeBonus()
    {
        int level = GetLevel("hackrange");
        float[] bonuses = { 0f, 1f, 2f, 3f };
        return bonuses[Mathf.Clamp(level, 0, 3)];
    }

    
    public float GetGadgetCooldownMultiplier()
    {
        int level = GetLevel("gadgeteff");
        float[] multipliers = { 1.0f, 0.9f, 0.8f, 0.7f };
        return multipliers[Mathf.Clamp(level, 0, 3)];
    }

    
    public float GetKnockbackResistance()
    {
        int level = GetLevel("knockbackres");
        float[] multipliers = { 1.0f, 0.8f, 0.6f, 0f }; 
        return multipliers[Mathf.Clamp(level, 0, 3)];
    }

    

    public SkillData GetSkillData(string nodeId)
    {
        foreach (var s in skills)
            if (s != null && s.nodeId == nodeId) return s;
        return null;
    }

    
    public int GetTotalShards() => PlayerPrefs.GetInt("TotalShards", 0);

    
	
	
	
	public void AddShards(int amount)
	{
		if (amount <= 0) return;
		int current = PlayerPrefs.GetInt("TotalShards", 0);
		PlayerPrefs.SetInt("TotalShards", current + amount);
		PlayerPrefs.Save();
	}
}
