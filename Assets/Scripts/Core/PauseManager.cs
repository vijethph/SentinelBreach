using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles pause state triggered by Escape key.
/// Pauses Time.timeScale and shows/hides the pause panel.
/// Does NOT show upgrades screen — that is only accessible from the Main Menu.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("UI References")]
    public GameObject pausePanel;

    [Header("GameObjects to disable while paused")]
    [Tooltip("Optional: objects to hide during pause (e.g. HUD elements).")]
    public GameObject[] hideWhilePaused;

    public bool IsPaused { get; private set; } = false;

    void Update()
    {
        // Toggle pause with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused) Resume();
            else          Pause();
        }
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;

        Time.timeScale = 0f;        // stops all physics, animations, coroutines
        if (pausePanel != null) pausePanel.SetActive(true);

        foreach (var obj in hideWhilePaused)
            if (obj != null) obj.SetActive(false);

        Debug.Log("[PauseManager] Game paused.");
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;

        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);

        foreach (var obj in hideWhilePaused)
            if (obj != null) obj.SetActive(true);

        Debug.Log("[PauseManager] Game resumed.");
    }

    /// <summary>Called by the Quit button in the pause panel.</summary>
    public void QuitToMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}