using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class TerminalTrigger : MonoBehaviour
{
    private bool triggered = false;
    public Renderer screenRenderer;   
	
	void OnEnable()
	{
		triggered = false;  
	}

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;
		
		
        if (NarrativeChoiceUI.Instance != null)
            NarrativeChoiceUI.Instance.ShowChoice();

        QuestManager.Instance?.NotifyExfilReached(); 
        ProgressionManager.Instance?.OnExfilReached();

        
        if (screenRenderer != null)
            screenRenderer.material.SetColor("_EmissionColor", Color.green * 3f);

        Debug.Log("[TerminalTrigger] Player hacked a terminal.");
    }
}