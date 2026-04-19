using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;





public class ExfilCheckpoint : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject exfilOverlayPanel;  

    private bool triggered = false;
	
	void OnEnable()
    {
        
        
        triggered = false;
    }

    void Start()
	{
		if (exfilOverlayPanel == null)
		{
			
			GameObject overlay = GameObject.Find("Exfil_Overlay");
			if (overlay != null) exfilOverlayPanel = overlay;
		}

		BoxCollider bc = gameObject.AddComponent<BoxCollider>();
		if (bc == null) bc = gameObject.AddComponent<BoxCollider>();
		bc.isTrigger = true;
		bc.size = new Vector3(5f, 4f, 1f);
		bc.center = Vector3.zero;
	}

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;

        QuestManager.Instance?.NotifyExfilReached();
        ProgressionManager.Instance?.OnExfilReached();

        StartCoroutine(ExfilSequence());
		Debug.Log("[ExfilCheckpoint] Triggered! SENTINEL tier will increase.");
    }

    IEnumerator ExfilSequence()
    {
        
        if (exfilOverlayPanel != null) exfilOverlayPanel.SetActive(true);
        Time.timeScale = 0.5f;

        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1f;
        if (exfilOverlayPanel != null) exfilOverlayPanel.SetActive(false);

        
        SentinelManager.Instance?.IncreaseTier();
        Debug.Log("[ExfilCheckpoint] Checkpoint reached. SENTINEL tier increased.");
		AudioManager.Instance?.PlayExfil();
    }
}