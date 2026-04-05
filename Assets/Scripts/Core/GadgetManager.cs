using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the 3 gadget slots: Dash, EMP, Time-Slow.
/// Tracks cooldowns. Exposes Activate(slot) for both PC and Android input.
/// Gadget effects reuse the custom physics system in PlayerController.
/// </summary>
public class GadgetManager : MonoBehaviour
{
    // ─── Singleton ───────────────────────────────────────────────
    public static GadgetManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─── Inspector ───────────────────────────────────────────────
    [Header("Gadget Data (assign GD_Dash, GD_EMP, GD_TimeSlow)")]
    public GadgetData[] gadgetData;   // Index 0=Dash, 1=EMP, 2=TimeSlow

    [Header("EMP Settings")]
    public float empRadius = 10f;

    // ─── Runtime State ───────────────────────────────────────────
    private float[] cooldownTimers;   // remaining cooldown per slot
    private bool[]  onCooldown;

    private PlayerController playerController;

    // How many slots are currently unlocked (level-gated)
    private int unlockedSlots = 3;   // starts with 1; CIPHER level unlocks more

    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        playerController = GameObject.FindGameObjectWithTag("Player")
                                      ?.GetComponent<PlayerController>();

        int count = gadgetData != null ? gadgetData.Length : 3;
        cooldownTimers = new float[count];
        onCooldown     = new bool[count];

        // Apply Gadget Efficiency skill tree bonus
        // (SkillTree reads from PlayerPrefs, which persists from MainMenu scene)
    }

    void Update()
    {
        // ── PC keyboard input ─────────────────────────────────
        if (Input.GetKeyDown(KeyCode.Q)) Activate(0);
        if (Input.GetKeyDown(KeyCode.E)) Activate(1);
        if (Input.GetKeyDown(KeyCode.R)) Activate(2);

        // ── Cooldown countdown ────────────────────────────────
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

    /// <summary>
    /// Activates a gadget by slot index.
    /// Called from Update (PC) and SwipeInputHandler (Android).
    /// </summary>
    public void Activate(int slot)
    {
        if (slot >= unlockedSlots) { Debug.Log($"[GadgetManager] Slot {slot} locked."); return; }
        if (slot >= gadgetData.Length) return;
        if (onCooldown[slot]) return;

        GadgetData data = gadgetData[slot];
        if (data == null) return;

        // Apply Gadget Efficiency multiplier from skill tree
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
		
		// In Activate(), after the switch statement:
		QuestManager.Instance?.NotifyGadgetUsed(data.gadgetId);

        // Start cooldown
        onCooldown[slot]     = true;
        cooldownTimers[slot] = cooldown;

        // Notify UI
        GadgetUI.Instance?.OnGadgetActivated(slot, cooldown);
    }

    // ─── Gadget Implementations ───────────────────────────────────

    /// <summary>
    /// DASH — forward burst using the SAME custom knockback system in PlayerController.
    /// This is student-written physics reuse: impulse = -forward * force.
    /// </summary>
    void ExecuteDash(float force)
    {
        if (playerController == null) return;
        // Negative source position behind CIPHER = forward knockback
        Vector3 behindCipher = playerController.transform.position - playerController.transform.forward * 5f;
        playerController.ApplyKnockback(behindCipher, force);
        Debug.Log("[GadgetManager] DASH activated.");
		AudioManager.Instance?.PlayDash();
    }

    /// <summary>
    /// EMP PULSE — disables all turrets and drones within empRadius for duration seconds.
    /// </summary>
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

    /// <summary>
    /// TIME-SLOW — reduces Time.timeScale to 0.3 for duration seconds.
    /// Uses WaitForSecondsRealtime so the coroutine isn't affected by the slowdown itself.
    /// </summary>
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

    // ─── Level-up Unlock ─────────────────────────────────────────

    /// <summary>Called by ProgressionManager when CIPHER reaches level 2 or 5.</summary>
    public void UnlockSlot(int slot)
    {
        if (slot < gadgetData.Length)
        {
            unlockedSlots = Mathf.Max(unlockedSlots, slot + 1);
            Debug.Log($"[GadgetManager] Slot {slot} unlocked.");
        }
    }

    // ─── Getters for UI ──────────────────────────────────────────

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
}