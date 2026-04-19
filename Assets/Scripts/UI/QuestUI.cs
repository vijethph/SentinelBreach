using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;





public class QuestUI : MonoBehaviour
{
    [System.Serializable]
    public class QuestCard
    {
        public GameObject          cardRoot;
        public TextMeshProUGUI     descText;
        public TextMeshProUGUI     progressText;
        public Image               progressBar;       
        public GameObject          completedOverlay;  
    }

    [Header("Quest Cards (must be exactly 3)")]
    public QuestCard[] cards;

    void Start()
    {
        
        StartCoroutine(InitAfterOneFrame());
    }

    IEnumerator InitAfterOneFrame()
    {
        yield return null;  

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[QuestUI] QuestManager.Instance is null — quest cards will be blank.");
            yield break;
        }

        
        QuestManager.Instance.OnProgressUpdated += UpdateCard;

        
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null) continue;

            QuestData q = QuestManager.Instance.GetQuest(i);
            if (q == null)
            {
                
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