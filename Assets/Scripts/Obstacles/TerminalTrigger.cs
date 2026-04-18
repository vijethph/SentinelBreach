using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Notifies QuestManager when the player runs through a terminal.
/// Plays a visual feedback flash on the terminal screen.
/// </summary>
public class TerminalTrigger : MonoBehaviour
{
    private bool triggered = false;
    public Renderer screenRenderer;   // drag Terminal_Prop's MeshRenderer here

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;
		
		// Show narrative choice UI
        if (NarrativeChoiceUI.Instance != null)
            NarrativeChoiceUI.Instance.ShowChoice();

        QuestManager.Instance?.NotifyExfilReached(); // reuses exfil notification for quest
        ProgressionManager.Instance?.OnExfilReached();

        // Flash screen green briefly
        if (screenRenderer != null)
            screenRenderer.material.SetColor("_EmissionColor", Color.green * 3f);

        Debug.Log("[TerminalTrigger] Player hacked a terminal.");
    }
}