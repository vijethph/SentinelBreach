using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays 3 active quest cards in the HUD.
/// Uses a one-frame delayed init coroutine so QuestManager is always ready first.
/// </summary>
public class QuestUI : MonoBehaviour
{
    [System.Serializable]
    public class QuestCard
    {
        public GameObject          cardRoot;
        public TextMeshProUGUI     descText;
        public TextMeshProUGUI     progressText;
        public Image               progressBar;       // Image Type = Filled, Horizontal fill
        public GameObject          completedOverlay;  // green panel shown when done
    }

    [Header("Quest Cards (must be exactly 3)")]
    public QuestCard[] cards;

    void Start()
    {
        // Delay one frame to guarantee QuestManager.Start() has run
        StartCoroutine(InitAfterOneFrame());
    }

    IEnumerator InitAfterOneFrame()
    {
        yield return null;  // skip one frame

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[QuestUI] QuestManager.Instance is null — quest cards will be blank.");
            yield break;
        }

        // Subscribe to live progress updates
        QuestManager.Instance.OnProgressUpdated += UpdateCard;

        // Populate initial state
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null) continue;

            QuestData q = QuestManager.Instance.GetQuest(i);
            if (q == null)
            {
                // More quests than cards or not enough quest data assets
                if (cards[i].cardRoot != null) cards[i].cardRoot.SetActive(false);
                continue;
            }

            if (cards[i].descText)     cards[i].descText.text = q.description;
            if (cards[i].progressText) cards[i].progressText.text = $"0 / {q.targetCount}";
            if (cards[i].progressBar)  cards[i].progressBar.fillAmount = 0f;
            if (cards[i].completedOverlay) cards[i].completedOverlay.SetActive(false);
        }
    }

    void UpdateCard(int slot, int current, int target)
    {
        if (slot >= cards.Length || cards[slot] == null) return;

        bool done = current >= target;
        float fill = target > 0 ? Mathf.Clamp01((float)current / target) : 0f;

        if (cards[slot].progressText)
            cards[slot].progressText.text = done ? "COMPLETE" : $"{current} / {target}";

        if (cards[slot].progressBar)
            cards[slot].progressBar.fillAmount = fill;

        if (cards[slot].completedOverlay)
            cards[slot].completedOverlay.SetActive(done);
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnProgressUpdated -= UpdateCard;
    }
}