using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;








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

        
        [System.NonSerialized] public float flashTimer = 0f;
        [System.NonSerialized] public Color originalIconColor = Color.white;
    }

    [Header("Gadget Slots (size 3)")]
    public GadgetSlotUI[] slots;
	
	[Header("Android Gadget Slots (size 3, mirrors PC slots)")]
	[Tooltip("Leave empty on PC builds — AndroidUIAdapter handles visibility.")]
	public GadgetSlotUI[] androidSlots;

    [Header("Colours")]
    public Color readyColour   = new Color(0f,   0.8f, 1f,  1f);   
    public Color cooldownColor = new Color(0.2f, 0.2f, 0.2f, 0.9f); 
    public Color flashColour   = new Color(0f,   1f,   0.4f, 1f);   
    public float flashDuration = 0.25f;

    

    void Start()
	{
		CacheOriginalColors(slots);
		CacheOriginalColors(androidSlots);
	}

	void CacheOriginalColors(GadgetSlotUI[] slotArray)
	{
		if (slotArray == null) return;
		for (int i = 0; i < slotArray.Length; i++)
			if (slotArray[i]?.iconBG != null)
				slotArray[i].originalIconColor = slotArray[i].iconBG.color;
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

            
            if (slot.lockedOverlay != null)
                slot.lockedOverlay.SetActive(locked);

            
            if (slot.cooldownOverlay != null)
            {
                float fill = (onCooldown && maxCD > 0f)
                    ? timer / maxCD
                    : 0f;

                slot.cooldownOverlay.fillAmount = fill;
                slot.cooldownOverlay.enabled    = fill > 0.01f;
            }

            
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
		
		
		if (androidSlots != null)
		{
			for (int i = 0; i < androidSlots.Length; i++)
			{
				GadgetSlotUI slot = slots[i];
				if (slot == null) continue;

				bool locked     = !GadgetManager.Instance.IsUnlocked(i);
				bool onCooldown =  GadgetManager.Instance.IsOnCooldown(i);
				float timer     =  GadgetManager.Instance.GetCooldownTimer(i);
				float maxCD     =  GadgetManager.Instance.GetMaxCooldown(i);

				
				if (slot.lockedOverlay != null)
					slot.lockedOverlay.SetActive(locked);

				
				if (slot.cooldownOverlay != null)
				{
					float fill = (onCooldown && maxCD > 0f)
						? timer / maxCD
						: 0f;

					slot.cooldownOverlay.fillAmount = fill;
					slot.cooldownOverlay.enabled    = fill > 0.01f;
				}

				
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
    }

    
    public void OnGadgetActivated(int slot, float cooldown)
    {
        if (slot < slots.Length && slots[slot] != null)
            slots[slot].flashTimer = flashDuration;
		
		if (androidSlots != null && slot < androidSlots.Length && androidSlots[slot] != null)
			androidSlots[slot].flashTimer = flashDuration;
    }
	
	public void RefreshLockState()
	{
		
		
		Debug.Log("[GadgetUI] Lock state refreshed.");
	}
}