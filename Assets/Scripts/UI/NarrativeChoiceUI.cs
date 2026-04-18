using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows the narrative choice popup when CIPHER reaches a terminal.
/// Pauses the game (timeScale 0) while the player chooses.
/// Auto-dismisses if no input for 5 seconds (defaults to Stealth).
/// </summary>
public class NarrativeChoiceUI : MonoBehaviour
{
    public static NarrativeChoiceUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("Panel")]
    public GameObject choicePanel;

    [Tooltip("Seconds before auto-choosing Stealth if player doesn't respond.")]
    public float autoChoiceTimeout = 5f;

    private Coroutine timeoutCoroutine;

    /// <summary>Called by TerminalTrigger when CIPHER enters a terminal zone.</summary>
    public void ShowChoice()
    {
        if (choicePanel == null) return;
        choicePanel.SetActive(true);
        Time.timeScale = 0f;   // freeze gameplay while player reads

        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(AutoChoiceAfterTimeout());
    }

    IEnumerator AutoChoiceAfterTimeout()
    {
        yield return new WaitForSecondsRealtime(autoChoiceTimeout);
        // Default to stealth if player doesn't respond
        OnStealthChosen();
    }

    public void OnStealthChosen()
    {
        if (timeoutCoroutine != null) { StopCoroutine(timeoutCoroutine); timeoutCoroutine = null; }
        NarrativeState.Instance?.ChooseStealth();
        HidePanel();
    }

    public void OnBruteForceChosen()
    {
        if (timeoutCoroutine != null) { StopCoroutine(timeoutCoroutine); timeoutCoroutine = null; }
        NarrativeState.Instance?.ChooseBruteForce();
        HidePanel();
    }

    void HidePanel()
    {
        if (choicePanel != null) choicePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
