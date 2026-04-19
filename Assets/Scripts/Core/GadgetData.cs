using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GadgetData", menuName = "CipherGame/Gadget Data")]
public class GadgetData : ScriptableObject
{
    public string gadgetId;
    public string displayName;
    public Sprite icon;             
    public float baseCooldown;      
    public float duration;          
    public float force;             
}
