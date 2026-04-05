using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows/hides UI elements based on platform at runtime.
/// Keeps the HUD appropriate for the device being used.
/// </summary>
public class AndroidUIAdapter : MonoBehaviour
{
    [Header("PC-only elements (hidden on Android)")]
    public GameObject[] pcOnlyElements;

    [Header("Android-only elements (hidden on PC)")]
    public GameObject[] androidOnlyElements;

    void Awake()
    {
        bool isMobile = Application.isMobilePlatform;

        foreach (var obj in pcOnlyElements)
            if (obj != null) obj.SetActive(!isMobile);

        foreach (var obj in androidOnlyElements)
            if (obj != null) obj.SetActive(isMobile);
    }
}
