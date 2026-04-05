using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GadgetData", menuName = "CipherGame/Gadget Data")]
public class GadgetData : ScriptableObject
{
    public string gadgetId;
    public string displayName;
    public Sprite icon;             // optional — assign a sprite for HUD icon
    public float baseCooldown;      // seconds
    public float duration;          // how long the effect lasts (for EMP, Time-Slow)
    public float force;             // used by Dash only
}
