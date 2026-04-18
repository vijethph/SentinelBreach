using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Briefly flashes CIPHER's material to white on taking damage.
/// Classic hit-flash technique used in all action games.
/// Works on a joined mesh with multiple material slots.
/// </summary>
public class HitFlash : MonoBehaviour
{
    public static HitFlash Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("Settings")]
    public float flashDuration = 0.12f;
    public Color flashColor    = Color.white;

    private List<Renderer>  renderers   = new List<Renderer>();
    private List<Material[]> origMats   = new List<Material[]>();
    private bool flashing = false;

    void Start()
    {
        // Collect all renderers on CIPHER and its children
        GetComponentsInChildren<Renderer>(true, renderers);

        foreach (var r in renderers)
            origMats.Add(r.materials);
    }

    public void Flash()
    {
        if (flashing) return;
        StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine()
    {
        flashing = true;

        // Create white override materials
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] == null) continue;
            Material[] flashMats = new Material[origMats[i].Length];
            for (int j = 0; j < flashMats.Length; j++)
            {
                flashMats[j] = new Material(origMats[i][j]);
                flashMats[j].color = flashColor;
                flashMats[j].SetColor("_EmissionColor", flashColor);
                flashMats[j].EnableKeyword("_EMISSION");
            }
            renderers[i].materials = flashMats;
        }

        yield return new WaitForSeconds(flashDuration);

        // Restore original materials
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null)
                renderers[i].materials = origMats[i];
        }

        flashing = false;
    }
}
