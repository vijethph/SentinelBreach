using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Info Modal panel on the Main Menu.
/// Opened by the info button (top-left corner).
/// Closed by the close button inside the modal.
/// Also closeable by pressing Escape.
/// </summary>
public class InfoModalController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The info modal panel GameObject.")]
    public GameObject infoModalPanel;

    [Tooltip("Optional: dim the background when modal is open.")]
    public GameObject backgroundDim;

    [Tooltip("The MainButtons_Panel — hidden while modal is open.")]
    public GameObject mainButtonsPanel;

    private bool isOpen = false;

    void Update()
    {
        // Allow Escape to close the info modal
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseInfo();
    }

    /// <summary>Called by the Info button (ⓘ) OnClick.</summary>
    public void OpenInfo()
    {
        if (infoModalPanel != null) infoModalPanel.SetActive(true);
        if (backgroundDim  != null) backgroundDim.SetActive(true);
        isOpen = true;
    }

    /// <summary>Called by the Close button inside the modal OnClick.</summary>
    public void CloseInfo()
    {
        if (infoModalPanel != null) infoModalPanel.SetActive(false);
        if (backgroundDim  != null) backgroundDim.SetActive(false);
        isOpen = false;
    }
}