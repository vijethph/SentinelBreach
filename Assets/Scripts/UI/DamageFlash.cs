using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Briefly flashes the screen red when CIPHER takes damage.
/// Called from PlayerHealth.TakeDamage().
/// </summary>
public class DamageFlash : MonoBehaviour
{
    public static DamageFlash Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("References")]
    public Image flashImage;

    [Header("Settings")]
    public float flashInTime  = 0.05f;   // how fast it appears (seconds)
    public float holdTime     = 0.04f;   // how long it stays at peak
    public float flashOutTime = 0.18f;   // how fast it fades
    public float maxAlpha     = 0.55f;

    private Coroutine activeFlash;

    public void TriggerFlash()
    {
        if (activeFlash != null) StopCoroutine(activeFlash);
        activeFlash = StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        if (flashImage == null) yield break;
        flashImage.gameObject.SetActive(true);

        // Fade IN
        float t = 0f;
        while (t < flashInTime)
        {
            t += Time.deltaTime;
            flashImage.color = new Color(1f, 0f, 0f, Mathf.Lerp(0f, maxAlpha, t / flashInTime));
            yield return null;
        }

        // HOLD
        flashImage.color = new Color(1f, 0f, 0f, maxAlpha);
        yield return new WaitForSeconds(holdTime);

        // Fade OUT
        t = 0f;
        while (t < flashOutTime)
        {
            t += Time.deltaTime;
            flashImage.color = new Color(1f, 0f, 0f, Mathf.Lerp(maxAlpha, 0f, t / flashOutTime));
            yield return null;
        }

        flashImage.color = new Color(1f, 0f, 0f, 0f);
        flashImage.gameObject.SetActive(false);
    }
}
