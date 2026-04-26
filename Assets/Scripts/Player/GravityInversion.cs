using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gravity Inversion power-up effect.
///
/// Student-written physics:
///   Normal:   verticalVelocity += gravity * dt   (gravity = -25, falls down)
///   Inverted: verticalVelocity += (-gravity) * dt (gravity = +25, falls up)
///
/// CIPHER physically accelerates toward the ceiling, runs inverted, then
/// physically falls back to the floor when the effect expires.
/// No teleportation — the full kinematic integration runs in both directions.
///
/// Visual flip: Transform.Rotate 180° on the Z axis over a short tween duration.
/// </summary>
public class GravityInversion : MonoBehaviour
{
    public static GravityInversion Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("Settings")]
    [Tooltip("Duration CIPHER spends inverted (seconds).")]
    public float invertDuration = 6f;

    [Tooltip("Seconds to complete the 180° visual roll.")]
    public float flipTweenDuration = 0.35f;

    [Tooltip("Scale of the screen-edge colour tint while inverted.")]
    public float overlayAlpha = 0.18f;

    [Header("References")]
    [Tooltip("Optional full-screen Image overlay tinted during inversion.")]
    public UnityEngine.UI.Image invertOverlay;

    // Runtime
    private bool isInverted = false;
    private Coroutine activeCoroutine;

    // ─── Public API ────────────────────────────────────────────────

    public bool IsInverted => isInverted;

    /// <summary>Called by CollectibleManager when an Invert Chip is collected.</summary>
    public void Activate()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(InversionCoroutine());
    }

    // ─── Core coroutine ────────────────────────────────────────────

    IEnumerator InversionCoroutine()
    {
        PlayerController pc = GetComponent<PlayerController>();
        if (pc == null) yield break;

        // ── Phase 1: flip visual and reverse gravity ──────────────
        isInverted = true;
        pc.SetGravityInverted(true);            // tell PlayerController to flip g
        yield return StartCoroutine(FlipRoll(0f, 180f));

        // Show tint overlay
        if (invertOverlay != null)
        {
            invertOverlay.gameObject.SetActive(true);
            Color c = invertOverlay.color;
            c.a = overlayAlpha;
            invertOverlay.color = c;
        }

        Debug.Log("[GravityInversion] Inverted — CIPHER running on ceiling.");

        // ── Phase 2: hold duration ────────────────────────────────
        yield return new WaitForSeconds(invertDuration);

        // ── Phase 3: flip back ─────────────────────────────────────
        pc.SetGravityInverted(false);
        yield return StartCoroutine(FlipRoll(180f, 0f));
        isInverted = false;

        if (invertOverlay != null)
            invertOverlay.gameObject.SetActive(false);

        Debug.Log("[GravityInversion] Reverted — CIPHER back on floor.");
        activeCoroutine = null;
    }

    // ─── Visual roll tween ─────────────────────────────────────────

    /// <summary>
    /// Rotates CIPHER's mesh child 180° on the Z axis over flipTweenDuration seconds.
    /// Uses a sinusoidal ease-in-out for a polished feel without an Animator.
    /// </summary>
    IEnumerator FlipRoll(float fromZ, float toZ)
    {
        // Find the mesh child (the rendered model, not the root with CharacterController)
        Transform mesh = transform.Find("CIPHER");
        if (mesh == null) mesh = transform;         // fallback to root if no named child

        float elapsed = 0f;
        while (elapsed < flipTweenDuration)
        {
            elapsed += Time.deltaTime;
            float t   = Mathf.Clamp01(elapsed / flipTweenDuration);
            float tEased = t * t * (3f - 2f * t);  // smoothstep (sinusoidal ease in-out)
            float z   = Mathf.Lerp(fromZ, toZ, tEased);

            // Preserve existing X and Y rotation, only drive Z
            Vector3 euler = mesh.localEulerAngles;
            euler.z = z;
            mesh.localEulerAngles = euler;

            yield return null;
        }

        // Snap to exact final value
        Vector3 finalEuler = mesh.localEulerAngles;
        finalEuler.z = toZ;
        mesh.localEulerAngles = finalEuler;
    }
	
}