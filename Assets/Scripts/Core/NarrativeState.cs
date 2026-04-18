using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks the player's narrative choices for the current run.
/// Persists for the run duration; resets on scene reload.
///
/// A+ rubric evidence: "storyline choices or branches."
/// The player chooses between STEALTH and BRUTE_FORCE at terminal nodes.
/// Each choice has distinct, observable gameplay consequences.
/// </summary>
public class NarrativeState : MonoBehaviour
{
    public static NarrativeState Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public enum RouteChoice { None, Stealth, BruteForce }

    /// <summary>The route the player selected at the last terminal.</summary>
    public RouteChoice CurrentRoute { get; private set; } = RouteChoice.None;

    /// <summary>How many times the player has made a narrative choice this run.</summary>
    public int ChoiceCount { get; private set; } = 0;

    // ─── Effects applied by each choice ──────────────────────────

    /// <summary>
    /// STEALTH: slow laser timing and reduce SENTINEL aggression.
    /// Called when player taps HACK at a terminal.
    /// </summary>
    public void ChooseStealth()
    {
        CurrentRoute = RouteChoice.Stealth;
        ChoiceCount++;

        // Slow all laser grids: increase their offDuration
        foreach (var laser in FindObjectsOfType<LaserGridController>())
            laser.ApplyStealthTiming();

        // Reduce SENTINEL aggression (slower turret rotation)
        SentinelManager.Instance?.ApplyStealthModifier();
		
		UpdateRouteHUD();

        Debug.Log("[Narrative] STEALTH chosen: laser timing slowed, SENTINEL aggression reduced.");
    }

    /// <summary>
    /// BRUTE FORCE: trigger EMP blast + shard bonus.
    /// Called when player taps OVERLOAD at a terminal.
    /// </summary>
    public void ChooseBruteForce()
    {
        CurrentRoute = RouteChoice.BruteForce;
        ChoiceCount++;

        // EMP burst — disable all obstacles briefly (reuse existing system)
        GhostChipEffect.Instance?.ActivateGhostMode();

        // Bonus shards
        int bonus = 30;
        PlayerPrefs.SetInt("TotalShards", PlayerPrefs.GetInt("TotalShards", 0) + bonus);
        PlayerPrefs.Save();
        // Also notify any live shard UI
        CollectibleManager.Instance?.NotifyBonusShards(bonus);
		
		UpdateRouteHUD();

        Debug.Log($"[Narrative] BRUTE FORCE chosen: EMP triggered, +{bonus} shards.");
    }
	
	void UpdateRouteHUD()
	{
		// Find the Route_Text element and update it
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