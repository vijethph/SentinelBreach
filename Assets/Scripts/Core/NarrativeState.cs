using System.Collections;
using System.Collections.Generic;
using UnityEngine;









public class NarrativeState : MonoBehaviour
{
    public static NarrativeState Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public enum RouteChoice { None, Stealth, BruteForce }

    
    public RouteChoice CurrentRoute { get; private set; } = RouteChoice.None;

    
    public int ChoiceCount { get; private set; } = 0;

    

    
    
    
    
    public void ChooseStealth()
    {
        CurrentRoute = RouteChoice.Stealth;
        ChoiceCount++;

        
        foreach (var laser in FindObjectsOfType<LaserGridController>())
            laser.ApplyStealthTiming();

        
        SentinelManager.Instance?.ApplyStealthModifier();
		
		UpdateRouteHUD();

        Debug.Log("[Narrative] STEALTH chosen: laser timing slowed, SENTINEL aggression reduced.");
    }

    
    
    
    
    public void ChooseBruteForce()
    {
        CurrentRoute = RouteChoice.BruteForce;
        ChoiceCount++;

        
        GhostChipEffect.Instance?.ActivateGhostMode();

        
        int bonus = 30;
        PlayerPrefs.SetInt("TotalShards", PlayerPrefs.GetInt("TotalShards", 0) + bonus);
        PlayerPrefs.Save();
        
        CollectibleManager.Instance?.NotifyBonusShards(bonus);
		
		UpdateRouteHUD();

        Debug.Log($"[Narrative] BRUTE FORCE chosen: EMP triggered, +{bonus} shards.");
    }
	
	void UpdateRouteHUD()
	{
		
		var texts = FindObjectsOfType<TMPro.TextMeshProUGUI>();
		foreach (var t in texts)
		{
			if (t.name == "Route_Text")
			{
				t.text = CurrentRoute == RouteChoice.Stealth
					? "ROUTE: STEALTH"
					: "ROUTE: BRUTE FORCE";
				t.color = CurrentRoute == RouteChoice.Stealth
					? new Color(0f, 0.8f, 1f)
					: new Color(1f, 0.3f, 0f);
			}
		}
	}
}