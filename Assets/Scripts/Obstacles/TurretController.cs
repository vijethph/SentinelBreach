using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks and fires at the player.
/// ⚠️ muzzleFlash field removed to prevent NullReferenceException.
/// Detection uses a Sphere Collider trigger on the Turret parent.
/// Firing uses a Raycast from the Muzzle transform.
/// </summary>
public class TurretController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The rotating child (TurretHead).")]
    public Transform turretHead;

    [Tooltip("Raycast origin — empty child of TurretHead named Muzzle.")]
    public Transform muzzle;

    [Header("Settings")]
    public float rotationSpeed  = 3f;
    public float fireInterval   = 1.5f;
    public float detectionRange = 12f;
    public int   damage         = 20;

    // Internal
    private Transform player;
    private float     fireTimer;
    private bool      isDisabled = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("[TurretController] No GameObject tagged 'Player' found.");

        // Stagger first shot so multiple turrets don't fire simultaneously
        fireTimer = Random.Range(0f, fireInterval);
    }

    void Update()
    {
        if (isDisabled || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange) return;

        TrackPlayer();
        HandleFiring();
    }

    void TrackPlayer()
    {
        if (turretHead == null) return;

        Vector3 dir = (player.position - turretHead.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        turretHead.rotation = Quaternion.Slerp(
            turretHead.rotation, targetRot,
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
        if (muzzle == null)
        {
            Debug.LogWarning("[TurretController] Muzzle not assigned!");
            return;
        }

        Vector3 fireDir = (player.position - muzzle.position).normalized;
        Ray ray = new Ray(muzzle.position, fireDir);

        // Debug line visible in Scene view during Play mode
        Debug.DrawLine(muzzle.position, muzzle.position + fireDir * detectionRange,
                       Color.red, 0.15f);

        if (Physics.Raycast(ray, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                PlayerHealth ph = hit.collider.GetComponent<PlayerHealth>();
                ph?.TakeDamage(damage, muzzle.position);
            }
        }
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