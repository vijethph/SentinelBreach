using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    // ⚠️ Use Awake for singleton + quest selection so QuestUI.Start() can safely read quests
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Select quests in Awake so they are ready before any Start() runs
        SelectQuests();
    }

    [Header("All quest definitions (drag all 8 QuestData assets)")]
    public QuestData[] allQuests;

    [Header("How many quests per run")]
    public int questsPerRun = 3;

    private QuestData[]  activeQuests;
    private int[]        progress;
    private bool[]       completed;

    public event Action<int>         OnQuestCompleted;    // passes XP reward
    public event Action<int,int,int> OnProgressUpdated;   // (questSlot, current, target)

    private PlayerController   playerController;
    private CollectibleManager collectibleManager;

    // No-damage tracking — properly managed
    private float lastDamageZ   = 0f;   // the Z distance when damage was last taken
    private bool  hasTakenDamage = false;

    void Start()
    {
        playerController   = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        collectibleManager = CollectibleManager.Instance;
        SubscribeToEvents();

        // Initialise no-damage baseline
        lastDamageZ = playerController != null ? playerController.DistanceRun : 0f;
    }

    void SelectQuests()
    {
        if (allQuests == null || allQuests.Length == 0)
        {
            Debug.LogWarning("[QuestManager] No quest data assets assigned in Inspector!");
            return;
        }

        List<QuestData> pool = new List<QuestData>(allQuests);
        // Fisher-Yates shuffle
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int count    = Mathf.Min(questsPerRun, pool.Count);
        activeQuests = new QuestData[count];
        progress     = new int[count];
        completed    = new bool[count];

        for (int i = 0; i < count; i++)
            activeQuests[i] = pool[i];

        Debug.Log($"[QuestManager] Active quests: {string.Join(", ", System.Array.ConvertAll(activeQuests, q => q.questId))}");
    }

    void SubscribeToEvents()
    {
        if (collectibleManager != null)
        {
            collectibleManager.OnShardCollected     += _ => IncrementQuests(QuestType.CollectShards, 1);
            collectibleManager.OnGhostChipCollected += () => IncrementQuests(QuestType.CollectGhostChips, 1);
        }
    }

    void Update()
    {
        if (playerController == null || activeQuests == null) return;

        float dist = playerController.DistanceRun;

        for (int i = 0; i < activeQuests.Length; i++)
        {
            if (completed[i]) continue;
            QuestData q = activeQuests[i];

            switch (q.questType)
            {
                case QuestType.ReachDistance:
                    // Update progress every frame so bar fills in real-time
                    int currentDist = (int)dist;
                    if (currentDist != progress[i])
                    {
                        progress[i] = currentDist;
                        OnProgressUpdated?.Invoke(i, progress[i], q.targetCount);
                        if (progress[i] >= q.targetCount) CompleteQuest(i);
                    }
                    break;

                case QuestType.TakeDamage:
                    // Measures the longest no-damage stretch
                    if (!hasTakenDamage)
                    {
                        int stretch = (int)(dist - lastDamageZ);
                        if (stretch != progress[i])
                        {
                            progress[i] = stretch;
                            OnProgressUpdated?.Invoke(i, progress[i], q.targetCount);
                            if (progress[i] >= q.targetCount) CompleteQuest(i);
                        }
                    }
                    break;

                case QuestType.ReachExfil:
                    // Driven by NotifyExfilReached()
                    break;
            }
        }

        // Reset damage flag each frame after processing
        hasTakenDamage = false;
    }

    // ─── Public notification methods ──────────────────────────────

    /// <summary>Call from PlayerHealth.TakeDamage().</summary>
    public void NotifyDamageTaken()
    {
        hasTakenDamage = true;
        if (playerController != null)
            lastDamageZ = playerController.DistanceRun;  // reset the no-damage start point
    }

    /// <summary>Call from GadgetManager.Activate() after the switch statement.</summary>
    public void NotifyGadgetUsed(string gadgetId)
    {
        if (gadgetId == "dash") IncrementQuests(QuestType.UseGadget, 1);
        if (gadgetId == "emp")  IncrementQuests(QuestType.UseEMPOnEnemies, 1);
    }

    /// <summary>Call from ExfilCheckpoint.</summary>
    public void NotifyExfilReached()
    {
        IncrementQuests(QuestType.ReachExfil, 1);
    }

    // ─── Internal ────────────────────────────────────────────────

    void IncrementQuests(QuestType type, int amount)
    {
        if (activeQuests == null) return;
        for (int i = 0; i < activeQuests.Length; i++)
        {
            if (completed[i]) continue;
            if (activeQuests[i].questType != type) continue;
            progress[i] += amount;
            OnProgressUpdated?.Invoke(i, progress[i], activeQuests[i].targetCount);
            if (progress[i] >= activeQuests[i].targetCount)
                CompleteQuest(i);
        }
    }

    void CompleteQuest(int slot)
    {
        if (completed[slot]) return;
        completed[slot] = true;
        QuestData q = activeQuests[slot];
        Debug.Log($"[QuestManager] COMPLETED: {q.questId}  XP:{q.xpReward}  Shards:{q.shardBonus}");
        SkillTree.Instance?.AddShards(q.shardBonus);
        OnQuestCompleted?.Invoke(q.xpReward);
        OnProgressUpdated?.Invoke(slot, q.targetCount, q.targetCount);
    }

    // ─── Getters for QuestUI ──────────────────────────────────────

    public QuestData GetQuest(int slot)    => activeQuests != null && slot < activeQuests.Length ? activeQuests[slot] : null;
    public int       GetProgress(int slot) => progress != null && slot < progress.Length ? progress[slot] : 0;
    public bool      IsCompleted(int slot) => completed != null && slot < completed.Length && completed[slot];
}