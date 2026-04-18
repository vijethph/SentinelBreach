using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives 3 gadget slot icons with:
///   - Radial cooldown fill overlay (dark overlay shrinks as cooldown expires)
///   - Countdown timer text
///   - LOCKED overlay when slot is not yet unlocked
///   - Green flash on activation
/// </summary>
public class GadgetUI : MonoBehaviour
{
    public static GadgetUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─── One struct per gadget slot ───────────────────────────────

    [System.Serializable]
    public class GadgetSlotUI
    {
        [Tooltip("Root panel for this slot — the whole group.")]
        public GameObject slotRoot;

        [Tooltip("Background icon Image (always visible).")]
        public Image iconBG;

        [Tooltip("Dark radial fill Image on top of icon. " +
                 "Image Type = Filled, Fill Method = Radial 360, Fill Origin = Top, Clockwise ON.")]
        public Image cooldownOverlay;

        [Tooltip("TMP text showing seconds remaining, e.g. '3.2'.")]
        public TextMeshProUGUI countdownText;

        [Tooltip("TMP text showing the key binding: Q / E / R.")]
        public TextMeshProUGUI keyLabel;

        [Tooltip("TMP text showing the gadget name: DASH / EMP / TIME-SLOW.")]
        public TextMeshProUGUI nameLabel;

        [Tooltip("Full-size Image shown when slot is LOCKED (grey with padlock text).")]
        public GameObject lockedOverlay;

        // Runtime flash state
        [System.NonSerialized] public float flashTimer = 0f;
        [System.NonSerialized] public Color originalIconColor = Color.white;
    }

    [Header("Gadget Slots (size 3)")]
    public GadgetSlotUI[] slots;

    [Header("Colours")]
    public Color readyColour   = new Color(0f,   0.8f, 1f,  1f);   // cyan  — slot ready
    public Color cooldownColor = new Color(0.2f, 0.2f, 0.2f, 0.9f); // dark  — cooldown overlay
    public Color flashColour   = new Color(0f,   1f,   0.4f, 1f);   // green — activation flash
    public float flashDuration = 0.25f;

    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        // Cache original icon colours
        for (int i = 0; i < slots.Length; i++)
            if (slots[i].iconBG != null)
                slots[i].originalIconColor = slots[i].iconBG.color;
    }

    void Update()
    {
        if (GadgetManager.Instance == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            GadgetSlotUI slot = slots[i];
            if (slot == null) continue;

            bool locked     = !GadgetManager.Instance.IsUnlocked(i);
            bool onCooldown =  GadgetManager.Instance.IsOnCooldown(i);
            float timer     =  GadgetManager.Instance.GetCooldownTimer(i);
            float maxCD     =  GadgetManager.Instance.GetMaxCooldown(i);

            // ── LOCKED state ──────────────────────────────────────
            if (slot.lockedOverlay != null)
                slot.lockedOverlay.SetActive(locked);

            // ── COOLDOWN OVERLAY ──────────────────────────────────
            if (slot.cooldownOverlay != null)
            {
                float fill = (onCooldown && maxCD > 0f)
                    ? timer / maxCD
                    : 0f;

                slot.cooldownOverlay.fillAmount = fill;
                slot.cooldownOverlay.enabled    = fill > 0.01f;
            }

            // ── COUNTDOWN TEXT ────────────────────────────────────
            if (slot.countdownText != null)
            {
                if (onCooldown && timer > 0.1f)
                {
                    slot.countdownText.text    = $"{timer:F1}";
                    slot.countdownText.enabled = true;
                }
                else
                {
                    slot.countdownText.enabled = false;
                }
            }

            // ── ICON COLOUR (ready = cyan, locked = grey) ─────────
            if (slot.iconBG != null)
            {
                if (slot.flashTimer > 0f)
                {
                    slot.iconBG.color = flashColour;
                    slot.flashTimer  -= Time.deltaTime;
                }
                else if (locked)
                {
                    slot.iconBG.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                }
                else if (onCooldown)
                {
                    slot.iconBG.color = new Color(
                        slot.originalIconColor.r * 0.4f,
                        slot.originalIconColor.g * 0.4f,
                        slot.originalIconColor.b * 0.4f,
                        slot.originalIconColor.a);
                }
                else
                {
                    slot.iconBG.color = readyColour;
                }
            }
        }
    }

    /// <summary>Called by GadgetManager when a gadget activates.</summary>
    public void OnGadgetActivated(int slot, float cooldown)
    {
        if (slot < slots.Length && slots[slot] != null)
            slots[slot].flashTimer = flashDuration;
    }
	
	public void RefreshLockState()
	{
		// Force the next Update() to re-evaluate locked state
		// (Update already reads IsUnlocked() — calling this just logs the refresh)
		Debug.Log("[GadgetUI] Lock state refreshed.");
	}
}