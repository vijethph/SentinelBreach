using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

/// <summary>
/// Displays a brief popup when a quest is completed.
/// Auto-dismisses after displayDuration seconds.
/// </summary>
public class QuestPopup : MonoBehaviour
{
    public static QuestPopup Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("References")]
    public GameObject      popupPanel;
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescText;
    public TextMeshProUGUI rewardText;
    public CanvasGroup     canvasGroup;   // for fade effect

    [Header("Settings")]
    public float displayDuration = 3f;
    public float fadeSpeed       = 4f;

    private Coroutine activeCoroutine;

    /// <summary>
    /// Call this to show the popup for a completed quest.
    /// </summary>
    public void ShowCompletion(QuestData quest)
    {
        if (quest == null) return;

        if (questTitleText) questTitleText.text = "QUEST COMPLETE";
        if (questDescText)  questDescText.text  = quest.description;

        string reward = "";
        if (quest.xpReward   > 0) reward += $"+{quest.xpReward} XP  ";
        if (quest.shardBonus > 0) reward += $"+{quest.shardBonus} SHARDS";
        if (rewardText) rewardText.text = reward.Trim();

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(ShowAndFade());
    }

    IEnumerator ShowAndFade()
    {
        // Instant show
        popupPanel.SetActive(true);
        canvasGroup.alpha = 1f;

        // Hold for displayDuration
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }

        popupPanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }
}
