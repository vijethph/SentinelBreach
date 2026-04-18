using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attaches to the Cinemachine virtual camera or Main Camera.
/// Shakes the camera for a brief duration when called.
/// Uses a perlin noise offset — no external packages required.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("Shake Settings")]
    public float defaultDuration  = 0.18f;
    public float defaultMagnitude = 0.12f;

    private Vector3 originalPos;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    /// <summary>Trigger a camera shake. Call from PlayerHealth.TakeDamage.</summary>
    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        if (duration  < 0f) duration  = defaultDuration;
        if (magnitude < 0f) magnitude = defaultMagnitude;
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float remaining = 1f - (elapsed / duration);   // fades out

            // Perlin noise gives smooth pseudo-random shake
            float x = (Mathf.PerlinNoise(elapsed * 30f, 0f) - 0.5f) * 2f * magnitude * remaining;
            float y = (Mathf.PerlinNoise(0f, elapsed * 30f) - 0.5f) * 2f * magnitude * remaining;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}