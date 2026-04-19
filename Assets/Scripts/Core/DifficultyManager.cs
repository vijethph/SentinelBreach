using System.Collections;
using System.Collections.Generic;
using UnityEngine;











public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

		
		var diffText = GameObject.Find("Difficulty_Text")
								 ?.GetComponent<TMPro.TextMeshProUGUI>();
		if (diffText != null)
			diffText.text = $"SENTINEL THREAT: LVL {CurrentLevel}";
    }

    

    public int CurrentLevel => PlayerPrefs.GetInt("CipherLevel", 1);

    

    
    public float TurretRotationSpeed
    {
        get
        {
            int lvl = CurrentLevel;
            if (lvl <= 2) return 8f;
            if (lvl <= 4) return 12f;
            if (lvl <= 6) return 16f;
            if (lvl <= 8) return 20f;
            return 25f;
        }
    }

    
    
    
    
    public float TurretMountHeight
    {
        get
        {
            int lvl = CurrentLevel;
            if (lvl <= 2) return 1.0f;  
            if (lvl <= 4) return 1.6f;  
            if (lvl <= 6) return 0.6f;  
            if (lvl <= 8) return 1.3f;  
            return 1.6f;                 
        }
    }

    
    public float TurretFireInterval
    {
        get
        {
            int lvl = CurrentLevel;
            if (lvl <= 2) return 1.8f;
            if (lvl <= 5) return 1.3f;
            if (lvl <= 8) return 0.9f;
            return 0.7f;
        }
    }

    

    
    public float DroneAmplitude
    {
        get
        {
            int lvl = CurrentLevel;
            if (lvl <= 2) return 1.0f;
            if (lvl <= 4) return 1.5f;
            if (lvl <= 6) return 2.0f;
            if (lvl <= 8) return 2.3f;
            return 2.8f;
        }
    }

    
    public float DronePatrolSpeed
    {
        get
        {
            int lvl = CurrentLevel;
            if (lvl <= 2) return 1.2f;
            if (lvl <= 4) return 1.6f;
            if (lvl <= 6) return 2.2f;
            if (lvl <= 8) return 2.8f;
            return 3.5f;
        }
    }

    
    public float DronePatrolHeight
    {
        get
        {
            int lvl = CurrentLevel;
            if (lvl <= 3) return 2.0f;  
            if (lvl <= 6) return 1.2f;  
            return 2.5f;                 
        }
    }

    

    
    
    
    
    public float LaserHeight
	{
		get
		{
			int lvl = CurrentLevel;
			
			
			if (lvl <= 2) return -0.8f;   
			if (lvl <= 4) return -1.4f;   
			if (lvl <= 6) return -0.4f;   
			if (lvl <= 8) return -1.6f;   
			return -0.9f;                  
		}
	}

    
    public float LaserOnDuration
    {
        get
        {
            int lvl = CurrentLevel;
            if (lvl <= 2) return 1.0f;
            if (lvl <= 4) return 1.3f;
            if (lvl <= 7) return 1.6f;
            return 2.0f;
        }
    }
}