using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Toggles the laser beam on and off on a timed cycle.
/// Creates a rhythm the player must time their movement around.
/// </summary>
public class LaserToggle : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("How long the laser stays ON (active/dangerous).")]
    public float onDuration = 1.5f;

    [Tooltip("How long the laser stays OFF (safe window to pass).")]
    public float offDuration = 1.0f;

    [Tooltip("Random offset added to start time so multiple lasers don't all sync.")]
    public float randomStartOffset = 0f;

    [Header("References")]
    [Tooltip("The Line Renderer component that renders the beam.")]
    public LineRenderer laserBeam;

    [Tooltip("The Box Collider (Is Trigger) on this object.")]
    public Collider laserCollider;

    void Start()
    {
        // Randomise start phase so multiple lasers in one segment don't all flash together
        randomStartOffset = Random.Range(0f, onDuration + offDuration);
        StartCoroutine(ToggleLaser());
    }

    IEnumerator ToggleLaser()
    {
        // Wait for random offset before starting
        yield return new WaitForSeconds(randomStartOffset);

        while (true)
        {
            // ── LASER ON ───────────────────────────────────────────
            SetLaserActive(true);
            yield return new WaitForSeconds(onDuration);

            // ── LASER OFF ──────────────────────────────────────────
            SetLaserActive(false);
            yield return new WaitForSeconds(offDuration);
        }
    }

    void SetLaserActive(bool active)
    {
        if (laserBeam != null)   laserBeam.enabled = active;
        if (laserCollider != null) laserCollider.enabled = active;
    }

    /// <summary>
    /// Called by EMP gadget (Week 3) to force the laser off temporarily.
    /// </summary>
    public void ForceOff(float duration)
    {
        StartCoroutine(ForceOffCoroutine(duration));
    }

    IEnumerator ForceOffCoroutine(float duration)
    {
        StopCoroutine(nameof(ToggleLaser));
        SetLaserActive(false);
        yield return new WaitForSeconds(duration);
        StartCoroutine(ToggleLaser());
    }
}