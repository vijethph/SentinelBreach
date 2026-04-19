using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class GadgetManager : MonoBehaviour
{
    
    public static GadgetManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    
    [Header("Gadget Data (assign GD_Dash, GD_EMP, GD_TimeSlow)")]
    public GadgetData[] gadgetData;   

    [Header("EMP Settings")]
    public float empRadius = 10f;

    
    private float[] cooldownTimers;   
    private bool[]  onCooldown;

    private PlayerController playerController;

    
    private int unlockedSlots = 1;   
	public int TotalGadgetsUsed { get; private set; } = 0;

    

    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag("Player")
                                      ?.GetComponent<PlayerController>();

        int count = gadgetData != null ? gadgetData.Length : 3;
        cooldownTimers = new float[count];
        onCooldown     = new bool[count];

        
        
		int savedLevel = PlayerPrefs.GetInt("CipherLevel", 1);
		unlockedSlots = 1;  

		if (savedLevel >= 2) UnlockSlot(1);
		if (savedLevel >= 5) UnlockSlot(2);
    }

    void Update()
    {
        
        #if !UNITY_ANDROID
		if (Input.GetKeyDown(KeyCode.Q)) Activate(0);
		if (Input.GetKeyDown(KeyCode.E)) Activate(1);
		if (Input.GetKeyDown(KeyCode.R)) Activate(2);
		#endif

        
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0f)
            {
                cooldownTimers[i] -= Time.deltaTime;
                if (cooldownTimers[i] <= 0f)
                {
                    cooldownTimers[i] = 0f;
                    onCooldown[i] = false;
                }
            }
        }
    }

    
    
    
    
    public void Activate(int slot)
    {
        if (slot >= unlockedSlots) { Debug.Log($"[GadgetManager] Slot {slot} locked."); return; }
        if (slot >= gadgetData.Length) return;
        if (onCooldown[slot]) return;

        GadgetData data = gadgetData[slot];
        if (data == null) return;

        
        float cooldownMult = SkillTree.Instance != null
            ? SkillTree.Instance.GetGadgetCooldownMultiplier() : 1f;
        float cooldown = data.baseCooldown * cooldownMult;

        switch (data.gadgetId)
        {
            case "dash":      ExecuteDash(data.force); break;
            case "emp":       ExecuteEMP(data.duration); break;
            case "timeslow":  ExecuteTimeSlow(data.duration); break;
            default: Debug.LogWarning($"Unknown gadget id: {data.gadgetId}"); return;
        }
		
		TotalGadgetsUsed++;
		
		
		QuestManager.Instance?.NotifyGadgetUsed(data.gadgetId);

        
        onCooldown[slot]     = true;
        cooldownTimers[slot] = cooldown;

        
        GadgetUI.Instance?.OnGadgetActivated(slot, cooldown);
    }

    

    
    
    
    
    void ExecuteDash(float force)
    {
        if (playerController == null) return;
        
        Vector3 behindCipher = playerController.transform.position - playerController.transform.forward * 5f;
        playerController.ApplyKnockback(behindCipher, force);
        Debug.Log("[GadgetManager] DASH activated.");
		AudioManager.Instance?.PlayDash();
    }

    
    
    
    void ExecuteEMP(float duration)
    {
        Vector3 origin = playerController != null
            ? playerController.transform.position : Vector3.zero;

        Collider[] hits = Physics.OverlapSphere(origin, empRadius);
        int affected = 0;
        foreach (Collider col in hits)
        {
            TurretController tc = col.GetComponentInParent<TurretController>();
            if (tc != null) { tc.Disable(duration); affected++; continue; }

            DronePatrol dp = col.GetComponentInParent<DronePatrol>();
            if (dp != null) { dp.Disable(duration); affected++; }
        }
        Debug.Log($"[GadgetManager] EMP activated. Affected {affected} enemies.");
		AudioManager.Instance?.PlayEMP();	
    }

    
    
    
    
    void ExecuteTimeSlow(float duration)
    {
        StartCoroutine(TimeSlowCoroutine(duration));
		AudioManager.Instance?.PlayTimeSlow();
    }

    System.Collections.IEnumerator TimeSlowCoroutine(float duration)
    {
        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Debug.Log("[GadgetManager] TIME-SLOW activated.");
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        Debug.Log("[GadgetManager] TIME-SLOW ended.");
    }

    

    
    public void UnlockSlot(int slot)
    {
		
		if (slot >= (gadgetData?.Length ?? 3)) return;
		if (slot < unlockedSlots) return;  

		unlockedSlots = Mathf.Max(unlockedSlots, slot + 1);

		string name = (gadgetData != null && slot < gadgetData.Length && gadgetData[slot] != null)
			? gadgetData[slot].displayName
			: $"Gadget {slot + 1}";

		Debug.Log($"[GadgetManager] Slot {slot} ({name}) unlocked.");
		
		GadgetUI.Instance?.RefreshLockState();
    }

    

    public float GetCooldownTimer(int slot) =>
        slot < cooldownTimers.Length ? cooldownTimers[slot] : 0f;

    public float GetMaxCooldown(int slot)
    {
        if (slot >= gadgetData.Length || gadgetData[slot] == null) return 1f;
        float mult = SkillTree.Instance != null
            ? SkillTree.Instance.GetGadgetCooldownMultiplier() : 1f;
        return gadgetData[slot].baseCooldown * mult;
    }

    public bool IsOnCooldown(int slot) =>
        slot < onCooldown.Length && onCooldown[slot];

    public bool IsUnlocked(int slot) => slot < unlockedSlots;
	
	
	
	
	
	public void StartSurgeRefill(float refillDuration = 3f)
	{
		StartCoroutine(SurgeRefillCoroutine(refillDuration));
	}

	IEnumerator SurgeRefillCoroutine(float duration)
	{
		Debug.Log("[GadgetManager] Surge Token: refilling gadget cooldowns.");
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float tickReduction = Time.deltaTime * (1f / duration); 

			for (int i = 0; i < cooldownTimers.Length; i++)
			{
				if (cooldownTimers[i] > 0f)
				{
					
					cooldownTimers[i] = Mathf.Max(0f,
						cooldownTimers[i] - tickReduction * GetMaxCooldown(i));

					if (cooldownTimers[i] <= 0f)
						onCooldown[i] = false;
				}
			}
			yield return null;
		}
		Debug.Log("[GadgetManager] Surge refill complete.");
	}
}