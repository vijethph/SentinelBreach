using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves a drone back and forth across the corridor using a sine wave.
/// Student-implemented movement pattern — no NavMesh or physics.
/// </summary>
public class DronePatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("How far left/right the drone swings (in world units).")]
    public float patrolAmplitude = 2.0f;

    [Tooltip("How quickly the drone oscillates left/right.")]
    public float patrolFrequency = 1.5f;

    [Header("Bob (Up/Down Float Effect)")]
    [Tooltip("Up/down bobbing range.")]
    public float bobAmplitude = 0.25f;

    [Tooltip("Speed of the bobbing.")]
    public float bobFrequency = 2.0f;

    [Header("Damage")]
    public int damageAmount = 15;

    [Tooltip("Prevents dealing damage every frame — cooldown between hits.")]
    public float damageCooldown = 1.0f;

    // Internal state
    private Vector3 originPosition;
    private float damageTimer = 0f;
    private bool isDisabled = false;

    void Start()
    {
        // Record spawn position as the origin of patrol oscillation
        originPosition = transform.position;

        // Randomise the phase so multiple drones in one segment don't sync perfectly
        originPosition.x += Random.Range(-0.3f, 0.3f);
    }

    void Update()
    {
        if (isDisabled) return;

        // ── SINE WAVE PATROL ─────────────────────────────────────
        // x(t) = origin.x + A × sin(ω × t)
        // This is student-written motion — not physics-based
        float newX = originPosition.x + Mathf.Sin(Time.time * patrolFrequency) * patrolAmplitude;
        float newY = originPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;

        transform.position = new Vector3(newX, newY, transform.position.z);

        // ── ROTATION to face movement direction ──────────────────
        float xVelocity = Mathf.Cos(Time.time * patrolFrequency) * patrolAmplitude * patrolFrequency;
        if (Mathf.Abs(xVelocity) > 0.05f)
        {
            Vector3 lookDir = new Vector3(xVelocity, 0f, 1f).normalized;
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                5f * Time.deltaTime
            );
        }

        // Count down damage cooldown
        if (damageTimer > 0f) damageTimer -= Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isDisabled) return;
        if (damageTimer > 0f) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        ph?.TakeDamage(damageAmount, transform.position);
        damageTimer = damageCooldown;
    }

    /// <summary>
    /// Called by EMP gadget to temporarily disable this drone (Week 3).
    /// </summary>
    public void Disable(float duration)
    {
        StartCoroutine(DisableCoroutine(duration));
    }

    IEnumerator DisableCoroutine(float duration)
    {
        isDisabled = true;
        yield return new WaitForSeconds(duration);
        isDisabled = false;
    }
}