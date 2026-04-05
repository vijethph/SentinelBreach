using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Draws 3 gadget icons with cooldown fill overlays on the HUD.
/// </summary>
public class GadgetUI : MonoBehaviour
{
    public static GadgetUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [System.Serializable]
    public class GadgetSlotUI
    {
        public Image iconBackground;      // filled image for cooldown overlay
        public Image cooldownFill;        // Image with Fill Method = Radial360
        public TextMeshProUGUI keyLabel;  // "Q", "E", "R"
        public TextMeshProUGUI nameLabel; // "DASH", "EMP", "TIME-SLOW"
    }

    [Header("Gadget Slot UI Elements")]
    public GadgetSlotUI[] slots;   // size 3

    private float[] maxCooldowns = new float[3];

    void Update()
    {
        if (GadgetManager.Instance == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || slots[i].cooldownFill == null) continue;

            bool locked = !GadgetManager.Instance.IsUnlocked(i);
            float timer = GadgetManager.Instance.GetCooldownTimer(i);
            float maxCD = GadgetManager.Instance.GetMaxCooldown(i);

            // Cooldown fill: 1 = full cooldown, 0 = ready
            float fillAmount = maxCD > 0 ? timer / maxCD : 0f;
            slots[i].cooldownFill.fillAmount = fillAmount;

            // Dim icon when locked
            if (slots[i].iconBackground != null)
            {
                Color c = slots[i].iconBackground.color;
                c.a = locked ? 0.3f : 1f;
                slots[i].iconBackground.color = c;
            }
        }
    }

    public void OnGadgetActivated(int slot, float cooldown)
    {
        if (slot < maxCooldowns.Length)
            maxCooldowns[slot] = cooldown;
    }
}