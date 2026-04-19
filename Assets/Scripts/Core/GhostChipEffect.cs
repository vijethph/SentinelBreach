using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class GhostChipEffect : MonoBehaviour
{
    public static GhostChipEffect Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("Settings")]
    public float ghostDuration = 4f;

    [Header("Visual Overlay (optional)")]
    [Tooltip("A full-screen Image or CanvasGroup that tints the screen purple briefly.")]
    public UnityEngine.UI.Image screenOverlay;

    
    public void ActivateGhostMode()
    {
        StartCoroutine(GhostCoroutine());
    }

    IEnumerator GhostCoroutine()
    {
        Debug.Log("[GhostChip] Ghost mode activated — disabling all obstacles.");

        
        if (screenOverlay != null)
        {
            screenOverlay.gameObject.SetActive(true);
            screenOverlay.color = new Color(0.5f, 0f, 1f, 0.18f);
        }

        
        foreach (var laser in FindObjectsOfType<LaserGridController>())
            laser.GhostDisable(ghostDuration);

        foreach (var turret in FindObjectsOfType<TurretController>())
            turret.Disable(ghostDuration);

        foreach (var drone in FindObjectsOfType<DronePatrol>())
            drone.Disable(ghostDuration);

        
        yield return new WaitForSeconds(ghostDuration);

        
        if (screenOverlay != null)
            screenOverlay.gameObject.SetActive(false);

        Debug.Log("[GhostChip] Ghost mode expired — obstacles re-enabled.");
    }
}