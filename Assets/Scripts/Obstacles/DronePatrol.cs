using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves drone back and forth across the corridor via sine wave.
/// Student-written motion — no NavMesh, no physics forces.
/// </summary>
public class DronePatrol : MonoBehaviour
{
    [Header("Patrol")]
    public float amplitude  = 2.0f;    // how far left-right it swings
    public float frequency  = 1.5f;    // how fast

    [Header("Bob (up/down float)")]
    public float bobAmplitude = 0.2f;
    public float bobFrequency = 2.0f;

    [Header("Damage")]
    public int   damage         = 15;
    public float damageCooldown = 1.0f;

    private Vector3 originPosition;
    private float   damageTimer = 0f;
    private bool    isDisabled  = false;
    private float   phaseOffset;

    void Start()
    {
        originPosition = transform.position;
        phaseOffset    = Random.Range(0f, Mathf.PI * 2f); // randomise phase
    }

    void Update()
    {
        if (isDisabled) return;

        // Sine-wave position: x(t) = origin.x + A·sin(ω·t + φ)
        float newX = originPosition.x + Mathf.Sin(Time.time * frequency + phaseOffset) * amplitude;
        float newY = originPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(newX, newY, transform.position.z);

        if (damageTimer > 0f) damageTimer -= Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDisabled || damageTimer > 0f) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        ph?.TakeDamage(damage, transform.position);
        damageTimer = damageCooldown;
    }

    /// <summary>Called by EMP gadget (Week 3).</summary>
    public void Disable(float duration) => StartCoroutine(DisableCoroutine(duration));

    IEnumerator DisableCoroutine(float dur)
    {
        isDisabled = true;
        yield return new WaitForSeconds(dur);
        isDisabled = false;
    }
}