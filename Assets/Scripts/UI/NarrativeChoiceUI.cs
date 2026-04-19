using System.Collections;
using System.Collections.Generic;
using UnityEngine;






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

    
    public void ShowChoice()
    {
        if (choicePanel == null) return;
        choicePanel.SetActive(true);
        Time.timeScale = 0f;   

        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(AutoChoiceAfterTimeout());
    }

    IEnumerator AutoChoiceAfterTimeout()
    {
        yield return new WaitForSecondsRealtime(autoChoiceTimeout);
        
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
