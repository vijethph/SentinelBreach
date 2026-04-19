using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;





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
    public float flashInTime  = 0.05f;   
    public float holdTime     = 0.04f;   
    public float flashOutTime = 0.18f;   
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

        
        float t = 0f;
        while (t < flashInTime)
        {
            t += Time.deltaTime;
            flashImage.color = new Color(1f, 0f, 0f, Mathf.Lerp(0f, maxAlpha, t / flashInTime));
            yield return null;
        }

        
        flashImage.color = new Color(1f, 0f, 0f, maxAlpha);
        yield return new WaitForSeconds(holdTime);

        
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
