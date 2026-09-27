using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camera proxy target for Cinemachine.
/// Mirrors CIPHER's X and Z exactly, but clamps Y to a safe range
/// so the camera never follows CIPHER into the ceiling during jumps
/// or gravity inversion.
///
/// The Cinemachine virtual camera's Follow and LookAt fields must point
/// to this GameObject, NOT to CIPHER directly.
/// </summary>
public class CameraTarget : MonoBehaviour
{
    [Header("Target to Follow")]
    [Tooltip("Drag the CIPHER root GameObject here.")]
    public Transform cipherTransform;

    [Header("Y Clamp Settings")]
    [Tooltip("Minimum Y the camera proxy will go (floor level + small offset).")]
    public float minY = 0.5f;

    [Tooltip("Maximum Y the camera proxy will go. " +
             "Keep below corridor ceiling (4.0m) minus camera offset. " +
             "Recommended: 1.8 — keeps camera well below ceiling on jumps.")]
    public float maxY = 1.8f;

    [Header("Smoothing")]
    [Tooltip("How fast the proxy follows CIPHER's Y. " +
             "Lower = more lag (better for inversion). " +
             "Recommended: 4–6.")]
    public float yFollowSpeed = 5f;

    [Tooltip("How fast the proxy follows CIPHER's X (lane switch). " +
             "Match PlayerController.laneSwitchSpeed for visual consistency.")]
    public float xFollowSpeed = 10f;

    private float currentY;

    void Start()
    {
        if (cipherTransform == null)
        {
            // Auto-find if not assigned
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) cipherTransform = found.transform;
        }

        if (cipherTransform != null)
        {
            // Initialise at the clamped position so there is no startup jump
            currentY = Mathf.Clamp(cipherTransform.position.y, minY, maxY);
            transform.position = new Vector3(
                cipherTransform.position.x,
                currentY,
                cipherTransform.position.z
            );
        }
    }

    void LateUpdate()
	{
		if (cipherTransform == null) return;

		Vector3 cipherPos = cipherTransform.position;

		// Adjust Y clamp during gravity inversion so the camera
		// rises slightly to keep CIPHER in frame on the ceiling.
		bool inverted = GravityInversion.Instance != null
						&& GravityInversion.Instance.IsInverted;

		float effectiveMaxY = inverted ? maxY + 1.5f : maxY;  // rise to 3.3 when inverted
		float effectiveMinY = inverted ? minY + 1.5f : minY;

		// ── X: smooth lateral follow ──────────────────────────────────
		float newX = Mathf.Lerp(transform.position.x, cipherPos.x,
								xFollowSpeed * Time.deltaTime);

		// ── Y: clamped smooth follow ──────────────────────────────────
		currentY = Mathf.Lerp(currentY, cipherPos.y, yFollowSpeed * Time.deltaTime);
		float clampedY = Mathf.Clamp(currentY, effectiveMinY, effectiveMaxY);

		// ── Z: exact mirror ────────────────────────────────────────────
		transform.position = new Vector3(newX, clampedY, cipherPos.z);
	}
}