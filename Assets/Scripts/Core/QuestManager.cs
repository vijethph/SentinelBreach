using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        
        SelectQuests();
    }

    [Header("All quest definitions (drag all 8 QuestData assets)")]
    public QuestData[] allQuests;

    [Header("How many quests per run")]
    public int questsPerRun = 3;

    private QuestData[]  activeQuests;
    private int[]        progress;
    private bool[]       completed;

    public event Action<int>         OnQuestCompleted;    
    public event Action<int,int,int> OnProgressUpdated;   

    private PlayerController   playerController;
    private CollectibleManager collectibleManager;

    
    private float lastDamageZ   = 0f;   
    private bool  hasTakenDamage = false;

    void Start()
    {
        playerController   = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        collectibleManager = CollectibleManager.Instance;
        SubscribeToEvents();

        
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
                    
                    int currentDist = (int)dist;
                    if (currentDist != progress[i])
                    {
                        progress[i] = currentDist;
                        OnProgressUpdated?.Invoke(i, progress[i], q.targetCount);
                        if (progress[i] >= q.targetCount) CompleteQuest(i);
                    }
                    break;

                case QuestType.TakeDamage:
                    
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
                    
                    break;
            }
        }

        
        hasTakenDamage = false;
    }

    

    
    public void NotifyDamageTaken()
    {
        hasTakenDamage = true;
        if (playerController != null)
            lastDamageZ = playerController.DistanceRun;  
    }

    
    public void NotifyGadgetUsed(string gadgetId)
    {
        if (gadgetId == "dash") IncrementQuests(QuestType.UseGadget, 1);
        if (gadgetId == "emp")  IncrementQuests(QuestType.UseEMPOnEnemies, 1);
    }

    
    public void NotifyExfilReached()
    {
        IncrementQuests(QuestType.ReachExfil, 1);
    }

    

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
		QuestPopup.Instance?.ShowCompletion(q);
    }

    

    public QuestData GetQuest(int slot)    => activeQuests != null && slot < activeQuests.Length ? activeQuests[slot] : null;
    public int       GetProgress(int slot) => progress != null && slot < progress.Length ? progress[slot] : 0;
    public bool      IsCompleted(int slot) => completed != null && slot < completed.Length && completed[slot];
}