using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a wall-mounted turret that tracks and fires at the player.
/// Rotates its head toward the player and periodically fires a raycast.
/// </summary>
public class TurretController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The child Transform that rotates to face the player (TurretHead).")]
    public Transform turretHead;

    [Tooltip("The muzzle point — raycast fires from here.")]
    public Transform muzzle;

    [Header("Settings")]
    [Tooltip("Maximum distance at which turret detects and engages the player.")]
    public float detectionRange = 12f;

    [Tooltip("Time between shots in seconds.")]
    public float fireInterval = 1.5f;

    [Tooltip("HP damage per shot.")]
    public int damageAmount = 20;

    [Tooltip("Speed at which the head rotates toward the player.")]
    public float rotationSpeed = 3f;

    [Header("VFX (optional)")]
    [Tooltip("Optional particle effect for muzzle flash.")]
    public ParticleSystem muzzleFlash;

    // Internal state
    private Transform player;
    private float fireTimer;
    private bool isDisabled = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        fireTimer = Random.Range(0f, fireInterval); // randomise first shot timing
    }

    void Update()
    {
        if (isDisabled || player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= detectionRange)
        {
            TrackPlayer();
            HandleFiring();
        }
    }

    void TrackPlayer()
    {
        // Rotate turret head smoothly toward the player
        Vector3 dirToPlayer = (player.position - turretHead.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer);
        turretHead.rotation = Quaternion.Slerp(
            turretHead.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void HandleFiring()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Fire();
            fireTimer = fireInterval;
        }
    }

    void Fire()
    {
        // Trigger muzzle flash VFX if assigned
        muzzleFlash?.Play();

        // Raycast from muzzle toward player
        Vector3 fireDirection = (player.position - muzzle.position).normalized;
        Ray ray = new Ray(muzzle.position, fireDirection);

        if (Physics.Raycast(ray, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                PlayerHealth ph = hit.collider.GetComponent<PlayerHealth>();
                ph?.TakeDamage(damageAmount, muzzle.position);
            }
        }

        // Draw a debug line visible in Scene view during Play mode
        Debug.DrawLine(muzzle.position, muzzle.position + fireDirection * detectionRange, Color.red, 0.2f);
    }

    /// <summary>
    /// Disables the turret for a set duration. Called by EMP gadget (Week 3).
    /// </summary>
    public void Disable(float duration)
    {
        StartCoroutine(DisableCoroutine(duration));
    }

    IEnumerator DisableCoroutine(float duration)
    {
        isDisabled = true;
        // Optionally dim the turret head material to indicate disabled state
        if (turretHead != null)
        {
            Renderer r = turretHead.GetComponent<Renderer>();
            if (r != null) r.material.SetColor("_EmissionColor", Color.blue * 0.3f);
        }

        yield return new WaitForSeconds(duration);

        isDisabled = false;
        if (turretHead != null)
        {
            Renderer r = turretHead.GetComponent<Renderer>();
            if (r != null) r.material.SetColor("_EmissionColor", Color.red);
        }
    }
}