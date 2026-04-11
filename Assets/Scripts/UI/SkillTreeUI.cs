using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the Skill Tree upgrade screen UI.
/// One instance manages all 5 node cards.
/// </summary>
public class SkillTreeUI : MonoBehaviour
{
    [System.Serializable]
    public class SkillCardUI
    {
        public string nodeId;                   // must match SkillData.nodeId
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI costText;
        public Button upgradeButton;
    }

    [Header("Skill Cards (one per node)")]
    public SkillCardUI[] cards;

    [Header("Shard Balance Display")]
    public TextMeshProUGUI shardBalanceText;

    void OnEnable()
    {
        // Refresh every time the panel opens
        RefreshAll();
    }

    void RefreshAll()
    {
        if (SkillTree.Instance == null) return;

        int shards = SkillTree.Instance.GetTotalShards();
        if (shardBalanceText != null)
            shardBalanceText.text = $"SHARDS: {shards}";

        foreach (var card in cards)
        {
            SkillData data = SkillTree.Instance.GetSkillData(card.nodeId);
            if (data == null) continue;

            int level = SkillTree.Instance.GetLevel(card.nodeId);
            bool maxed = level >= data.MaxLevel;

            if (card.nameText) card.nameText.text = data.displayName;

            if (card.levelText) card.levelText.text = $"Level: {level} / {data.MaxLevel}";

            if (card.descText)
                card.descText.text = maxed ? "MAX LEVEL" : data.levelDescriptions[level];

            if (card.costText)
                card.costText.text = maxed ? "—" : $"Cost: {data.costs[level]} Shards";

            if (card.upgradeButton)
            {
                card.upgradeButton.interactable = !maxed && shards >= (maxed ? 0 : data.costs[level]);
            }
        }
    }

    /// <summary>
    /// Called by each Upgrade button's OnClick event.
    /// Pass the nodeId as a string parameter in the Button Inspector.
    /// </summary>
    public void OnUpgradePressed(string nodeId)
    {
        if (SkillTree.Instance == null) return;

        int currentShards = SkillTree.Instance.GetTotalShards();
        bool success = SkillTree.Instance.TryUpgrade(nodeId, currentShards, out int newShards);

        if (success)
        {
            Debug.Log($"[SkillTreeUI] Upgraded {nodeId}. New shards: {newShards}");
            RefreshAll();
        }
        else
        {
            Debug.Log($"[SkillTreeUI] Cannot upgrade {nodeId} — insufficient shards or max level.");
        }
    }
}