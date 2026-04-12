using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the transparent shield sphere around CIPHER.
/// Activated by collecting a Shield Cell collectible.
/// While active, PlayerHealth.IsInvincible is set to true.
/// </summary>
public class ShieldEffect : MonoBehaviour
{
    public static ShieldEffect Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("References")]
    [Tooltip("The ShieldSphere child GameObject on CIPHER.")]
    public GameObject shieldSphere;

    [Tooltip("Duration of the shield in seconds.")]
    public float shieldDuration = 5f;

    private PlayerHealth playerHealth;
    private Coroutine activeShield;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (shieldSphere != null) shieldSphere.SetActive(false);
    }

    /// <summary>
    /// Activates the shield. Called by CollectibleManager when a Shield Cell is collected.
    /// </summary>
    public void ActivateShield()
    {
        if (activeShield != null) StopCoroutine(activeShield);
        activeShield = StartCoroutine(ShieldCoroutine());
    }

    IEnumerator ShieldCoroutine()
    {
        Debug.Log("[ShieldEffect] Shield activated.");

        // Show sphere and grant invincibility
        if (shieldSphere != null) shieldSphere.SetActive(true);
        playerHealth?.SetInvincible(true, shieldDuration);

        // Pulse the sphere: slightly grow and shrink while active
        float elapsed = 0f;
        Vector3 baseScale = shieldSphere != null ? shieldSphere.transform.localScale : Vector3.one;

        while (elapsed < shieldDuration)
        {
            elapsed += Time.deltaTime;

            if (shieldSphere != null)
            {
                float pulse = 1f + 0.04f * Mathf.Sin(elapsed * 6f);
                shieldSphere.transform.localScale = baseScale * pulse;

                // Fade out in last 1 second
                if (elapsed > shieldDuration - 1f)
                {
                    Renderer r = shieldSphere.GetComponent<Renderer>();
                    if (r != null)
                    {
                        Color c = r.material.color;
                        c.a = Mathf.Lerp(0.24f, 0f, (elapsed - (shieldDuration - 1f)));
                        r.material.color = c;
                    }
                }
            }
            yield return null;
        }

        // Deactivate
        if (shieldSphere != null)
        {
            // Reset alpha before hiding
            Renderer r = shieldSphere.GetComponent<Renderer>();
            if (r != null)
            {
                Color c = r.material.color;
                c.a = 0.24f;
                r.material.color = c;
            }
            shieldSphere.SetActive(false);
        }

        Debug.Log("[ShieldEffect] Shield expired.");
        activeShield = null;
    }
}
