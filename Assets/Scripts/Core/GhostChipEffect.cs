using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When a Ghost Chip is collected, all active obstacles in the scene
/// are disabled for ghostDuration seconds. A visual overlay tints the screen purple.
/// </summary>
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

    /// <summary>Called by CollectibleManager when a Ghost Chip is collected.</summary>
    public void ActivateGhostMode()
    {
        StartCoroutine(GhostCoroutine());
    }

    IEnumerator GhostCoroutine()
    {
        Debug.Log("[GhostChip] Ghost mode activated — disabling all obstacles.");

        // Show purple overlay
        if (screenOverlay != null)
        {
            screenOverlay.gameObject.SetActive(true);
            screenOverlay.color = new Color(0.5f, 0f, 1f, 0.18f);
        }

        // Find and disable ALL obstacle components in the scene
        foreach (var laser in FindObjectsOfType<LaserGridController>())
            laser.GhostDisable(ghostDuration);

        foreach (var turret in FindObjectsOfType<TurretController>())
            turret.Disable(ghostDuration);

        foreach (var drone in FindObjectsOfType<DronePatrol>())
            drone.Disable(ghostDuration);

        // Wait for duration
        yield return new WaitForSeconds(ghostDuration);

        // Hide overlay
        if (screenOverlay != null)
            screenOverlay.gameObject.SetActive(false);

        Debug.Log("[GhostChip] Ghost mode expired — obstacles re-enabled.");
    }
}