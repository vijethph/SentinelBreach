using System.Collections;
using System.Collections.Generic;
using UnityEngine;












public class AndroidUIAdapter : MonoBehaviour
{
    [Header("Shown ONLY on Android")]
    [Tooltip("AndroidGadgets_Panel, Calibrate_Button, etc.")]
    public GameObject[] androidOnlyElements;

    [Header("Shown ONLY on PC / Desktop")]
    [Tooltip("GadgetsPC_Panel (Q/E/R cooldown UI), desktop hint labels, etc.")]
    public GameObject[] pcOnlyElements;

    [Header("Always visible on both platforms")]
    [Tooltip("Health bar, distance, score, quest cards, etc. — leave empty if none.")]
    public GameObject[] alwaysVisibleElements;

    void Awake()
    {
        bool isMobile = Application.isMobilePlatform;

        foreach (var obj in androidOnlyElements)
            if (obj != null) obj.SetActive(isMobile);

        foreach (var obj in pcOnlyElements)
            if (obj != null) obj.SetActive(!isMobile);

        foreach (var obj in alwaysVisibleElements)
            if (obj != null) obj.SetActive(true);

        Debug.Log($"[AndroidUIAdapter] Platform: {(isMobile ? "Android" : "PC")}. " +
                  $"Android elements: {(isMobile ? "SHOWN" : "HIDDEN")}.");
    }
}